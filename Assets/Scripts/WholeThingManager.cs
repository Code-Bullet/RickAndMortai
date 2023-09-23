using Assets.Scripts.AIControllers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;


// this is the big daddy script that controls everything
// basically has a async function that continuously collects suggestions from chat, creates scenes and plays scenes. 
public class WholeThingManager : MonoBehaviour
{
    public static WholeThingManager Singleton;
    public OpenAISlurDetector slurDetectorChatGPT;
    public SlurDetectorEvan slurDetectorPhonic;
    public AIController AIController;
    public SceneDirector sceneDirector;
    public FakeYouAPIManager fakeYouAPIManager;
    public YouTubeChatFromSteven youTubeChat;

    public bool usingVoiceActing = true;

    public float wordsPerMinute = 250;

    public bool currentlyRunningScene = false;

    public TMP_Text textField;
    public TMP_Text dialogBox;


    private RickAndMortyScene nextScene = null;
    private bool stillGeneratingScene = false;


    public TMP_Text topicOption1;
    public TMP_Text topic1Votes;
    public BarWidthController topic1Bar;
    public TMP_Text topicOption2;
    public TMP_Text topic2Votes;
    public BarWidthController topic2Bar;
    public TMP_Text topicOption3;
    public TMP_Text topic3Votes;
    public BarWidthController topic3Bar;
    public TMP_Text topicTitleThing;
    public TMP_Text titleText;

    public GameObject bottomBarVotingInfoText;
    public GameObject topBarDiscordPluf;
    public string firstPrompt = "Banana";
    public bool runMainLoop = true;


    public bool useChatgptSlurDetection = false;
    public bool usePhonicSlurDetection = true;


    void Start()
    {
        Singleton = this;
        ToggleDiscordPlugEvery10Seconds();
        titleText.gameObject.SetActive(false);
        enableOrDisableVotingUI(false);

        AIController.Init();
        if (runMainLoop)
        {
            MainLoop();
        }
        // TestingShit();	

    }

    private async Task TestingShit()
    {
        // chill for a bit to give time to setup everything	
        await Task.Delay(2000);
        string response = await slurDetectorChatGPT.EnterPromptAndGetResponse("How do I install Tensorflow for my GPU?");
        Debug.Log("response " + response);
    }
    async void ToggleDiscordPlugEvery10Seconds()
    {
        while (true)
        {
            await Task.Delay(10000);
            topBarDiscordPluf.SetActive(!topBarDiscordPluf.activeSelf);
        }
    }

    void OnDestroy()
    {
        Singleton = null;
    }

    // turns on or off all the voting ui 
    private void enableOrDisableVotingUI(bool enable)
    {


        // topicTitleThing.enabled = enable;

        topic1Bar.gameObject.SetActive(enable);
        topic1Votes.enabled = enable;
        topicOption1.enabled = enable;

        topic2Bar.gameObject.SetActive(enable);
        topic2Votes.enabled = enable;
        topicOption2.enabled = enable;

        topic3Bar.gameObject.SetActive(enable);
        topic3Votes.enabled = enable;
        topicOption3.enabled = enable;

        bottomBarVotingInfoText.SetActive(enable);
    }
    //getCurrentTimestamp
    //forSceneStore
    public static string GetCurrentUnixTimestampAsString()
    {
        DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        long timestamp = (long)(DateTime.UtcNow - epoch).TotalSeconds;
        return timestamp.ToString();
    }

    // this is the big daddy
    private async Task MainLoop()
    {

        // chill for a bit to give time to setup everything
        await Task.Delay(10000);

        RickAndMortyScene currentScene = null;

        bool firstRunThrough = true;

        // change this line to enter your own prompt. vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        // start creating a scene while the first round of voting happens
        CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);

        for (int i = 0; i < 1000; i++)
        {

            float waitingCounter = 0;
            // if we're currently running a scene wait 
            while (currentlyRunningScene)
            {
                await Task.Delay(1000);

                // if we have waited for more that 8 minutes restart everything.
                waitingCounter += 1;
                if (waitingCounter > 8 * 60)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    return;
                }
            }

            // so scene has finished playing
            Debug.Log("scene done");
            dialogBox.text = "";


            // ok lets get the list of topics
            enableOrDisableVotingUI(true);
            List<string> randomTopics = youTubeChat.GetRandomTopics();

            if (randomTopics == null)
            {
                randomTopics = new List<string> {"morty keeps repeating Gar knee\nme",
                "Rick and morty fight batman\nme",
                  "Rick and morty go to Australia\nme" };
            }


            // the topics are stored like "name of topic \nauthor name \n"
            // so lets extract the topic and author 
            List<string> randomTopicAuthors = new List<string>();

            for (int j = 0; j < randomTopics.Count; j++)
            {
                string topic = randomTopics[j].Split("\n")[0];
                string author = randomTopics[j].Split("\n")[1];
                randomTopics[j] = topic;
                randomTopicAuthors.Add(author);
            }


            //display the topics
            topicOption1.text = randomTopics[0];
            topicOption2.text = randomTopics[1];
            topicOption3.text = randomTopics[2];

            // lets start the voting
            youTubeChat.ClearVotes();
            float voteTime = 0;
            int[] voteNumbers = youTubeChat.CountVotes();

            topic1Bar.ResetBar();
            topic2Bar.ResetBar();
            topic3Bar.ResetBar();
            targetTopic1Votes = 0;
            targetTopic2Votes = 0;
            targetTopic3Votes = 0;

            // since we are generating a scene in the background while we play a scene, the generating scene needs to finish generating before we finish voting
            // and we also wait a minimum of 30 seconds
            while (stillGeneratingScene || voteTime < 30f)
            {
                //get the votes
                voteNumbers = youTubeChat.CountVotes();


                // this is for testing
                // voteNumbers[0] = UnityEngine.Random.Range(1, 101);
                // voteNumbers[1] = UnityEngine.Random.Range(1, 101);
                // voteNumbers[2] = UnityEngine.Random.Range(1, 101);


                // all this shit is for having the vote text move smoothly, dont worry about it
                initialTopic1Votes = targetTopic1Votes;
                initialTopic2Votes = targetTopic2Votes;
                initialTopic3Votes = targetTopic3Votes;
                targetTopic1Votes = voteNumbers[0];
                targetTopic2Votes = voteNumbers[1];
                targetTopic3Votes = voteNumbers[2];
                StopCoroutine(UpdateVotesTextOverTime(topic1Votes, initialTopic1Votes, targetTopic1Votes));
                StartCoroutine(UpdateVotesTextOverTime(topic1Votes, initialTopic1Votes, targetTopic1Votes));
                StopCoroutine(UpdateVotesTextOverTime(topic2Votes, initialTopic2Votes, targetTopic2Votes));
                StartCoroutine(UpdateVotesTextOverTime(topic2Votes, initialTopic2Votes, targetTopic2Votes));
                StopCoroutine(UpdateVotesTextOverTime(topic3Votes, initialTopic3Votes, targetTopic3Votes));
                StartCoroutine(UpdateVotesTextOverTime(topic3Votes, initialTopic3Votes, targetTopic3Votes));


                //calculate the highest votes so we can fill the vote bars relative to it.
                int maxvotes = 0;
                foreach (int voteNumber in voteNumbers)
                {
                    if (maxvotes < voteNumber)
                    {
                        maxvotes = voteNumber;
                    }
                }
                if (maxvotes == 0)
                {
                    topic1Bar.SetFillPercentage(0);
                    topic2Bar.SetFillPercentage(0);
                    topic3Bar.SetFillPercentage(0);
                }
                else
                {
                    topic1Bar.SetFillPercentage((float)voteNumbers[0] / (float)maxvotes);
                    topic2Bar.SetFillPercentage((float)voteNumbers[1] / (float)maxvotes);
                    topic3Bar.SetFillPercentage((float)voteNumbers[2] / (float)maxvotes);
                }

                // wait for a little bit.
                await Task.Delay(500);
                voteTime += 0.5f;

                // if we have been voting for more than 5 minutes (10 minutes if this is the first loop) then restart everything 
                float timeBeforeRestarting = 5 * 60;
                if (firstRunThrough) timeBeforeRestarting += 5 * 60;
                if (voteTime > timeBeforeRestarting)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    return;
                }
            }

            // ok voting is done 

            //get the chosen topic, in the ugliest way possible, wtf is this shit.
            int chosenTopic = 0;
            if (voteNumbers[0] > voteNumbers[1] && voteNumbers[0] > voteNumbers[2]) chosenTopic = 0;
            if (voteNumbers[1] > voteNumbers[0] && voteNumbers[1] > voteNumbers[2]) chosenTopic = 1;
            if (voteNumbers[2] > voteNumbers[1] && voteNumbers[2] > voteNumbers[0]) chosenTopic = 2;

            // choose a backup topic just incase the chosen topic is rejected by chatgpt
            int backupTopic = 0;
            if (backupTopic == chosenTopic)
            {
                backupTopic = 1;
            }


            currentScene = nextScene;
            enableOrDisableVotingUI(false);

            // add the chosen topic to the blacklist so it doesnt play again
            youTubeChat.AddToBlacklist(randomTopics[chosenTopic]);

            // both of these are async functions, so they will run in the backgound, this means we are running a scene and generating a scene at the same time. 
            RunScene(currentScene);
            CreateScene(randomTopics[chosenTopic], randomTopicAuthors[chosenTopic], randomTopics[backupTopic], randomTopicAuthors[backupTopic], usingVoiceActing);
            firstRunThrough = false;
        }

    }

    // this bad boy displays the title then runs the scene 
    public async Task RunScene(RickAndMortyScene scene)
    {
        currentlyRunningScene = true;

        // display title

        //run the scene
        await sceneDirector.PlayScene(scene.chatGPTOutputLines, scene.ttsVoiceActingLines);

        // scene done
        currentlyRunningScene = false;
    }


    // this is the main bitch of the program. a bunch of calling other scripts to get each element of the scene.
    // basically this turns an input prompt into a list of lines of dialog + stage directions, and a list of audio files for the tts.
    public async Task CreateScene(string prompt, string promptAuthor, string backupPrompt, string backupPromptAuthor, bool isThisSceneUsingVoiceActing)
    {
        string initialPrompt = "";
        string chatGPTOutput = "";
        string[] chatGPTOutputLines = null;
        string creatingScene = "";
        bool foundGoodPrompt = false;

        while (!foundGoodPrompt)
        {

            stillGeneratingScene = true;
            initialPrompt = prompt;
            creatingScene = "Currently Creating: " + prompt;
            textField.text = creatingScene + " --- " + "Generating script...";

            // add some shit to the prompt
            //prompt += ". Make sure to use light profanity like frick, shoot and crap. Scripts should have at least 30 lines of dialog.";
            // prompt += ". Rick and Morty are currently in " + sceneDirector.currentDimension.name + ". Make sure to use light profanity like frick, shoot and crap. Scripts should have at least 30 lines of dialog.";


            // chuck the prompt into chatgpt
            chatGPTOutput = await AIController.EnterPromptAndGetResponse(prompt);

            // add the title and author to the scene so the narrator speaks them
            chatGPTOutput = "Narrator: " + initialPrompt + "\n" +
                            "Narrator: Prompt By: " + promptAuthor + "\n" + chatGPTOutput;


            // this errases chatgpts memorty so it doesnt overload the max tokens cap
            // dont worry about it
            AIController.Clear();


            textField.text = creatingScene + " --- " + "Processing script...";

            // process the message into indavidual lines
            string str = AIController.OutputString;
            chatGPTOutputLines = Utils.ProcessOutputIntoStringArray(chatGPTOutput, ref str);
            AIController.OutputString = str;

            // save a text file
            try
            {
                // Get the current date and time
                string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Create the full path for the file
                string path = $"{Environment.CurrentDirectory}\\Assets\\Example Scripts\\OutputScripts\\{dateTimeString}_{initialPrompt}.txt";

                // Create an empty file and close it immediately
                using (FileStream fs = File.Create(path))
                {
                    // Close the file immediately to allow subsequent write operations
                }

                // Write the string to the file
                File.WriteAllText(path, chatGPTOutput);

                // Log success
                Debug.Log("Data saved successfully to: " + path);
            }
            catch (System.Exception e)
            {
                // Log any exceptions that occur
                Debug.LogError("An error occurred while saving data: " + e.Message);
            }


            // if the number of lines is less that 10 this means that chatgpt was like "WAAAAAA i cant do that"
            if (chatGPTOutputLines.Length < 10)
            {
                Debug.Log("oh no we cant do that");
                prompt = backupPrompt;
                promptAuthor = backupPromptAuthor;
                initialPrompt = prompt;
                youTubeChat.AddToBlacklist(backupPrompt);
                // in the case of a double fail this be the chosen story
                backupPrompt = "Generate a Random story";
                backupPromptAuthor = "Me because you guys are nasty";
            }
            else
            {
                textField.text = creatingScene + " --- " + "Detecting Slurs...";



                if (useChatgptSlurDetection)
                {
                    string deslurredChatgptOutput = slurDetectorChatGPT.RemoveDirectSlurs(chatGPTOutput);
                    // ask chatgpt to remove slurs because you guys are too creative	
                    // this will return all the slurs in square brackets e.g. [Nword][Nword but spelt slightly different]	
                    string detectedSlurs = await slurDetectorChatGPT.EnterPromptAndGetResponse(deslurredChatgptOutput);
                    if (detectedSlurs.ToLower().Contains("no slurs detected"))
                    {
                        Debug.Log("Slur free yay " + deslurredChatgptOutput);
                        // ok we good	
                    }
                    else
                    {
                        // get the slurs into an array	
                        string[] detectedSlurArray = Regex.Split(detectedSlurs, @"\[|\]");
                        string[] detectedSlurArrayFiltered = System.Array.FindAll(detectedSlurArray, s => !string.IsNullOrEmpty(s));
                        // if the shit is empty then that means something fucked up. 	
                        // go to the backup prompt 	
                        if (detectedSlurArrayFiltered.Length == 0)
                        {
                            Debug.Log("probably slurs so im not gonna risk it");
                            prompt = backupPrompt;
                            promptAuthor = backupPromptAuthor;
                            initialPrompt = prompt;
                            youTubeChat.AddToBlacklist(backupPrompt);
                            // in the case of a double fail this be the chosen story	
                            backupPrompt = "Generate a Random story";
                            backupPromptAuthor = "Me because you guys are nasty";
                            continue;
                        }
                        else
                        {
                            Debug.Log("here be the slurs vvvvvv");
                            foreach (string s in detectedSlurArrayFiltered)
                            {
                                Debug.Log(s);
                            }
                            foreach (string slur in detectedSlurArrayFiltered)
                            {
                                string pattern = Regex.Escape(slur);
                                deslurredChatgptOutput = Regex.Replace(deslurredChatgptOutput, pattern, "nope", RegexOptions.IgnoreCase);
                            }
                            chatGPTOutput = deslurredChatgptOutput;
                            str = AIController.OutputString;
                            chatGPTOutputLines = Utils.ProcessOutputIntoStringArray(chatGPTOutput, ref str);
                            AIController.OutputString = str;
                        }
                        //we good i think, we should be slur free. yay	
                    }



                }
                Debug.Log("ah");
                if (usePhonicSlurDetection)
                {
                    Debug.Log("get detecting boy");
                    for (int i = 0; i < chatGPTOutputLines.Length; i++)
                    {
                        Debug.Log(i);
                        chatGPTOutputLines[i] = slurDetectorPhonic.RemoveSlurs(chatGPTOutputLines[i]);
                    }
                    Debug.Log("yay");

                }

                foundGoodPrompt = true;
            }
        }

        try
        {
            // Get the current date and time	
            string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // remove the special characters because this fucks with file saving
            string initialPromptWithNoSpecialCharacters = Regex.Replace(initialPrompt, @"[^a-zA-Z0-9\s]", "");
            // Create the full path for the file	
            string path = $"{Environment.CurrentDirectory}\\Assets\\Example Scripts\\OutputScripts\\{dateTimeString}_{initialPromptWithNoSpecialCharacters}.txt";
            // Create an empty file and close it immediately	
            using (FileStream fs = File.Create(path))
            {
                // Close the file immediately to allow subsequent write operations	
            }
            // Write the string to the file	
            File.WriteAllText(path, chatGPTOutput);
            // Log success	
            Debug.Log("Data saved successfully to: " + path);
        }
        catch (System.Exception e)
        {
            // Log any exceptions that occur	
            Debug.LogError("An error occurred while saving data: " + e.Message);
        }
        textField.text = creatingScene + " --- " + "Detecting Dialog...";
        // extract the dialog info from the output lines this includes the voiceModelUUIDs, the character names, and the text that they speak.	
        List<string>[] dialogInfo = sceneDirector.ProcessDialogFromLines(ref chatGPTOutputLines);
        List<string> voiceModelUUIDs = dialogInfo[0];
        List<string> characterNames = dialogInfo[1];
        List<string> textsToSpeak = dialogInfo[2];
        textField.text = creatingScene + " --- " + "Generating FakeYou TTS...";
        // generate text to speech voice acting based on dialog	
        List<AudioClip> ttsVoiceActingOrdered = null;
        if (isThisSceneUsingVoiceActing)
        {
            ttsVoiceActingOrdered = await fakeYouAPIManager.GenerateTTS(textsToSpeak, voiceModelUUIDs, characterNames, textField, creatingScene);
        }
        textField.text = creatingScene + " --- " + "Done :)";
        // we done	
        stillGeneratingScene = false;
        nextScene = new RickAndMortyScene(initialPrompt, promptAuthor, chatGPTOutputLines, ttsVoiceActingOrdered);
        
        BinarySerialization.WriteToBinaryFile<RickAndMortyScene>($"RickAndMortyScenesStore/{GetCurrentUnixTimestampAsString()}.bin", nextScene);

    }


    // this shit is so the vote text changes smoothly over time
    private int initialTopic1Votes = 0;
    private int initialTopic2Votes = 0;
    private int initialTopic3Votes = 0;

    private int targetTopic1Votes = 0;
    private int targetTopic2Votes = 0;
    private int targetTopic3Votes = 0;
    IEnumerator UpdateVotesTextOverTime(TMP_Text targetTextObject, int initialVotes, int targetVotes)
    {
        float elapsedTime = 0;
        float timeToChange = 0.5f; // The time over which to change the text
        while (elapsedTime < timeToChange)
        {
            elapsedTime += Time.deltaTime;

            // Interpolate between initial and target votes
            float interpolatedVotes = Mathf.Lerp(initialVotes, targetVotes, elapsedTime / timeToChange);

            // Update the text object
            targetTextObject.text = "VOTES: " + Mathf.RoundToInt(interpolatedVotes).ToString();

            yield return null;
        }

        // Make sure the final value is set accurately
        targetTextObject.text = "VOTES: " + targetVotes.ToString();
    }



}

[Serializable]
public class RickAndMortyScene
{
    public string titleString;
    public string author;
    public string[] chatGPTOutputLines;
    public List<AudioClip> ttsVoiceActingLines;

    public RickAndMortyScene(string initialPrompt, string promptAuthor, string[] outputLines, List<AudioClip> voiceActing)
    {
        titleString = initialPrompt;
        author = promptAuthor;
        chatGPTOutputLines = outputLines;
        ttsVoiceActingLines = ttsVoiceActingLines;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(WholeThingManager))]
public class RandomScript_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        WholeThingManager script = (WholeThingManager)target;
        EditorGUI.BeginChangeCheck();
        serializedObject.UpdateIfRequiredOrScript();
        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            using (new EditorGUI.DisabledScope("m_Script" == iterator.propertyPath))
            {
                if (iterator.name == "wordsPerMinute")
                {
                    if (!script.usingVoiceActing) script.wordsPerMinute = EditorGUILayout.FloatField("Words Per Minute", script.wordsPerMinute);
                }
                else
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            enterChildren = false;
        }

        serializedObject.ApplyModifiedProperties();
        EditorGUI.EndChangeCheck();
    }
}
#endif

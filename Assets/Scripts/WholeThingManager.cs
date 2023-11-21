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
using System.Linq;


// this is the big daddy script that controls everything
// basically has a async function that continuously collects suggestions from chat, creates scenes and plays scenes. 
public class WholeThingManager : MonoBehaviour
{
    public static WholeThingManager Singleton;
    public OpenAISlurDetector slurDetectorChatGPT;
    public SlurDetectorEvan slurDetectorPhonic;
    public AIController AIController;
    public OpenAICameraDirector openAICameraDirector;
    public SceneDirector sceneDirector;
    public FakeYouAPIManager fakeYouAPIManager;
    public YouTubeChatFromSteven youTubeChat;
    public ReplicateAPI replicateAPI;

    public bool usingVoiceActing = true;
    public bool generateCustomScript = false;
    public bool replayOldScene = true;
    public string oldSceneID = "scene-0eb7fec2-e9ed-5a25-ebe8-771a38c2dcff";

    public float wordsPerMinute = 250;

    public bool currentlyRunningScene = false;

    public TMP_Text textField;
    public TMP_Text dialogBox;




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

    public bool usingChatGptCameraShots = true;

    public bool useChatgptSlurDetection = false;
    public bool usePhonicSlurDetection = true;
    public bool useAiArt = true;
    public bool useDanceAnimations = true;

    public CharacterInfo defaultGuy;
    public AiArtDimensionController aiArtDimension;
    public AiArtCharacterController aiArtCharacter;

    public bool useDefaultScript = false;
    public TextAsset defaultScript;


    public bool waitForVoting = true;

    public bool justDoOneScene = false;

    public bool runningTestTopicList = false;
    public List<string> testTopicList;

    public RandomCameraDance danceFloorManager;

    // Environment variables / Global configuration.
    public GlobalConfigData config;

    private void loadConfig()
    {
        string configFilePath = Path.Combine(Application.dataPath, "config.json");

        if (!File.Exists(configFilePath))
        {
            Debug.LogError("config.json not found in the project root.");
            this.config = GlobalConfigData.CreateFromEnvVars();
        }
        else
        {
            // Read the JSON file
            string jsonText = File.ReadAllText(configFilePath);
            this.config = GlobalConfigData.CreateFromJSON(jsonText);
        }
    }



    // Start -> MainLoop -> RunScene(currentScene)
    //                      -> sceneDirector.PlayScene
    //                   -> CreateScene
    //                      this.nextScene = blah...
    //                                 


    void Awake()
    {
        Singleton = this;
        loadConfig();

        ToggleDiscordPlugEvery10Seconds();
        titleText.gameObject.SetActive(false);
        enableOrDisableVotingUI(false);

        AIController.Init();
        openAICameraDirector.Init();


        // TestingShit();
    }

    async void Start()
    {
        sceneDirector.ResetStuff();
        if (this.generateCustomScript)
        {
            enableOrDisableVotingUI(false);

            Debug.Log("generating one custom scene then playing it");
            var scene = await CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);

            Debug.Log("rendering newly generated scene");
            await RunScene(scene);

            return;
        }
        else if (this.replayOldScene)
        {
            enableOrDisableVotingUI(false);

            textField.text = "Re-running old scene...";
            //CreateScene(firstPrompt, "me", "banana", "me", false);
            Debug.Log("loaded old scene");
            var scene = RickAndMortyScene.ReadFromDir(oldSceneID);
            await RunScene(scene);

            //await RunScene(RickAndMortyScene.ReadFromDir("scene-0eb7fec2-e9ed-5a25-ebe8-771a38c2dcff"));
            //await RunScene(RickAndMortyScene.ReadFromDir("scene-2b934590-3862-df99-43bf-3514e0ef8493"));

            return;
        }
        else if (runMainLoop)
        {
            await MainLoop();
        }
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
        //topicTitleThing.enabled = enable;

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



    // this is the big daddy
    private async Task MainLoop()
    {
        // The scene generating in the background.
        Task<RickAndMortyScene> nextSceneTask;

        // Start creating a scene while the first round of voting happens
        // change this line to enter your own prompt. vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        nextSceneTask = CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);

        // if (runningTestTopicList && testTopicList.Count > 0) 
        // {
        //     CreateScene(testTopicList[0], "me", "banana", "me", usingVoiceActing);
        //     testTopicList.RemoveAt(0);
        // }
        // else
        // {
        //     CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);
        // }

        bool testingTopics = true;
        List<string> testTopics = new List<string> {
            "morty talks with sadam hussein\nme",
            "Rick and morty fight batman\nme",
            "Rick and morty go to Australia\nme"
        };
        int testTopicIndex = 0;


        // Generate 1000 episodes in a loop.
        //
        // Each iteration:
        // 1. Run the voting UI.
        // 2. Wait until the next topic is selected by the audience.
        // 3. Play the generated scene from the previous voting round.
        // 4. In the background, generate the nextScene from the topic selected in (2).
        //
        for (int i = 0; i < 1000; i++)
        {
            bool firstRunThrough = (i == 0);
            float waitingCounter = 0;

            // if we're currently running a scene wait 
            while (currentlyRunningScene)
            {
                await Task.Delay(1000);

                // if we have waited for more that 8 minutes restart everything.
                waitingCounter += 1;
                // if (waitingCounter > 8 * 60)
                // {
                //     SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                //     return;
                // }
            }

            // so scene has finished playing
            if (i > 0) Debug.Log("scene done");
            dialogBox.text = "";


            // ok lets get the list of topics
            enableOrDisableVotingUI(true);
            List<string> randomTopics = youTubeChat.GetRandomTopics();

            if (randomTopics == null)
            {
                randomTopics = new List<string> {"morty talks with yoda\nme",
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
            if(useDanceAnimations) danceFloorManager.DanceCameraStart();


            while (!nextSceneTask.IsCompleted || (voteTime < 30f && waitForVoting))
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
                //     float timeBeforeRestarting = 5 * 60;
                //     if (firstRunThrough) timeBeforeRestarting += 5 * 60;
                //     if (voteTime > timeBeforeRestarting)
                //     {
                //         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                //         return;
                //     }
            }

            // ok voting is done 

            // Get the chosen topic (not so ugly, actually kinda gurd).
            int chosenTopic = 0;
            for(int j = 0; j < voteNumbers.Length; j++)
            {
                if (voteNumbers[j] > voteNumbers[chosenTopic]) chosenTopic = j;
            }
            
            if(testingTopics)
            {
                // Testing main loop.

                // Just cycle through the test topics.
                testTopicIndex = (testTopicIndex + 1) % testTopics.Count; // i+1 mod n
                chosenTopic = testTopicIndex;
            }

            // choose a backup topic just incase the chosen topic is rejected by chatgpt
            int backupTopic = 0;
            if (backupTopic == chosenTopic)
            {
                backupTopic = 1;
            }

            // Get the next scene.
            RickAndMortyScene currentScene = await nextSceneTask;
            enableOrDisableVotingUI(false);
            
            // add the chosen topic to the blacklist so it doesnt play again
            youTubeChat.AddToBlacklist(randomTopics[chosenTopic]);

            // if (runningTestTopicList && testTopicList.Count > 0)
            // {
            //     CreateScene(testTopicList[0], "me", "banana", "me", usingVoiceActing);
            //     testTopicList.RemoveAt(0);

            // }
            // else
            // {
            //     CreateScene(randomTopics[chosenTopic], randomTopicAuthors[chosenTopic], randomTopics[backupTopic], randomTopicAuthors[backupTopic], usingVoiceActing);
            // }

            if (useDanceAnimations) danceFloorManager.DanceCameraStop();

            // Run the current scene.
            // In the background, generate the next scene based on voting topic.
            if (justDoOneScene)
            {
                await RunScene(currentScene);
                return;
            }

            Debug.Log($"main loop #{i}: \ncurrent scene: {currentScene.chatGPTRawOutput}\n\nnext scene: {randomTopics[chosenTopic]}");
            nextSceneTask = CreateScene(randomTopics[chosenTopic], randomTopicAuthors[chosenTopic], randomTopics[backupTopic], randomTopicAuthors[backupTopic], usingVoiceActing);
            await RunScene(currentScene);
        }

    }

    // this bad boy displays the title then runs the scene 
    public async Task RunScene(RickAndMortyScene scene)
    {
        currentlyRunningScene = true;

        // 
        // 1. Prepare.
        //
        if (scene.aiArt.character?.head3d != null)
        {
            Debug.Log("using AI head");

            // 1. Tell scene director to use 3D default guy.
            sceneDirector.use3DGuy = true;

            // 2. Rig the head to the default guy.
            var character = sceneDirector.defaultGuy3d;
            var headRiggerComponent = character.GetComponent<AIHeadRiggerBase>();
            if (headRiggerComponent == null)
            {
                throw new Exception("Character does not have AIHeadRiggerBase behaviour");
            }

            headRiggerComponent.RenderGeneration(
                scene.aiArt.character.head3d.characterKey,
                //scene.aiArt.character.head3d.generationIds[1]
                scene.aiArt.character.head3d.selectedGeneration
            );
        }
        else
        {
            sceneDirector.use3DGuy = false;
            aiArtCharacter.Prepare(scene.aiArt.character);
        }

        aiArtDimension.Prepare(scene.aiArt.dimension);
        aiArtCharacter.Prepare(scene.aiArt.character);


        //
        // 2. Play scene.
        //
        await sceneDirector.PlayScene(scene.chatGPTOutputLines, scene.ttsVoiceActingLines);


        //
        // 3. Reset for next scene.
        //
        aiArtDimension.Reset();
        aiArtCharacter.Reset();

        // scene done
        currentlyRunningScene = false;
    }

    // Generates a script based on a prompt.
    // Uses various checks to make a good one:
    // - profanity filter
    // - ChatGPT ragequit filter
    public async void GenerateScript()
    {
        
    }

    // Parses a script.
    // Extracts:
    // - character dialogue (name, lines, voice track uuid)
    public void ParseScriptExtractCharacters()
    {
    }

    // GetWhosTalking (string line) -> string characterName, bool isGeneratedCharacter?
    // ParseStageDirections -> (string? generatedDimension)
    // GetCharacters - "[rick walks to morty]  returns chacters 1 rick, character 2 morty.



    // this is the main bitch of the program. a bunch of calling other scripts to get each element of the scene.
    // basically this turns an input prompt into a list of lines of dialog + stage directions, and a list of audio files for the tts.
    public async Task<RickAndMortyScene> CreateScene(string prompt, string promptAuthor, string backupPrompt, string backupPromptAuthor, bool isThisSceneUsingVoiceActing)
    {
        Debug.Log("CreateScene");

        string initialPrompt = "";
        string chatGPTOutput = "";
        string[] chatGPTOutputLines = null;
        string[] chatGPTOutputLinesWithSwearing = null;
        string creatingScene = "";
        bool foundGoodPrompt = false;

        while (!foundGoodPrompt)
        {

            initialPrompt = prompt;
            creatingScene = "Currently Creating: " + prompt;
            textField.text = creatingScene + " --- " + "Generating script...";

            // add some shit to the prompt
            //prompt += ". Make sure to use light profanity like frick, shoot and crap. Scripts should have at least 30 lines of dialog.";
            // prompt += ". Rick and Morty are currently in " + sceneDirector.currentDimension.name + ". Make sure to use light profanity like frick, shoot and crap. Scripts should have at least 30 lines of dialog.";

            if (!useDefaultScript)
            {
                // chuck the prompt into chatgpt

                chatGPTOutput = await AIController.EnterPromptAndGetResponse(prompt);
            }
            else
            {

                chatGPTOutput = defaultScript.text;
                await Task.Delay(1000);
            }



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
            // if (!useDefaultScript)
            // {
            //     // save a text file
            //     try
            //     {
            //         // Get the current date and time
            //         string dateTimeString = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            //         // Create the full path for the file
            //         string path = $"{Environment.CurrentDirectory}\\Assets\\Example Scripts\\OutputScripts\\{dateTimeString}_{initialPrompt}.txt";

            //         // Create an empty file and close it immediately
            //         using (FileStream fs = File.Create(path))
            //         {
            //             // Close the file immediately to allow subsequent write operations
            //         }

            //         // Write the string to the file
            //         File.WriteAllText(path, chatGPTOutput);

            //         // Log success
            //         Debug.Log("Data saved successfully to: " + path);
            //     }
            //     catch (System.Exception e)
            //     {
            //         // Log any exceptions that occur
            //         Debug.LogError("An error occurred while saving data: " + e.Message);
            //     }
            // }


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
                // // add camera angles
                // if (usingChatGptCameraShots)
                // {
                //     textField.text = creatingScene + " --- " + "Adding Camera Angles";

                //     chatGPTOutput = await openAICameraDirector.EnterPromptAndGetResponse(chatGPTOutput);
                //     openAICameraDirector.Clear();


                //     // chatGPTOutput = "Narrator: " + initialPrompt + "\n" + "Narrator: Prompt By: " + promptAuthor + "\n" + chatGPTOutput;

                //     string str2 = AIController.OutputString;
                //     chatGPTOutputLines = Utils.ProcessOutputIntoStringArray(chatGPTOutput, ref str2);
                //     AIController.OutputString = str2;

                // }




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

                if (usePhonicSlurDetection)
                {
                    for (int i = 0; i < chatGPTOutputLines.Length; i++)
                    {
                        chatGPTOutputLines[i] = slurDetectorPhonic.RemoveSlurs(chatGPTOutputLines[i]);
                    }

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

        //if (generateCustomScript)
        //{
        //    //chatGPTOutput = @"Narrator: Liam and Sia talk about lesbian swimming pools
        //    //Spongebob: hey sia, what's the deal with sponge baths
        //    //Morty: for the purposes of this script, I am liam
        //    //Morty: I don't know sia!
        //    //";


        //    chatGPTOutput = @"
        //    {Wide shot}
        //    Rick: I gotta get to that coffee shop before they close Morty
        //    [Rick and Morty enter the portal to an Amsterdam coffee shop]
        //    Dutch Man: hallo, wil je een junko?
        //    Morty: ummmmmm. ja. ik zoek...
        //    Morty: rick I'm not sure what weed to get. they all have weird names...like gorilla glue? why would gorillas need glue
        //    Rick: it's a metaphor morty, a beautiful european metaphor. just go with it
        //    Rick: hi I'd like the albert heinous headfucker tripel de luxe
        //    Dutch Man: zeker man
        //    Narrator: four hours and twenty minutes later
        //    [Rick and Morty enter the portal to the Garage]
        //    Rick: oh man I really shouldn't have eaten those stroopwaffels
        //    Morty: Pakker wat je pakken kan
        //    Rick: what did you say to me you lil shit?";
            

        //    //chatGPTOutput = @"Narrator: Rick and morty talk to LLaMa69 about getting funding for a startup
        //    //        {Close up Rick}
        //    //        Rick: morty! come over here, grandpa needs your help grifting venture capitalists's
        //    //        Rick: with a chat g p t wrapper aye eye startup
        //    //        Morty: aw rick, this sounds like a lot of work
        //    //        Morty: can't we just build a crypto startup and sit on the money?
        //    //        Rick: no Morty, we have to build Aye Gee Eye. I have to beat Sam Altman and reclaim the valley from twink CEE EE OHS's
        //    //        Morty: don't you think that OpenAI has a real moat...you know...compared to our React app
        //    //        Rick: shut up morty, you don't anything about frontend. now where did grandpa leave his ritalin
        //    //        Rick: it's time to deployyyyyyyy!!!";

        //    promptAuthor = "LLaMa69";

        //    // Trim any extraneous space from the string.
        //    chatGPTOutputLines = chatGPTOutput.Split("\n").Select(line => line.Trim()).ToArray();
        //}


        chatGPTOutputLinesWithSwearing = Utils.AddSwearing(chatGPTOutputLines);


        string nameOfAiGeneratedCharacter = null;
        string nameOfAiGeneratedDimension = null;

        Debug.Log("sceneDirector.ProcessDialogFromLines");
        // extract the dialog info from the output lines this includes the voiceModelUUIDs, the character names, and the text that they speak.	
        List<string>[] dialogInfo = sceneDirector.ProcessDialogFromLines(ref chatGPTOutputLines, ref nameOfAiGeneratedCharacter, ref nameOfAiGeneratedDimension);
        List<string>[] dialogInfoWithSwearing = sceneDirector.ProcessDialogFromLines(ref chatGPTOutputLinesWithSwearing, ref nameOfAiGeneratedCharacter, ref nameOfAiGeneratedDimension);
        List<string> voiceModelUUIDs = dialogInfoWithSwearing[0];
        List<string> characterNames = dialogInfoWithSwearing[1];
        List<string> textsToSpeak = dialogInfoWithSwearing[2];


        Debug.Log("ai generated names for stuff");
        Debug.Log(nameOfAiGeneratedCharacter);
        Debug.Log(nameOfAiGeneratedDimension);

        List<Task> allConcurrentTasks = new List<Task>();


        // Start both tasks in parallel
        // var aiArtTask = replicateAPI.GenerateAndSetTexturesForCharacter(defaultGuy, nameOfAiGeneratedCharacter);
        Task<AIArtStuff> aiArtTask = null;
        if (useAiArt)
        {
            aiArtTask = replicateAPI.DoAllTheAiArtStuffForAScene(nameOfAiGeneratedCharacter, nameOfAiGeneratedDimension);
            allConcurrentTasks.Add(aiArtTask);
        }

        textField.text = creatingScene + " --- " + "Generating FakeYou TTS...";
        Debug.Log(creatingScene + " --- " + "Generating FakeYou TTS...");

        Task<List<AudioClip>> ttsVoiceActingTask = null;
        if (isThisSceneUsingVoiceActing)
        {
            ttsVoiceActingTask = fakeYouAPIManager.GenerateTTS(textsToSpeak, voiceModelUUIDs, characterNames, textField, creatingScene);
            allConcurrentTasks.Add(ttsVoiceActingTask);

        }

        Task<string> CameraShotsChatGPTTask = null;
        if (usingChatGptCameraShots)
        {
            string outputLinesReMerged = string.Join("\n", chatGPTOutputLines);
            CameraShotsChatGPTTask = openAICameraDirector.EnterPromptAndGetResponse(outputLinesReMerged);
            // string cameraChatGPTOutput = await openAICameraDirector.EnterPromptAndGetResponse(outputLinesReMerged);
            // // string cameraChatGPTOutput = CameraShotsChatGPTTask.Result;
            // char[] delims = new[] { '\r', '\n' };
            // string[] outputLinesProcessedWithCameraShots = cameraChatGPTOutput.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            // chatGPTOutputLines = outputLinesProcessedWithCameraShots;
            allConcurrentTasks.Add(CameraShotsChatGPTTask);

        }




        Debug.Log("start await");
        if (allConcurrentTasks.Count > 0)
        {
            await Task.WhenAll(allConcurrentTasks);
        }


        // if (aiArtTask != null && ttsVoiceActingTask != null)
        // {
        //     await Task.WhenAll(aiArtTask, ttsVoiceActingTask);
        // }
        // else if (aiArtTask != null)
        // {
        //     await Task.WhenAll(aiArtTask);
        // }
        // else if (ttsVoiceActingTask != null)
        // {
        //     await Task.WhenAll(ttsVoiceActingTask);
        // }

        Debug.Log("finish await");
        //retrieve the result of ttsVoiceActingTask after awaiting it

        if (usingChatGptCameraShots)
        {

            string cameraChatGPTOutput = CameraShotsChatGPTTask.Result;
            char[] delims = new[] { '\r', '\n' };
            string[] outputLinesProcessedWithCameraShots = cameraChatGPTOutput.Split(delims, StringSplitOptions.RemoveEmptyEntries);

            // ok so now we have 2 scripts 1 with camera angles and 1 without, sometimes the one without removes lines and shit, so we cant
            // just use that we have to merge them


            List<string> combinedList = chatGPTOutputLines.ToList();

            int checkedIndexOnCombinedList = 0;
            //length -1 because we dont care if a camera instruciton is at the end of the list
            for (int i = 0; i < outputLinesProcessedWithCameraShots.Length - 1; i++)
            {
                string lineWeChecking = outputLinesProcessedWithCameraShots[i];
                //if this bitch is a camera command
                if (lineWeChecking.Contains("{") && !lineWeChecking.Contains(":"))
                {

                    // then we get the instuction after this one and find it in the original array.
                    string nextInstruction = outputLinesProcessedWithCameraShots[i + 1];

                    // dont start at 0 so if 2 lines are the same we dont insert it again
                    for (int j = checkedIndexOnCombinedList; j < combinedList.Count; j++)
                    {
                        // Clean strings by removing special characters, spaces, and converting to lowercase

                        string cleanedString1 = Regex.Replace(combinedList[j], "[^a-zA-Z0-9]", "").ToLower();
                        string cleanedString2 = Regex.Replace(nextInstruction, "[^a-zA-Z0-9]", "").ToLower();
                        //match found
                        if (cleanedString1 == cleanedString2)
                        {
                            // add the instruction in before j
                            combinedList.Insert(j, lineWeChecking);

                            // move the checked index forward so we dont add another line before this.
                            // its +2 becauses we inserted an item which increases the index by 1 and then we want to move the pointer to the next instuction
                            checkedIndexOnCombinedList = j + 2;
                            break;
                        }
                    }
                }

            }


            // ok now check for entering portals, only 2 shots actually look good so change it to either wide shot, or tracking shot behind.
            for (int i = 1; i < combinedList.Count; i++)
            {
                string lineWeChecking = combinedList[i];
                if (lineWeChecking.Contains("[") && lineWeChecking.ToLower().Contains("portal to"))
                {
                    string previousLine = combinedList[i - 1];
                    if (!previousLine.Contains("{"))
                    {
                        combinedList.Insert(i, "{Wide Shot}");
                        i += 1;
                        continue;
                    }
                    else if (!previousLine.ToLower().Contains("wide shot"))
                    {
                        //if the previous shot isnt a wide shot then add a tracking shot behind.
                        combinedList[i - 1] = "{Tracking shot, Morty, behind}";
                        continue;
                    }
                }
            }




            string outputLinesReMerged = string.Join("\n", chatGPTOutputLines);

            Debug.Log("original: \n " + string.Join("\n", chatGPTOutputLines));
            Debug.Log("Chatgpt camer angles: \n " + string.Join("\n", outputLinesProcessedWithCameraShots));
            Debug.Log("Combined: \n " + string.Join("\n", combinedList.ToArray()));



            chatGPTOutputLines = combinedList.ToArray();
            chatGPTOutputLinesWithSwearing = Utils.AddSwearing(chatGPTOutputLines);

            // chatGPTOutputLines = outputLinesProcessedWithCameraShots;
        }


        List<AudioClip> ttsVoiceActingOrdered = new List<AudioClip>();
        if (isThisSceneUsingVoiceActing)
        {
            ttsVoiceActingOrdered = ttsVoiceActingTask.Result;
        }

        //-------------------------------------------------

        // await replicateAPI.GenerateAndSetTexturesForCharacter(defaultGuy, nameOfAiGeneratedCharacter);



        // textField.text = creatingScene + " --- " + "Generating FakeYou TTS...";
        // // generate text to speech voice acting based on dialog	
        // List<AudioClip> ttsVoiceActingOrdered = null;
        // if (isThisSceneUsingVoiceActing)
        // {
        //     ttsVoiceActingOrdered = await fakeYouAPIManager.GenerateTTS(textsToSpeak, voiceModelUUIDs, characterNames, textField, creatingScene);
        // }
        textField.text = creatingScene + " --- " + "Done :)";

        Debug.Log("done creating scene");

        AIArtStuff aiArtStuff = new AIArtStuff();
        if (useAiArt) aiArtStuff = await aiArtTask;

        RickAndMortyScene scene = new RickAndMortyScene(
            null,
            initialPrompt,
            promptAuthor,
            this.AIController.SystemMessage.text,
            chatGPTOutput,
            chatGPTOutputLines,
            ttsVoiceActingOrdered,
            aiArtStuff
        );


        scene.WriteToDir();
        return scene;
    }

    bool AreStringsEqual(string s1, string s2)
    {
        // Clean strings by removing special characters, spaces, and converting to lowercase
        s1 = Regex.Replace(s1, "[^a-zA-Z0-9]", "").ToLower();
        s2 = Regex.Replace(s2, "[^a-zA-Z0-9]", "").ToLower();

        return s1 == s2;
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

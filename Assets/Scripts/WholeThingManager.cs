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
using Newtonsoft.Json;

public class TopicVoteResults
{
    public string topic;
    public string author;
    public string backupTopic;
    public string backupAuthor;
}

public class CharacterVotesData
{
    public static string characterVotesFilePath = "character-votes.json";
    public CharacterVoteResults[] voteResults = new CharacterVoteResults[] { };

    public static CharacterVotesData ReadFromDisk()
    {
        if(!File.Exists(characterVotesFilePath))
        {
            throw new Exception("character-votes.json not found");
        }
        string data = File.ReadAllText(characterVotesFilePath);
        var x = JsonConvert.DeserializeObject<CharacterVotesData>(data);
        return x;
    }

    public void WriteToDisk()
    {
        string json = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(characterVotesFilePath, json);
    }
}

public class Lookup3dHeads
{
    public Dictionary<string, string> selectedGenerations;

    public CharacterVotesData votes;

    public AIHead3D GetForCharacter(string characterKey)
    {
        if (!selectedGenerations.ContainsKey(characterKey)) return null;

        AIHead3D aiHead = new AIHead3D();
        aiHead.characterKey = characterKey;
        aiHead.selectedGeneration = selectedGenerations[characterKey];
        aiHead.generationIds = new string[] { selectedGenerations[characterKey] };
        return aiHead;
    }

    public static Lookup3dHeads Load()
    {
        Lookup3dHeads x = new Lookup3dHeads();
        x.votes = CharacterVotesData.ReadFromDisk();
        Debug.Log(x.votes.voteResults);
        x.selectedGenerations = new Dictionary<string, string>();

        List<string> _3dScenes = new List<string>();

        foreach(var res in x.votes.voteResults)
        {
            x.selectedGenerations[res.characterKey] = res.selectedGeneration;
        }

        // List all of the scenes in saved-scenes/
        //foreach (string dir in PathUtils.GetSubDirs("saved-scenes/"))
        //{
        //    if (!File.Exists($"saved-scenes/{dir}/scene.json")) continue;

        //    // Load the scene.json
        //    string data = File.ReadAllText($"saved-scenes/{dir}/scene.json");
        //    var scene = JsonConvert.DeserializeObject<RickAndMortyScene>(data);

        //    if (scene.aiArt.character != null)
        //    {
        //        var character = scene.aiArt.character;
        //        if (character.head3d == null) continue;
        //        if (character.head3d.selectedGeneration.Length == 0) continue;
        //        x.selectedGenerations[character.characterName] = character.head3d.selectedGeneration;
        //    }
        //}

        return x;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WholeThingManager))]
public class DropdownExampleEditor : Editor
{
    private SerializedProperty runModeIndex;

    private void OnEnable()
    {
        runModeIndex = serializedObject.FindProperty("runModeIndex");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Dropdown for different run modes.
        EditorGUILayout.LabelField("Select an option:");
        runModeIndex.intValue = EditorGUILayout.Popup(runModeIndex.intValue, WholeThingManager.RUN_MODE_OPTIONS);
        EditorGUILayout.LabelField("Run mode: " + WholeThingManager.RUN_MODE_OPTIONS[runModeIndex.intValue]);

        serializedObject.ApplyModifiedProperties();

        DrawDefaultInspector();
    }
}
#endif




// this is the big daddy script that controls everything
// basically has a async function that continuously collects suggestions from chat, creates scenes and plays scenes. 
public class WholeThingManager : MonoBehaviour
{
    private static string RUN_MAIN_LOOP = "Run main loop";
    private static string GENERATE_CUSTOM_SCRIPT = "Generate custom script";
    private static string REPLAY_OLD_SCENE = "Replay old scene";
    private static string TEST_WORKFLOWS = "Test workflows";

    public static string[] RUN_MODE_OPTIONS = new string[] {
        RUN_MAIN_LOOP,
        GENERATE_CUSTOM_SCRIPT,
        REPLAY_OLD_SCENE,
        TEST_WORKFLOWS
    };

    [Range(0, 3)]
    public int runModeIndex = 0;

    public static WholeThingManager Singleton;
    public OpenAISlurDetector slurDetectorChatGPT;
    public SlurDetectorEvan slurDetectorPhonic;
    public AIController AIController;
    public OpenAICameraDirector openAICameraDirector;
    public SceneDirector sceneDirector;
    public FakeYouAPIManager fakeYouAPIManager;
    public YouTubeChatFromSteven youTubeChat;
    public ReplicateAPI replicateAPI;
    public CharacterVotingChamber characterVotingChamber;

    public bool usingVoiceActing = true;
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

    public GameObject uiCanvas;
    public GameObject topicVotingUI;
    public GameObject charVotingUI;
    public GameObject episodeStuffUI;

    private Lookup3dHeads lookup3dHeads;
    private bool forceAiCharacter = false;

    public void SetUI(GameObject ui)
    {
        // Hide all UI's.
        for (int i = 0; i < uiCanvas.transform.childCount; i++)
        {
            // Access each child GameObject using index
            GameObject o = uiCanvas.transform.GetChild(i).gameObject;

            if (o == ui) o.SetActive(true);
            else o.SetActive(false);
        }
    }

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


    void Awake()
    {
        Singleton = this;
        loadConfig();

        ToggleDiscordPlugEvery10Seconds();
        titleText.gameObject.SetActive(false);
        enableOrDisableVotingUI(false);

        AIController.Init();
        openAICameraDirector.Init();

        SetUI(null);

        lookup3dHeads = Lookup3dHeads.Load();

        Debug.Log("lookup 3d heads cache:");
        foreach (var kvp in lookup3dHeads.selectedGenerations)
        {
            Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
        }

        // Disbale some shit for performance
        characterVotingChamber.gameObject.SetActive(false);

        // TestingShit();
    }

    async void Start()
    {
        sceneDirector.ResetStuff();

        string runMode = RUN_MODE_OPTIONS[runModeIndex];

        bool generateCustomScript = runMode == GENERATE_CUSTOM_SCRIPT;
        bool testWorkflow = runMode == TEST_WORKFLOWS;
        bool replayOldScene = runMode == REPLAY_OLD_SCENE;
        bool runMainLoop = runMode == RUN_MAIN_LOOP;

        if (testWorkflow)
        {
            // ID10T

            //RickAndMortyScene scene = RickAndMortyScene.ReadFromDir("scene-samaltman");
            //scene.WriteToDir();
            //await RunScene(scene);
            //return;

            //var scene = RickAndMortyScene.ReadFromDir("scene-17d82f91-bafb-5a4b-4341-b39baedc1c01");
            //scene.WriteToDir();
            //await RunScene(scene);

            //var scene = await CreateScene("rick and morty talk about edward bernays propaganda theory", "liam", "", "", true);

            // NOTE(liamz): okay this is the ONE time I'm gonna use globals
            forceAiCharacter = true;
            //var scene = await CreateScene("rick and morty go see the dalai lama about peace", "me", "banana", "me", usingVoiceActing);
            var scene = RickAndMortyScene.ReadFromDir("scene-8c2cc0a2-f188-b5b6-1c93-0b0fdd4a6331");
            forceAiCharacter = false;

            //var scene = RickAndMortyScene.ReadFromDir("scene-dalai-lama");

            //DialogueInfo dialogInfoWithSwearing = sceneDirector.ProcessDialogFromLines(scene.chatGPTOutputLines);
            //scene.chatGPTOutputLines = dialogInfoWithSwearing.script;
            //Debug.Log($"dialogInfoWithSwearing \n{string.Join("\n", dialogInfoWithSwearing.script)}");
            //Debug.Log($"scene.chatGPTOutputLines \n{string.Join("\n", scene.chatGPTOutputLines)}");

            //scene.chatGPTOutputLines = dialogInfoWithSwearing.script;

            //scene.WriteToDir("scene-dalai-lama");

            await RunScene(scene);

            //await testWorkflow4();
            //await testWorkflow3();
            //await testWorkflow2();
            //await testWorkflow1();
        }
        else if (generateCustomScript)
        {
            enableOrDisableVotingUI(false);

            Debug.Log("generating one custom scene then playing it");
            var scene = await CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);

            Debug.Log("rendering newly generated scene");
            await RunScene(scene);

            return;
        }
        else if (replayOldScene)
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
        SetUI(topicVotingUI);

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

    private async Task<CharacterVoteResults> RunCharacterVote(string characterName, string[] generationIds, int timeToVoteMilliseconds)
    {
        characterVotingChamber.gameObject.SetActive(true);

        SetUI(charVotingUI);

        characterVotingChamber.stageCamera.enabled = true;

        characterVotingChamber.Setup(characterName, generationIds);
        CharacterVoteResults results = await characterVotingChamber.RunVote(youTubeChat, timeToVoteMilliseconds);

        characterVotingChamber.stageCamera.enabled = false;

        SetUI(null);

        characterVotingChamber.gameObject.SetActive(false);

        return results;
    }

    private async Task<TopicVoteResults> RunTopicVote(Task nextSceneTask, float timeToVoteSeconds)
    {
        TopicVoteResults voteResults = new TopicVoteResults();

        bool testingTopics = true;
        List<string> testTopics = new List<string> {
            "morty talks with sadam hussein\nme",
            "Rick and morty fight batman\nme",
            "Rick and morty go to Australia\nme"
        };
        int testTopicIndex = 0;



        // since we are generating a scene in the background while we play a scene, the generating scene needs to finish generating before we finish voting
        // and we also wait a minimum of 30 seconds
        if (useDanceAnimations) danceFloorManager.DanceCameraStart();


        // ok lets get the list of topics
        enableOrDisableVotingUI(true);
        List<string> randomTopics = youTubeChat.GetRandomTopics();

        if (randomTopics == null)
        {
            randomTopics = new List<string> {
                "morty talks with yoda\nme",
                "Rick and morty fight batman\nme",
                "Rick and morty go to Australia\nme"
            };
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


        // While we're still waiting on the next scene, or we still have time to vote:
        while (!nextSceneTask.IsCompleted || (voteTime < timeToVoteSeconds && waitForVoting))
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

        // Tally votes, get the chosen topic.
        int chosenTopic = 0;
        for (int j = 0; j < voteNumbers.Length; j++)
        {
            if (voteNumbers[j] > voteNumbers[chosenTopic]) chosenTopic = j;
        }

        if (testingTopics)
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

        enableOrDisableVotingUI(false);

        if (useDanceAnimations) danceFloorManager.DanceCameraStop();

        voteResults.topic = randomTopics[chosenTopic];
        voteResults.author = randomTopicAuthors[chosenTopic];
        voteResults.backupTopic = randomTopics[backupTopic];
        voteResults.backupAuthor = randomTopicAuthors[backupTopic];

        return voteResults;
    }


    // this is the big daddy
    private async Task MainLoop()
    {
        // The scene generating in the background.
        Task<RickAndMortyScene> nextSceneTask;

        // Start creating a scene while the first round of voting happens
        // change this line to enter your own prompt. vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv
        nextSceneTask = CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);

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

            // Scene has finished playinh.
            if (i > 0) Debug.Log("scene done");
            dialogBox.text = "";


            // Run a vote.
            TopicVoteResults voteResults = await RunTopicVote(nextSceneTask, 30f);
            // Add the chosen topic to the blacklist so it doesnt play again
            youTubeChat.AddToBlacklist(voteResults.topic);

            // Get the next scene.
            RickAndMortyScene currentScene = await nextSceneTask;

            // Run the current scene.
            // In the background, generate the next scene based on voting topic.
            if (justDoOneScene)
            {
                await RunScene(currentScene);
                return;
            }

            // In the background, create a scene.
            Debug.Log($"main loop #{i}: \ncurrent scene: {currentScene.chatGPTRawOutput}\n\nnext scene: {voteResults.topic}");
            nextSceneTask = CreateScene(
                voteResults.topic,
                voteResults.author,
                voteResults.backupTopic,
                voteResults.backupAuthor,
                usingVoiceActing
            );

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

        // If we have an AI character, check if we can reuse a 3D head generated for it.
        if(scene.aiArt?.character != null)
        {
            string characterName = scene.aiArt?.character.characterName;
            Debug.Log($"detected ai character: {characterName}");

            AIHead3D head = lookup3dHeads.GetForCharacter(characterName);
            if (head != null)
            {
                Debug.Log($"reusing AI head for character {characterName}");
                scene.aiArt.character.head3d = head;
            }
        }

        // 2d/3d character loader:
        if (scene.aiArt.character?.head3d != null) // Detect: 3D character
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
        else if (scene.aiArt.character != null) // Detect: 2D character
        {
            sceneDirector.use3DGuy = false;
            aiArtCharacter.Prepare(scene.aiArt.character);
        }


        // Detect: AI art dimension.
        if (scene.aiArt.dimension != null)
        {
            aiArtDimension.Prepare(scene.aiArt.dimension);
        }


        //
        // 2. Play scene.
        //
        SetUI(episodeStuffUI);

        // TODO: it'd be cool to play scenes without the censored stuff.
        // but this messes up the syncing of the voice tracks
        // leaving this here for now

        //string[] script = scene.chatGPTRawOutput
        //    .Split("\n")
        //    .Where((val) => val.Trim().Length > 0)
        //    .ToList().ToArray();

        //if(usePhonicSlurDetection || useChatgptSlurDetection)
        //{
        //    script = scene.chatGPTOutputLines;
        //}

        await sceneDirector.PlayScene(scene.chatGPTOutputLines, scene.ttsVoiceActingLines);

        SetUI(null);


        //
        // 3. Reset for next scene.
        //
        aiArtDimension.Reset();
        aiArtCharacter.Reset();

        // scene done
        currentlyRunningScene = false;
    }

    // GetWhosTalking (string line) -> string characterName, bool isGeneratedCharacter?
    // ParseStageDirections -> (string? generatedDimension)
    // GetCharacters - "[rick walks to morty]  returns chacters 1 rick, character 2 morty.

    // Takes a script and adds camera shots to it.
    public async Task<string[]> GenerateCameraShots(string script)
    {
        // Call ChatGPT with the script
        // It returns a response which is the script with camera directions added to it.
        // We have to process this script since it isn't perfect.
        string response = await openAICameraDirector.EnterPromptAndGetResponse(script);
        char[] delims = new[] { '\r', '\n' };
        string[] scriptWithCameraShotsUnprocessed = response.Split(delims, StringSplitOptions.RemoveEmptyEntries);

        // ok so now we have 2 scripts 1 with camera angles and 1 without, sometimes the one without removes lines and shit, so we cant
        // just use that we have to merge the two
        List<string> combinedList = script.Split(delims).ToList();

        int checkedIndexOnCombinedList = 0;
        //length -1 because we dont care if a camera instruciton is at the end of the list
        for (int i = 0; i < scriptWithCameraShotsUnprocessed.Length - 1; i++)
        {
            string lineWeChecking = scriptWithCameraShotsUnprocessed[i];
            //if this bitch is a camera command
            if (lineWeChecking.Contains("{") && !lineWeChecking.Contains(":"))
            {

                // then we get the instuction after this one and find it in the original array.
                string nextInstruction = scriptWithCameraShotsUnprocessed[i + 1];

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

        string[] scriptWithCameraShotsProcessed = combinedList.ToArray();

        Debug.Log("original: \n " + string.Join("\n", script));
        Debug.Log("Chatgpt camera angles: \n " + string.Join("\n", scriptWithCameraShotsUnprocessed));
        Debug.Log("Combined: \n " + string.Join("\n", scriptWithCameraShotsProcessed));

        return scriptWithCameraShotsProcessed;
    }

    // this is the main bitch of the program. a bunch of calling other scripts to get each element of the scene.
    // basically this turns an input prompt into a list of lines of dialog + stage directions, and a list of audio files for the tts.
    public async Task<RickAndMortyScene> CreateScene(
        string prompt,
        string promptAuthor,
        string backupPrompt,
        string backupPromptAuthor,
        bool isThisSceneUsingVoiceActing
    )
    {
        Debug.Log("CreateScene");

        string initialPrompt = "";

        // the raw chatGPT output
        string chatGPTOutput = "";
        // the processed lines of the script
        string[] chatGPTOutputLines = null;
        string creatingScene = "";
        bool foundGoodPrompt = false;

        generate:
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

        //if (generateCustomScript)
        //{
        //    chatGPTOutput = @"";
        //    promptAuthor = "LLaMa69";
        //
        //    // Trim any extraneous space from the string.
        //    chatGPTOutputLines = chatGPTOutput.Split("\n").Select(line => line.Trim()).ToArray();
        //}

        chatGPTOutputLines = Utils.AddSwearing(chatGPTOutputLines);

        Debug.Log("sceneDirector.ProcessDialogFromLines");
        // extract the dialog info from the output lines this includes the voiceModelUUIDs, the character names, and the text that they speak.	
        DialogueInfo dialogInfo = sceneDirector.ProcessDialogFromLines(chatGPTOutputLines);
        chatGPTOutputLines = dialogInfo.chatGPTOutputLines;

        Debug.Log("ai generated names for stuff");
        string nameOfAiGeneratedCharacter = dialogInfo.nameOfAiGeneratedCharacter;
        string nameOfAiGeneratedDimension = dialogInfo.nameOfAiGeneratedDimension;
        Debug.Log(nameOfAiGeneratedCharacter);
        Debug.Log(nameOfAiGeneratedDimension);

        if (forceAiCharacter)
        {
            if (nameOfAiGeneratedCharacter == null)
            {
                await Task.Delay(400);
                Debug.Log("didn't get AI character, looping");
                goto generate;
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






        List<Task> allConcurrentTasks = new List<Task>();

        // Start tasks in parallel

        //-------------------------------------------------

        // 1. AI art - start.
        Task<AIArtStuff> aiArtTask = null;
        if (useAiArt)
        {
            aiArtTask = replicateAPI.DoAllTheAiArtStuffForAScene(nameOfAiGeneratedCharacter, nameOfAiGeneratedDimension);
            allConcurrentTasks.Add(aiArtTask);
        }

        // 2. Camera shots - start.
        Task<string[]> CameraShotsChatGPTTask = null;
        if (usingChatGptCameraShots)
        {
            CameraShotsChatGPTTask = GenerateCameraShots(string.Join("\n", chatGPTOutputLines));
            allConcurrentTasks.Add(CameraShotsChatGPTTask);
        }

        textField.text = creatingScene + " --- " + "Generating FakeYou TTS...";
        Debug.Log(creatingScene + " --- " + "Generating FakeYou TTS...");

        // 3. Voice acting - start.
        Task<List<AudioClip>> ttsVoiceActingTask = null;
        if (isThisSceneUsingVoiceActing)
        {
            ttsVoiceActingTask = fakeYouAPIManager.GenerateTTS(
                dialogInfo.textsToSpeak,
                dialogInfo.voiceModelUUIDs,
                dialogInfo.characterNames,
                textField,
                creatingScene
            );
            allConcurrentTasks.Add(ttsVoiceActingTask);
        }


        // Await all tasks being done.
        Debug.Log("start await");
        if (allConcurrentTasks.Count > 0)
        {
            await Task.WhenAll(allConcurrentTasks);
        }


        Debug.Log("finish await");

        // 1. AI art (2d) - done.
        AIArtStuff aiArtStuff = new AIArtStuff();
        if (useAiArt)
        {
            aiArtStuff = await aiArtTask;
        }

        // 2. Voice acting - done.
        List<AudioClip> ttsVoiceActingOrdered = new List<AudioClip>();
        if (isThisSceneUsingVoiceActing)
        {
            ttsVoiceActingOrdered = ttsVoiceActingTask.Result;
        }

        // 3. Camera shots - done.
        if (usingChatGptCameraShots)
        {
            chatGPTOutputLines = await CameraShotsChatGPTTask;
        }

        //-------------------------------------------------

        textField.text = creatingScene + " --- " + "Done :)";

        Debug.Log("done creating scene");

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






    //
    // INTEGRATION TESTS OF LIKE DIFFERENT STUFF (TM)
    // "Sometimes, a programmer needs to test, and the best way to do that is generate
    // politically incorrect Rick and Morty AI TV episodes." - Barack Obama
    //
    //
    //

    // Test 1: Run a topic vote, run a character vote, sub the 3D character into a scene read from disk.
    // This tests:
    // - transitioning between the voting UI's
    // - the functionality of the character voting interface
    private async Task testWorkflow1()
    {
        // Run test main loop:

        // PRODUCT 0:

        // Show voting scene w/ custom timeout.
        Task mockNextSceneTask = Task.Delay(4000);
        await RunTopicVote(mockNextSceneTask, 2f);
        Debug.Log("topic vote done");

        // Show character voting scene w/ custom timeout.
        CharacterVoteResults voteRes = await RunCharacterVote(
            "sadam hussein",
            //new string[] { "fhnfrslb6zrrb7mr55pri5rvwi", "lhinfidbdsy3ay32qnedgwgskq", "nuedrilbhxjvyo5bhn7ndycjbu", "pejzlqlb2aviwzqvktw47nrntq" },
            AIHeadRigger.GetGenerationsForCharacter("sadam hussein"),
            5000
        );
        Debug.Log("char vote done");


        var scene = RickAndMortyScene.ReadFromDir("scene-sadam-hussein");
        scene.aiArt.character.head3d.selectedGeneration = voteRes.selectedGeneration;

        await RunScene(scene);
    }

    // Test 2: create a scene with an AI character in it, generate the 3D AI character, run the character vote, and then play the scene
    // This tests:
    // - detecting when to generate 3d char
    // - the integration with the 3d gen pipeline (also when it fails)
    // - the progress of the generation / async performance
    // - running a character vote on the newly downloaded characters
    private async Task testWorkflow2()
    {
        // 1. Generate scene.
        //var scene = await CreateScene("rick talks to the dalai lama about testiucular torsion", "me", "banana", "me", usingVoiceActing);
        //Debug.Log($"saved scene {scene.id}");
        //scene.WriteToDir();
        //var scene = RickAndMortyScene.ReadFromDir("scene-trump-3d");
        var scene = RickAndMortyScene.ReadFromDir("scene-dccfd02c-4634-c038-0de1-f09687854ebd");

        if (scene.aiArt.character == null) throw new Exception("No AI art character wtf???");

        // 2. Generate 3d head character.
        Debug.Log("Generating 3d character");
        Task<AIHead3D> aiHead3dTask = LiamzHeadGenAPI.GenerateAIHead3D(scene.aiArt.character.characterName);

        int expectedTimeSecs = 60 + 60 + 60 + 60 + 30; // 4m30s
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!aiHead3dTask.IsCompleted)
        {
            double progress = stopwatch.Elapsed.TotalSeconds / expectedTimeSecs;
            progress = Math.Round(progress * 100, 2);
            Debug.Log($"Generating 3d character ({stopwatch.Elapsed.TotalSeconds}s of {expectedTimeSecs})");
            textField.text = $"Generating AI character ({progress}%)...";

            await Task.Delay(1000);
        }

        AIHead3D aiHead3d = await aiHead3dTask;

        // 3. Run vote on character.
        scene.aiArt.character.head3d = aiHead3d;
        var voteResults = await RunCharacterVote(
            aiHead3d.characterKey,
            aiHead3d.generationIds,
            10000
        );

        // 4. Render scene with selected character.
        scene.aiArt.character.head3d = aiHead3d;
        scene.aiArt.character.head3d.selectedGeneration = voteResults.selectedGeneration;
        scene.WriteToDir(); // write with new 3d head data.

        await RunScene(scene);
    }


    private async Task<RickAndMortyScene> mockCreateScene(RickAndMortyScene s, int delay)
    {
        await Task.Delay(delay);
        return s;
    }

    // Test 3: run the main loop with 3d character scenes generating in the background.
    // This tests:
    // - running 3d scene gen in the background while also playing a current scene
    // - "lazy loading" a character scene into the mix
    private async Task testWorkflow3()
    {
        // PRODUCT 3:
        // 1. Run mock main loop.
        // 2. Generate script.
        // 3. Detect if script has AI character.
        // 4. aiTask = generate3dAiScene()
        // 5. Meanwhile, we run the classic voting loop until it's ready.
        // 6. When it is ready, we run the vote.
        // 7. And then finally, play the scene.

        // The scene generating in the background.
        //Task<RickAndMortyScene> nextSceneTask = mockCreateScene(RickAndMortyScene.ReadFromDir("scene-sadam-hussein"), 30000);
        Task<RickAndMortyScene> nextSceneTask = CreateScene("rick and morty talk to a random famous person", "me", "random", "me", usingVoiceActing);

        // Ok now in the background:
        // Sometimes we run a vote and then fork it into 2d and 3d
        // when we get the scene back, if there is an ai character, we generate one.
        // while we are waiting on this, continue the main loop
        // as soon as this is ready, then we run the voting thing, and then we run the actual character fooblah.
        // that seems like it would work.

        for (int i = 0; i < 1000; i++)
        {
            Debug.Log($"workflow3 iteration i={i}");

            // Run a vote.
            TopicVoteResults voteResults = await RunTopicVote(nextSceneTask, 30f);

            // Add the chosen topic to the blacklist so it doesnt play again
            youTubeChat.AddToBlacklist(voteResults.topic);

            // Get the next scene.
            RickAndMortyScene currentScene = await nextSceneTask;

            // Check if we can make a 3d scene from it.
            // If we can, generate a 3d scene in background, and play reruns instead.
            if (currentScene.aiArt?.character != null)
            {
                // Background: Generate 3d scene.
                Task<PlayItem3DCharacterScene> item3dSceneTask = MakeScene3d(currentScene);

                // Foreground: play reruns.
                List<RickAndMortyScene> reruns = new List<RickAndMortyScene>();
                //string[] scenesToRerun = PathUtils.GetSubDirs("saved-scenes/");

                reruns.AddRange(new RickAndMortyScene[] {
                    //RickAndMortyScene.ReadFromDir("scene-sadam-hussein"),
                    //RickAndMortyScene.ReadFromDir("scene-trump-3d"),
                    RickAndMortyScene.ReadFromDir("scene-samaltman"),
                    //RickAndMortyScene.ReadFromDir("scene-da00c9d5-7789-517d-4116-a3a280308daf")
                });

                int j = 0;
                while (!item3dSceneTask.IsCompleted)
                {
                    Debug.Log($"workflow3 iteration j={j}");
                    int rerunIdx = j % reruns.Count;
                    Debug.Log($"playing rerun #{j}, iteration {rerunIdx}");
                    await RunScene(reruns[rerunIdx]);
                    j++;
                }


                // While we are generating the 3d scene, which can take a while (7mins)
                // we run the normal loop:
                // - play scene 0 (rerun)
                // - vote on new topic for scene 1
                // - create scene 1
                // - play scene 1
                // - create scene 2 based on topic vote
                // - play scene 2
                //await RunScene(reruns[0]);
                //Task<RickAndMortyScene> nextSceneTask2 = CreateScene(firstPrompt, "me", "banana", "me", usingVoiceActing);
                //while (!item3dSceneTask.IsCompleted)
                //{
                //    var topicVoteResults2 = await RunTopicVote(nextSceneTask2, 30f);

                //    // Add the chosen topic to the blacklist so it doesnt play again
                //    youTubeChat.AddToBlacklist(voteResults.topic);

                //    // Get the next scene.
                //    RickAndMortyScene scene = await nextSceneTask2;

                //    // Background: generate next scene.
                //    nextSceneTask2 = CreateScene(
                //        topicVoteResults2.topic,
                //        topicVoteResults2.author,
                //        topicVoteResults2.backupTopic,
                //        topicVoteResults2.backupAuthor,
                //        usingVoiceActing
                //    );

                //    // Foreground: play scene.
                //    await RunScene(scene);
                //}


                // Once the 3d scene task is completed, we check if it was successful, and if it was,
                // we play it.
                if (item3dSceneTask.IsCompletedSuccessfully)
                {
                    // Background: create next 2d scene.
                    nextSceneTask = CreateScene(
                        voteResults.topic,
                        voteResults.author,
                        voteResults.backupTopic,
                        voteResults.backupAuthor,
                        usingVoiceActing
                    );

                    // 
                    // Forground:
                    //

                    PlayItem3DCharacterScene item = await item3dSceneTask;
                    RickAndMortyScene scene = item.scene;
                    AIHead3D head3d = item.head3d;

                    // Run vote on character.
                    scene.aiArt.character.head3d = head3d;
                    var characterVoteResults = await RunCharacterVote(
                        head3d.characterKey,
                        head3d.generationIds,
                        10000
                    );

                    // Save winning character.
                    //save3dHeadAssetsToScene(scene, item.head3d, characterVoteResults);
                    scene.aiArt.character.head3d = item.head3d;
                    scene.aiArt.character.head3d.selectedGeneration = characterVoteResults.selectedGeneration;
                    scene.WriteToDir(); // write with new 3d head data.

                    // Play 3d scene.
                    await RunScene(scene);
                } else
                {
                    // In the background, create a scene.
                    //Debug.Log($"main loop #{i}: \ncurrent scene: {currentScene.chatGPTRawOutput}\n\nnext scene: {voteResults.topic}");
                    nextSceneTask = CreateScene(
                        voteResults.topic,
                        voteResults.author,
                        voteResults.backupTopic,
                        voteResults.backupAuthor,
                        usingVoiceActing
                    );

                    // Just play the original 2d scene.
                    await RunScene(currentScene);
                    continue;
                }
            } else
            {
                // In the background, create a scene.
                //Debug.Log($"main loop #{i}: \ncurrent scene: {currentScene.chatGPTRawOutput}\n\nnext scene: {voteResults.topic}");
                nextSceneTask = CreateScene(
                    voteResults.topic,
                    voteResults.author,
                    voteResults.backupTopic,
                    voteResults.backupAuthor,
                    usingVoiceActing
                );

                // Just play the original 2d scene.
                await RunScene(currentScene);
            }
        }
    }

    
    async Task<PlayItem3DCharacterScene> MakeScene3d(RickAndMortyScene scene)
    {
        AIHead3D head3d = await Make3dCharacter(scene);
        PlayItem3DCharacterScene item3dScene = new PlayItem3DCharacterScene();
        item3dScene.head3d = head3d;
        item3dScene.scene = scene;
        return item3dScene;
    }

    // Make a 3d character version of a rickandmorty scene with a 2d ai character.
    async Task<AIHead3D> Make3dCharacter(RickAndMortyScene scene)
    {
        if(scene.aiArt == null || scene.aiArt.character == null)
        {
            throw new Exception("can't make 3d character, RickAndMortyScene doesn't have any AI character");
        }

        Task<AIHead3D> characterGenTask = LiamzHeadGenAPI.GenerateAIHead3D(scene.aiArt.character.characterName);

        int expectedTimeSecs = 60 * 7; // 7m
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (!characterGenTask.IsCompleted)
        {
            double progress = stopwatch.Elapsed.TotalSeconds / expectedTimeSecs;
            progress = Math.Round(progress * 100, 2);
            Debug.Log($"Generating 3d character ({stopwatch.Elapsed.TotalSeconds}s of {expectedTimeSecs})");
            textField.text = $"Generating AI character ({progress}%)...";

            await Task.Delay(1000);
        }

        return await characterGenTask;
    }

    private async Task testWorkflow4()
    {
        List<string> _3dScenes = new List<string>();

        // List all of the scenes in saved-scenes/
        foreach(string dir in PathUtils.GetSubDirs("saved-scenes/"))
        {
            // Load the scene.json
            string data = File.ReadAllText($"saved-scenes/{dir}/scene.json");
            var scene = JsonConvert.DeserializeObject<RickAndMortyScene>(data);

            if(scene.aiArt.character != null)
            {
                if(scene.aiArt.character.head3d != null)
                {
                    _3dScenes.Add(dir);
                }
            }
        }

        Debug.Log($"found {_3dScenes.Count} 3d scenes to play");

        //int i = 0;
        //foreach (string sceneId in _3dScenes)
        //{
        //    Debug.Log($"regen scene {i}");
        //    i++;

        //    RickAndMortyScene scene = RickAndMortyScene.ReadFromDir(sceneId);
        //    Debug.Log("adding camera shots to scene");
        //    string[] scriptWithCameraShots = await GenerateCameraShots(scene.chatGPTRawOutput);
        //    Debug.Log("script with \n\n" + string.Join("\n", scriptWithCameraShots));
        //    scene.chatGPTOutputLines = scriptWithCameraShots;
        //    scene.WriteToDir();
        //}

        int i = 0;
        foreach (string sceneId in _3dScenes)
        {
            Debug.Log($"play regen scene {i}");
            i++;

            //RickAndMortyScene scene = RickAndMortyScene.ReadFromDir("scene-6ddab2bc-2fb9-1ed8-3140-dfc7dc323f19");
            RickAndMortyScene scene = RickAndMortyScene.ReadFromDir(sceneId);

            if (i == 2)
            {
                await RunScene(scene);
            }
        }
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


class PlayItem { }

class PlayItem3DCharacterScene : PlayItem
{
    public AIHead3D head3d;
    public RickAndMortyScene scene;
}

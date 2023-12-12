using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Cinemachine;
using DialogueAI;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Net;
//using OpenCover.Framework.Model;


// this script was pretty much copied from porkais ai sponge unity project.
// https://www.youtube.com/channel/UCVe358khfRWQ6NRT89jEhBA

// this is the part of the whole project i hate the most, its slow and i dont know wtf most of this shit does.
// basically it just takes a list of dialog then sends it to FakeYou api and then downloads the audio files
// to get this to work you need to set your fake you user name and password as EnvironmentVariables.




#pragma warning disable CS4014
public class FakeYouAPIManager : MonoBehaviour
{

    private string fakeYouUsernameOrEmail;
    private string fakeYouPassword;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private List<AudioClip> _clips;
    private HttpClient _client = new();


    public VideoClip clipToPlay;
    public TextAsset proxyTextFile;

    public SceneDirector sceneDirector;
    public bool usingProxies = true;


    void Start()
    {
        // get username and password
        fakeYouUsernameOrEmail = WholeThingManager.Singleton.config.FAKE_YOU_USERNAME_OR_EMAIL;
        fakeYouPassword = WholeThingManager.Singleton.config.FAKE_YOU_PASSWORD;

        // get a list of proxy servers. 
        if (usingProxies && proxyTextFile != null)
        {
            proxyArray = proxyTextFile.text.Split('\n');
        }
        Init();

    }

    // all this shit i have no fucking idea what its doing, the cookie shit, no idea. the configure http client, no idea. 
    // shit is just copied and pasted from porkai.
    async void Init()
    {
        string cookie = LoadCookie();

        if (cookie == "")
        {
            cookie = await FetchAndStoreCookie();
        }

        ConfigureHttpClient(cookie);

        await CheckCookieValidity(_client);

    }
    private int _proxyIndex = 0;
    [SerializeField] private string[] proxyArray;
    private string LoadCookie()
    {
        string cookieFilePath = $"{Environment.CurrentDirectory}\\Assets\\Scripts\\Keys\\keyForTheFakeYouCookiesOrSomething.txt";
        if (!System.IO.File.Exists(cookieFilePath))
            System.IO.File.WriteAllText(cookieFilePath, "");

        return System.IO.File.ReadAllText(cookieFilePath);
    }

    private async Task<string> FetchAndStoreCookie()
    {
        var loginDetails = new
        {
            username_or_email = fakeYouUsernameOrEmail,
            password = fakeYouPassword
        };

        var response = await _client.PostAsync("https://api.fakeyou.com/login",
            new StringContent(JsonConvert.SerializeObject(loginDetails), Encoding.UTF8, "application/json"));

        var cookieData = JsonConvert.SerializeObject(response.Headers.GetValues("set-cookie").First());
        var cookieParts = cookieData.Split(';');
        string cookie = cookieParts[0].Replace("session=", "").Replace("\"", "");

        System.IO.File.WriteAllText($"{Environment.CurrentDirectory}\\Assets\\Scripts\\Keys\\keyForTheFakeYouCookiesOrSomething.txt", cookie);

        return cookie;
    }

    private void ConfigureHttpClient(string cookie)
    {
        var handler = new HttpClientHandler();
        handler.CookieContainer = new CookieContainer();
        handler.CookieContainer.Add(new Uri("https://api.fakeyou.com"), new Cookie("session", cookie));
        if (proxyArray.Length > 0)
        {
            // Set proxy for HttpClientHandler only if proxies are available
            string[] proxyParts = proxyArray[_proxyIndex].Split(':');
            var proxy = new WebProxy(proxyParts[0] + ":" + proxyParts[1]);
            proxy.Credentials = new NetworkCredential(proxyParts[2], proxyParts[3]);
            handler.UseProxy = true;
            handler.Proxy = proxy;
            // if (proxyParts.Length >= 4)
            // {
            //     usingProxies = true;
            // }
        }

        _client = new HttpClient(handler);
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
        _fakeYouClient = new HttpClient(handler);  // Create _fakeYouClient with the handler
        _fakeYouClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }
    private async Task CheckCookieValidity(HttpClient client)
    {
        var checkKey = await client.GetAsync("https://api.fakeyou.com/v1/billing/active_subscriptions");
        var checkString = await checkKey.Content.ReadAsStringAsync();
        Debug.Log(checkString);
    }



    private HttpClient _fakeYouClient; // This client will be used for FakeYou API calls

    private Dictionary<string, GameObject> characters = new Dictionary<string, GameObject>();

    // ok im back i know whats going on now. 
    // this is where we chuck the audio clips once they are downloaded
    public List<AudioClip> generatedAudioClips = new List<AudioClip>();
    public List<int> failedAudioClips = new List<int>();
    public AudioClip defaultSound;

    public TMP_Text statisText;
    public string statisStatingText = "";
    int numberOfClipsCompleted = 0;
    // ok this is the main shit. it converts a list of lines to say to audio files. also the characterNames list is nothing, it has no reason to be here, i just cant be fucked changing it.
    public async Task<List<AudioClip>> GenerateTTS(List<string> _linesToSay, List<string> _characterUuids, List<string> _characterNames, TMP_Text updateText, string updateTextStart)
    {
        statisText = updateText;
        statisStatingText = updateTextStart;
        numberOfClipsCompleted = 0;
        // reset the audioclips list and populate it with default sounds
        generatedAudioClips = new List<AudioClip>();
        for (int i = 0; i < _linesToSay.Count; i++)
        {
            bool foundCharacter = false;
            // add the default sound based on the character that is speaking.
            foreach (CharacterInfo character in sceneDirector.characterList)
            {
                if (character.name == _characterNames[i] && character.defaultSound != null)
                {
                    generatedAudioClips.Add(character.defaultSound);
                    foundCharacter = true;
                    break;
                }
            }

            if (!foundCharacter)
            {

                generatedAudioClips.Add(defaultSound);
            }
        }


        // holds a bunch of info about the dialogs, i think
        List<Dialogue> dialogues = new List<Dialogue>();

        string[] characterUUIDS = _characterUuids.ToArray();
        string[] textsToSay = _linesToSay.ToArray();
        string[] characterStrings = _characterNames.ToArray();


        try
        {
            // create the tts requests and wait until all the requests are confirmed.
            Debug.Log("creating tts request tasks");
            List<Task> ttsTasks = CreateTTSRequestTasks(dialogues, characterUUIDS, textsToSay, characterStrings);
            Debug.Log($"creating tts request tasks: num={ttsTasks.Count}");
            await Task.WhenAll(ttsTasks);
            Debug.Log($"done tts reqs");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error while creating TTS requests: {ex.Message}");
            // Handle or log the exception as needed
        }


        Debug.Log(99999);
        // i was planning on downloading all the shit at once but that broke everything for some reason
        // so thats why i commented it out
        List<Task> downloadTasks = new List<Task>();
        if (updateText != null)
        {
            updateText.text = updateTextStart + " --- " + "Generating FakeYou TTS " + 0 + "/" + _linesToSay.Count;
        }
        Debug.Log(99999);

        // we go through each dialog and wait until its downloaded.
        // for (int i = 0; i < dialogues.Count; i++)
        // {

        //     if (!dialogues[i].failed)
        //     {
        //         // downloadTasks.Add(DownloadDialogFromFakeYou(dialogues[i], 0));
        //         await DownloadDialogFromFakeYou(dialogues[i], 0);
        //     }

        // }

        Debug.Log($"begin tts downloads");

        for (int i = 0; i < dialogues.Count; i++)
        {
            try
            {
                if (!dialogues[i].failed)
                {
                    await DownloadDialogFromFakeYou(dialogues[i], 0);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while downloading dialog for index {i}: {ex.Message}\n{ex.StackTrace}");
                try
                {
                    if (!dialogues[i].failed)
                    {
                        await DownloadDialogFromFakeYou(dialogues[i], 0);
                    }
                }
                catch (Exception ex2)
                {
                    Debug.LogError($"Error while downloading dialog for index {i}: {ex2.Message} \n{ex2.StackTrace}");
                    // Handle or log the exception as needed
                }
                // Handle or log the exception as needed
            }
        }
        // await Task.WhenAll(downloadTasks);

        Debug.Log(99999);

        return generatedAudioClips;
    }



    void UpdateStatisText()
    {
        if (statisText != null)
        {
            statisText.text = statisStatingText + " --- " + "Generating FakeYou TTS " + numberOfClipsCompleted + "/" + generatedAudioClips.Count;
        }


    }

    // fake you breaks when you send it less than 3 character, so we just repeat the last character if its under 3
    string RepeatLastNonSpaceCharacter(string input)
    {
        int nonSpaceCount = 0;
        char lastNonSpaceChar = ' ';

        // Count non-space characters and remember the last non-space character
        foreach (char c in input)
        {
            if (c != ' ')
            {
                nonSpaceCount++;
                lastNonSpaceChar = c;
            }
        }

        // If fewer than 3 non-space characters, repeat the last non-space character
        if (nonSpaceCount < 3 && lastNonSpaceChar != ' ')
        {
            int charactersToAdd = 3 - nonSpaceCount;
            for (int i = 0; i < charactersToAdd; i++)
            {
                input += new string(lastNonSpaceChar, 1);
            }
        }
        Debug.Log(input);
        return input;
    }

    //creates each tts request based on input lists
    private List<Task> CreateTTSRequestTasks(List<Dialogue> dialogues, string[] characterUUUIDs, string[] textsToSay, string[] characterStrings)
    {
        List<Task> ttsTasks = new List<Task>();

        for (int i = 0; i < textsToSay.Length; i++)
        {
            // something breaks if there is only 2 characters so thats what the repeat last non space chaaracter shit sis about.
            ttsTasks.Add(CreateTTSRequest(RepeatLastNonSpaceCharacter(textsToSay[i]), characterUUUIDs[i], dialogues, characterStrings[i], i));
        }

        return ttsTasks;
    }

    // sents a message to fakeyou api saying what to say and what character to say it.
    private async Task CreateTTSRequest(string textToSay, string voicemodelUuid, List<Dialogue> dialogues, string character, int clipNumber_)
    {
        var jsonObj = new
        {
            inference_text = textToSay,
            tts_model_token = voicemodelUuid,
            uuid_idempotency_token = Guid.NewGuid().ToString()
        };
        var content = new StringContent(JsonConvert.SerializeObject(jsonObj), Encoding.UTF8, "application/json");

        // so sometimes a tts request doesnt work and so we need to retry it a bunch. 
        int maxAttempts = 3;  // Maximum number of attempts
        int retryCount = 0;  // Current retry count

        while (retryCount < maxAttempts)
        {
            try
            {
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                if (proxyArray.Length > 0)
                {
                    _proxyIndex = (_proxyIndex + 1) % proxyArray.Length;
                    string[] proxyParts = proxyArray[_proxyIndex].Split(':');
                    var proxy = new WebProxy(proxyParts[0] + ":" + proxyParts[1]);
                    proxy.Credentials = new NetworkCredential(proxyParts[2], proxyParts[3]);
                    httpClientHandler.UseProxy = true;
                    httpClientHandler.Proxy = proxy;
                }

                CookieContainer cookieContainer = new CookieContainer();
                string cookieFilePath = $"{Environment.CurrentDirectory}\\Assets\\Scripts\\Keys\\keyForTheFakeYouCookiesOrSomething.txt";
                string cookieData = System.IO.File.Exists(cookieFilePath) ? System.IO.File.ReadAllText(cookieFilePath) : "";
                cookieContainer.Add(new Uri("https://api.fakeyou.com"), new Cookie("session", cookieData));
                httpClientHandler.CookieContainer = cookieContainer;

                HttpClient fakeYouClient = proxyArray.Length > 0 ? new HttpClient(httpClientHandler) : _client;
                fakeYouClient.DefaultRequestHeaders.Add("Accept", "application/json");

                // only wait for a max of 5 second for the fake you client request
                Task timeout = null;
                if (usingProxies)
                {
                    timeout = Task.Delay(TimeSpan.FromSeconds(5));
                }
                else
                {
                    timeout = Task.Delay(TimeSpan.FromSeconds(30));
                }
                // Make the request
                Task<HttpResponseMessage> requestTask = fakeYouClient.PostAsync("https://api.fakeyou.com/tts/inference", content);

                Task completedTask = await Task.WhenAny(requestTask, timeout);

                if (completedTask == timeout)
                {
                    // Task took longer that  1 second
                    Debug.Log("timed out on: " + textToSay);
                    retryCount++;
                    continue;

                }

                var response2 = await requestTask;  // retrieve the result
                var responseString = await response2.Content.ReadAsStringAsync();
                SpeakResponse speakResponse = JsonConvert.DeserializeObject<SpeakResponse>(responseString);

                if (!speakResponse.success)
                {
                    Debug.Log("ahh: " + textToSay);
                    continue;
                }

                dialogues.Add(new Dialogue
                {
                    uuid = speakResponse.inference_job_token,
                    text = textToSay,
                    character = character,
                    clipNumber = clipNumber_,
                    failed = false
                });
                Debug.Log(responseString);
                break;  // Successful, so break out of the loop
            }
            catch (System.Net.Sockets.SocketException e)
            {
                Debug.Log($"SocketException caught: {e.Message}. Retrying...");
                retryCount++;
            }
            catch (Exception e)  // Catch other exceptions that might occur
            {
                // this is normally where the exception is caught
                Debug.Log($"An error occurred: {e.Message}");
                Debug.Log("text to say: " + textToSay);
                retryCount++;
                // retry that bitch i guess. 

            }
        }

        if (retryCount >= maxAttempts)
        {
            failedAudioClips.Add(clipNumber_);
            dialogues.Add(new Dialogue
            {
                uuid = null,
                text = textToSay,
                character = character,
                clipNumber = clipNumber_,
                failed = true
            });

            Debug.Log("we fucked up");
        }
    }


    private async Task DownloadDialogFromFakeYou(Dialogue d, int callNumber)
    {
        Debug.Log("Getting stuff from fake you, Attempt " + callNumber + ": " + d.text);
        var content = await _client.GetAsync($"https://api.fakeyou.com/tts/job/{d.uuid}");
        Debug.Log("Attempt " + callNumber + ": " + d.text);
        var responseContent = await content.Content.ReadAsStringAsync();
        var v = JsonConvert.DeserializeObject<GetResponse>(responseContent);
        Debug.Log("Attempt " + callNumber + ": " + responseContent);
        // Debug.Log(responseContent);

        if (v.state == null || v.state.status == "pending" || v.state.status == "started" || v.state.status == "attempt_failed")
        {
            // if this hasent changed for like 50 attempts then we pull the plug. 
            if (callNumber > 30)
            {
                // recurse by creating a new request
                if (callNumber < 60)
                {
                    string newUuid = null;
                    await CreateNewVoiceRequest(d, result => { newUuid = result; });

                    if (!string.IsNullOrEmpty(newUuid))
                    {
                        d.uuid = newUuid;
                        await DownloadDialogFromFakeYou(d, 30);  // Recursive call
                    }
                    else
                    {
                        Debug.LogError("Failed to create new voice request");
                    }
                }
                
                Debug.LogError("timed out on call so just use a burp here");
                return;
            }
            await Task.Delay(100);  // Wait for 100 ms
            await DownloadDialogFromFakeYou(d, callNumber + 1);  // Recursive call
        }
        else if (v.state.status == "complete_success")
        {
            await ActuallyDownloadTheTTS(d, v);
        }
        else
        {
            string newUuid = null;
            await CreateNewVoiceRequest(d, result => { newUuid = result; });

            if (!string.IsNullOrEmpty(newUuid))
            {
                d.uuid = newUuid;
                await DownloadDialogFromFakeYou(d, 0);  // Recursive call
            }
            else
            {
                Debug.LogError("Failed to create new voice request");
            }
        }
    }

    private async Task CreateNewVoiceRequest(Dialogue d, Action<string> callback)
    {
        var jsonObj = new
        {
            tts_model_token = d.model,
            uuid_idempotency_token = Guid.NewGuid().ToString(),
            inference_text = d.text,
        };

        var content = new StringContent(JsonConvert.SerializeObject(jsonObj), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://api.fakeyou.com/tts/inference", content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            var speakResponse = JsonConvert.DeserializeObject<SpeakResponse>(responseString);

            callback(speakResponse.inference_job_token);
        }
        else
        {
            Debug.LogError("Error in FakeYou API request: " + response.StatusCode);
            callback(null);
        }
    }

    private async Task ActuallyDownloadTheTTS(Dialogue d, GetResponse v)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip($"https://storage.googleapis.com/vocodes-public{v.state.maybe_public_bucket_wav_audio_path}", AudioType.WAV);

        UnityWebRequestAsyncOperation op = uwr.SendWebRequest();
        op.completed += (AsyncOperation operation) =>
        {
            if (uwr.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(uwr.error);
                tcs.SetResult(false);
            }
            else
            {

                generatedAudioClips[d.clipNumber] = DownloadHandlerAudioClip.GetContent(uwr);
                numberOfClipsCompleted += 1;
                UpdateStatisText();
                tcs.SetResult(true);
            }

            // Make sure to dispose of the UnityWebRequest.
            uwr.Dispose();
        };

        // Wait until UnityWebRequest is done.
        await tcs.Task;
    }

}
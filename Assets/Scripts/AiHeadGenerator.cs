using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System;
using TMPro;
using System.Text.RegularExpressions;

public class AIHeadGenerator : MonoBehaviour
{
    // The generate button.
    public Button generateButton;

    // The object which contains the character name TextMesh.
    public GameObject characterNameText;

    // The 4 character models which we will rig with AI-generated heads.
    public List<GameObject> choices;


    private string GetCharacterName()
    {
        //foreach (var x in characterNameText.GetComponents<Component>())
        //{
        //    Debug.Log($"{x.GetType()} {x.name}");
        //}
        //throw new Exception("fio");
        //return "";

        var textmesh = characterNameText.GetComponent<TMP_InputField>();
        string characterName = textmesh.text;
        return characterName;
    }

    private async void Generate()
    {
        Debug.Log("generating 3d character models");

        // Call head generation API backend
        string characterName = GetCharacterName();
        string characterKey = Regex.Replace(characterName, "[^a-zA-Z ]", ""); // escape for backend, since this is a folder name
        var res = await GenerateHead(characterName);
        //var res = await GenerateHead_mock(characterKey);

        Debug.Log($"have {this.choices.Count} characters to rig, and we generated {res.generation_ids.Length} heads");

        // Once we get the generation ID's, load them into the models.
        for (int i = 0; i < Math.Min(res.generation_ids.Length, this.choices.Count); i++)
        {
            // TODO(liamz): this could be improved
            // we return only the generation_id from the API, but the full folder contains {id}_mesh. 
            var generationId = res.generation_ids[i] + "_mesh";
            var character = this.choices[i];

            Debug.Log($"rigging object {character.name} with generation {generationId} of character {characterKey}");

            var headRiggerComponent = character.GetComponent<AIHeadRigger>();
            if (headRiggerComponent == null)
            {
                throw new Exception("Character does not have AIHeadRigger behaviour");
            }

            headRiggerComponent.RenderGeneration(characterKey, generationId);
        }
    }


    public async Task<CharacterHeadsGenerateResponse> GenerateHead(string character)
    {
        using HttpClient client = new HttpClient();
        string apiUrl = "http://127.0.0.1:10001/v1/character-heads/generate";
        var args = new
        {
            character = character
        };
        client.Timeout = TimeSpan.FromSeconds(60 * 8); // 5min timeout

        HttpResponseMessage res = await client.PostAsync(
            apiUrl,
            new StringContent(
                JsonConvert.SerializeObject(args),
                Encoding.UTF8,
                "application/json"
            )
        );

        var body = await res.Content.ReadAsStringAsync();
        var retval = JsonConvert.DeserializeObject<CharacterHeadsGenerateResponse>(body);

        return retval;
    }

    public async Task<CharacterHeadsGenerateResponse> GenerateHead_mock(string character)
    {
        // wait 2s
        System.Threading.Thread.Sleep(2000);
        CharacterHeadsGenerateResponse res = new CharacterHeadsGenerateResponse
        {
            generation_ids = new string[] { "6jpkjglb773m5qy5jne2h6e72y" },
            error = ""
        };
        return res;

    }

    private void RunModel()
    {
        // run:
        // pipenv shell
        // python generate.py --char-name "character name"
        // wait for exit, check error code is 0 (does this work on windows)
        // errorlevel on windows - https://stackoverflow.com/questions/334879/how-do-i-get-the-application-exit-code-from-a-windows-command-line/11476681#11476681
        // if not success: throw exception
        // access the 3 character models, and instruct the AIHeadshot



        // Create a new process to run the Python script.
        //Process process = new Process();

        //// Configure the process start info.
        ////string executableDir = "";
        //string fullPath = Path.Combine(Environment.CurrentDirectory, "/local-image-gen/headshot/");
        //ProcessStartInfo startInfo = new ProcessStartInfo();
        ////ProcessStartInfo startInfo = new ProcessStartInfo(pythonPath);
        //startInfo.FileName = "python";
        //startInfo.UseShellExecute = true;
        ////startInfo.Arguments = "test-unity.py";
        //startInfo.Arguments = "-c \"print(1)\"";
        ////startInfo.WorkingDirectory = "local-image-gen/headshot/";
        //startInfo.WorkingDirectory = fullPath;
        //startInfo.RedirectStandardOutput = true;
        //startInfo.RedirectStandardError = true;
        //startInfo.UseShellExecute = false;
        //startInfo.CreateNoWindow = true;

        //process.StartInfo = startInfo;

        //// Redirect output and error streams.
        //process.OutputDataReceived += (sender, e) => Debug.Log("Python Output: " + e.Data);
        //process.ErrorDataReceived += (sender, e) => Debug.LogError("Python Error: " + e.Data);

        //// Start the process.
        //process.Start();

        //// Begin asynchronous reading of the output and error streams.
        //process.BeginOutputReadLine();
        //process.BeginErrorReadLine();

        //// Wait for the process to exit.
        //process.WaitForExit();

        //// Get the exit code.
        //int exitCode = process.ExitCode;
        //Debug.Log("Python Exit Code: " + exitCode);

        //// Clean up the process.
        //process.Close();

    }


    // Start is called before the first frame update
    void Start()
    {
        generateButton.onClick.AddListener(() => {
            generateButton.interactable = false;
            this.Generate();
            generateButton.interactable = true;
        });

        RunModel();
    }
}


public class CharacterHeadsGenerateResponse
{
    public string error;
    public string[] generation_ids;
}
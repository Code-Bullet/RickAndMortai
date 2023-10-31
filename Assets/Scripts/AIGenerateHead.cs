using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class AIGenerateHead : MonoBehaviour
{
    // The generate button.
    public Button generateButton;

    // The object which contains the character name TextMesh.
    public GameObject characterNameText;

    // The 4 character models which we will rig with AI-generated heads.
    public GameObject choice1;
    public GameObject choice2;
    public GameObject choice3;
    public GameObject choice4;

    // Python path.
    public string pythonPath = "/Users/liamz/.local/share/virtualenvs/local-image-gen-51gqmnA4/bin/python";


    private string GetCharacterName()
    {
        TextMesh textmesh = characterNameText.GetComponent<TextMesh>();
        string characterName = textmesh.text;
        return characterName;
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

    private void Generate()
    {
        Debug.Log("generating 3d character models");
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

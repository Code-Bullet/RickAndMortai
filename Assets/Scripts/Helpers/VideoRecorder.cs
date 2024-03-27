using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEngine;

public class VideoRecorder
{
    public RecorderController controller;

    public VideoRecorder() { }

    // Start is called before the first frame update
    public static VideoRecorder Start(string fname)
    {
        //bool debug = false;
        // Only run in editor mode if in debug.
        //if (!debug && Application.isEditor) {
        //    return;
        //}

        //string sceneId = System.Environment.GetEnvironmentVariable("RAM_SCENE_ID");
        //Debug.Log($"RAM_SCENE_ID: {sceneId}");

        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html

        var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var TestRecorderController = new RecorderController(controllerSettings);

        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "My Video Recorder";
        videoRecorder.Enabled = true;

        videoRecorder.ImageInputSettings = new GameViewInputSettings
        {
            //OutputWidth = 640,
            //OutputHeight = 480
            OutputWidth = 790,
            OutputHeight = 442
        };

        videoRecorder.AudioInputSettings.PreserveAudio = true;
        videoRecorder.OutputFile = $"GenerationServer/{fname}";

        controllerSettings.AddRecorderSettings(videoRecorder);
        controllerSettings.FrameRate = 30;
        // Don't skip.
        //Time.captureFramerate = 30;

        RecorderOptions.VerboseMode = true;
        TestRecorderController.PrepareRecording();
        TestRecorderController.StartRecording();




        VideoRecorder v = new VideoRecorder();
        v.controller = TestRecorderController;
        return v;
    }

    // Update is called once per frame
    public void Stop()
    {
        controller.StopRecording();
    }
}

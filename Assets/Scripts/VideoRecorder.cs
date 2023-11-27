using System.Collections;
using System.Collections.Generic;
//using UnityEditor;
//using UnityEditor.Recorder;
//using UnityEditor.Recorder.Input;
using UnityEngine;

public class VideoRecorder : MonoBehaviour
{
    // Start is called before the first frame update
    async void Start()
    {
        bool debug = false;
        // Only run in editor mode if in debug.
        if (!debug && Application.isEditor) {
            return;
        }

        string sceneId = System.Environment.GetEnvironmentVariable("RAM_SCENE_ID");
        Debug.Log($"RAM_SCENE_ID: {sceneId}");

        // https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html

        //var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        //var TestRecorderController = new RecorderController(controllerSettings);

        //var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        //videoRecorder.name = "My Video Recorder";
        //videoRecorder.Enabled = true;
        //videoRecorder.VideoBitRateMode = VideoBitrateMode.High;

        //videoRecorder.ImageInputSettings = new GameViewInputSettings
        //{
        //    //OutputWidth = 640,
        //    //OutputHeight = 480
        //    OutputWidth = 790,
        //    OutputHeight = 442
        //};

        //videoRecorder.AudioInputSettings.PreserveAudio = true;
        //videoRecorder.OutputFile = $"recorded-scene-{sceneId}";

        //controllerSettings.AddRecorderSettings(videoRecorder);
        //controllerSettings.FrameRate = 60;

        //RecorderOptions.VerboseMode = true;
        //TestRecorderController.PrepareRecording();
        //TestRecorderController.StartRecording();


        var thingMgr = WholeThingManager.Singleton;

        //await thingMgr.RunScene(RickAndMortyScene.ReadFromDir("scene-0eb7fec2-e9ed-5a25-ebe8-771a38c2dcff"));
        await thingMgr.RunScene(RickAndMortyScene.ReadFromDir(sceneId));

        //TestRecorderController.StopRecording();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

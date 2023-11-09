using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


public class RickAndMortyScene
{
    public string id;

    // Title string / initial prompt.
    public string titleString;

    // Author of script.
    // e.g. "me", "Me because you guys are nasty"
    public string author;

    // Input to ChatGPT.
    public string chatGPTSystemMessage;

    // Output from ChatGPT.

    // Unprocessed.
    public string chatGPTRawOutput;

    // Processed (for slurs etc.).
    public string[] chatGPTOutputLines;

    // Voice tracks.
    [System.NonSerialized]
    public List<AudioClip> ttsVoiceActingLines;

    // A list of file names for the voice tracks.
    public string[] voiceTracks;

    // Generated dimensions and characters.
    public AIArtStuff aiArt;

    public RickAndMortyScene(
        string id,
        string initialPrompt,
        string promptAuthor,
        string chatGPTSystemMessage,
        string rawOutput,
        string[] outputLines,
        List<AudioClip> voiceActing,
        AIArtStuff aiArt
    )
    {
        if (id == null)
        {
            this.id = GuidUtil.GenerateRandomUuid().ToString();
        }
        else
        {
            this.id = id;
        }

        titleString = initialPrompt;
        this.chatGPTRawOutput = rawOutput;
        this.chatGPTSystemMessage = chatGPTSystemMessage;
        author = promptAuthor;
        chatGPTOutputLines = outputLines;
        ttsVoiceActingLines = voiceActing;
        this.aiArt = aiArt;
    }

    public void WriteToDir()
    {
        // 1. Create the directory.
        string sceneId = $"scene-{this.id}";

        // Combine the directory path and file name
        string basePath = $"saved-scenes/{sceneId}/";
        string sceneFilePath = $"{basePath}/scene.json";
        string voiceTracksDir = $"{basePath}/voice-tracks";
        string aiCharacterDir = $"{basePath}/characters";
        string aiDimensionDir = $"{basePath}/dimensions";

        // Make a new directory.
        if (!Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
            Debug.Log("Directory created at: " + basePath);
            Directory.CreateDirectory(voiceTracksDir);
            Directory.CreateDirectory(aiCharacterDir);
            Directory.CreateDirectory(aiDimensionDir);
        }
        else
        {
            Debug.Log("Directory already exists at: " + basePath);
        }

        // 2. Write the audio clips to disk.
        List<string> voiceTracks = new List<string>();
        for (int i = 0; i < this.ttsVoiceActingLines.Count; i++)
        {
            AudioClip clip = this.ttsVoiceActingLines[i];
            string filename = $"clip_{i}.wav";
            Debug.Log($"writing clip #{i} to disk {filename}");

            // Save the file as a .ogg to the saved-scenes/ directory.
            byte[] wavData = WavUtility.ConvertAudioClipToWAV(clip);
            File.WriteAllBytes($"{voiceTracksDir}/{filename}", wavData);

            voiceTracks.Add(filename);
        }
        this.voiceTracks = voiceTracks.ToArray();

        // 3. Write the AI art to disk.
        if (aiArt.character != null)
        {
            byte[] pngData = aiArt.character.texture.EncodeToPNG();
            string filename = $"character_0.png";
            File.WriteAllBytes($"{aiCharacterDir}/{filename}", pngData);
            aiArt.character.texturePath = filename;
        }
        if (aiArt.dimension != null)
        {
            byte[] pngData = aiArt.dimension.texture.EncodeToPNG();
            string filename = $"dimension_0.png";
            File.WriteAllBytes($"{aiDimensionDir}/{filename}", pngData);
            aiArt.dimension.texturePath = filename;
        }

        // 4. Write the scene data as .json.
        try
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);

            // Write the JSON data to the file
            File.WriteAllText(sceneFilePath, json);
            Debug.Log("JSON data saved to " + basePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save JSON data: " + e.Message);
        }
    }

    public static RickAndMortyScene ReadFromDir(string sceneId)
    {
        Debug.Log($"Reading scene from directory: {sceneId}");
        // Combine the directory path and file name
        string basePath = $"saved-scenes/{sceneId}/";
        string sceneFilePath = $"{basePath}/scene.json";
        string voiceTracksDir = $"{basePath}/voice-tracks";
        string aiCharacterDir = $"{basePath}/characters";
        string aiDimensionDir = $"{basePath}/dimensions";

        // Check scene exists.
        if (!Directory.Exists(basePath))
        {
            throw new Exception($"no scene found by ID: {sceneId}");
        }

        // Read scene.json.
        string data = File.ReadAllText(sceneFilePath);
        var scene = JsonConvert.DeserializeObject<RickAndMortyScene>(data);

        // Load in audio tracks.
        scene.ttsVoiceActingLines = new List<AudioClip>();
        foreach (string filename in scene.voiceTracks)
        {
            byte[] wavData = File.ReadAllBytes($"{voiceTracksDir}/{filename}");
            AudioClip clip = WavUtility.ConvertWAVToAudioClip(wavData);
            scene.ttsVoiceActingLines.Add(clip);
        }

        // Load in AI art.
        if (scene.aiArt != null)
        {
            AIArtStuff aiArt = scene.aiArt;
            if (aiArt.character != null)
            {
                // Load texture.
                byte[] pngData = File.ReadAllBytes($"{aiCharacterDir}/{aiArt.character.texturePath}");
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(pngData);
                aiArt.character.texture = tex;
            }

            if (aiArt.dimension != null)
            {
                // Load texture.
                byte[] pngData = File.ReadAllBytes($"{aiDimensionDir}/{aiArt.dimension.texturePath}");
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(pngData);
                aiArt.dimension.texture = tex;
            }
        }

        return scene;
    }
}


public class AIArtStuff
{
    public AIDimension dimension;
    public AICharacter character;

    public AIArtStuff() { }
}

public class AIDimension
{
    public string dimensionName;
    public string prompt;

    [System.NonSerialized]
    public Texture2D texture;
    public string texturePath;

    public AIDimension() { }
}

public class AICharacter
{
    public string characterName;
    public string prompt;

    [System.NonSerialized]
    public Texture2D texture;
    public string texturePath;

    public AICharacter() { }
}


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

    /// <summary>
    /// The processed lines of the script.
    /// </summary>
    // Processed (for slurs etc.).
    // This script includes:
    // - dialogue
    // - stage directions
    // - camera angles
    // - slurs processed
    public string[] chatGPTOutputLines;

    // Voice tracks.
    [System.NonSerialized]
    public List<AudioClip> ttsVoiceActingLines;

    // A list of file names for the voice tracks.
    public string[] voiceTracks;

    // Generated dimensions and characters.
    // NOTE: not nullable.
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

    /// <summary>
    /// Writes to the saved scenes directory with `sceneId`
    /// </summary>
    /// <param name="sceneId"></param>
    public void WriteToDir(string sceneId = null)
    {
        // 1. Create the directory.
        if(sceneId == null) sceneId = $"scene-{this.id}";

        // Combine the directory path and file name
        string basePath = $"{WholeThingManager.Singleton.GetDataDir()}/saved-scenes/{sceneId}/";
        string sceneFilePath = $"{basePath}/scene.json";
        string voiceTracksDir = $"{basePath}/voice-tracks";
        string aiCharacterDir = $"{basePath}/characters";
        string aiHeadDirName = $"{basePath}/heads3d";
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

            // Save the file as a .wav to the saved-scenes/ directory.
            byte[] wavData = WavUtility.ConvertAudioClipToWAV(clip);
            if (clip.samples == 0) throw new Exception("failed to write clip, 0 samples");
            if (clip.length < 0.1) throw new Exception("failed to write clip, length seems too short");
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
        if(aiArt.character?.head3d != null)
        {
            AIHead3D head3d = aiArt.character?.head3d;
            // Copy the 3D head from the generations directory.
            //  src: "local-image-gen/headshot/data/3d/dalai lama/7as7d7f8ds72"
            // dest: "heads3d/7as7d7f8ds72/..."
            string pathToGeneration = $"local-image-gen/headshot/data/3d/{head3d.characterKey}/{head3d.selectedGeneration}_mesh";
            string outputPath = $"{aiHeadDirName}/{head3d.selectedGeneration}";
            PathUtils.CopyDirectory(pathToGeneration, outputPath, true);
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
            Debug.Log("JSON data saved to " + sceneFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save JSON data: " + e.Message);
        }
    }

    /// <summary>
    /// Reads from the saved scenes directory with `sceneId`
    /// </summary>
    /// <param name="sceneId"></param>
    public static RickAndMortyScene ReadFromDir(string sceneId)
    {
        Debug.Log($"Reading scene from directory: {sceneId}");

        // Combine the directory path and file name
        string basePath = $"{WholeThingManager.Singleton.GetDataDir()}/saved-scenes/{sceneId}/";
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
            Debug.Log($"reading audio clip {voiceTracksDir}/{filename}");
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
                Debug.Log($"reading AI art character texture {aiCharacterDir}/{aiArt.character.texturePath}");
                byte[] pngData = File.ReadAllBytes($"{aiCharacterDir}/{aiArt.character.texturePath}");
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(pngData);
                aiArt.character.texture = tex;
            }

            if (aiArt.dimension != null)
            {
                // Load texture.
                Debug.Log($"reading AI art dimension texture {aiCharacterDir}/{aiArt.dimension.texturePath}");
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
    // Nullable
    public AIDimension dimension;
    // Nullable
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

    // Nullable
    public AIHead3D head3d;

    [System.NonSerialized]
    public Texture2D texture;
    public string texturePath;

    public AICharacter() { }
}

public class AIHead3D
{
    public string characterKey;
    // The generation ID from the remote 3d headshot generation server.
    public string[] generationIds;
    // The selected generation.
    public string selectedGeneration;
}
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;
using System.Net.Http;

using System.Linq;

// this script has a function that takes a tts audio file and adds burps to it.
public class Burpifier : MonoBehaviour
{
    private AudioClip clip;
    public float silenceThreshold = 0.01f;
    public float minClipDuration = 0.6f;
    public float pauseBetweenClips = 2f;

    public List<AudioClip> burps;

    public float burpChance = 0.1f;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public AudioClip Burpify(AudioClip clipToBurpify)
    {
        
        clip = clipToBurpify;
        List<AudioClip> clips = clip.SplitBySilence(silenceThreshold);
        List<AudioClip> clipsWithBurps = new();

        int previousBurpSelected = 0;
        for (int i = 0; i < clips.Count; i++)
        {

            clipsWithBurps.Add(clips[i]);

            float randomValue = UnityEngine.Random.Range(0f, 1f);

            if (randomValue < burpChance)
            {

                int randomIndex = UnityEngine.Random.Range(0, burps.Count);
                while (randomIndex == previousBurpSelected)
                {
                    randomIndex = UnityEngine.Random.Range(0, burps.Count);
                }

                previousBurpSelected = randomIndex;

                clipsWithBurps.Add(burps[randomIndex]);
            }
        }

        AudioClip combinedClip = clipsWithBurps.Combine();
        if (combinedClip == null)
        {
            return clipToBurpify;
        }

        return combinedClip;
    }
}
public static class AudioClipExtensions
{
    public static List<AudioClip> SplitBySilence(this AudioClip clip, float silenceThreshold, int consecutiveSamplesForSilence = 1000)
    {
        if (clip == null)
            throw new ArgumentNullException("clip", "AudioClip must not be null");

        int sampleCount = clip.samples * clip.channels;
        float[] data = new float[sampleCount];
        clip.GetData(data, 0);

        List<int> cutPoints = new()
        { 0 };  // Start with 0 as the first cut point

        int silenceCounter = 0;
        for (int i = 0; i < sampleCount; i++)
        {
            if (Mathf.Abs(data[i]) < silenceThreshold)
            {

                if (silenceCounter >= consecutiveSamplesForSilence)
                {
                    cutPoints.Add(i - consecutiveSamplesForSilence); // Record the position before the silence
                                                                            // Skip the rest of the silent part
                    while (i < sampleCount && Mathf.Abs(data[i]) < silenceThreshold)
                        i++;
                    silenceCounter = 0;
                }

                silenceCounter++;
            }
            else
            {
                silenceCounter = 0;
            }
        }

        cutPoints.Add(sampleCount);  // The end of the original clip is the last cut point

        List<AudioClip> result = new();
        // Debug.Log(cutPoints.Count);
        for (int i = 0; i < cutPoints.Count - 1; i++)
        {
            int start = cutPoints[i];
            int end = cutPoints[i + 1];
            int clipLength = end - start;
            if (clipLength <= 0)
            {
                continue;
            }
            // Debug.Log("Iteration: " + i);
            // Debug.Log("Start: " + start);
            // Debug.Log("End: " + end);
            // Debug.Log("Clip Length: " + clipLength);
            float[] clipData = new float[clipLength];

            Array.Copy(data, start, clipData, 0, clipLength);
            AudioClip newClip = AudioClip.Create("Clip" + i, clipLength / clip.channels, clip.channels, clip.frequency, false);
            newClip.SetData(clipData, 0);
            result.Add(newClip);
        }

        return result;
    }
    public static AudioClip Combine(this List<AudioClip> clips)
    {
        if (clips == null || clips.Count < 2)
        {
            Debug.LogError("oh no one of these cunts is null");
            return null;

        }
        // throw new System.ArgumentException("At least two AudioClips are required to combine.");

        int channels = clips[0].channels;
        int frequency = clips[0].frequency;
        Debug.Log("Error check: 1");

        // Check if all clips have the same channels and frequency
        for (int i = 1; i < clips.Count; i++)
        {
            if (clips[i] == null || clips[i].channels != channels || clips[i].frequency != frequency)
            {
                // Try to convert the clips to the same format
                if (clips[i].channels != channels)
                {
                    Debug.LogWarning($"Converting AudioClip '{clips[i].name}' to {channels} channels.");
                    clips[i] = ConvertChannels(clips[i], channels);
                }

                if (clips[i].frequency != frequency)
                {
                    Debug.LogWarning($"Converting AudioClip '{clips[i].name}' to {frequency} Hz.");
                    clips[i] = ConvertFrequency(clips[i], frequency);
                }

                if (clips[i].channels != channels || clips[i].frequency != frequency)
                    throw new ArgumentException("AudioClips must have the same channels and frequency.");
            }
        }

        // Calculate the total length of the combined audio clip
        int totalSamples = clips.Sum(clip => clip.samples);

        // Create an array to hold the combined audio data
        float[] combinedData = new float[totalSamples * channels];

        // Copy the audio data from each clip to the combinedData array
        int offset = 0;
        foreach (AudioClip clip in clips)
        {
            float[] clipData = new float[clip.samples * channels];
            clip.GetData(clipData, 0);
            clipData.CopyTo(combinedData, offset * channels);
            offset += clip.samples;
        }

        // Create the combined audio clip
        AudioClip combinedClip = AudioClip.Create("Combined", totalSamples, channels, frequency, false);

        combinedClip.SetData(combinedData, 0);

        return combinedClip;
    }

    // Helper method to convert channels
    private static AudioClip ConvertChannels(AudioClip clip, int targetChannels)
    {
        if (targetChannels is < 1 or > 2)
            throw new ArgumentException("Target channels must be 1 (mono) or 2 (stereo).");

        int length = clip.samples * targetChannels;
        float[] data = new float[length];

        if (targetChannels == 1 && clip.channels == 2) // Stereo to Mono
        {
            float[] stereoData = new float[clip.samples * 2];
            clip.GetData(stereoData, 0);

            for (int i = 0; i < clip.samples; i++)
            {
                data[i] = (stereoData[i * 2] + stereoData[(i * 2) + 1]) * 0.5f;
            }
        }
        else if (targetChannels == 2 && clip.channels == 1) // Mono to Stereo
        {
            clip.GetData(data, 0);

            for (int i = clip.samples - 1; i >= 0; i--)
            {
                data[(i * 2) + 1] = data[i * 2] = data[i];
            }
        }
        else
        {
            throw new ArgumentException("Unsupported channel conversion.");
        }

        AudioClip convertedClip = AudioClip.Create(clip.name, clip.samples, targetChannels, clip.frequency, false);
        convertedClip.SetData(data, 0);
        return convertedClip;
    }

    // Helper method to convert frequency
    private static AudioClip ConvertFrequency(AudioClip clip, int targetFrequency)
    {
        if (targetFrequency is not 8000 and not 11025 and not 22050 and not 44100)
            throw new ArgumentException("Target frequency must be 8000, 11025, 22050, or 44100 Hz.");

        int channels = clip.channels;
        int length = Mathf.CeilToInt(clip.samples * (float)targetFrequency / clip.frequency);
        float[] data = new float[length * channels];

        clip.GetData(data, 0);

        AudioClip convertedClip = AudioClip.Create(clip.name, length, channels, targetFrequency, false);
        convertedClip.SetData(data, 0);
        return convertedClip;
    }
}

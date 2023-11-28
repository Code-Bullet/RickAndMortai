using System;
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
using System.Text;
using Newtonsoft.Json;

public static class WavUtility
{
    public static byte[] ConvertAudioClipToWAV(AudioClip audioClip)
    {
        int sampleCount = audioClip.samples;
        int channelCount = audioClip.channels;

        float[] audioData = new float[sampleCount * channelCount];
        audioClip.GetData(audioData, 0);

        byte[] wavFile = WavUtility.CreateWav(audioData, sampleCount, channelCount, audioClip.frequency);

        return wavFile;
    }


    // Function to create a WAV file from audio data
    public static byte[] CreateWav(float[] audioData, int sampleCount, int channelCount, int sampleRate)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + audioData.Length * 2);
            writer.Write(new char[4] { 'W', 'A', 'V', 'E' });

            writer.Write(new char[4] { 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1); // PCM format
            writer.Write((short)channelCount);
            writer.Write(sampleRate);
            writer.Write(sampleRate * 2 * channelCount);
            writer.Write((short)(2 * channelCount));
            writer.Write((short)16); // Bits per sample

            writer.Write(new char[4] { 'd', 'a', 't', 'a' });
            writer.Write(audioData.Length * 2);

            foreach (float value in audioData)
            {
                writer.Write((short)(value * 32767f));
            }

            writer.Close();
            return stream.GetBuffer();
        }
    }

    public static AudioClip ConvertWAVToAudioClip(byte[] wavData)
    {
        using (MemoryStream stream = new MemoryStream(wavData))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            // Read WAV header
            char[] riffHeader = reader.ReadChars(4); // "RIFF"
            int fileSize = reader.ReadInt32();
            char[] waveHeader = reader.ReadChars(4); // "WAVE"

            char[] fmtHeader = reader.ReadChars(4); // "fmt "
            int fmtSize = reader.ReadInt32();
            short audioFormat = reader.ReadInt16();
            int numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            short blockAlign = reader.ReadInt16();
            short bitsPerSample = reader.ReadInt16();

            char[] dataHeader = reader.ReadChars(4); // "data"
            int dataLength = reader.ReadInt32();

            // Read audio data
            int sampleCount = dataLength / (bitsPerSample / 8);
            float[] audioData = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                audioData[i] = reader.ReadInt16() / 32767.0f;
            }

            AudioClip audioClip = AudioClip.Create("WAV", sampleCount, numChannels, sampleRate, false);
            audioClip.SetData(audioData, 0);

            return audioClip;
        }
    }

}

public static class PathUtils
{
    public static string[] GetSubDirs(string directoryPath)
    {
        //string directoryPath = "local-image-gen/headshot/data/3d/";

        // Glob all directories.
        List<string> dirs = new List<string>();
        foreach (var d in Directory.GetDirectories(directoryPath))
        {
            var dirName = new DirectoryInfo(d).Name;
            dirs.Add(dirName);   
        }

        return dirs.ToArray();
    }
}


public static class GuidUtil
{
    public static Guid GenerateRandomUuid()
    {
        byte[] buffer = new byte[16];
        new System.Random().NextBytes(buffer);
        return new Guid(buffer);
    }
}

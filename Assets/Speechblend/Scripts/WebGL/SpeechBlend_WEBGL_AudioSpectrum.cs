// Copyright 2020, Tiny Angle Labs, All rights reserved.

#if UNITY_WEBGL
using UnityEngine;
using System.Runtime.InteropServices;

public static class SpeechBlend_WEBGL_AudioSpectrum
{
	[DllImport("__Internal")]
    private static extern void JSLIB_SB_InitializeBytes(int fftSize);

    [DllImport("__Internal")]
    private static extern void JSLIB_SB_GetSpectrumData(byte[] array);

    [DllImport("__Internal")]
    private static extern void JSLIB_SB_GetTimeDomainData(byte[] array);

    static byte[] spectrumBytes;
    static float last_freqCall = -1; 
    static byte[] timeDomainBytes; 
    static float volume_sum;  
    static float last_call = -1; 

    public static void GetFrequencySpectrum(float[] samples) 
    {
        if(last_freqCall != Time.realtimeSinceStartup) 
        {
            JSLIB_SB_GetSpectrumData(spectrumBytes);
            last_freqCall = Time.realtimeSinceStartup;
        }

        int traceLength = samples.Length;
        for (int i = 0; i < traceLength; i++)
        {
            samples[i] = spectrumBytes[i] / 512f;
            samples[i] = samples[i] * samples[i];
        }
    }

    public static float GetSpeechVolume()
    {
        if (last_call != Time.realtimeSinceStartup) 
        {
            JSLIB_SB_GetTimeDomainData(timeDomainBytes); 

            int traceLength = timeDomainBytes.Length;
            int sum = 0;
            for (int i = 0; i < traceLength; i++)
            {
                sum += Mathf.Abs(timeDomainBytes[i] - 128); 
            }
            volume_sum = sum / (float)(traceLength * 128);

            last_call = Time.realtimeSinceStartup;
        }
        return volume_sum;
    }

    static int initializedSamples = -1;
    public static int InitializeBytes(int numSamples) 
    {
        if (initializedSamples == -1)
        {
            JSLIB_SB_InitializeBytes(numSamples);
            initializedSamples = numSamples;
            spectrumBytes = new byte[numSamples];
            timeDomainBytes = new byte[2 * numSamples];
        }
        return initializedSamples;
    }
}

#endif
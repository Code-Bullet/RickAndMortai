#if (UNITY_WEBGL==false)

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SpeechBlendMicrophoneInput : MonoBehaviour
{
    AudioSource source;

    private void Start()
    {
        string device_name = Microphone.devices[0];

        source = GetComponent<AudioSource>();
        source.clip = UnityEngine.Microphone.Start(device_name, true, 5, 44100);
        source.loop = true;
        while (!(UnityEngine.Microphone.GetPosition(null) != 0)) { }
        source.Play();
    }

}

#endif
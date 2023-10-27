using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Linq; 
public class AudioBasedAmbientOcclusion : MonoBehaviour
{
    public PostProcessVolume volume;
    public float maxAmbientOcclusionIntensity = 1.0f;

    public AudioSource _audioSource;
    private AmbientOcclusion _ambientOcclusion;

    private void Start()
    {
        if (volume.profile.TryGetSettings(out _ambientOcclusion) == false)
        {
            Debug.LogError("No Ambient Occlusion found in the Post Processing Volume!");
        }
    }

    private void Update()
    {
        if (_ambientOcclusion != null && _audioSource != null)
        {
            float[] spectrum = new float[256];
            _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

            float intensity = Mathf.Clamp01(spectrum.Max());
            Debug.Log(intensity);
            _ambientOcclusion.intensity.value = intensity * maxAmbientOcclusionIntensity;
        }else{
            Debug.Log("oop");
        }
    }
}

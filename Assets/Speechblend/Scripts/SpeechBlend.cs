// Copyright 2020, Tiny Angle Labs, All rights reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SpeechBlendEngine;
using TMPro;

public class SpeechBlend : MonoBehaviour
{
    public AudioSource voiceAudioSource;
    public SkinnedMeshRenderer headMesh;
    [HideInInspector]
    public bool showBlendShapeMenu = false;
    [HideInInspector]
    public SpeechUtil.VisemeBlendshapeIndexes faceBlendshapes;
    [HideInInspector]
    public SpeechUtil.VisemeWeight visemeWeightTuning;
    [HideInInspector]
    public int template_subgroup = 0; // For templates (such as CC4) in which multiple variations of blendshapes exist
    [Header("Settings")]
    [Space(10)]
    [Tooltip("Toggle lipsyncing")]
    public bool lipsyncActive = true; // Toggle lipsyncing
    [Tooltip("Select whether visemes are used")]
    public SpeechUtil.Mode trackingMode = SpeechUtil.Mode.jawOnly; // Select whether visemes are used
#if UNITY_WEBGL
    public float WebGLVolumeAdjust = 200;
#endif
    [Tooltip("Amplitude of jaw movement")]
    [Range(0.0f, 1.0f)]
    public float jawMovementAmount = 0.5f; // Amplitude of jaw movement
    [Tooltip("Jaw motion speed")]
    [Range(0.0f, 1.0f)]
    public float jawMovementSpeed = 0.5f; // Jaw motion speed
    [Tooltip("Amplitude of lip movement")]
    [Range(0.0f, 1.0f)]
    public float lipsBlendshapeMovementAmount = 0.5f; // Amplitude of lip movement
    [Tooltip("Lip viseme movement speed")]
    [Range(0.0f, 1.0f)]
    public float lipsBlendshapeChangeSpeed = 0.5f; // Lip viseme movement speed
    [Tooltip("Number of calculations to use.")]
    public SpeechUtil.Accuracy accuracy = SpeechUtil.Accuracy.Medium; // Number of calculations to use. Cannot be changed at runtime 
    [Tooltip("Number of frames to wait before next calculation (higher number uses less resources but reponds slower)")]
    [Range(1, 10)]
    public int framesPerUpdate = 1; // Number of frames to wait before next calculation (higher number uses less resources but reponds slower)   
    [Tooltip("Ignore distance between AudioSource and AudioListener when accounting for volume.")] 
    public bool volumeEqualization = false;
    [Tooltip("Voice type of character")]
    public VoiceProfile.VoiceType voiceType = VoiceProfile.VoiceType.female; // Voice type of character
    [Tooltip("Jaw joint for when not using a mouth open blendshape")]
    public Transform jawJoint; // Jaw joint for when not using a mouth open blendshape
    [Tooltip("Direction adjust for jaw opening")]
    public Vector3 jawOpenDirection = new Vector3(1, 0, 0); // Direction adjust for jaw joint opening
    [Tooltip("Angular offset for jaw joint opening")]
    public Vector3 jawJointOffset; // Angular offset for jaw joint opening
    [Tooltip("Blendshape template for visemes shapes. (default: DAZ)")]
    public VoiceProfile.VisemeBlendshapeTemplate shapeTemplate; // Blendshape template for visemes shapes. (default: DAZ)
    [HideInInspector] 
    public AudioListener activeListener; // Source audiolistener for use when calculating the volume equalization

    float bs_volume_scaling = 20f;
    float jaw_volume_scaling = 20f;

    int f_low;
    int f_high;
    float fres;

    float[,] extractor; // Speech extractor model
    float[,] transformer; // Speech data transformer
    float[] modifier; // Speech data modifier
    float[] bs_setpoint;
    float[] bs_setpoint_last;
    float[,] cmem;
    float bs_mouthOpen_setpoint;
    Quaternion trans_mouthOpen_setpoint;
    Quaternion trans_mouthOpen_rest;
    float current_volume;

    [HideInInspector]
    public VoiceProfile.VisemeBlendshapeTemplate template_saved = VoiceProfile.VisemeBlendshapeTemplate.DAZ;
    [HideInInspector]
    public SpeechUtil.VisemeBlendshapeIndexes faceBlendshapes_saved = new SpeechUtil.VisemeBlendshapeIndexes(VoiceProfile.G2_template);
    [HideInInspector]
    public SpeechUtil.VisemeWeight visemeWeightTuning_saved = new SpeechUtil.VisemeWeight(VoiceProfile.G2_template);

    float jaw_CSF = 1;
    float bs_CSF = 1;

    int updateFrame = 0;

    SpeechUtil.Accuracy accuracy_last;

    bool[] blendshapeInfluenceActive;
    
    void Start()
    {
        bs_setpoint = new float[faceBlendshapes.template.Nvis];
        bs_setpoint_last = new float[faceBlendshapes.template.Nvis];
        
        if (jawJoint != null)
        {
            trans_mouthOpen_setpoint = jawJoint.localRotation;
        }
        trans_mouthOpen_rest = trans_mouthOpen_setpoint;
        bs_mouthOpen_setpoint = 0;
        accuracy_last = accuracy;

        fres = ExtractFeatures.CalculateFres();

        UpdateExtractor();

        if (jawJoint == null & !faceBlendshapes.AnyAssigned())
        {
            print("Warning (SpeechBlend): Neither jaw joint or face blendshapes have been assigned");
            lipsyncActive = false;
        }
        if (trackingMode.Equals(SpeechUtil.Mode.jawAndVisemes) & faceBlendshapes.JawOnly())
        {
            print("Warning (SpeechBlend): No viseme blendshapes detected, jaw-only mode enabled.");
            trackingMode = SpeechUtil.Mode.jawOnly;
        }
        else if (trackingMode.Equals(SpeechUtil.Mode.jawAndVisemes))
        {
            blendshapeInfluenceActive = new bool[faceBlendshapes.template.Nvis];
        }
    }

    void FixedUpdate()
    {
        if (voiceAudioSource.isPlaying & lipsyncActive)
        {
            bs_volume_scaling = 500f * Mathf.Exp(-6.111f * lipsBlendshapeMovementAmount);
            jaw_volume_scaling = 500f * Mathf.Exp(-6.111f * jawMovementAmount);

            if (++updateFrame >= framesPerUpdate)
            {
                updateFrame = 0;

                float last_volume = current_volume;
                current_volume = 0;
#if UNITY_WEBGL
                int no_samples = SpeechBlend_WEBGL_AudioSpectrum.InitializeBytes(4096); // Get volume data
                current_volume = SpeechBlend_WEBGL_AudioSpectrum.GetSpeechVolume();
                current_volume = current_volume * WebGLVolumeAdjust;
#else
                float[] audioTrace = new float[256];
                voiceAudioSource.GetOutputData(audioTrace, 0);
                for (int i = 0; i < 256; i++)
                    current_volume += Mathf.Abs(audioTrace[i]);
#endif
                if (volumeEqualization)
                    current_volume = ExtractFeatures.EqualizeDistance(current_volume, voiceAudioSource, activeListener);
                current_volume = last_volume * (1 - lipsBlendshapeChangeSpeed) + current_volume * lipsBlendshapeChangeSpeed;

                // Calculate jaw open amount
                bs_mouthOpen_setpoint = 100 * current_volume / jaw_volume_scaling * .1f * (1 / jaw_CSF);
                if (jawJoint != null)
                {
                    //current_volume = 0;
                    trans_mouthOpen_setpoint = Quaternion.Euler(jawJointOffset + trans_mouthOpen_rest.eulerAngles * (1 - jawMovementAmount * 3) + (trans_mouthOpen_rest.eulerAngles + jawOpenDirection * current_volume / jaw_volume_scaling) * jawMovementAmount * 3);
                }

                // Calculate viseme amounts
                if (trackingMode == SpeechUtil.Mode.jawAndVisemes)
                {
                    f_low = Mathf.RoundToInt(ExtractFeatures.getlf(accuracy) / fres);
                    f_high = Mathf.RoundToInt(ExtractFeatures.gethf(accuracy) / fres);
                    if (accuracy_last != accuracy)
                        UpdateExtractor();

                    accuracy_last = accuracy;
#if UNITY_WEBGL

                    float[] rawData = new float[4096];
                    SpeechBlend_WEBGL_AudioSpectrum.GetFrequencySpectrum(rawData); // get the spectrum data
                    for (int i = 0; i < rawData.Length; i++)
                        rawData[i] += 1e-10f;
#else
                    float[] rawData = ExtractFeatures.GetSoundData(voiceAudioSource);
#endif
                    float[] features = ExtractFeatures.ExtractSample(rawData, extractor, transformer, modifier, ref cmem, f_low, f_high, accuracy);

                    ExtractFeatures.FeatureOutput W = ExtractFeatures.Evaluate(features, voiceType, accuracy);

                    float[] influences = new float[ExtractFeatures.no_visemes];
                    for (int i = 0; i < W.size; i++)
                    {
                        for (int j = 0; j < ExtractFeatures.no_visemes; j++)
                            influences[j] += VoiceProfile.Influence(voiceType, W.reg[i], j, accuracy) * W.w[i];
                    }
                    float[] influences_template = VoiceProfile.InfluenceTemplateTransform(influences, shapeTemplate, template_subgroup);
                    blendshapeInfluenceActive = new bool[faceBlendshapes.template.Nvis];
                    for (int i = 0; i < faceBlendshapes.template.Nvis; i++)
                    {
                        float visemeWeight = visemeWeightTuning.GetByIndex(i);
                        influences_template[i] *= visemeWeight;
                        if (visemeWeight < 1e-2)
                            blendshapeInfluenceActive[i] = false;
                        else
                            blendshapeInfluenceActive[i] = true;
                    }

                    for (int i = 0; i < faceBlendshapes.template.Nvis; i++)
                    {
                        bs_setpoint[i] = influences_template[i] * 100 * current_volume / bs_volume_scaling;
                    }
                    jaw_CSF = VoiceProfile.Influence(voiceType, W.reg[0], ExtractFeatures.no_visemes, accuracy);
                    bs_CSF = VoiceProfile.Influence(voiceType, W.reg[0], ExtractFeatures.no_visemes, accuracy);
                    bs_mouthOpen_setpoint /= VoiceProfile.Influence(voiceType, W.reg[0], ExtractFeatures.no_visemes, accuracy);
                }
            }
        }
    }

    private void LateUpdate()
    {
        // Update jaw joint animation
        if (!faceBlendshapes.BlendshapeAssigned("mouthOpen"))
        {
            if (jawJoint != null)
            {
                float moveSpeed = 2.5f * Mathf.Exp(3.658f * jawMovementSpeed);

                jawJoint.transform.localRotation = Quaternion.Lerp(jawJoint.transform.localRotation, trans_mouthOpen_setpoint, Time.deltaTime * moveSpeed);
            }
        }
        // Update Facial Blendshapes
        UpdateBlendshapes();
    }

    void UpdateBlendshapes()
    {
        if (trackingMode == SpeechUtil.Mode.jawAndVisemes)
        {
            for (int i = 0; i < faceBlendshapes.template.Nvis; i++)
            {
                if (faceBlendshapes.BlendshapeAssigned(i) & blendshapeInfluenceActive[i])
                {
                    float currentVisemeValue = bs_setpoint_last[i];
                    float a1 = bs_setpoint[i];
                    var a2 = faceBlendshapes.GetByIndex(i);

                    headMesh.SetBlendShapeWeight(a2, currentVisemeValue * (1 - lipsBlendshapeChangeSpeed * bs_CSF) + a1 * lipsBlendshapeChangeSpeed * bs_CSF);
                    bs_setpoint_last[i] = headMesh.GetBlendShapeWeight(faceBlendshapes.GetByIndex(i));
                }

            }
        }
        if (faceBlendshapes.BlendshapeAssigned("mouthOpen"))
        {
            float currentValue = headMesh.GetBlendShapeWeight(faceBlendshapes.mouthOpenIndex);
            headMesh.SetBlendShapeWeight(faceBlendshapes.mouthOpenIndex, currentValue * (1 - (jawMovementSpeed * jaw_CSF)) + bs_mouthOpen_setpoint * (jawMovementSpeed * jaw_CSF));
        }
    }

    public void UpdateExtractor()
    {
        extractor = ExtractFeatures.BuildExtractor(fres, ExtractFeatures.getlf(accuracy), ExtractFeatures.gethf(accuracy), accuracy);
        cmem = new float[ExtractFeatures.getC(accuracy) + 1, 2];
        modifier = ExtractFeatures.CreateCC_lifter(accuracy);
        transformer = ExtractFeatures.GenerateTransformer(accuracy);
        f_low = Mathf.RoundToInt(ExtractFeatures.getlf(accuracy) / fres);
        f_high = Mathf.RoundToInt(ExtractFeatures.gethf(accuracy) / fres);
    }


}









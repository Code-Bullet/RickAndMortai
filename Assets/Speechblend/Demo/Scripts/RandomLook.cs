using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLook : MonoBehaviour {

    public Transform[] eyeballJoints;
    public SkinnedMeshRenderer headMesh;
    public string blinkBlendShapeName;
    [Tooltip("Eyes look target. Leave blank for random")]
    public Transform lookTarget;
    public bool randomLook = true;
    public bool eyesTrackTarget = false;
    public bool randomBlink = true;
    [Header("\tLook Settings")]
    [Tooltip("Average time between changing random look direction (s)")]
    [Range(0.5f, 10.0f)]
    public float changeLookPeriod = 3f;
    [Tooltip("Maximum random look direction angle (degrees, < 90)")]
    [Range(0f, 70.0f)]
    public float randomLookMaxAngle = 20f;
    [Tooltip("Cutoff angle for eye tracking (degrees, < 90)")]
    [Range(0f, 70.0f)]
    public float eyeTrackMaxAngle = 10f;
    [Header("\tBlink Settings")]
    [Tooltip("Average blink time (s)")]
    [Range(0.5f, 10.0f)]
    public float blinkPeriod = 3f;
    [Tooltip("Blink speed")]
    [Range(0.0f, 1.0f)]
    public float blinkSpeed = 0.7f;
    [Tooltip("Blink hold closed time")]
    [Range(0.0f, 0.2f)]
    public float blinkHoldTime = 0.03f;
    [Tooltip("Blink closed amount")]
    [Range(0.0f, 1.0f)]
    public float blinkClosedLimit = 1f;
    [Tooltip("Blink open amount")]
    [Range(0.0f, 1.0f)]
    public float blinkOpenLimit = 0f;

    public Vector3 lookAngleOffset = Vector3.zero;
    public Vector3 eyesForwardVector = Vector3.forward;


    int blinkBSindex = -1;

    float lastBlinkTime = 0;
    float lastLookChangeTime = 0;

    float blinkPeriodRandom = 0;
    float lookChangePeriodRandom = 0;

    public bool blinking = false;

    Quaternion[] EyeballStartDirection = new Quaternion[2];

    GameObject RandomLookTarget;
    GameObject EyesCenterObject;

    Transform randomLookTarget;
    

    Coroutine blinkBlendRoutine;
    Mesh m;

    // Use this for initialization
    void Start () {
        if (eyeballJoints.Length == 0)
        {
            randomLook = false;
            eyesTrackTarget = false;
        }
        else if (eyeballJoints[0] == null | eyeballJoints[1] == null)
        {
            randomLook = false;
            eyesTrackTarget = false;
        }
        else
        {
            StartCoroutine(GetStartAngles());
            
            if (!randomLook & (lookTarget != null))
                eyesTrackTarget = true;
            else if (randomLook)
            {
                InitializeRandomLook();
            }
            else
            {
                eyesTrackTarget = false;
            }
        }

        if (!string.IsNullOrEmpty(blinkBlendShapeName)){
            m = headMesh.sharedMesh;
            for (int i = 0; i < m.blendShapeCount; i++)
            {
                string s = m.GetBlendShapeName(i);
                if (m.GetBlendShapeName(i) == blinkBlendShapeName)
                {
                    blinkBSindex = i;
                    break;
                }
            }
            if (blinkBSindex == -1)
            {
                print("Warning: blink blendshape " + blinkBlendShapeName + " not found");
                randomBlink = false;
            }
        }
        lastBlinkTime = 0;
        blinkPeriodRandom = (Random.Range(0f, 10f) - 5f) / 5f;
        if (blinkClosedLimit < blinkOpenLimit)
            blinkClosedLimit = blinkOpenLimit;

    }
	
	// Update is called once per frame
	void FixedUpdate () {
		if (randomBlink & ((Time.time - (lastBlinkTime+blinkPeriodRandom)) > blinkPeriod))
        {
            StartBlink();
            lastBlinkTime = Time.time;
            blinkPeriodRandom = (Random.Range(0f, 10f) - 5f) / 5f;
        }
        if ((randomLook) & ((Time.time - (lastLookChangeTime + lookChangePeriodRandom)) > changeLookPeriod))
        {
            StartRandomLook();
            lastLookChangeTime = Time.time;
            lookChangePeriodRandom = (Random.Range(0f, 10f) - 5f) / 5f;
        }
        else if (eyesTrackTarget)
        {
            eyeballJoints[0].LookAt(lookTarget);
            eyeballJoints[0].Rotate(lookAngleOffset);
            eyeballJoints[1].localRotation = eyeballJoints[0].localRotation;

            float difference = Quaternion.Angle(eyeballJoints[0].localRotation*Quaternion.Euler(lookAngleOffset), EyeballStartDirection[0]);

            if (difference > eyeTrackMaxAngle)
            {
                eyeballJoints[0].localRotation = Quaternion.Lerp(EyeballStartDirection[0], eyeballJoints[0].localRotation, eyeTrackMaxAngle / difference);
                eyeballJoints[1].localRotation = eyeballJoints[0].localRotation;
            }

        }


    }

    public void StartRandomLook()
    {
        // Set new random look target position
        if (randomLookTarget == null)
            InitializeRandomLook();
        float randomTargetDistance = 1f;
        float randomTheta = Random.Range(-randomLookMaxAngle, randomLookMaxAngle) * Mathf.PI / 180f;
        float randomPhi = Random.Range(-randomLookMaxAngle, randomLookMaxAngle) * Mathf.PI / 180f;

        randomLookTarget.localPosition = new Vector3(randomTargetDistance / Mathf.Tan( Mathf.PI / 2 - randomTheta ), randomTargetDistance / Mathf.Tan(Mathf.PI / 2 - randomPhi), randomTargetDistance);
        eyeballJoints[0].LookAt(randomLookTarget);
        eyeballJoints[0].Rotate(lookAngleOffset);
        eyeballJoints[1].localRotation = eyeballJoints[0].localRotation;
    }

    void InitializeRandomLook()
    {
        eyesTrackTarget = false;
        EyesCenterObject = new GameObject("EyesCenter");
        EyesCenterObject.transform.position = (eyeballJoints[0].position + eyeballJoints[1].position) / 2;
        EyesCenterObject.transform.forward = eyesForwardVector;
        EyesCenterObject.transform.parent = eyeballJoints[0].parent;
        RandomLookTarget = new GameObject("RandomLookTarget");
        randomLookTarget = RandomLookTarget.transform;
        randomLookTarget.SetParent(EyesCenterObject.transform, true);
    }

    public void StartBlink()
    {
        if (!blinking)
        {
            blinkBlendRoutine = StartCoroutine(BlinkBlend());
        }
    }

    IEnumerator BlinkBlend()
    {
        // Start blink animation
        blinking = true;
        float dt = Time.deltaTime;
        int steps = Mathf.RoundToInt((0.1f / (blinkSpeed*3 + 0.01f)) / dt * (blinkClosedLimit - blinkOpenLimit));
        headMesh.SetBlendShapeWeight(blinkBSindex, 0);
        for (int i=0; i<steps; i++)
        {
            float amount = (float)i / (float)steps * 100f;
            headMesh.SetBlendShapeWeight(blinkBSindex, amount);
            if (i%2==0)
                yield return new WaitForSeconds(dt);
        }
        headMesh.SetBlendShapeWeight(blinkBSindex, blinkClosedLimit*100);
        yield return new WaitForSeconds(blinkHoldTime);
        for (int i = 0; i < steps; i++)
        {
            float amount = (float)i / (float)steps * 100f;
            headMesh.SetBlendShapeWeight(blinkBSindex, blinkClosedLimit*100 - amount);
            if (i%2==0)
                yield return new WaitForSeconds(dt);
        }
        headMesh.SetBlendShapeWeight(blinkBSindex, 0);
        blinking = false;
    }

    IEnumerator GetStartAngles()
    {
        yield return new WaitForSeconds(.3f);
        EyeballStartDirection[0] = eyeballJoints[0].localRotation;
        EyeballStartDirection[1] = eyeballJoints[1].localRotation;
    }

}

using UnityEngine;

public class EyeLookAt : MonoBehaviour
{
    public Transform leftEye;
    public Transform rightEye;
    public Transform lookTarget;

    [Header("Left Eye Rotation Limits")]
    public float leftEyeMaxXRotation = 45.0f;
    public float leftEyeMinXRotation = -45.0f;
    public float leftEyeMaxYRotation = 45.0f;
    public float leftEyeMinYRotation = -45.0f;

    [Header("Right Eye Rotation Limits")]
    public float rightEyeMaxXRotation = 45.0f;
    public float rightEyeMinXRotation = -45.0f;
    public float rightEyeMaxYRotation = 45.0f;
    public float rightEyeMinYRotation = -45.0f;

    void Update()
    {
        if (leftEye) RotateEyeTowardsTarget(leftEye, leftEyeMaxXRotation, leftEyeMinXRotation, leftEyeMaxYRotation, leftEyeMinYRotation);
        if (rightEye) RotateEyeTowardsTarget(rightEye, rightEyeMaxXRotation, rightEyeMinXRotation, rightEyeMaxYRotation, rightEyeMinYRotation);
    }

    void RotateEyeTowardsTarget(Transform eye, float maxX, float minX, float maxY, float minY)
    {
        Vector3 directionToTarget = lookTarget.position - eye.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

        // Apply the target rotation
        eye.rotation = targetRotation;

        // Clamp the rotation values
        Vector3 clampedRotation = eye.localEulerAngles;

        // X rotation clamping
        clampedRotation.x = ClampAngle(clampedRotation.x, minX, maxX);

        // Y rotation clamping
        clampedRotation.y = ClampAngle(clampedRotation.y, minY, maxY);

        eye.localEulerAngles = clampedRotation;
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180f) angle -= 360f; // Convert to -180 to +180 range
        return Mathf.Clamp(angle, min, max);
    }
}

using UnityEngine;

public class SunRotation : MonoBehaviour
{
    public float dayLength; // in seconds

    [Range(0f, 90f)]
    public float latitude; // in degrees, between 0 and 90

    public float zenithAngle; // in degrees, between 0 and 360

    private Transform sunTransform;

    void Start()
    {
        sunTransform = GetComponent<Transform>();
        sunTransform.eulerAngles = new(0f, zenithAngle, 0f);
    }

    void Update()
    {
        // Sun original rotation
        Vector3 rotation = new();

        float v = Time.deltaTime / dayLength * 360f;
        // Rotate the sun around the Y axis according to the time of day
        rotation.y = v * Mathf.Cos((90 - latitude) * (Mathf.PI / 180));

        // Rotate the sun around the X axis according to the latitude
        rotation.x = v * Mathf.Sin((90 - latitude) * (Mathf.PI / 180));

        // Set the rotation
        sunTransform.Rotate(rotation);
    }
}
using UnityEngine;

public class VisibilityBasedOnAngle : MonoBehaviour
{
    public GameObject mainCamera;

    [System.Serializable]
    public class PlaneVisibilityRange
    {
        public Renderer planeRenderer; // Renderer of the plane
        public float minAngle; // Minimum angle for this plane's visibility
        public float maxAngle; // Maximum angle for this plane's visibility
    }

    public PlaneVisibilityRange[] planeVisibilityRanges; // List of planes and their visibility ranges

    public SceneDirector sceneDirector; 
    private void Start()
    {


        // Initially turn off all planes
        foreach (var plane in planeVisibilityRanges)
        {
            plane.planeRenderer.enabled = false;
        }
    }

    private void Update()
    {

        mainCamera = sceneDirector.currentDimension.actualCamera;
        Vector3 toCameraDirection = mainCamera.transform.position - transform.position;

        // Zero out the vertical component to only consider horizontal angle
        toCameraDirection.y = 0;
        toCameraDirection.Normalize();

        float angle = Vector3.SignedAngle(transform.forward, toCameraDirection, transform.up);
        if (angle < 0) angle += 360;  // Convert negative angles to positive values

        // Turn off all planes initially
        foreach (var plane in planeVisibilityRanges)
        {
            plane.planeRenderer.enabled = false;
        }

        // Determine which plane to show
        foreach (var plane in planeVisibilityRanges)
        {
            if (angle >= plane.minAngle && angle < plane.maxAngle)
            {
                plane.planeRenderer.enabled = true;
                break; // Once a plane is turned on, exit the loop
            }
        }

    }
}

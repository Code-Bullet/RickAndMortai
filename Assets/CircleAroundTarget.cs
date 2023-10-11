using UnityEngine;

public class CircleAroundTarget : MonoBehaviour
{
    public Transform target;          // Target to circle around
    public float orbitSpeed = 10f;    // Speed of circling around the target
    public float distance = 5.0f;     // Distance from the target

    private void Update()
    {
        // Check if target is assigned
        if (target == null)
        {
            Debug.LogWarning("CircleAroundTarget: Target not assigned!");
            return;
        }

        // Calculate the new position
        transform.position = target.position + (transform.position - target.position).normalized * distance;

        // Orbit around the object
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);

        // Always look at the target
        transform.LookAt(target);
    }
}

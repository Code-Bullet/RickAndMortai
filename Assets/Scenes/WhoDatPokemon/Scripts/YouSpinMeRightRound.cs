using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YouSpinMeRightRound : MonoBehaviour
{
    public Transform targetCamera; // Reference to the camera transform
    public float rotationSpeed = 200.0f; // Rotation speed of the character

    public bool isRotating = true;

    void Update()
    {
        if (isRotating)
        {
            RotateWithEase();
        }
    }

    public void RotateWithEase()
    {

        Vector3 targetDir = targetCamera.position - transform.position;
        float step = rotationSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(targetDir);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        // Check if the character is facing the camera direction with a small tolerance
        //if (Quaternion.Angle(transform.rotation, targetRotation) < 1.0f)
        //{
        //    isRotating = false; // Stop rotating
        //}
    }

}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorCameraMovement : MonoBehaviour
{
    public Transform targetObject; // The object whose local position we want to mirror.

    void Update()
    {
        if (targetObject)
        {
            Vector3 mirroredLocalPosition = targetObject.localPosition;
            mirroredLocalPosition.y = -mirroredLocalPosition.y; // Flip the y-axis
            transform.localPosition = mirroredLocalPosition;
        }
    }
}

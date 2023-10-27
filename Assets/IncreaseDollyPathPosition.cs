using UnityEngine;
using Cinemachine;

public class IncreaseDollyPathPosition : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Drag and drop your Cinemachine Virtual Camera here in the Inspector
    public float increaseBy = 0.1f; // The amount by which you want to increase the path position

    void Update()
    {
        if (virtualCamera != null)
        {
            CinemachineTrackedDolly trackedDolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
            if (trackedDolly != null)
            {
                trackedDolly.m_PathPosition += increaseBy * Time.deltaTime;
            }
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Video;

// this bad boy just plays a video of a portal on a plane. its not a big deal.
public class PortalController : MonoBehaviour
{
    public GameObject openPortalPlane;
    public GameObject closePortalPlane;

    private VideoPlayer openPortalPlayer;
    private VideoPlayer closePortalPlayer;

    private MeshRenderer openPortalRenderer;
    private MeshRenderer closePortalRenderer;

    private void Start()
    {
        // Get the VideoPlayer and MeshRenderer from each plane
        openPortalPlayer = openPortalPlane.GetComponent<VideoPlayer>();
        closePortalPlayer = closePortalPlane.GetComponent<VideoPlayer>();

        openPortalRenderer = openPortalPlane.GetComponent<MeshRenderer>();
        closePortalRenderer = closePortalPlane.GetComponent<MeshRenderer>();
    }
    public void OpenPortal()
    {
        StartCoroutine(PlayPortalVideo(openPortalPlayer, openPortalRenderer));
    }

    public void ClosePortal()
    {
        StartCoroutine(PlayPortalVideo(closePortalPlayer, closePortalRenderer));
    }

private IEnumerator PlayPortalVideo(VideoPlayer videoPlayer, MeshRenderer meshRenderer)
{
    // Enable the components
    videoPlayer.enabled = true;
    videoPlayer.Play();

    // Wait for like 30ish frames, i know i had a reason for doing this but i have no idea what that was. trust in past Evan
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;
    yield return null;

    meshRenderer.enabled = true;
    // Play the video

    // Wait for the video to finish playing
    yield return new WaitForSeconds((float)videoPlayer.clip.length);

    // Disable the components
    videoPlayer.Stop();
    videoPlayer.enabled = false;
    meshRenderer.enabled = false;
}
}

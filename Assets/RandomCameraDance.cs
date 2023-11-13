using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;  // At the beginning of your script



public class RandomCameraDance : MonoBehaviour
{
    public List<GameObject> virtualCameraObjects = new List<GameObject>();
    private List<GameObject> shuffledCameras = new List<GameObject>();
    private Coroutine danceRoutine;

    // Music-related properties
    public List<AudioClip> songs = new List<AudioClip>();
    public AudioClip defaultSong;
    public AudioSource audioSource;
    public float defaultMusicVolume = 1f;
    public float danceFloorMusicVolume = 1f;
    public TextMeshProUGUI songNameText;

    public List<Animator> characterAnimators = new List<Animator>();


    private Dictionary<Animator, Vector3> initialCharacterPositions = new Dictionary<Animator, Vector3>();
    private Dictionary<Animator, Quaternion> initialCharacterRotations = new Dictionary<Animator, Quaternion>();

    public GameObject actualCamera;

    private void Awake()
    {
        foreach (var animator in characterAnimators)
        {
            initialCharacterPositions[animator] = animator.gameObject.transform.position;
            initialCharacterRotations[animator] = animator.gameObject.transform.rotation;
        }

        if (songNameText != null)
            songNameText.text = "";

        DisableAllCameras();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not set!");
        }
    }


    private void DisableAllCameras()
    {
        foreach (var vcam in virtualCameraObjects)
        {
            vcam.SetActive(false);
        }
    }

    private void ShuffleCameras()
    {
        shuffledCameras = virtualCameraObjects.OrderBy(x => Random.value).ToList();
    }

    private IEnumerator CameraDanceRoutine()
    {
        while (true)
        {
            if (shuffledCameras.Count == 0)
            {
                ShuffleCameras();
            }

            DisableAllCameras();

            // Pick a camera from the shuffled list
            GameObject selectedCamera = shuffledCameras[0];
            shuffledCameras.RemoveAt(0);

            selectedCamera.SetActive(true);

            // Wait for a random duration between 5 and 15 seconds
            yield return new WaitForSeconds(Random.Range(5f, 15f));
        }
    }


    public void DanceCameraStart()
    {
        actualCamera.SetActive(true);

        int danceNumber = Random.Range(0, 7); // Random number between 0 and 6

        foreach (var pair in initialCharacterPositions)
        {
            Debug.Log($"Animator: {pair.Key}, Position: {pair.Value}");
        }

        foreach (var animator in characterAnimators)
        {
            // Vector3 newPosition = animator.gameObject.transform.position;
            // newPosition.x = initialCharacterPositions[animator].x;
            // newPosition.z = initialCharacterPositions[animator].z;
            // animator.gameObject.transform.position = newPosition;

            animator.gameObject.transform.position = initialCharacterPositions[animator];
            animator.gameObject.transform.rotation = initialCharacterRotations[animator];
        }

        foreach (Animator animator in characterAnimators)
        {
            animator.SetInteger("dance number", danceNumber);
        }

        ShuffleCameras();
        PlayRandomSong(danceFloorMusicVolume);

        if (danceRoutine == null)
        {
            danceRoutine = StartCoroutine(CameraDanceRoutine());
        }
    }


    public void DanceCameraStop()
    {
        if (danceRoutine != null)
        {
            StopCoroutine(danceRoutine);
            danceRoutine = null;
            DisableAllCameras();
        }
        PlayDefaultSong(defaultMusicVolume);

        if (songNameText != null)
            songNameText.text = "";

        actualCamera.SetActive(false);

    }

    private void PlayRandomSong(float volume = 1f)
    {
        if (audioSource != null && songs.Count > 0)
        {
            AudioClip chosenSong = songs[Random.Range(0, songs.Count)];
            audioSource.clip = chosenSong;
            audioSource.time = Random.Range(0f, chosenSong.length);
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.Play();

            if (songNameText != null)
                songNameText.text = chosenSong.name;
        }
    }
    private void PlayRandomSongFromStart(float volume = 1f)
    {
        if (audioSource != null && songs.Count > 0)
        {
            AudioClip chosenSong = songs[Random.Range(0, songs.Count)];
            audioSource.clip = chosenSong;
            audioSource.time = 0f;
            audioSource.volume = volume;
            audioSource.loop = false;
            audioSource.Play();

            if (songNameText != null)
                songNameText.text = chosenSong.name;
        }
    }

    private void PlayDefaultSong(float volume = 1f)
    {
        if (audioSource != null && defaultSong != null)
        {
            audioSource.clip = defaultSong;
            audioSource.time = Random.Range(0f, defaultSong.length);
            audioSource.volume = volume;
            audioSource.loop = true;
            audioSource.Play();
        }
    }
    private void Update()
    {
        // Check if a song has ended and play a new one if DanceCamera is ongoing
        if (audioSource != null && !audioSource.isPlaying && danceRoutine != null)
        {
            PlayRandomSongFromStart(danceFloorMusicVolume);
        }


        // // Check for 's' key press to start dancing
        // if (Input.GetKeyDown(KeyCode.S))
        // {
        //     DanceCameraStart();  // Assuming danceNumber 1 for this example
        // }

        // // Check for 'd' key press to stop dancing
        // if (Input.GetKeyDown(KeyCode.D))
        // {
        //     DanceCameraStop();
        // }
    }
}

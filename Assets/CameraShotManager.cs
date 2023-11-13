using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShotManager : MonoBehaviour
{
    // need this to get the character list
    public SceneDirector sceneDirector;
    public string testString;
    public bool runTestString = false;

    public List<GameObject> allCameras;


    private float lastCameraSwitchTime;

    // Start is called before the first frame update
    void Start()
    {
        lastCameraSwitchTime = -1f; // Initialize with a negative value
    }

    // Update is called once per frame
    void Update()
    {
        if (runTestString)
        {
            ChangeCameraShot(testString);
            runTestString = false;
        }
    }




    public void SetVirtualCamera(GameObject virtualCamera)
    {
        // Check the time difference since the last switch
        if (Time.time - lastCameraSwitchTime < 1f)
        {
            SetAllBrainsBlendMethodToEaseInOut();
        }
        else
        {
            SetAllBrainsBlendMethodToCut();
        }

        // turn off all other vcs
        foreach (CharacterInfo character in sceneDirector.characterList)
        {
            if (character.CloseUpVirtualCamera != null)
            {
                character.CloseUpVirtualCamera.SetActive(false);
                character.LongShotVirtualCamera.SetActive(false);
                character.ThirdPersonVirtualCamera.SetActive(false);
                character.OverTheShoulderVirtualCamera.SetActive(false);
            }

        }
        sceneDirector.currentDimension.virtualCamera.SetActive(false);
        virtualCamera.SetActive(true);
        lastCameraSwitchTime = Time.time;
    }

    // Reset the camera to a good state.
    // For this, we assume {wide shot}
    public void Reset()
    {
        SetVirtualCamera(sceneDirector.currentDimension.virtualCamera);
    }


    // input should be a single line. e.g. {Close Up, Rick}
    public void ChangeCameraShot(string cameraShotDescription)
    {
        Debug.Log(cameraShotDescription);
        if (cameraShotDescription == null)
        {
            return;
        }
        string cameraShotDescriptionLower = cameraShotDescription.ToLower();
        Debug.Log(cameraShotDescriptionLower);

        CharacterInfo character1 = null;
        CharacterInfo character2 = null;

        sceneDirector.GetCharacters(cameraShotDescriptionLower, ref character1, ref character2);

        // if the character is the narrator then we cant really do camera stuff so just fuck off
        if ((character1 != null && character1.name == "narrator") || (character2 != null && character2.name == "narrator"))
        {
            return;
        }

        // ok first lets get the shot type.
        if (cameraShotDescriptionLower.Contains("close up"))
        {
            //Format: {Close up, CharacterName}
            if (character1 != null)
            {
                SetVirtualCamera(character1.CloseUpVirtualCamera);
            }
        }
        else if (cameraShotDescriptionLower.Contains("3rd person shot"))
        {

            //Format: {3rd person shot, CharacterName}
            if (character1 != null)
            {
                SetVirtualCamera(character1.ThirdPersonVirtualCamera);
            }

        }
        else if (cameraShotDescriptionLower.Contains("long shot"))
        {
            Debug.Log("long shot");
            //Format: {Long shot, CharacterName}
            if (character1 != null)
            {
                SetVirtualCamera(character1.LongShotVirtualCamera);
            }


        }
        else if (cameraShotDescriptionLower.Contains("over the shoulder"))
        {
            //Format: {over the shoulder, over CharacterAs shoulder looking at CharacterB}
            if (character1 != null)
            {
                SetVirtualCamera(character1.OverTheShoulderVirtualCamera);

                if (character2 != null)
                {
                    character1.OverTheShoulderVirtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = character2.headTransform;
                }
                else
                {
                    character1.OverTheShoulderVirtualCamera.GetComponent<CinemachineVirtualCamera>().LookAt = null;
                }
            }
        }
        else if (cameraShotDescriptionLower.Contains("wide shot"))
        {
            //Format: {Wide Shot}
            SetVirtualCamera(sceneDirector.currentDimension.virtualCamera);
        }
        else if (cameraShotDescriptionLower.Contains("tracking shot"))
        {
            //Format: {Tracking shot, CharacterName, Infront}  or {Tracking shot, CharacterName, behind}  
            if (character1 != null)
            {
                if (cameraShotDescriptionLower.Contains("infront"))
                {
                    SetVirtualCamera(character1.LongShotVirtualCamera);
                }
                else
                {
                    SetVirtualCamera(character1.ThirdPersonVirtualCamera);
                }
            }
        }

    }

    private void SetAllBrainsBlendMethodToEaseInOut()
    {
        CinemachineBrain[] brains = FindObjectsOfType<CinemachineBrain>();
        foreach (CinemachineBrain brain in brains)
        {
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            brain.m_DefaultBlend.m_Time = 0.5f;

        }
    }

    private void SetAllBrainsBlendMethodToCut()
    {
        CinemachineBrain[] brains = FindObjectsOfType<CinemachineBrain>();
        foreach (CinemachineBrain brain in brains)
        {
            brain.m_DefaultBlend.m_Style = CinemachineBlendDefinition.Style.Cut;
        }
    }
}

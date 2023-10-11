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

    // Start is called before the first frame update
    void Start()
    {

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
}

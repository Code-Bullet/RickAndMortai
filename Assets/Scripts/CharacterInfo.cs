using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores character info
public class CharacterInfo : MonoBehaviour
{
    public string name;
    public string fakeYouUUID;
    public MyCharacterController characterController;
    public DimensionGameObjects dimension;

    public GameObject cameraTarget;
    public bool hasTalkedThisDimension = false;

    public AudioClip defaultSound;

}

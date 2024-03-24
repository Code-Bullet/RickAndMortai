using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores character info
public class CharacterInfo : MonoBehaviour
{
    // The default position of this character in their environment. Used for resetting scenes.
    public Vector3 initialPosition;

    public string name;
    public string fakeYouUUID;
    public MyCharacterController characterController;
    public DimensionGameObjects dimension;

    public GameObject cameraTarget;
    public bool hasTalkedThisDimension = false;

    public AudioClip defaultSound;


    public GameObject CloseUpVirtualCamera;
    public GameObject LongShotVirtualCamera;
    public GameObject ThirdPersonVirtualCamera;
    public GameObject OverTheShoulderVirtualCamera;

    public Transform headTransform;


    // this shit is for ai art stuff
    public Texture2D walkingTexture;
    public Texture2D idleTexture;
    public Texture2D nextSceneWalkingTexture;
    public Texture2D nextSceneIdleTexture;


    public void Awake()
    {
        var p = this.transform.position;
        this.initialPosition = new Vector3(p.x, p.y, p.z);
    }
}

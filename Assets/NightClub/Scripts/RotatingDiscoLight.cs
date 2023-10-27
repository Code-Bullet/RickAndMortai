using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingDiscoLight : MonoBehaviour
{


    [SerializeField] Light blueLight;
    [SerializeField] Light greenLight;
    [SerializeField] Light pinkLight;
    [SerializeField] Light yellowLight;

    [SerializeField] float rotateSpeed = 30f;

    [SerializeField] Boolean blueLightOn;
    [SerializeField] Boolean greenLightOn;
    [SerializeField] Boolean pinkLightOn;
    [SerializeField] Boolean yellowLightOn;

   
    
    void Start()
    {

    }

    void Update()
    {

        // enable or disable the lights
        blueLight.enabled = blueLightOn;
        greenLight.enabled = greenLightOn;
        pinkLight.enabled = pinkLightOn;
        yellowLight.enabled = yellowLightOn;

        // Rotate
        transform.Rotate(0, -1f * rotateSpeed * Time.deltaTime, 0,Space.Self);




    }


}

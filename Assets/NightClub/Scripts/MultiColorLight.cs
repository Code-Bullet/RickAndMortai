using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

public class MultiColorLight : MonoBehaviour
{

    [SerializeField] Light light;

    private float mHue = 0f;

    void Start()
    {
        
    }

    void Update()
    {

        // Set the hue of the lighth color, increment its value.
        mHue += 0.001f;
        if (mHue >= 1f)
        { 
            mHue=  0f;
        }
        
        light.color = Color.HSVToRGB(mHue, 1f, 1f);
             
    }
}

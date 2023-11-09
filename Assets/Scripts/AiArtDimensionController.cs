using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores character info
public class AiArtDimensionController : MonoBehaviour
{
    // public string name;

    public Renderer dimensionBackgroundPlaneRenderer;

    public void Prepare(AIDimension aiDimension)
    {
        if (aiDimension == null) return;

        dimensionBackgroundPlaneRenderer.material.mainTexture = aiDimension.texture;
    }
}

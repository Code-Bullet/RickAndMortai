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
        dimensionBackgroundPlaneRenderer.material.mainTexture = aiDimension.texture;
    }

    public void Reset()
    {
        dimensionBackgroundPlaneRenderer.material.mainTexture = null;
    }
}

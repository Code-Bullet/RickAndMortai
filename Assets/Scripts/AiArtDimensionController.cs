using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stores character info
public class AiArtDimensionController : MonoBehaviour
{
    // public string name;

    // this shit is for ai art stuff
    private Texture2D nextSceneTexture;

    public Renderer dimensionBackgroundPlaneRenderer;



    public void UpdateTextureForNextScene()
    {

        if (nextSceneTexture!= null)
        {
            dimensionBackgroundPlaneRenderer.material.mainTexture = nextSceneTexture;
        }
    }


    public void SetTextureForNextScene(Texture2D nextSceneTexture_)
    {
        nextSceneTexture = nextSceneTexture_;
    }


}

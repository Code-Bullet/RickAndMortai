using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiArtCharacterController : MonoBehaviour
{

    // public Renderer walkingPlaneRenderer;
    // public Renderer walkingPlaneRendererBack;
    // public Renderer idlePlaneRenderer;
    // public Renderer idlePlaneRendererBack;
    public Renderer characterFront;
    public Renderer characterLeft;
    public Renderer characterRight;
    public Renderer characterBack;

    private Texture2D nextSceneTexture;





    public void UpdateTextureForNextScene()
    {

        if (nextSceneTexture != null)
        {
            // this is where we convert a single texture into 2 textures
            Texture2D[] splitTextures = SplitTexture(nextSceneTexture);

            Texture2D rightFaceTexture = splitTextures[0];
            Texture2D frontFaceTexture = splitTextures[1];
            Texture2D leftFaceTexture = FlipTextureHorizontally(rightFaceTexture);


            characterRight.material.mainTexture = rightFaceTexture;
            characterFront.material.mainTexture = frontFaceTexture;
            characterLeft.material.mainTexture = leftFaceTexture;
            characterBack.material.mainTexture = frontFaceTexture;
        }
    }

    public void SetTextureForNextScene(Texture2D nextSceneTexture_)
    {
        nextSceneTexture = nextSceneTexture_;
    }



    // public void SetPlanesToTextures(CharacterInfo character)
    // {

    //     if (character.nextSceneIdleTexture != null)
    //     {
    //         character.idleTexture = character.nextSceneIdleTexture;

    //         idlePlaneRenderer.material.mainTexture = character.idleTexture;
    //         idlePlaneRendererBack.material.mainTexture = character.idleTexture;

    //     }

    //     if (character.nextSceneWalkingTexture != null)
    //     {
    //         // update the current textures
    //         character.walkingTexture = character.nextSceneWalkingTexture;

    //         // set them to the plane renderer
    //         walkingPlaneRenderer.material.mainTexture = character.walkingTexture;
    //         walkingPlaneRendererBack.material.mainTexture = character.walkingTexture;
    //     }

    // }

    private Texture2D[] SplitTexture(Texture2D originalTexture)
    {
        int halfWidth = originalTexture.width / 2;

        if (halfWidth != originalTexture.height)
        {
            Debug.LogError("Texture dimensions are not suitable for splitting in half!");
            return null;
        }

        Texture2D[] splitTextures = new Texture2D[2];

        for (int i = 0; i < 2; i++)
        {
            Texture2D tex = new Texture2D(halfWidth, originalTexture.height);

            Color[] pixels = originalTexture.GetPixels(i * halfWidth, 0, halfWidth, originalTexture.height);
            tex.SetPixels(pixels);
            tex.Apply();

            splitTextures[i] = tex;
        }

        return splitTextures;
    }

    private Texture2D FlipTextureHorizontally(Texture2D originalTexture)
    {
        Texture2D flippedTexture = new Texture2D(originalTexture.width, originalTexture.height);

        for (int i = 0; i < originalTexture.width; i++)
        {
            for (int j = 0; j < originalTexture.height; j++)
            {
                // The pixel is taken from the opposite side, creating a mirrored effect.
                Color pixelColor = originalTexture.GetPixel(originalTexture.width - i - 1, j);
                flippedTexture.SetPixel(i, j, pixelColor);
            }
        }
        flippedTexture.Apply();

        return flippedTexture;
    }



}

using UnityEngine;
using System.Collections;
using Dummiesman;
using System.Collections.Generic;
using System;

public class AiHeadStatic : MonoBehaviour
{
    // The name of the character, which we use to find the 3D model on the file system.
    public string characterKey = "";

    // The ID of the AI generation to use.
    public string generationId = "";

    // The parent object where to place the head.
    public GameObject headParent;

    // The head object loaded at runtime.
    public GameObject head;
    // The shader to use when adding the head object. Can be overridden.
    public Shader headShader;


    // Use this for initialization
    void Start()
    {
        if (characterKey.Length == 0 || generationId.Length == 0) { throw new Exception("character or genreation id is null"); }
        RenderGeneration(characterKey, generationId);
    }

    public void RenderGeneration(string characterKey, string generationId)
    {
        this.generationId = generationId;

        //Approach 1:
        //ObjImporter ImporterMesh = new ObjImporter();
        //Approach 2:
        //var loadedObject : GameObject = Instantiate(Resources.Load("modelName"));/

        // Approach 3:
        GameObject head = new OBJLoader().Load($"3d-headshot-pipeline/headshot/data/3d/{characterKey}/{generationId}_mesh/logs/image.obj");
        head.name = $"{this.characterKey} (${generationId.Substring(0, 5)})";

        // Add texture to head.
        //

        // 1. Load the Material.
        Dictionary<string, Material> materials = new MTLLoader().Load($"3d-headshot-pipeline/headshot/data/3d/{characterKey}/{generationId}_mesh/logs/image.mtl");
        Material defaultMat = materials["defaultMat"];

        // 2. Configure it.
        defaultMat.SetFloat("_Mode", 0); // 0 for Opaque
        // HACK: Force re-render.
        defaultMat.shader = headShader == null ? Shader.Find("Particles/Standard Unlit") : headShader;

        // 3. Set it on the child called "default".
        GameObject defaultChild = head.transform.Find("default").gameObject;
        var renderer = defaultChild.GetComponent<Renderer>();
        renderer.material = defaultMat;

        // Reposition head for rendering.
        //

        // 1. Reset transform.
        head.transform.position = Vector3.zero;
        head.transform.rotation = Quaternion.identity;
        head.transform.localScale = Vector3.one;

        //head.transform.position = Vector3.zero;
        //head.transform.rotation = Quaternion.identity;
        //head.transform.localScale = Vector3.one;

        // 2. Set the default look-and-feel.
        // NOTE: Local transform modifications MUST go BEFORE transform.SetParent.
        head.transform.position = new Vector3(0, 7.05000019f, 1.20000005f);
        head.transform.rotation = new Quaternion(0, 0, 0, 1);
        head.transform.localScale = new Vector3(14.6000004f, 14.7558718f, 14.7558718f);

        // 3. Add as child to parent head object.
        head.transform.SetParent(this.headParent.transform, false);

        if (this.head != null) Destroy(this.head);
        this.head = head;
    }
}

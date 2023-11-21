using System.Collections.Generic;
using UnityEngine;
using Dummiesman;

public class AIHeadRiggerBase : MonoBehaviour
{
    // The name of the character, which we use to find the 3D model on the file system.
    public string characterKey = "";

    // The list of generation ID's we indexed on the file system.
    [SerializeField]
    private string[] generations = new string[] { };

    // The ID of the AI generation to use.
    public string generationId = "<to be loaded>";
    public int selectedGeneration = 0;

    // The parent object where to place the head.
    public GameObject headParent;

    // The head object loaded at runtime.
    public GameObject head;

    // Cycle through generations every 2s.
    public bool cycleAutomatically = false;
    private float timer = 0.0f;
    private float interval = 2.0f;

    void Update()
    {
    }

    public void RenderGeneration(string characterKey, string generationId)
    {
        this.generationId = generationId;

        //Approach 1:
        //ObjImporter ImporterMesh = new ObjImporter();
        //Approach 2:
        //var loadedObject : GameObject = Instantiate(Resources.Load("modelName"));/

        // Approach 3:
        GameObject head = new OBJLoader().Load($"local-image-gen/headshot/data/3d/{characterKey}/{generationId}_mesh/logs/image.obj");
        head.name = $"{this.characterKey} (${generationId.Substring(0, 5)})";

        // Add texture to head.
        //

        // 1. Load the Material.
        Dictionary<string, Material> materials = new MTLLoader().Load($"local-image-gen/headshot/data/3d/{characterKey}/{generationId}_mesh/logs/image.mtl");
        Material defaultMat = materials["defaultMat"];

        // 2. Configure it.
        defaultMat.SetFloat("_Mode", 0); // 0 for Opaque
        // HACK: Force re-render.
        defaultMat.shader = Shader.Find("Standard");

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
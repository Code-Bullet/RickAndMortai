using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//using UnityEditor;

//public class SilhouetteControllerWindow : EditorWindow
//{
//    float sliderValue = 0;
//    string labelText = "-";

//    [MenuItem("Window/Silhouettes")]
//    static void Init()
//    {
//        var example = (SilhouetteControllerWindow)EditorWindow.GetWindow(typeof(SilhouetteControllerWindow));
//        example.Show();
//    }

//    void OnGUI()
//    {
//        EditorGUILayout.LabelField("New value", labelText);

//        // Start a code block to check for GUI changes
//        EditorGUI.BeginChangeCheck();

//        sliderValue = EditorGUILayout.Slider(sliderValue, 0, 1);

//        // End the code block and update the label if a change occurred
//        if (EditorGUI.EndChangeCheck())
//        {
//            labelText = sliderValue.ToString();
//        }
//    }
//}


public class SilhouetteController : MonoBehaviour
{
    public Shader silhouetteShader; // Reference to the silhouette shader
    public Shader standardShader;

    private bool isHiding = false;

    void Start()
    {
        if(standardShader == null)
            standardShader = Shader.Find("Standard");
    }

    public void SetHiding(bool _isHiding)
    {
        isHiding = _isHiding;
        render();
    }

    private void OnTransformChildrenChanged()
    {
        // rerender since we dynamically import the gameobject for the 3d head guy
        render();
    }

    private void render()
    {
        Shader shader;
        if (isHiding)
        {
            shader = silhouetteShader;
        }
        else
        {
            shader = standardShader;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            
            foreach (Material mat in mats)
            {
                mat.shader = shader;
            }
        }
    }

    void Update()
    {
        // Toggle between original materials and silhouette materials on key press (for demonstration purposes)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isHiding = !isHiding;
            SetHiding(isHiding);
        }
    }

}
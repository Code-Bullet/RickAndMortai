using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class CustomizableGridShaderGUI : ShaderGUI
{

    bool _debugMode = false;

    bool _showBase = true;
    bool _showBasePattern = false;
    bool _showCenterGrid  = false;
    bool _showCenterLine  = false;
    bool _showEdge = false;
    bool _showText = false;

    bool _worldPos = false;

    MaterialProperty _BaseColor = null;
    MaterialProperty _TilingX   = null;
    MaterialProperty _TilingY   = null;

    MaterialProperty _GlobalMet = null;
    MaterialProperty _GlobalSmo = null;
    MaterialProperty _GlobalEmi = null;

    MaterialProperty _BasePatternColor = null;
    MaterialProperty _BasePatternTex = null;
    MaterialProperty _BasePatternMet = null;
    MaterialProperty _BasePatternSmo = null;
    MaterialProperty _BasePatternEmi = null;

    MaterialProperty _CenterGridColor = null;
    MaterialProperty _CenterGridTex = null;
    MaterialProperty _CenterGridMet = null;
    MaterialProperty _CenterGridSmo = null;
    MaterialProperty _CenterGridEmi = null;

    MaterialProperty _CenterLineColor = null;
    MaterialProperty _CenterLineTex = null;
    MaterialProperty _CenterLineMet = null;
    MaterialProperty _CenterLineSmo = null;
    MaterialProperty _CenterLineEmi = null;

    MaterialProperty _EdgeColor = null;
    MaterialProperty _EdgeTex = null;
    MaterialProperty _EdgeMet = null;
    MaterialProperty _EdgeSmo = null;
    MaterialProperty _EdgeEmi = null;

    MaterialProperty _TextColor = null;
    MaterialProperty _TextTex = null;
    MaterialProperty _TextMet = null;
    MaterialProperty _TextSmo = null;
    MaterialProperty _TextEmi = null;

    MaterialProperty _WolrdBpattTiling = null;
    MaterialProperty _WolrdCgridTiling = null;
    MaterialProperty _WolrdClineTiling = null;
    MaterialProperty _WolrdEdgeTiling = null;
    MaterialProperty _WolrdTextTiling = null;

    public void FindProperties(MaterialProperty[] properties)
    {
        _BaseColor = FindProperty("_BaseColor", properties);
        _TilingX = FindProperty("_TilingX", properties);
        _TilingY = FindProperty("_TilingY", properties);

        _GlobalMet = FindProperty("_GlobalMet", properties);
        _GlobalSmo = FindProperty("_GlobalSmo", properties);
        _GlobalEmi = FindProperty("_GlobalEmi", properties);

        _BasePatternColor = FindProperty("_BasePatternColor", properties);
        _BasePatternTex = FindProperty("_BasePatternTex", properties);
        _BasePatternMet = FindProperty("_BasePatternMet", properties);
        _BasePatternSmo = FindProperty("_BasePatternSmo", properties);
        _BasePatternEmi = FindProperty("_BasePatternEmi", properties);

        _CenterGridColor = FindProperty("_CenterGridColor", properties);
        _CenterGridTex = FindProperty("_CenterGridTex", properties);
        _CenterGridMet = FindProperty("_CenterGridMet", properties);
        _CenterGridSmo = FindProperty("_CenterGridSmo", properties);
        _CenterGridEmi = FindProperty("_CenterGridEmi", properties);

        _CenterLineColor = FindProperty("_CenterLineColor", properties);
        _CenterLineTex = FindProperty("_CenterLineTex", properties);
        _CenterLineMet = FindProperty("_CenterLineMet", properties);
        _CenterLineSmo = FindProperty("_CenterLineSmo", properties);
        _CenterLineEmi = FindProperty("_CenterLineEmi", properties);

        _EdgeColor = FindProperty("_EdgeColor", properties);
        _EdgeTex = FindProperty("_EdgeTex", properties);
        _EdgeMet = FindProperty("_EdgeMet", properties);
        _EdgeSmo = FindProperty("_EdgeSmo", properties);
        _EdgeEmi = FindProperty("_EdgeEmi", properties);

        _TextColor = FindProperty("_TextColor", properties);
        _TextTex = FindProperty("_TextTex", properties);
        _TextMet = FindProperty("_TextMet", properties);
        _TextSmo = FindProperty("_TextSmo", properties);
        _TextEmi = FindProperty("_TextEmi", properties);

        _WolrdBpattTiling = FindProperty("_WolrdBpattTiling", properties);
        _WolrdCgridTiling = FindProperty("_WolrdCgridTiling", properties);
        _WolrdClineTiling = FindProperty("_WolrdClineTiling", properties);
        _WolrdEdgeTiling = FindProperty("_WolrdEdgeTiling", properties);
        _WolrdTextTiling = FindProperty("_WolrdTextTiling", properties);

    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {

        FindProperties(properties); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
        Material material = materialEditor.target as Material;
        string[] keyWords = material.shaderKeywords;

        EditorGUI.BeginChangeCheck();        

        // Render default properties
        if (_debugMode)
        {
            EditorGUILayout.BeginVertical("Box");
            base.OnGUI(materialEditor, properties);
            EditorGUILayout.EndVertical();
        }
        else
        {
            //Button collapse-expand
            if (GUILayout.Button("Collapse-Expand"))
            {
                _showBasePattern = _showCenterGrid = _showCenterLine = _showEdge = _showText = (_showBase = !_showBase);
            }

            EditorGUILayout.Separator();

            //World position
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("World Position UV's");
            EditorGUILayout.Separator();

            _worldPos = keyWords.Contains("WPOS_ON");
            _worldPos = EditorGUILayout.Toggle("Enable", _worldPos);

            if (EditorGUI.EndChangeCheck())
            {
                List<string> keywords = new()
                { _worldPos ? "WPOS_ON" : "WPOS_OFF" };
                material.shaderKeywords = keywords.ToArray();
                EditorUtility.SetDirty(material);
            }

             if (_worldPos)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.LabelField("XY Tiling     -      ZW Offset");
                materialEditor.VectorProperty(_WolrdBpattTiling, "Base Pattern");
                materialEditor.VectorProperty(_WolrdCgridTiling, "Center Grid");
                materialEditor.VectorProperty(_WolrdClineTiling, "Center Line");
                materialEditor.VectorProperty(_WolrdEdgeTiling, "Edge");
                materialEditor.VectorProperty(_WolrdTextTiling, "Text");
            }
                       
            EditorGUILayout.EndVertical();

            //BASE
            EditorGUILayout.BeginVertical("Box");
            _showBase = EditorGUILayout.Foldout(_showBase, "Base properties");

            if (_showBase)
            {
                EditorGUILayout.Separator();
                materialEditor.ColorProperty(_BaseColor, "Base Color");
                EditorGUILayout.Separator();

                materialEditor.FloatProperty(_TilingX, "Tile X");
                materialEditor.FloatProperty(_TilingY, "Tile Y");
                EditorGUILayout.Separator();

               

                EditorGUILayout.Separator();
                materialEditor.RangeProperty(_GlobalMet, "Global Metallic Mult");
                materialEditor.RangeProperty(_GlobalSmo, "Global Smoothness Mult");
                materialEditor.RangeProperty(_GlobalEmi, "Global Emission Mult");

                EditorGUILayout.Separator();

            }

            EditorGUILayout.EndVertical();

            //BASE PATTERN
            EditorGUILayout.BeginVertical("Box");
            _showBasePattern = EditorGUILayout.Foldout(_showBasePattern, "Base Pattern properties");

            if (_showBasePattern)
            {
                EditorGUILayout.Separator();

                materialEditor.TextureProperty(_BasePatternTex,"Texture", !_worldPos);
                materialEditor.ColorProperty(_BasePatternColor, "Color");
                EditorGUILayout.Separator();

                materialEditor.RangeProperty(_BasePatternMet, "Metallic");
                materialEditor.RangeProperty(_BasePatternSmo, "Smoothness");
                materialEditor.RangeProperty(_BasePatternEmi, "Emission");
                EditorGUILayout.Separator();

            }

            EditorGUILayout.EndVertical();

            //CENTER GRID
            EditorGUILayout.BeginVertical("Box");
            _showCenterGrid = EditorGUILayout.Foldout(_showCenterGrid, "Center Grid properties");

            if (_showCenterGrid)
            {
                EditorGUILayout.Separator();

                materialEditor.TextureProperty(_CenterGridTex, "Texture", !_worldPos);
                materialEditor.ColorProperty(_CenterGridColor, "Color");
                EditorGUILayout.Separator();

                materialEditor.RangeProperty(_CenterGridMet, "Metallic");
                materialEditor.RangeProperty(_CenterGridSmo, "Smoothness");
                materialEditor.RangeProperty(_CenterGridEmi, "Emission");
                EditorGUILayout.Separator();

            }

            EditorGUILayout.EndVertical();

            //CENTER LINE
            EditorGUILayout.BeginVertical("Box");
            _showCenterLine = EditorGUILayout.Foldout(_showCenterLine, "Center Line properties");

            if (_showCenterLine)
            {
                EditorGUILayout.Separator();

                materialEditor.TextureProperty(_CenterLineTex, "Texture", !_worldPos);
                materialEditor.ColorProperty(_CenterLineColor, "Color");
                EditorGUILayout.Separator();

                materialEditor.RangeProperty(_CenterLineMet, "Metallic");
                materialEditor.RangeProperty(_CenterLineSmo, "Smoothness");
                materialEditor.RangeProperty(_CenterLineEmi, "Emission");
                EditorGUILayout.Separator();

            }

            EditorGUILayout.EndVertical();

            //EDGE
            EditorGUILayout.BeginVertical("Box");
            _showEdge = EditorGUILayout.Foldout(_showEdge, "Edge properties");

            if (_showEdge)
            {
                EditorGUILayout.Separator();

                materialEditor.TextureProperty(_EdgeTex, "Texture", !_worldPos);
                materialEditor.ColorProperty(_EdgeColor, "Color");
                EditorGUILayout.Separator();

                materialEditor.RangeProperty(_EdgeMet, "Metallic");
                materialEditor.RangeProperty(_EdgeSmo, "Smoothness");
                materialEditor.RangeProperty(_EdgeEmi, "Emission");
                EditorGUILayout.Separator();

            }

            EditorGUILayout.EndVertical();

            //EDGE
            EditorGUILayout.BeginVertical("Box");
            _showText = EditorGUILayout.Foldout(_showText, "Text properties");

            if (_showText)
            {
                EditorGUILayout.Separator();

                materialEditor.TextureProperty(_TextTex, "Texture", !_worldPos);
                materialEditor.ColorProperty(_TextColor, "Color");
                EditorGUILayout.Separator();

                materialEditor.RangeProperty(_TextMet, "Metallic");
                materialEditor.RangeProperty(_TextSmo, "Smoothness");
                materialEditor.RangeProperty(_TextEmi, "Emission");
                EditorGUILayout.Separator();

            }

            EditorGUILayout.EndVertical();

        }// End else

        EditorGUILayout.BeginVertical("Box");
        _debugMode = EditorGUILayout.Toggle("Debug Mode", _debugMode);
        EditorGUILayout.EndVertical();
       
    }
}


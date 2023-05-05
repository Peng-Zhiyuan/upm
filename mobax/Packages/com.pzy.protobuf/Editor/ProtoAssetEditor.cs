using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


[CustomEditor(typeof(ProtoAsset))]
public class ProtoAssetEditor : Editor
{
    private ProtoAsset asset;
    private void OnEnable()
    {
        asset = target as ProtoAsset;
    }

    public override void OnInspectorGUI()
    {
        GUI.enabled = true;

        // 编译按钮
        if(GUILayout.Button("Compile To Csharp"))
        {
            var filePath = AssetDatabase.GetAssetPath(asset);
            ProtoCompiler.CompileToCsharp(filePath);
        }
        if (GUILayout.Button("Compile All Proto To Js"))
        {
            ProtoCompiler.CompileAllProtoFileToJs();
        }

        var path = AssetDatabase.GetAssetPath(asset);
        var text = File.ReadAllText(path);


    var MaxTextPreviewLength = 4096;
        if (text.Length > MaxTextPreviewLength + 3)
        {
            text = text.Substring(0, MaxTextPreviewLength) + "...";
        }

        GUIStyle style = "ScriptText";
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), style);
        rect.x = 0f;
        rect.y -= 3f;
        rect.width = EditorGUIUtility.currentViewWidth + 1f;
        GUI.Box(rect, text, style);
    }
}

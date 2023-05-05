using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;

[ScriptedImporter(2, new[] { "proto" })]
public class ProtoImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var path = ctx.assetPath;
        Build(path);

        var icon = LoadIcon();
        //Debug.Log("icon: " + icon);
        var asset = ProtoAsset.CreateInstance<ProtoAsset>();
        ctx.AddObjectToAsset("main obj", asset, icon);
        ctx.SetMainObject(asset);
    }

    private Texture2D LoadIcon()
    {
        var asset = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.pzy.protobuf/icon.png");
        return asset;
    }

    public void Build(string protoFilePath)
    {
        var protoParentDir = Path.GetDirectoryName(protoFilePath);
        var fileName = Path.GetFileName(protoFilePath);
        var currentDir = System.Environment.CurrentDirectory;
        var protogenPath = $"{currentDir}/Packages/com.pzy.protobuf/protogen~/protogen.exe";
        var genCsharpDir = $"{currentDir}/Assets/ProtoGen";
        ExecUtil.Run(protogenPath, $"--csharp_out={genCsharpDir} {fileName}", false, protoParentDir);

        var csFileName = Path.ChangeExtension(fileName, ".cs");
        Debug.Log("csFile: " + csFileName);
        //var genFilePath = $"{genCsharpDir}/{csFileName}";

        //CSharpCodeUtil.AddNamespaceToFile(genFilePath, "ProtoBuf");

        AssetDatabase.Refresh();
    }
}


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

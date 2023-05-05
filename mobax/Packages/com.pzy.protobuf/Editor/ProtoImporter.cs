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
        //var path = ctx.assetPath;
        //ProtoCompiler.CompileToCsharp(path);

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

}


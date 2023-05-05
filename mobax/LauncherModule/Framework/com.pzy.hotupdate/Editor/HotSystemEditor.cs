using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

using HybridCLR.Editor.Commands;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using CustomLitJson;
public static class HotSystemEditor 
{
    public static void Build(Dictionary<string, string> buildInfo = null)
    {
        WriteBuildInfo(buildInfo);

        var useHclr = HclrEditorUtil.IsHclrEnabled;

        if(useHclr)
        {
            Debug.Log("compile hotfix dll to server data files");
            GenerateHotfixDllToGroup();

            // gen unity auto init list
            Debug.Log("generate hotfix dll auto init class list");
            UnityAutoInitEditor.GenerateHotAssemblyAutoInitList();
        }

        // reset all address
        Debug.Log("reset Address");
        AddressableExt.ResetAddress(false);

        // set address all work in local
        Debug.Log("Addressable set all in local");
        AddressableEditorUtil.NormalizeAllGroup();

        // 构建资源
        Debug.Log("build addressable Content");
        AddressableEditorUtil.RebuildContent("BuildScriptWwisePacked");
    }

    public static void WriteBuildInfo(Dictionary<string, string> buildInfo)
    {
        if(buildInfo == null)
        {
            buildInfo = new Dictionary<string, string>();
        }
        var path = $"Assets/$hotSystem/hot-build-info.json";
        var json = JsonMapper.Instance.ToJson(buildInfo);
        json = JsonUtil.Buitify(json);
        var d = Path.GetDirectoryName(path);
        if (!Directory.Exists(d))
        {
            Directory.CreateDirectory(d);
        }
        File.WriteAllText(path, json);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 编译，然后复制到约定的 Group 中。不压缩，也不产生 hash 文件
    /// </summary>
    public static void GenerateHotfixDllToGroup()
    {
        // hclr build hot asembly
        CompileDllCommand.CompileDllActiveBuildTarget();

        // copy hot assmbly to server bundle dir
        var groupDir = $"Assets/$assembly";
        HclrEditorUtil.CopyHotfixDll(EditorUserBuildSettings.activeBuildTarget, groupDir, false, ".bytes", false);
    }

    static void CompileHotfixDllToServerData()
    {
        // hclr build hot asembly
        CompileDllCommand.CompileDllActiveBuildTarget();

        // copy hot assmbly to server bundle dir
        var serverDataDir = $"ServerData/{EditorUserBuildSettings.activeBuildTarget}";
        var dllList = HclrEditorUtil.CopyHotfixDll(EditorUserBuildSettings.activeBuildTarget, serverDataDir);
        foreach (var dll in dllList)
        {
            var hash = HashUtil.MD5File(dll);
            var filePath = Path.ChangeExtension(dll, ".hash");
            File.WriteAllText(filePath, hash);
        }
    }
}

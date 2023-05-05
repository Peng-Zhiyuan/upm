using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

using HybridCLR.Editor.Commands;

[BuildSchema(BuildTarget.NoTarget, "构建主程序集，然后上传到远端作为测试")]
public class BuildAndUpdateOverrideAssembly : BuildSchema
{
    public override async void Build(Dictionary<string, string> paramDic)
    {
        HotSystemEditor.GenerateHotfixDllToGroup();
        var buildTargetPlatform = EditorUserBuildSettings.activeBuildTarget;
        
        var from = Application.dataPath + "/$assembly/a.bytes";
        var to = $"/Library/WebServer/Documents/res-mobax-develop/main/Addressable/{buildTargetPlatform}/a.bytes";
        SftpUtil.UploadAsync("10.26.17.93", 22, "IGG", "igg123", from, to);
    }

}

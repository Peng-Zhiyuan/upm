using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

using HybridCLR.Editor.Commands;

[BuildSchema(BuildTarget.NoTarget, "完整构建")]
[Param("useHclr", "true")]
[Param("roleTest", "false")]
[Param("gitCommit", "")]
[Param("build", "0")]
[Param("variant", "dev")]
[Param("embed", "all")]
[Param("unityDev", "false")]
[Param("connectWithProfiler", "false")]

public class BuildFull : BuildSchema
{
    public override void Build(Dictionary<string, string> paramDic)
    {
        bool.TryParse(paramDic["useHclr"], out var useHclr);
        bool.TryParse(paramDic["roleTest"], out var roleTest);
        var embed = paramDic["embed"].ToString();
        bool.TryParse(paramDic["unityDev"], out var unityDev);
        bool.TryParse(paramDic["connectWithProfiler"], out var connectWithProfiler);

        DefineUtil.AddDefine("BuildFull");

        if (useHclr)
        {
            HclrEditorUtil.IsHclrEnabled = true;
        }
        else
        {
            HclrEditorUtil.IsHclrEnabled = false;
        }

        if (roleTest)
        {
            RefreshEditorBuildSetting.EnableRoleRoomOnly();
            RoleEditTool.GenerateModelList();
        }
        else 
        {
            RefreshEditorBuildSetting.EnableRootOnly();
        }
        HotSystemEditor.Build(paramDic);
        LauncherEditor.Build(embed, unityDev, connectWithProfiler, paramDic);

        DefineUtil.RemoveDefine("BuildFull");
    }

}

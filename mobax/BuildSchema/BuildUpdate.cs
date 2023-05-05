using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

[BuildSchema(BuildTarget.NoTarget, "资源更新构建")]
[Param("useHclr", "true")]
[Param("gitCommit", "")]
[Param("build", "0")]
public class BuildUpdate : BuildSchema
{

    public override void Build(Dictionary<string, string> paramDic)
    {
        DefineUtil.AddDefine("BuildUpdate");
        bool.TryParse(paramDic["useHclr"], out var useHclr);
        if (useHclr)
        {
            HclrEditorUtil.IsHclrEnabled = true;
        }
        else
        {
            HclrEditorUtil.IsHclrEnabled = false;
        }

        HotSystemEditor.Build(paramDic);
        DefineUtil.AddDefine("BuildUpdate");

    }

}

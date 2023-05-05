using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class DefineUtil 
{
    public static void AddDefine(string define)
    {
        InternalAddDefine(define, BuildTargetGroup.Android);
        InternalAddDefine(define, BuildTargetGroup.iOS);
        InternalAddDefine(define, BuildTargetGroup.Standalone);
    }

    static void InternalAddDefine(string define, BuildTargetGroup buildTargetGroup)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if(defines.Contains(define))
        {
            return;
        }
        defines += $";{define}";
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
    }

    public static void RemoveDefine(string define)
    {
        InternalRemoveDefine(define, BuildTargetGroup.Android);
        InternalRemoveDefine(define, BuildTargetGroup.iOS);
        InternalRemoveDefine(define, BuildTargetGroup.Standalone);
    }

    public static void InternalRemoveDefine(string define, BuildTargetGroup buildTargetGroup)
    {
        string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        defines = defines.Replace($"{define};", "");
        defines = defines.Replace(define, "");
        PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
    }
}

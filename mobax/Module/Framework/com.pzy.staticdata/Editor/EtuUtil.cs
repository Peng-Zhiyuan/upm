using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public static class EtuUtil 
{
    public static void Run(string excelDirPath, bool cleanCache, Dictionary<string, string> extraInfo)
    {

        var currentDir = System.Environment.CurrentDirectory;
        var toolPath = $"{currentDir}/Packages/com.pzy.staticdata/etu~/etucli.exe";
        var argSb = new StringBuilder();
        argSb.Append($"{excelDirPath} {cleanCache} ");
        foreach(var kv in extraInfo)
        {
            var key = kv.Key;
            var value = kv.Value;
            argSb.Append("--");
            argSb.Append(key);
            argSb.Append(" ");
            argSb.Append(value);
            argSb.Append(" ");
        }
        var arg = argSb.ToString();
        var exitCode = ExecUtil.Run(toolPath, arg, false);
        if(exitCode != 0)
        {
            throw new Exception("[EtuUtil] exception in etu, return code: " + exitCode);
        }

    }

}

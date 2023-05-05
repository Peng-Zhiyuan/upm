using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;
using System.Text.RegularExpressions;

public static class InjectLog
{
    /// <summary>
    /// 搜索 Assets 下所有 cs 文件，删除所有 Editor 宏中的内容
    /// </summary>
    [MenuItem("pzy.com.*/InjectLog/Inject")]
    public static void DeleteEditorDeinfesInAllCode()
    {
        var assetPath = Application.dataPath;
        var pathList = Directory.GetFiles(assetPath, "*.cs", SearchOption.AllDirectories);

        foreach (var path in pathList)
        {
            var code = File.ReadAllText(path);
            var fileName = Path.GetFileName(path);
            if(fileName == "InjectLog.cs")
            {
                continue;
            }
            var newCode = ProcessCodeOfOneFile(code, fileName);
            File.WriteAllText(path, newCode);
        }
        AssetDatabase.Refresh();
    }

    public static string ProcessCodeOfOneFile(string code, string fileName)
    {
        var patter = @"\n(?!.* new )(?!.* switch)(?!.* class)(?!.* if\()[a-zA-Z0-9 ]* ([a-zA-Z_0-9]+)\(.*\)\r?\n[ ]*\{";
        var replecement = $"$0\nUnityEngine.Debug.Log(\"{fileName}: $1\");\n";
        var newCode = Regex.Replace(code, patter, replecement);
        return newCode;
    }
}



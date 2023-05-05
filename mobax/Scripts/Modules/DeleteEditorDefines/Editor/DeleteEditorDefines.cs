using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;

public static class DeleteEditorDefines 
{
    /// <summary>
    /// 搜索 Assets 下所有 cs 文件，删除所有 Editor 宏中的内容
    /// </summary>
    [MenuItem("pzy.com.*/DeleteEditorDeinfesInAllCode/Delete")]
    public static void DeleteEditorDeinfesInAllCode()
    {
        var assetPath = Application.dataPath;
        var pathList = Directory.GetFiles(assetPath, "*.cs", SearchOption.AllDirectories);
        
        foreach(var path in pathList)
        {
            var code = File.ReadAllText(path);
            var (changed, newCode) = ProcessCodeOfOneFile(code);
            if(changed)
            {
                File.WriteAllText(path, newCode);
            }
        }
        AssetDatabase.Refresh();
    }

    static (bool, string) ProcessCodeOfOneFile(string code)
    {
        var lineList = code.Split('\n');
        var inDefinesFlag = false;
        var changed = false;
        var deepth = 0;
        var inElse = false;
        for(int i = 0; i < lineList.Length; i++)
        {
            var line = lineList[i];
            var trimed = line.Trim();
            if(trimed == "#if UNITY_EDITOR")
            {
                changed = true;
                inDefinesFlag = true;
                deepth++;
                inElse = false;
                lineList[i] = "//" + line;
                continue;
            }

            if(inDefinesFlag)
            {
                if(deepth == 1)
                {
                    if (trimed.StartsWith("#else"))
                    {
                        inElse = true;
                        lineList[i] = "//" + line;
                        continue;
                    }
                }


                if(trimed.StartsWith("#if") || trimed.StartsWith("# if"))
                {
                    deepth++;
                }

                if (trimed.StartsWith("#endif") || trimed.StartsWith("# endif"))
                {
                    deepth--;
                    if (deepth == 0)
                    {
                        inDefinesFlag = false;
                        lineList[i] = "//" + line;
                        continue;
                    }

                }

                if(deepth >= 1 && !inElse)
                {
                    lineList[i] = "//" + line;
                }

             }


        }

        if(changed)
        {
            var newCode = string.Join("\n", lineList);
            return (true, newCode);
        }
        else
        {
            return (false, null);
        }
    }
}

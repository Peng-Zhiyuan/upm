using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class LocalTools 
{
    public static string RequireTool(string toolName, string confirmMsg)
    {
        var isValidate = IsSavedPathValidate(toolName);
        if (!isValidate)
        {
            EditorUtility.DisplayDialog($"need {toolName}", confirmMsg, "ok");
            var path = EditorUtility.OpenFilePanel($"select command {toolName}", "", "");
            if(string.IsNullOrEmpty(path))
            {
                throw new Exception($"[LocalTools] user cancel select {toolName}");
            }
            Save(toolName, path);
            return path;
        }
        else
        {
            var path = GetSavedPath(toolName);
            return path;
        }
    }


    static string GetSavedPath(string toolName)
    {
        var rootDir = Application.dataPath + "/../LocalTools";
        var toolFile = $"{rootDir}/{toolName}.txt";
        var path = File.ReadAllText(toolFile);
        return path;
    }

    static void Save(string tool, string path)
    {
        var rootDir = Application.dataPath + "/../LocalTools";
        var toolFile = $"{rootDir}/{tool}.txt";
        CreateParentIfNeed(toolFile);
        File.WriteAllText(toolFile, path);
    }

    static void CreateParentIfNeed(string path)
    {
        var dir = Path.GetDirectoryName(path);
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    static bool IsSavedPathValidate(string toolName)
    {
        var rootDir = Application.dataPath + "/../LocalTools";
        var toolFile = $"{rootDir}/{toolName}.txt";

        var isExists = File.Exists(toolFile);
        if(!isExists)
        {
            return false;
        }

        var path = File.ReadAllText(toolFile);
        var toolExists = File.Exists(path);
        if(!toolExists)
        {
            return false;
        }

        return true;
    }


}

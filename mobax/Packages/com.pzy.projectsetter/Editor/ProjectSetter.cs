using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEditorInternal;
using UnityEngineInternal;
using UnityEngine.Internal;
using CustomLitJson;

public static class ProjectSetter
{

    public static string _lastSetType;
    public static void Set(string type)
    {
        Debug.Log("[ProjectSetter] apply: " + type);
        SetIcon(type);
        SetSplash(type);
        _lastSetType = type;
        SetJavaFiles(type);
        SetAssemblyDefination(type);
        RenameFile(type);
        Debug.Log("[ProjectSetter] complete");
    }

    public const string settingsRoot = "Assets/ProjectSetterSettings";

    static void RenameFile(string type)
    {
        var root = $"{settingsRoot}/{type}";
        var filePath = $"{root}/Rename.json";
        var exits = File.Exists(filePath);
        if (!exits)
        {
            return;
        }
        var json = File.ReadAllText(filePath);
        var renameDic = JsonMapper.Instance.ToObject<Dictionary<string, string>>(json);
        foreach(var kv in renameDic)
        {
            var fromPath = kv.Key;
            var toPath = kv.Value;
            var fromExsits = Directory.Exists(fromPath);
            if(fromExsits)
            {
                var targetExtist = Directory.Exists(toPath);
                if(targetExtist)
                {
                    Directory.Delete(toPath, true);
                }
                var directoryInfo = new DirectoryInfo(fromPath);
                directoryInfo.MoveTo(toPath);
                Debug.Log($"rename dir: {fromPath} -> {toPath}");
            }
        }
    }

    static void SetAssemblyDefination(string type)
    {
        var root = $"{settingsRoot}/{type}";
        var filePath = $"{root}/AssemblyDefination.json";
        var exits = File.Exists(filePath);
        if(!exits)
        {
            return;
        }
        var json = File.ReadAllText(filePath);
        var dic = JsonMapper.Instance.ToObject<Dictionary<string, Dictionary<string, JsonData>>>(json);
        foreach(var kv in dic)
        {
            var fileName = kv.Key;
            var overrideDic = kv.Value;
            OverrideAsseblyDefination(fileName, overrideDic);
        }
        AssetDatabase.Refresh();
    }

    private static void SetIcon(string type)
    {
        var root = $"{settingsRoot}/{type}";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(root + "/icon.png");
        PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] {texture});   
    }

    private static void SetSplash(string type)
    {
        var root = $"{settingsRoot}/{type}";
        var splashList = new List<PlayerSettings.SplashScreenLogo>();
        var index = 0;
        while(true)
        {
            var path = $"{root}/logo{index}.png";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(sprite == null)
            {
                break;
            }
            splashList.Add(PlayerSettings.SplashScreenLogo.Create(2, sprite));
            index++;
        }
        var splashArray = splashList.ToArray();
        PlayerSettings.SplashScreen.logos = splashArray;
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }

    private static void SetJavaFiles(string type)
    {
        if(type == "bilibili")
        {
            var guidList = AssetDatabase.FindAssets("WXProxy");
            foreach(var one in guidList)
            {
                var path = AssetDatabase.GUIDToAssetPath(one);
                var fileName = Path.GetFileName(path);
                if(fileName == "WXProxy.java")
                {
                    Debug.Log(path);
                    var importer = AssetImporter.GetAtPath(path) as PluginImporter;
                    importer.SetCompatibleWithPlatform(BuildTarget.Android, false);
                    importer.SaveAndReimport();
                    //AssetDatabase.ImportAsset(path);
                }
            }
        }
    }

    public static void OverrideAsseblyDefination(string assetName, Dictionary<string, JsonData> overideDic)
    {
        Debug.Log("set assembly: " + assetName);
        var guidList = AssetDatabase.FindAssets(assetName);

        var guid = guidList[0];
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var json = File.ReadAllText(path);
        var jd = JsonMapper.Instance.ToObject(json);
        foreach(var kv in overideDic)
        {
            var key = kv.Key;
            var overrideJd = kv.Value;
            jd[key] = overrideJd;
        }
        var postJson = JsonMapper.Instance.ToJson(jd);
        File.WriteAllText(path, postJson);

    }


}
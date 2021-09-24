using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Linq;
using UnityEditor.Callbacks;


public class BundleSetter
{
    //private const string rootPath = "Assets/Game";


    [MenuItem("BundleSetter/Aoto Reset Bundle")]
    public static void RefreshBundle()
    {
        Debug.Log("Refresh Bundle");
        var bundleDirList = GetAllBundleDir();
        RemoveAllBundle(bundleDirList);
        Deal(bundleDirList);
        Debug.Log($"{bundleDirList.Count} bundles created");
    }

    private static void Deal(List<string> bundleDirList)
    {
        foreach (var dir in bundleDirList)
        {
            var bundleName = Path.GetFileName(dir);
            AssetImporter importer = AssetImporter.GetAtPath(dir);
            importer.assetBundleName = bundleName.ToLower();
            Debug.Log($"add bundle {dir} -> {bundleName}");
        }
    }

    private static void RemoveAllBundle(List<string> bundleDirList)
    {
        foreach (var dir in bundleDirList)
        {
            AssetImporter importer = AssetImporter.GetAtPath(dir);
            importer.assetBundleName = null;
        }


        var bundleNameList = AssetDatabase.GetAllAssetBundleNames();
        foreach (var bundleName in bundleNameList)
        {
            var assetList = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            foreach (var asset in assetList)
            {
                var extension = Path.GetExtension(asset);
                if (extension == ".cs")
                {
                    continue;
                }
                var importer = AssetImporter.GetAtPath(asset);
                importer.assetBundleName = null;
            }
        }
    }

    private static List<string> GetAllBundleDir()
    {
        //var allBundleGUID = AssetDatabase.FindAssets("$", new string[] { rootPath });
        var allBundleGUID = AssetDatabase.FindAssets("$");
        var ret = new List<string>();
        foreach (var guid in allBundleGUID)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Directory.Exists(path))
            {
                var dir = Path.GetFileName(path);
                if (dir.StartsWith("$"))
                {
                    ret.Add(path);
                }
            }
        }
        return ret;
    }
}

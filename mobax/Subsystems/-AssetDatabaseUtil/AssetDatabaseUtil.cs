using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class AssetDatabaseUtil 
{
    public static List<T> LoadAllAssetInFolder<T>(string path) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        var ret = new List<T>();
        string[] fileEntries = Directory.GetFiles(path, "*", SearchOption.AllDirectories);

        foreach (string fileName in fileEntries)
        {
            int assetPathIndex = fileName.IndexOf("Assets");
            string localPath = fileName.Substring(assetPathIndex);

            var asset = AssetDatabase.LoadAssetAtPath<T>(localPath);
            if(asset != null)
            {
                ret.Add(asset);
            }
        }
     
        return ret;
#else
        throw new Exception("[AssetDatabaseUtil] noly accessable in editor");
#endif

    }

    public static T FindThenLoadAsset<T>(string filter) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        var guidList = AssetDatabase.FindAssets(filter);
        if(guidList.Length == 0)
        {
            throw new Exception("[AssetDatabaseUtil] filter " + filter + " not found any asset");
        }
        if(guidList.Length > 1)
        {
            throw new Exception("[AssetDatabaseUtil] more than one asset found at filter " + filter );
        }
        var guid = guidList[0];
        var path = AssetDatabase.GUIDToAssetPath(guid);
        var assset = AssetDatabase.LoadAssetAtPath<T>(path);
        return assset;
#else
        throw new Exception("[AssetDatabaseUtil] noly accessable in editor");
#endif
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.Experimental.SceneManagement;
using System.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using System.Text;
using CustomLitJson;
using Object = UnityEngine.Object;
using System.Timers;
using Stopwatch = System.Diagnostics.Stopwatch;

public static class AddressableEditorLoader 
{
 
    /// <summary>
    /// 按照 address 加载一个资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <returns></returns>
    public static T Load<T>(string address) where T : Object
    {
        var assetList = Serach<T>(address);
        if(assetList.Count == 0)
        {
            return null;
        }
        else
        {
            var first = assetList[0];
            return first;
        }
    }

    /// <summary>
    /// 加载相同 address 的所有资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <returns></returns>
    public static List<T> LoadAll<T>(string address) where T : Object
    {
        var ret = Serach<T>(address);
        return ret;
    }

    /// <summary>
    /// 加载一个组内的所有资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="groupName"></param>
    /// <returns></returns>
    public static List<T> LoadByGroup<T>(string groupName) where T : Object
    {
        if(!groupName.StartsWith("$"))
        {
            Debug.LogError("[AddressableEditorLoader] groupName should start with '$'");
            return new List<T>();
        }
        var folderList = SearchFolder(groupName);
        var assetList = Serach<T>("", folderList);
        return assetList;
    }



    static List<string> SearchFolder(string folderName)
    {
        var seatchText = $"{folderName} t:folder";
        var guidList = AssetDatabase.FindAssets(seatchText);

        var ret = new List<string>();
        foreach (var guid in guidList)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            ret.Add(path);
        }
        return ret;
    }


    static List<T> Serach<T>(string address, List<string> folderList = null) where T : Object
    {
        var assetName = Path.GetFileNameWithoutExtension(address);
        var type = typeof(T);
        var typeName = type.Name;
        var seatchText = $"{assetName} t:{typeName}";

        string[] guidList;
        if(folderList != null)
        {
            guidList = AssetDatabase.FindAssets(seatchText, folderList.ToArray());
        }
        else
        {
            guidList = AssetDatabase.FindAssets(seatchText);
        }

        var ret = new List<T>();
        foreach (var guid in guidList)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if(!string.IsNullOrEmpty(address))
            {
                var thisAssetName = Path.GetFileNameWithoutExtension(path);
                if (thisAssetName != assetName)
                {
                    continue;
                }
            }
 
            var (isValid, _, _) = AddressableExt.IsValidAddressableAssetPath(path);
            if(isValid)
            {
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                ret.Add(asset);
            }
        }
        return ret;
    }


}


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



public static class AddressableEditorUtil 
{
    // 移除所有标签
    public static void RemoveAllLabel(AddressableAssetSettings settings)
    {
        var labelList = settings.GetLabels();
        var oldLabels = new List<string>(labelList);
        foreach (var label in oldLabels)
        {
            settings.RemoveLabel(label);
        }
    }

    // 移除所有 addressable asset 项目
    public static void RemoveAllAddressableAsset(AddressableAssetSettings settings)
    {
        var groups = new List<AddressableAssetGroup>(settings.groups);
        foreach (var g in groups)
        {
            if (g == null)
            {
                settings.RemoveGroup(g);
                continue;
            }
            // addressableSettings.RemoveGroup(g);
            List<AddressableAssetEntry> entrys = new List<AddressableAssetEntry>();
            foreach (var e in g.entries)
            {
                entrys.Add(e);
            }
            foreach (var e in entrys)
            {
                g.RemoveAssetEntry(e);
            }
        }
    }

    static void RemoveAllGroup(AddressableAssetSettings settings)
    {
        var groupList = new List<AddressableAssetGroup>(settings.groups);
        foreach (var g in groupList)
        {
            settings.RemoveGroup(g);
        }
    }

    public static void RemoveAllAddressableInfo(AddressableAssetSettings settings)
    {
        RemoveAllAddressableAsset(settings);
        RemoveAllLabel(settings);
        RemoveAllGroup(settings);
    }

    // 移除所有空的 group
    public static void RemoveAllEmptyGroups(AddressableAssetSettings settings)
    {
        var groupList = new List<AddressableAssetGroup>(settings.groups);
        foreach (var g in groupList)
        {
            if (g.entries.Count == 0)
            {
                settings.RemoveGroup(g);
            }
        }
    }

    public static void SetCatlogAndAllGroupLoacal()
    {
        var settings = AddressableExt.AddressableSettings;
        settings.BuildRemoteCatalog = false;
        var groupList = settings.groups;
        foreach(var group in groupList)
        {
            var schema = group.GetSchema<BundledAssetGroupSchema>();
            schema.BuildPath.SetVariableByName(settings, "LocalBuildPath");
            schema.LoadPath.SetVariableByName(settings, "LocalLoadPath"); 
        }
    }


}

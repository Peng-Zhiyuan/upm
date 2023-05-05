
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

public static class AddressableExtMenu 
{
    static List<string> GetAllValidAssetPath()
    {
        List<string> avaliablePaths = new List<string>();

        var pathList = AssetDatabase.GetAllAssetPaths();
        foreach (var path in pathList)
        {
            var (isValid, _, _) = AddressableExt.IsValidAddressableAssetPath(path);
            if (isValid)
            {
                avaliablePaths.Add(path);
            }
        }
        return avaliablePaths;
    }

    [MenuItem("AddressableExt/Auto Reset Assets")]
    static void RefreshAddressableBtn()
    {
        AddressableExt.ResetAddress();
    }

    [MenuItem("AddressableExt/Select Settings")]
    static void SelectSettingsMenu()
    {
        var settings = AddressableExt.AddressableSettings;
        Selection.objects = new Object[] { settings };
    }

    [MenuItem("AddressableExt/Check Name Unique")]
    static void CheckIfAssetsNameUniqueMenu()
    {
        List<string> validPathList = GetAllValidAssetPath();
        AddressableExt.CheckIfAssetsNameUnique(validPathList);
    }


    [MenuItem("AddressableExt/Remove All Addressables Info")]
    static void RemoveAllAddressableInfoMenu()
    {
        var b = EditorUtility.DisplayDialog("", "Are you sure?", "yes", "no");
        if (!b)
        {
            return;
        }
        AddressableEditorUtil.RemoveAllAddressableInfo(AddressableExt.AddressableSettings);
    }

    [MenuItem("AddressableExt/Build New Bundles")]
    static void BuildBundleWithResetGroup()
    {
        AddressableExt.ResetAddress();
        AddressableEditorUtil.RebuildContent("BuildScriptWwisePacked");
    }

    [MenuItem("AddressableExt/Build Update")]
    static void BuildUpdate()
    {
        AddressableEditorUtil.RebuildUpdate();
    }

    [MenuItem("AddressableExt/Normalize All Group")]
    static void NormalizeAllGroup()
    {
        AddressableEditorUtil.NormalizeAllGroup();
    }

}

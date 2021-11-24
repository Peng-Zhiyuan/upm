
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
            var isValidPath = AddressableExt.IsValidAddressableAssetPath(path);
            if (isValidPath)
            {
                avaliablePaths.Add(path);
            }
        }
        return avaliablePaths;
    }

    [MenuItem("AddressableExt/Auto Reset Assets")]
    static void RefreshAddressableBtn()
    {
        var yesOrNo = EditorUtility.DisplayDialog("确定要刷新 Addressable Asset 吗？", "这可能会让你卡上一阵子", "是", "否");
        if (!yesOrNo)
        {
            return;
        }
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
        var b = EditorUtility.DisplayDialog("确定要清除所有 Addressable 信息吗？", "你最好知道自己在干什么", "是", "否");
        if (!b)
        {
            return;
        }
        AddressableExt.enablePostProcess = false;
        AddressableEditorUtil.RemoveAllAddressableInfo(AddressableExt.AddressableSettings);
        AddressableExt.enablePostProcess = true;
    }

    [MenuItem("AddressableExt/Build New Bundles")]
    static void BuildBundleWithResetGroup()
    {
        var yesOrNo = EditorUtility.DisplayDialog("确定要重新打包吗？", "这可能会让你卡上一阵子，而且你必须在此之后清除缓存", "是", "否");
        if (!yesOrNo)
        {
            return;
        }
        AddressableExt.ResetAddress();
        AddressableExt.BuildPlayerContent();
    }

    [MenuItem("AddressableExt/Build Update")]
    static void BuildUpdate()
    {
        var path = ContentUpdateScript.GetContentStateDataPath(false);
        var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
    }

}

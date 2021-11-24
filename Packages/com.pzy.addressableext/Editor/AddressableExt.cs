
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



public class AddressableExt : AssetPostprocessor
{
    static AddressableAssetSettings _addressableSettings;
    public static AddressableAssetSettings AddressableSettings
    {
        get
        {
            if(_addressableSettings == null)
            {
                var fileName = "t:AddressableAssetSettings";
                var guidList = AssetDatabase.FindAssets(fileName);
                if (guidList.Length == 0)
                {
                    throw new Exception($"{fileName} not found in project");
                }
                var guid = guidList[0];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                _addressableSettings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
            }
            return _addressableSettings;
        }
    }
    
    static BundleSettings _bundleSettings;
    static BundleSettings BundleSettings
    {
        get
        {
            if (_bundleSettings == null)
            {
                var fileName = "t:BundleSettings";
                var guidList = AssetDatabase.FindAssets(fileName);
                if (guidList.Length == 0)
                {
                    throw new Exception($"{fileName} not found in project");
                }
                var guid = guidList[0];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                _bundleSettings = AssetDatabase.LoadAssetAtPath<BundleSettings>(path);
            }
            return _bundleSettings;
        }
    }


    public static bool enablePostProcess = true;

    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deleteAsset, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if(!enablePostProcess || !BundleSettings.autoProcessNewAssets)
        {
           return;
        }

        bool addressableChanged = false;
        foreach(var i in importedAsset)
        {
            var b = TrySetAsAddressable(i);
            if(b)
            {
                addressableChanged = true;
            }
           
        }

        foreach(var i in movedAssets)
        {
            var b = TrySetAsAddressable(i);
            if(b)
            {
                addressableChanged = true;
            }
        }


        if(addressableChanged)
        {
            AddressableEditorUtil.RemoveAllEmptyGroups(AddressableSettings);
        }

    }

    static AddressableAssetGroup GetOrCreateGroup(string groupName)
    {
        //SetGroupDefaultInfo(AddressableSettings.DefaultGroup);
        var group = AddressableSettings.FindGroup(groupName);
        if (group == null)
        {
            group = AddressableSettings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema> (AddressableSettings.DefaultGroup.Schemas) , typeof(AddressableAssetGroupSchema));
            SetGroupDefaultInfo(group);
        }
        //SetGroupDefaultInfo(group);
        //if (groupName != DefaultGroupName)
        //{
        //    SetGroupDefaultInfo(group);
        //}

        return group;
    }

    static void SetGroupDefaultInfo(AddressableAssetGroup group)
    {
        var schema = group.GetSchema<BundledAssetGroupSchema>();

        if(schema == null)
        {
            schema = group.AddSchema<BundledAssetGroupSchema>();
        }

        schema.BuildPath.SetVariableByName(AddressableSettings, "RemoteBuildPath");
        schema.LoadPath.SetVariableByName(AddressableSettings, "RemoteLoadPath");

        var updateSchema = group.GetSchema<ContentUpdateGroupSchema>();
        updateSchema.StaticContent = true;



        //if (BundleSettings.BuildLocalBundle)
        //{
        //    schema.BuildPath.SetVariableByName(AddressableSettings, "LocalBuildPath");
        //    schema.LoadPath.SetVariableByName(AddressableSettings, "LocalLoadPath");
        //}
        //else{
        //    schema.BuildPath.SetVariableByName(AddressableSettings, "RemoteBuildPath");
        //    schema.LoadPath.SetVariableByName(AddressableSettings, "RemoteLoadPath");
        //}
    }

    public static void BuildPlayerContent()
    {
        AddressableAssetSettings.BuildPlayerContent();
    }

    /// <summary>
    /// 将资源设置到指定 group
    /// 使用资源名作为 address
    /// 同时将 group 名设置为 label
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="group"></param>
    static void AddAssetToGroup(string assetPath, string groupName)
    {
        var group = GetOrCreateGroup(groupName);
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = AddressableSettings.CreateOrMoveEntry(guid, group);
        if(entry == null)
        {
            Debug.Log($"entray is null, assetPath: {assetPath}, group: {group}");
        }

        var address = Path.GetFileName(assetPath);
        entry.SetAddress(address, true);
        entry.SetLabel(groupName, true, true);
    }



    static bool IsIgnoreExtension(string path)
    {
        var extension = Path.GetExtension(path);
        if(extension == "")
        {
            return true;
        }

        if(extension == ".cs")
        {
            return true;
        }

        var ignoreTypeList = BundleSettings.IgnoreFileTypes;
        foreach (var ignoreType in ignoreTypeList)
        {
            if (extension == ignoreType)
            {
                return true;
            }
        }
        return false;
    }

    public static bool IsValidAddressableAssetPath(string path)
    {
        // 文件夹不是有效的 addressable 资产
        var isFolder = AssetDatabase.IsValidFolder(path);
        if(isFolder)
        {
            return false;
        }

        // Addressable 的配置文件夹内的东西，不是有效的 addressable 资产
        var isAddressableData = path.StartsWith("Assets/AddressableAssetsData");
        if(isAddressableData)
        {
            return false;
        }

        // StreamingAsset 的配置文件夹内的东西，不是有效的 addressable 资产
        var isStreamingAsset = path.StartsWith("Assets/StreamingAssets");
        if (isStreamingAsset)
        {
            return false;
        }

        // 尝试根据路径分析 group，没有指定 group 不是有效的 addressable 资产
        var groupName = GetGroupName(path);
        if(groupName == null)
        {
            return false;
        }

        // 是否在逃逸文件夹里
        var isInEscapFolder = path.Contains("/_");
        if(isInEscapFolder)
        {
            return false;
        }

        // 如果是指定排除的文件后缀，不是有效的 addressabel 资产
        var isIgnoreType = IsIgnoreExtension(path);
        if (isIgnoreType)
        {
            return false;
        }


        return true;
    }






    static int count = 0;
    public static void ResetAddress(bool interactive = true)
    {
        try
        {
            enablePostProcess = false;


            List<string> pathList = new List<string>(AssetDatabase.GetAllAssetPaths());
            pathList.Sort();
            var validatePathList = new List<string>();
            {
                var validateCount = 0;
                var showCount = 0;

                foreach (var path in pathList)
                {
                    if (showCount == 0 || validateCount - showCount > 100)
                    {
                        var isCancel = EditorUtility.DisplayCancelableProgressBar("计算资源个数", $"{validateCount}/{pathList.Count}", (float)validateCount / pathList.Count);
                        showCount = validateCount;
                        if (isCancel)
                        {
                            EditorUtility.ClearProgressBar();
                            throw new Exception("canceled");
                        }
                    }

                    var isValid = IsValidAddressableAssetPath(path);
                    if (isValid)
                    {
                        validatePathList.Add(path);
                    }
                    validateCount++;
                }
            }

            var isAddressUniq = CheckIfAssetsNameUnique(validatePathList);
            if(!isAddressUniq)
            {
                if(interactive)
                {
                    var b = EditorUtility.DisplayDialog("发现重复的 address，这会造成问题", "还要继续吗？", "是", "否");
                    if (!b)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new Exception("canceled");
                    }
                }
                else
                {
                    Debug.LogWarning("[AddressableEx] 发现重复的 address，这会造成问题");
                }
 
            }


            AddressableEditorUtil.RemoveAllAddressableAsset(AddressableSettings);
            AddressableEditorUtil.RemoveAllLabel(AddressableSettings);
            {
                count = 0;
                var showCount = 0;
                var procssedCount = 0;
                foreach (var path in validatePathList)
                {
                    if (showCount == 0 || procssedCount - showCount > 100)
                    {
                        bool isCancel = EditorUtility.DisplayCancelableProgressBar("执行中...", $"{procssedCount}/{validatePathList.Count}", (float)procssedCount / validatePathList.Count);
                        showCount = procssedCount;
                        if (isCancel)
                        {
                            EditorUtility.ClearProgressBar();
                            throw new Exception("canceled");
                        }
                    }

                    TrySetAsAddressable(path);
                    count++;
                    procssedCount++;
                }
            }

            AddressableEditorUtil.RemoveAllEmptyGroups(AddressableSettings);
            EditorUtility.ClearProgressBar();

            Debug.Log("[AddressableAssetAutoProcess] Addressable refreshed");

            enablePostProcess = true;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static string GetGroupName(string path)
    {
        string group = null;
        {
            // 如果能从后缀判断分包
            group = GetGroupNameFromExtForce(path);
            if (group != null)
            {
                goto aleradyGroupNameDecided;
            }

            // 如果能文件夹判断分包
            group = GetGroupNameFromPath(path);
            if (group != null)
            {
                goto aleradyGroupNameDecided;
            }
        }


        aleradyGroupNameDecided:
        group = ChangeGroupNameFromExtSuggest(path, group);
        return group;
    }

    static string ChangeGroupNameFromExtSuggest(string path, string alreadyDecidedGroupName)
    {
        // 检查改后缀是否有强制分组
        var settings = BundleSettings;
        var extToGroupDic = settings.sergestExtensionToGroup;
        if (extToGroupDic != null)
        {
            var ext = Path.GetExtension(path);
            var hasKey = extToGroupDic.ContainsKey(ext);
            if (hasKey)
            {
                var suggerst = extToGroupDic[ext];
                var toGroupName = suggerst.forceToGroup;
                if(alreadyDecidedGroupName != null)
                {
                    var exludeFromGroupList = suggerst.exludeFromGroupList;
                    var isExlude = exludeFromGroupList.Contains(alreadyDecidedGroupName);
                    if (isExlude)
                    {
                        return alreadyDecidedGroupName;
                    }
                    else
                    {
                        return toGroupName;
                    }
                }
                else
                {
                    return toGroupName;
                }
    
            }
        }
        return alreadyDecidedGroupName;
    }

    static string GetGroupNameFromExtForce(string path)
    {
        // 检查改后缀是否有强制分组
        var settings = BundleSettings;
        var extToGroupDic = settings.forceExtensionToGroup;
        if (extToGroupDic != null)
        {
            var ext = Path.GetExtension(path);
            var hasKey = extToGroupDic.ContainsKey(ext);
            if (hasKey)
            {
                var groupName = extToGroupDic[ext];
                return groupName;
            }
        }
        return null;
    }

    static string GetGroupNameFromPath(string path)
    {
        // 以 $ 符号的文件夹，作为 group
        var parts = path.Split('/');
        for (var index = parts.Length - 2; index >= 0; index--)
        {
            var folderName = parts[index];
            if (folderName.StartsWith("$"))
            {
                return folderName;
            }
        }
        return null;
    }

    static bool TrySetAsAddressable(string path)
    {
        if(!IsValidAddressableAssetPath(path))
        {
            return false;
        }
        var groupName = GetGroupName(path);
        AddAssetToGroup(path, groupName);
        return true;
    }


    public static bool CheckIfAssetsNameUnique(List<string> pathList)
    {
        var pass = true;
        Dictionary<string, string> fileNameToPathDic = new Dictionary<string, string>();
        foreach(var path in pathList)
        {
           var fileName = Path.GetFileName(path);
           if(!fileNameToPathDic.ContainsKey(fileName))
           {
                fileNameToPathDic[fileName] = path;
           }
           else
           {
                var a = path;
                var b = fileNameToPathDic[fileName];
                Debug.LogError($"[Addressable] same file named : {a} and {b}");
                pass = false;
           }
        }
        return pass;
    }


   

}

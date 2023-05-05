
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
using UnityEditor.Profiling;
using UnityEngine.Profiling;


public class AddressableExt 
{
    static AddressableAssetSettings _addressableSettings;
    public static AddressableAssetSettings AddressableSettings
    {
        get
        {
            if (_addressableSettings == null)
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

    static HashSet<string> _aheadGroupSet;
    public static HashSet<string> AheadGroupSet
    {
        get
        {
            if(_aheadGroupSet == null)
            {
                _aheadGroupSet = new HashSet<string>();
                var filePath = "Assets/AddressableAssetsExtData/ahead.txt";
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                var text = asset.text;
                var lineList = text.Split('\n');
                foreach(var line in lineList)
                {
                    var groupName = line.Trim().ToLower();
                    if(string.IsNullOrEmpty(groupName))
                    {
                        continue;
                    }
                    _aheadGroupSet.Add(groupName);
                }
            }
            return _aheadGroupSet;
        }
    }

    public static void ClearMissinGroup()
    {
        var settings = AddressableSettings;
        var groups = settings.groups;
        var removeList = new List<AddressableAssetGroup>();
        foreach(var one in groups)
        {
            if(one == null)
            {
                removeList.Add(one);

            }
        }
        foreach(var one in removeList)
        {
            Debug.Log($"[AddressableExt] {one} is null, removed");
            settings.groups.Remove(one);
        }
        Debug.Log($"[AddressableExt] {removeList.Count} groups pre removed.");

    }
    
    static BundleSettings _bundleSettings;
    public static BundleSettings BundleSettings
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


    static AddressableAssetGroup GetOrCreateGroup(string groupName)
    {
        var group = AddressableSettings.FindGroup(groupName);
        if (group == null)
        {
            Debug.Log("[AddressableExt] create group: " + groupName);
            group = AddressableSettings.CreateGroup(groupName, false, false, false, null);
            var settings = AddressableSettings;
            AddressableEditorUtil.SetGroupAsNormalize(settings, group);
        }
        return group;
    }

    static bool IsAheadGroup(string groupName)
    {
        if (AheadGroupSet.Contains(groupName))
        {
            return true;
        }
        return false;
    }


    /// <summary>
    /// 将资源设置到指定 group
    /// 使用资源名作为 address
    /// 同时将 group 名设置为 label
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="group"></param>
    static void SetAddressAndGroup(string assetPath, string groupName, string address)
    {
        var group = GetOrCreateGroup(groupName);
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = AddressableSettings.CreateOrMoveEntry(guid, group);
        if (entry == null)
        {
            Debug.Log($"entray is null, assetPath: {assetPath}, group: {group}");
        }

        //var address = Path.GetFileName(assetPath);
        //var address = GetAddressFromPath(assetPath, groupName);
        entry.SetAddress(address, true);
        entry.SetLabel(groupName, true, true);
        entry.SetLabel("any", true, true);

        var isAheadGroup = IsAheadGroup(groupName);
        if(isAheadGroup)
        {
            entry.SetLabel("ahead", true, true);
        }

        //if(extraLabel != null)
        //{
        //    entry.SetLabel(extraLabel, true, true);
        //}
    }



    static bool IsIgnoreExtension(string path)
    {
        var extension = Path.GetExtension(path);
        var ret =  BundleSettings.IsIgnoreExtension(extension);
        return ret;
       
    }

    public static (bool isValdiate, string groupName, string address) IsValidAddressableAssetPath(string path, Action<string> report = null)
    {
        report?.Invoke("path.Contains('/Editor/'");
        if (path.Contains("/Editor/"))
        {
            return (false, null, null);
        }


        report?.Invoke("IsValidFolder");
        // 文件夹不是有效的 addressable 资产
        var isFolder = AssetDatabase.IsValidFolder(path);
        if(isFolder)
        {
            return (false, null, null);
        }

        report?.Invoke(" path.StartsWith('Assets/AddressableAssetsData')");
        // Addressable 的配置文件夹内的东西，不是有效的 addressable 资产
        var isAddressableData = path.StartsWith("Assets/AddressableAssetsData");
        if(isAddressableData)
        {
            return (false, null, null);
        }

        report?.Invoke(" path.StartsWith('Assets/StreamingAssets')");
        // StreamingAsset 的配置文件夹内的东西，不是有效的 addressable 资产
        var isStreamingAsset = path.StartsWith("Assets/StreamingAssets");
        if (isStreamingAsset)
        {
            return (false, null, null);
        }

        // 尝试根据路径分析 group，没有指定 group 不是有效的 addressable 资产
        report?.Invoke("GetGroupAndAddressFromPath");
        var (groupName, address) = GetGroupAndAddressFromPath(path);
        if(groupName == null)
        {
            return (false, null, null);
        }

        report?.Invoke("path.Contains('/_')");
        // 是否在逃逸文件夹里
        var isInEscapFolder = path.Contains("/_");
        if(isInEscapFolder)
        {
            return (false, null, null);
        }

        report?.Invoke("IsIgnoreExtension");
        // 如果是指定排除的文件后缀，不是有效的 addressabel 资产
        var isIgnoreType = IsIgnoreExtension(path);
        if (isIgnoreType)
        {
            return (false, null, null);
        }

        report?.Invoke("complete");
        return (true, groupName, address);
    }



    public class CheckedInfo
    {
        public string path;
        public string groupName;
        public string address;
    }

    static int count = 0;
    public static void ResetAddress(bool interactive = true)
    {
        ClearMissinGroup();

        try
        {

            List<string> pathList = new List<string>(AssetDatabase.GetAllAssetPaths());
            pathList.Sort();

            //var sb = new StringBuilder();
            //for(int i = 0; i < 10; i++)
            //{
            //    var p = pathList[i];
            //    sb.AppendLine(p);
            //}
            //for (int i = pathList.Count - 10; i < pathList.Count; i++)
            //{
            //    var p = pathList[i];
            //    sb.AppendLine(p);
            //} 
            //Debug.Log(sb);
            //enablePostProcess = true;
            //return;

           
            var validatePathList = new List<CheckedInfo>();
            {
                var validateCount = 0;
                var showCount = 0;

                foreach (var path in pathList)
                {

                    //if (showCount == 0 || validateCount - showCount > 100)
                    //{
                    //    var isCancel = EditorUtility.DisplayCancelableProgressBar("计算资源个数", $"{validateCount}/{pathList.Count}", (float)validateCount / pathList.Count);
                    //    showCount = validateCount;
                    //    if (isCancel)
                    //    {
                    //        EditorUtility.ClearProgressBar();
                    //        throw new Exception("canceled");
                    //    }
                    //}

                    var fileName = Path.GetFileName(path);
                    var isCancel = EditorUtility.DisplayCancelableProgressBar("计算资源个数", $"{validateCount}/{pathList.Count} - {fileName}", (float)validateCount / pathList.Count);
                    showCount = validateCount;
                    if (isCancel)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new Exception("canceled");
                    }

                    Profiler.BeginSample("IsValidAddressableAssetPath");
                    var (isValid, groupName, address) = IsValidAddressableAssetPath(path, (des) =>
                    {
                        EditorUtility.DisplayCancelableProgressBar("计算资源个数", $"{validateCount}/{pathList.Count} - {fileName}: {des}", (float)validateCount / pathList.Count);
                    });
                    EditorUtility.DisplayCancelableProgressBar("计算资源个数", $"{validateCount}/{pathList.Count} - {fileName}: end", (float)validateCount / pathList.Count);

                    Profiler.EndSample();
                    if (isValid)
                    {
                        var info = new CheckedInfo();
                        info.path = path;
                        info.groupName = groupName;
                        info.address = address;
                        validatePathList.Add(info);
                    }
                    validateCount++;
                }
            }

            var validPathList = validatePathList.Select(info => info.path).ToList();
            var isAddressUniq = CheckIfAssetsNameUnique(validPathList);
            if(!isAddressUniq)
            {
                if(interactive)
                {
                    var b = EditorUtility.DisplayDialog("发现重复的 address，这可能会造成问题", "还要继续吗？", "是", "否");
                    if (!b)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new Exception("canceled");
                    }
                }
                else
                {
                    Debug.LogWarning("[AddressableEx] 发现重复的 address，这可能会造成问题");
                }
 
            }

            var sw = new Stopwatch();
            sw.Start();

            AddressableEditorUtil.RemoveAllEntry(AddressableSettings);
            AddressableEditorUtil.RemoveAllLabel(AddressableSettings);
            {
                count = 0;
                var showCount = 0;
                var procssedCount = 0;
                foreach (var info in validatePathList)
                {
                    //if (showCount == 0 || procssedCount - showCount > 10) 
                    //{
                    //    var fileName = Path.GetFileName(info.path);
                    //    bool isCancel = EditorUtility.DisplayCancelableProgressBar("执行中...", $"{procssedCount}/{validatePathList.Count} - {fileName}", (float)procssedCount / validatePathList.Count);
                    //    showCount = procssedCount;
                    //    if (isCancel)
                    //    {
                    //        EditorUtility.ClearProgressBar();
                    //        throw new Exception("canceled");
                    //    }
                    //}

                    var fileName = Path.GetFileName(info.path);
                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("执行中...", $"{procssedCount}/{validatePathList.Count} - {fileName}", (float)procssedCount / validatePathList.Count);
                    showCount = procssedCount;
                    if (isCancel)
                    {
                        EditorUtility.ClearProgressBar();
                        throw new Exception("canceled");
                    }

                    TrySetAsAddressable(info.path, true, info);
                    count++;
                    procssedCount++;
                }
            }

            AddressableEditorUtil.RemoveAllEmptyGroups(AddressableSettings);

            AddressableEditorUtil.NormalizeAllGroup();
            
            EditorUtility.ClearProgressBar();

            sw.Start();
            Debug.Log($"[AddressableExt] Addressable refreshed. duration: {sw.Elapsed}, processed: {validatePathList.Count}");


            AssetDatabase.SaveAssets();

        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
     
    enum AddressNamingType
    {
        FileName,
        Path,
    }

    static AddressNamingType GetNamingTypeFromGroupName(string groupName)
    {
        if(groupName.StartsWith("$$"))
        {
            return AddressNamingType.Path;
        }
        return AddressNamingType.FileName;
    }

    static string GetAddressFromGroupNameAndPath(string groupName, string path)
    {
        var type = GetNamingTypeFromGroupName(groupName);
        if (type == AddressNamingType.Path)
        {
            return path;
        }
        else if(type == AddressNamingType.FileName)
        {
            var name = Path.GetFileName(path);
            return name;
        }
        throw new Exception("[AddressableExt] unsupport type:" + type);
    }

    static (string group, string address) GetGroupAndAddressFromPath(string path)
    {
        string group = null;
        // 如果能从 package 逻辑中确定分包
        group = GetGroupNameFromPackage(path);
        if (group != null)
        {
            goto groupNameDecided;
        }

        // 如果能文件夹判断分包
        group = GetGroupNameFromPath(path);
        if (group != null)
        {
            goto groupNameDecided;
        }

        // 特殊逻辑
        // 如果是 wwise 的 addressnable bank 资源
        if (path.StartsWith("Assets/wwiseData/"))
        {
            var ext = Path.GetExtension(path);
            if(ext == ".asset")
            {
                group = "wwise";
                goto groupNameDecided;
            }
            else if(ext == ".bnk" || ext == ".xml")
            {
                if(path.Contains("/Android/"))
                {
                    var filename = Path.GetFileName(path);
                    if (filename == "Init.bnk")
                    {
                        return ("WwiseData_Android_InitBank", path);
                    }
                    else
                    {
                        return ("WwiseData_Android", path);
                    }
                }
                else if (path.Contains("/iOS/"))
                {
                    var filename = Path.GetFileName(path);
                    if (filename == "Init.bnk")
                    {
                        return ("WwiseData_iOS_InitBank", path);
                    }
                    else
                    {
                        return ("WwiseData_iOS", path);
                    }
                }
                else if (path.Contains("/Mac/"))
                {
                    var filename = Path.GetFileName(path);
                    if (filename == "Init.bnk")
                    {
                        return ("WwiseData_Mac_InitBank", path);
                    }
                    else
                    {
                        return ("WwiseData_Mac", path);
                    }
                }
                else if (path.Contains("/Windows/"))
                {
                    var filename = Path.GetFileName(path);
                    if (filename == "Init.bnk")
                    {
                        return ("WwiseData_Windows_InitBank", path);
                    }
                    else
                    {
                        return ("WwiseData_Windows", path);
                    }
                }
            }
        }

        if (path.StartsWith("Assets/Wwise/ScriptableObjects/"))
        {
            group = "wwise";
            goto groupNameDecided;
        }

        if (path.StartsWith("Assets/Arts/SceneEditor/MapData/"))
        {
            group = "map_data";
            return (group, path);
        }

        if (path.StartsWith("Assets/ThirdParts/MTEExtend"))
        {
            group = "map_data";
            return (group, path);
        }
        if (path.StartsWith("Assets/Subsystems/-NGGrass"))
        {
            group = "map_data";
            return (group, path);
        }

    groupNameDecided:
        string address = null;
        if (group != null)
        {
            // 组名强制全小写
            group = group.ToLower();
            address = GetAddressFromGroupNameAndPath(group, path);
        }
 
        return (group, address);
    }


    static string GetGroupNameFromPackage(string path)
    {
        // 如果不是 Packages/ 开始，则不是包中的资产
        if(!path.StartsWith("Packages/"))
        {
            return null;
        }

        var settings = BundleSettings;
        var isValid = settings.IsPackageIncludingPath(path);
        if(isValid)
        {
            var parts = path.Split('/');
            var packageDirName = parts[1];

            return packageDirName;
        }
        else
        {
            return null;
        }
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

    static bool TrySetAsAddressable(string path, bool alreadyCheckedValidation = false, CheckedInfo checkedInfo = null)
    {
        if(!alreadyCheckedValidation)
        {
            var (isValid, _, _) = IsValidAddressableAssetPath(path);
            if (!isValid)
            {
                return false;
            }
        }

        if(checkedInfo == null)
        {
            var (groupName, address) = GetGroupAndAddressFromPath(path);
            SetAddressAndGroup(path, groupName, address);
        }
        else
        {
            SetAddressAndGroup(path, checkedInfo.groupName, checkedInfo.address);
        }

        return true;
    }


    public static bool CheckIfAssetsNameUnique(List<string> pathList)
    {
        var pass = true;
        Dictionary<string, string> fileNameToPathDic = new Dictionary<string, string>();
        foreach(var path in pathList)
        {
           if(path.Contains("wwiseData"))
           {
                continue;
           }
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

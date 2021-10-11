
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




public class AddressableAssetAutoProcess : AssetPostprocessor
{
    static AddressableAssetSettings _addressableSettings;
    static AddressableAssetSettings addressableSettings
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
    static BundleSettings bundleSettings
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


    static bool enablePostProcess = true;

    static string DefaultGroupName = "Default Local Group";

    public static void OnPostprocessAllAssets(string[] importedAsset, string[] deleteAsset, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if(!enablePostProcess || !bundleSettings.autoProcessNewAssets){
           return;
        }

        foreach(var a in importedAsset){
            Debug.Log(a);
        }

        foreach(var a in movedAssets){
            Debug.Log(a);
        }

        bool any = false;
        foreach(var i in importedAsset){
            ProcessNewAssetPath(i);
            any = true;
        }

        foreach(var i in movedAssets){
            ProcessNewAssetPath(i);
            any = true;
        }


        if(any){
            RemoveEmptyGroups();
        }

    }

    static AddressableAssetGroup GetGroup(string groupName, bool autoCreateNew = true){
        InitGroupSettings(addressableSettings.DefaultGroup);
        var group = addressableSettings.FindGroup(groupName);
        if (autoCreateNew && group == null)
        {
            group = addressableSettings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema> (addressableSettings.DefaultGroup.Schemas) , typeof(AddressableAssetGroupSchema));
        }
        if(groupName != DefaultGroupName){
            InitGroupSettings(group);
        }

        return group;
    }

    static void InitGroupSettings(AddressableAssetGroup group){
        var schema = group.GetSchema<BundledAssetGroupSchema>();

        if(schema == null)
        {
            schema = group.AddSchema<BundledAssetGroupSchema>();
        }

        if(bundleSettings.BuildLocalBundle){
            schema.BuildPath.SetVariableByName(addressableSettings, "LocalBuildPath");
            schema.LoadPath.SetVariableByName(addressableSettings, "LocalLoadPath");
        }
        else{
            schema.BuildPath.SetVariableByName(addressableSettings, "RemoteBuildPath");
            schema.LoadPath.SetVariableByName(addressableSettings, "RemoteLoadPath");
        }
        // if(bundleSettings.BuildLocalBundle){
        //     schema.BuildPath.SetVariableByName(addressableSettings, AddressableAssetSettings.kLocalBuildPath);
        //     schema.LoadPath.SetVariableByName(addressableSettings, AddressableAssetSettings.kLocalLoadPath);
        // }
        // else{
        //     schema.BuildPath.SetVariableByName(addressableSettings, AddressableAssetSettings.kRemoteBuildPath);
        //     schema.LoadPath.SetVariableByName(addressableSettings, AddressableAssetSettings.kRemoteLoadPath);
        // }
    }

    /// <summary>
    /// 将资源设置到指定 group
    /// 使用资源名作为 adress
    /// 同时将 group 名设置为 label
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="group"></param>
    static void AddAssetToGroup(string assetPath, AddressableAssetGroup group)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = addressableSettings.CreateOrMoveEntry(guid, group);
        if(entry == null)
        {
            Debug.Log($"entray is null, assetPath: {assetPath}, group: {group}");
        }

        var address = GetPathFileName(assetPath);
        entry.SetAddress(address, true);

        var groupName = group.name;
        entry.SetLabel(groupName, true, true);
    }

    static string GetPathFileName(string fileName){
        var splts = fileName.Split('/');
        return splts[splts.Length - 1];
    }

    static bool IsIgnorePath(string path)
    {
        var extension = Path.GetExtension(path);
        if(extension == "")
        {
            return true;
        }

        var ignoreTypeList = bundleSettings.IgnoreFileTypes;
        foreach (var ignoreType in ignoreTypeList)
        {
            if (extension == ignoreType)
            {
                return true;
            }
        }
        return false;
    }

    static bool IsValidPath(string path)
    {
        var isFolder = AssetDatabase.IsValidFolder(path);
        if(isFolder)
        {
            return false;
        }
        
        var groupName = GetPathGroupName(path);
        if(groupName == null){
            return false;
        }

        var isIgnoreType = IsIgnorePath(path);
        if (isIgnoreType)
        {
            return false;
        }


        return true;
    }

    static List<string> GetValidAssetPathList(){
        List<string> avaliablePaths = new List<string>();

        var paths = AssetDatabase.GetAllAssetPaths();
        foreach(var p in paths){
            if(IsValidPath(p)){
                avaliablePaths.Add(p);
            }
        }
        return avaliablePaths;
    }

    static void ClearAddressableLabels(){
        var oldLabels = new List<string>(addressableSettings.GetLabels());
        foreach(var label in oldLabels){
            addressableSettings.RemoveLabel(label);
        }
    }

    [MenuItem("AssetsTools/RefreshAddress")]
    static void RefreshAddressableBtn()
    { 
		var yesOrNo = EditorUtility.DisplayDialog("确定要刷新Addressable引用吗？", "这可能会让你卡上一阵子", "是", "否");
        if(!yesOrNo){
            return;
        }		
        RefreshAddress();
    }

    static int count = 0;
    static void RefreshAddress()
    {
        try
        {
            enablePostProcess = false;

            RemoveAddressablesAssets();
            ClearAddressableLabels();

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
                            throw new Exception("canceld");
                        }
                    }

                    var isValid = IsValidPath(path);
                    if (isValid)
                    {
                        validatePathList.Add(path);
                    }
                    validateCount++;
                }
            }

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
                            throw new Exception("canceld");
                        }
                    }

                    ProcessNewAssetPath(path);
                    count++;
                    procssedCount++;
                }
            }

            EditorUtility.ClearProgressBar();

            RemoveEmptyGroups();

            Debug.Log("[AddressableAssetAutoProcess] Addressable refreshed");

            enablePostProcess = true;
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static string GetPathGroupName(string path)
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

    static void ProcessNewAssetPath(string path)
    {
        if(!IsValidPath(path)){
            return;
        }
        var groupName = GetPathGroupName(path);
        groupName = groupName.Replace("$", "");
        var group = GetGroup(groupName);
        AddAssetToGroup(path, group);
    }

    [MenuItem("AssetsTools/CheckNameUnique")]
    static void CheckIfAssetsNameUnique(){
        List<string> avaliablePaths = GetValidAssetPathList();
        Dictionary<string, string> checker = new Dictionary<string, string>();
        foreach(var path in avaliablePaths){
           if(path.Contains("$")){
               continue;
           }
           var fileName = GetPathFileName(path);
           if(!checker.ContainsKey(fileName)){
               checker[fileName] = path;
           }
           else{
               Debug.LogError($"[Addressable] Multiple file named : {fileName}");
               Debug.LogError($"[Addressable] {checker[fileName]}");
               Debug.LogError($"[Addressable] {path}");
           }
        }
    }

    [MenuItem("AssetsTools/RemoveAddressables")]
    static void RemoveAddressablesFunction(){
		var yesOrNo = EditorUtility.DisplayDialog("确定要清除Addressable引用吗？", "你最好知道自己在干什么", "是", "否");
        if(!yesOrNo){
            return;
        }		
        enablePostProcess = false;
        RemoveAddressablesAssets();
        enablePostProcess = true;
    }

    static void RemoveAddressablesAssets(){
        var groups = new List<AddressableAssetGroup>(addressableSettings.groups);
        foreach(var g in groups){
            if(g == null){
                addressableSettings.RemoveGroup(g);
                continue;
            }
            // addressableSettings.RemoveGroup(g);
            List<AddressableAssetEntry> entrys = new List<AddressableAssetEntry>();
            foreach(var e in g.entries){
                entrys.Add(e);
            }
            foreach(var e in entrys){
                g.RemoveAssetEntry(e);
            }
        }
    }
    
    static void RemoveEmptyGroups(){
        var groups = new List<AddressableAssetGroup>(addressableSettings.groups);
        foreach(var g in groups){
            if(g.entries.Count == 0){
                addressableSettings.RemoveGroup(g);
            }
        }
    }
    
    [MenuItem("AssetsTools/BuildNewBundles")]
    static void BuildBundleWithResetGroup(){
		var yesOrNo = EditorUtility.DisplayDialog("确定要重新打包吗？", "这可能会让你卡上一阵子，而且你必须在此之后清除缓存", "是", "否");
        if(!yesOrNo){
            return;
        }		
        RefreshAddress();
        AddressableAssetSettings.BuildPlayerContent();
    }

    [MenuItem("AssetsTools/BuildUpdate")]
    static void BuildUpdate()
    {    
        // RefreshLabelFile();
        var path = ContentUpdateScript.GetContentStateDataPath(false);
        var m_Settings = AddressableAssetSettingsDefaultObject.Settings;
        AddressablesPlayerBuildResult result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, path);
        Debug.Log("BuildFinish path = " + m_Settings.RemoteCatalogBuildPath.GetValue(m_Settings));
    }

    // [MenuItem("AssetsTools/Prepare Update Content")]
    // 现在的设置不需要这玩意儿
    static void CheckForUpdateContent()
    {
        //与上次打包做资源对比
        string buildPath = ContentUpdateScript.GetContentStateDataPath(false);
        var m_Settings = AddressableAssetSettingsDefaultObject.Settings;
        List<AddressableAssetEntry> entrys =
            ContentUpdateScript.GatherModifiedEntries(
                m_Settings, buildPath); 
        if (entrys.Count == 0) return;
        StringBuilder sbuider = new StringBuilder();
        sbuider.AppendLine("Need Update Assets:");
        foreach (var _ in entrys)
        {
            sbuider.AppendLine(_.address);
        }
        Debug.Log(sbuider.ToString());
        
        //将被修改过的资源单独分组
        var groupName = string.Format("UpdateGroup_{0}",DateTime.Now.ToString("yyyyMMdd"));
        ContentUpdateScript.CreateContentUpdateGroup(m_Settings, entrys, groupName);
    }

}

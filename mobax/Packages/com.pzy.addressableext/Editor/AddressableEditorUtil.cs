
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


        // 某些时候其中会包含 null 元素，使用 api 进行移除会报错
        // settings.groups.Clear();
    }

    // 移除所有 addressable asset 项目
    public static void RemoveAllEntry(AddressableAssetSettings settings)
    {
        var groups = new List<AddressableAssetGroup>(settings.groups);
        foreach (var g in groups)
        {
            if (g == null)
            {
                //settings.RemoveGroup(g);
                continue;
            }
            // addressableSettings.RemoveGroup(g);
            var name = g.Name;
            if(name.Contains("WwiseData_"))
            {
                //Debug.Log("[AddressableEditorUtil] ignore group: " + name);
                continue;
            }
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
            if(g == null)
            {
                continue;
            }
            settings.RemoveGroup(g);
        }
    }

    public static void RemoveAllAddressableInfo(AddressableAssetSettings settings)
    {
        RemoveAllEntry(settings);
        RemoveAllLabel(settings);
        RemoveAllGroup(settings);
    }

    // 移除所有空的 group
    public static void RemoveAllEmptyGroups(AddressableAssetSettings settings)
    {
        var groupList = new List<AddressableAssetGroup>(settings.groups);
        foreach (var g in groupList)
        {
            // pzy:
            // Wwise 有自己的 address 自动化处理脚本，这里不管理
            var name = g.Name;
            if(name.Contains("WwiseData_"))
            {
                continue;
            }

            if (g.entries.Count == 0)
            {
                settings.RemoveGroup(g);
            }
        }
    }

    public static T AddOrCreateSchema<T>(AddressableAssetGroup group) where T : AddressableAssetGroupSchema
    {
        var schema = group.GetSchema<T>();
        if (schema == null)
        {
            schema = group.AddSchema<T>();
        }
        return schema;
    }

    public static void NormalizeAllGroup()
    {
        var settings = AddressableExt.AddressableSettings;
        var groupList = settings.groups;
        foreach (var group in groupList)
        {
            SetGroupAsNormalize(settings, group);
        }
    }

    public static void SetGroupAsNormalize(AddressableAssetSettings settings, AddressableAssetGroup group)
    {
        var bundleSchema = AddOrCreateSchema<BundledAssetGroupSchema>(group);
        bundleSchema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
        bundleSchema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");

        var isSeparatePackGroup = AddressableExt.BundleSettings.IsSeparatePackGroup(group.name);
        if(isSeparatePackGroup)
        {
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        }
        else
        {
            bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        }

        bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
 

        var updateSchema = AddOrCreateSchema<ContentUpdateGroupSchema>(group);
        updateSchema.StaticContent = false;

        // 曾经错误的默认给 group 加上了这个 schema，但是不需要
        group.RemoveSchema<AddressableAssetGroupSchema>();

        //group.entries
  
    }

    //// 清除 group 对象的缓存字段 "m_SerializeEntries" 
    //// 让其保存时重新生成，用来保证顺序
    //public void CleanGroupEntriesSerializeCache()
    //{

    //}


    public static string PlayModeScriptName
    {
        get
        {
            var setting = AddressableAssetSettingsDefaultObject.Settings;
            var name = setting.ActivePlayModeDataBuilder.Name;
            return name;
        }
    }

    public static void CleanServerDataDirectory()
    {
        var path = "ServerData";
        if(Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    static void SetActiveBuilder(string builderName)
    {
        var builderList = AddressableAssetSettingsDefaultObject.Settings.DataBuilders;
        var index = 0;
        foreach (var builder in builderList)
        {
            var name = builder.name;
            if (name == builderName)
            {
                AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilderIndex = index;
                return;
            }
            index++;
        }
        throw new Exception($"[AddressableExt] builder '{builderName}' not found");
    }

    public static AddressablesPlayerBuildResult RebuildUpdate(string stateFilePath = null, bool browseIfNotProvide = false)
    {
        if(stateFilePath == null)
        {
            stateFilePath = ContentUpdateScript.GetContentStateDataPath(browseIfNotProvide);
        }
        CleanServerDataDirectory();
        Debug.Log("[AddressableEditorUtil] state file path: " + stateFilePath);
        var result = ContentUpdateScript.BuildContentUpdate(AddressableAssetSettingsDefaultObject.Settings, stateFilePath);
        var error = result.Error;
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception("[AddressableExt] UpdateContent error: " + error);
        }
        EditorUtility.ClearProgressBar();
        return result;
    }

    public static AddressablesPlayerBuildResult RebuildContent(string builderName = null)
    {
        if (builderName != null)
        {
            SetActiveBuilder(builderName);
        }
        CleanServerDataDirectory();
        AddressableAssetSettings.BuildPlayerContent(out var result);
        var error = result.Error;
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception("[AddressableExt] BuildPlayerContent error: " + error);
        }
        //EmbedRemoteBundle();
        EditorUtility.ClearProgressBar();
        return result;
    }

    public static string EmbededBundleDir
    {
        get
        {
            return $"{Addressables.RuntimePath}/Embeded";
        }
    }

    public static void DeleteFilesToKeepTotalSizeIn(string rootDir, long limitSizeInByte)
    {
        var fileList = Directory.GetFiles(rootDir).ToList();
        fileList.Sort();
        var sum = 0L;
        var outLimit = false;
        foreach(var file in fileList)
        {
            if(outLimit)
            {
                File.Delete(file);
                continue;
            }
            var info = new FileInfo(file);
            var size = info.Length;
            sum += size;
            if(sum <= limitSizeInByte)
            {
                continue;
            }
            else
            {
                outLimit = true;
                File.Delete(file);
            }
        }
    }

    static bool IsBuiltinBundle(string bundleFileName)
    {
        if(bundleFileName.Contains("_unitybuiltin"))
        {
            return true;
        }
        return false;
    }
    static string GetGroupNameFromBundleFileName(string bundleFileName)
    {
        {
            var parts = bundleFileName.Split(new string[] { "_assets_all_" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var groupName = parts[0];
                return groupName;
            }
        }
        {
            var parts = bundleFileName.Split(new string[] { "_scenes_all_" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var groupName = parts[0];
                return groupName;
            }
        }
        return null;

    }

    public static void DeleteBundleWhichNotInKeepList(string rootDir, HashSet<string> keepGroupNameSet)
    {
        var fileList = Directory.GetFiles(rootDir).ToList();
        foreach (var file in fileList)
        {
            var fileName = Path.GetFileName(file);
            var groupName = GetGroupNameFromBundleFileName(fileName);
            var isBuiltin = IsBuiltinBundle(fileName);
            if(isBuiltin)
            {
                continue;
            }
            var inKeepList = keepGroupNameSet.Contains(groupName);
            if (!inKeepList)
            {
                File.Delete(file);
            }
        }
    }

    public static void DeleteAllFileInDir(string rootDir)
    {
        var fileList = Directory.GetFiles(rootDir).ToList();
        foreach (var file in fileList)
        {
            File.Delete(file);
        }
    }

    public enum EmbedType
    {
        All,
        List,
        Size,
        None,
    }

    /// <summary>
    /// 将远端 bundle 内置
    /// 远端目录为 ServerData
    /// 内置到的位置为 Library\com.unity.addressables\aa\{Platform}\{Platform}\Embeded
    /// 此目录是编辑器时的 RuntimePath，也是 BuildPath
    /// </summary>
    public static void EmbedRemoteBundle(EmbedType type, HashSet<string> embedList = null, int sizeInM = 0)
    {
        var from = $"ServerData/{EditorUserBuildSettings.activeBuildTarget}";
        var to = EmbededBundleDir;
        if(type == EmbedType.None)
        {
            if(!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }
            DeleteAllFileInDir(to);
        }
        else
        {
            IOUtil.SyncDir(from, to, new string[] { "catalog_fw." });

            if (type == EmbedType.Size)
            {
                if (sizeInM != 0)
                {
                    var sizeInByte = sizeInM * 1024 * 1024;
                    DeleteFilesToKeepTotalSizeIn(to, sizeInByte);
                }
            }
            else if (type == EmbedType.List)
            {
                DeleteBundleWhichNotInKeepList(to, embedList);
            }
        }


         // 产生目录
        var fileList = Directory.GetFiles(to);

        var list = new List<string>();
        foreach (var file in fileList)
        {
            var fileName = Path.GetFileName(file);
            list.Add(fileName);
        }
        var json = JsonMapper.Instance.ToJson(list);
        var indexPath = $"{to}/index.json";
        File.WriteAllText(indexPath, json);

    }


}

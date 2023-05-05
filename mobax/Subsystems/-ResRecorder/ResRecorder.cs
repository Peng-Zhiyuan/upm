#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using System.Collections;


public class ResRecorder : StuffObject<ResRecorder>
{
    [ShowInInspector]
    HashSet<Object> assetSet = new HashSet<Object>();
    static List<string> DependencyInfo = new List<string>();
    public void Record(Object asset)
    {
        assetSet.Add(asset);
    }

    [ShowInInspector]
    public void Generate()
    {
        DependencyInfo.Clear();
        var pathSet = AssetSetToPathSet(assetSet);
        var groupSet = AssetPathSetToGroupSet(pathSet);
        var reoslvedGroupList = ReolveGroupDependency(groupSet);
        WriteAheadFile(reoslvedGroupList);
        WriteDependFile(DependencyInfo);

        var resolvedUsedAsset = ResolveAssetsDependency(assetSet);
        WriteUsedAssetFile(resolvedUsedAsset);

        var allAssetInBundle = GetAllAssetPathHashSetFromGroupSet(reoslvedGroupList);
        var lianDaiAssetSet = SetSub(allAssetInBundle, resolvedUsedAsset);
        WriteLiandaiFile(lianDaiAssetSet);

        WriteSaveFile(pathSet);
    }

    [MenuItem("test/test")]
    public static void Test()
    {
        PrintGroupRef($"$plot3d");
    }

    public static void PrintGroupRef(string groupName)
    {
        var group = NameToGroup(groupName);
        if(group == null)
        {
            Debug.Log("group not found");
            return;
        }
        var groupSet = ReolveGroupDependency(new HashSet<AddressableAssetGroup>() { group });
        Debug.Log($"{groupName} dependecies bundle count: {groupSet.Count}");
        var index = 0;
        foreach(var one in groupSet)
        {
            Debug.Log($"[{index}] {one.Name}");
            index++;
        }
    }

    static AddressableAssetGroup NameToGroup(string groupName)
    {
        var settings = AddressableExt.AddressableSettings;
        var ret = settings.FindGroup(groupName);
        return ret;
    }

    void WriteSaveFile(HashSet<string> unresolvedUsedAssetSet)
    {
        var filePath = "Assets/AddressableAssetsExtData/save.txt";
        var list = new List<string>(unresolvedUsedAssetSet);
        list.Sort();

        var sb = new StringBuilder();
        foreach (var one in list)
        {
            sb.AppendLine(one);
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    [ShowInInspector]
    void Load()
    {
        assetSet.Clear();
        var filePath = "Assets/AddressableAssetsExtData/save.txt";
        var text = File.ReadAllText(filePath);
        var lineList = text.Split('\n');
        foreach (var line in lineList)
        {
            var path = line.Trim();
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null)
            {
                assetSet.Add(asset);
            }
        }
    }

    void WriteUsedAssetFile(HashSet<string> resolvedUsedAssetPathSet)
    {
        var filePath = "Assets/AddressableAssetsExtData/used.txt";

        var list = new List<string>(resolvedUsedAssetPathSet);
        list.Sort();
        var sb = new StringBuilder();
        foreach (var one in list)
        {
            sb.AppendLine(one);
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    void WriteAheadFile(HashSet<AddressableAssetGroup> reoslvedGroupList)
    {
        var filePath = "Assets/AddressableAssetsExtData/ahead.txt";

        var list = new List<string>();
        foreach (var one in reoslvedGroupList)
        {
            list.Add(one.Name);
        }
        list.Sort();

        var sb = new StringBuilder();
        foreach (var one in list)
        {
            sb.AppendLine(one);
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    void WriteDependFile(List<string> dependList)
    {
        var filePath = "Assets/AddressableAssetsExtData/depend.txt";

        var list = new List<string>();
        foreach (var one in dependList)
        {
            list.Add(one);
        }
        list.Sort();

        var sb = new StringBuilder();
        foreach (var one in list)
        {
            sb.AppendLine(one);
        }
        File.WriteAllText(filePath, sb.ToString());
    }

    void WriteLiandaiFile(HashSet<string> lianDaiAssetSet)
    {
        var list = new List<string>(lianDaiAssetSet);
        list.Sort();

        var filePath = "Assets/AddressableAssetsExtData/liandai.txt";
        var sb = new StringBuilder();
        foreach (var path in list)
        {
            sb.AppendLine(path);
        }
        File.WriteAllText(filePath, sb.ToString());
    }



    //[ShowInInspector]
    //public void Clean()
    //{
    //    assetSet.Clear();
    //}

    HashSet<string> AssetSetToPathSet(HashSet<Object> assetSet)
    {
        var ret = new HashSet<string>();
        foreach (var asset in assetSet)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            ret.Add(path);
        }
        return ret;
    }

    static HashSet<T> SetSub<T>(HashSet<T> origin, HashSet<T> subSet)
    {
        var ret = new HashSet<T>();
        SetAdd(ret, origin);
        foreach (var itemToSub in subSet)
        {
            origin.Remove(itemToSub);
        }
        return ret;
    }

    string AssetToPath(Object asset)
    {
        var path = AssetDatabase.GetAssetPath(asset);
        return path;
    }

    HashSet<AddressableAssetGroup> AssetPathSetToGroupSet(HashSet<string> assetPathSet)
    {
        var settings = AddressableExt.AddressableSettings;
        var ret = new HashSet<AddressableAssetGroup>();
        foreach (var assetPath in assetPathSet)
        {
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.FindAssetEntry(guid);
            if (entry != null)
            {
                var group = entry.parentGroup;
                ret.Add(group);
            }
        }
        return ret;
    }

    static AddressableAssetGroup AssetPathToGrpup(string assetPath)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        var settings = AddressableExt.AddressableSettings;
        var entry = settings.FindAssetEntry(guid);
        if (entry != null)
        {
            var group = entry.parentGroup;
            return group;
        }
        else
        {
            return null;
        }
    }

    static HashSet<AddressableAssetGroup> AssetPathListToGroupList(string[] assetPathList)
    {
        HashSet<AddressableAssetGroup> ret = new HashSet<AddressableAssetGroup>();
        foreach (var assetPath in assetPathList)
        {
            var group = AssetPathToGrpup(assetPath);
            if (group != null)
            {
                ret.Add(group);
            }
        }
        return ret;
    }

    static public void SetAdd<T>(HashSet<T> origin, HashSet<T> addtional)
    {
        foreach (var one in addtional)
        {
            origin.Add(one);
        }
    }

    static HashSet<AddressableAssetGroup> InternalGetGroupDirectDependency(AddressableAssetGroup group)
    {
        var ret = new HashSet<AddressableAssetGroup>();
        var entryList = group.entries;
        foreach (var entry in entryList)
        {
            var assetPath = entry.AssetPath;
            var childDpAssetPathList = AssetDatabase.GetDependencies(assetPath, false);
            for (int i = 0; i < childDpAssetPathList.Length; i++)
            {
                DependencyInfo.Add($"{assetPath} => {childDpAssetPathList[i]}");
            }
            var childDpGroupSet = AssetPathListToGroupList(childDpAssetPathList);
            SetAdd(ret, childDpGroupSet);
        }
        return ret;
    }

    static HashSet<AddressableAssetGroup> ReolveGroupDependency(HashSet<AddressableAssetGroup> rootGroupSet)
    {
        var accessedSet = new HashSet<AddressableAssetGroup>();
        foreach (var group in rootGroupSet)
        {
            InternalAccess(group, accessedSet);
        }
        return accessedSet;
    }


    static void InternalAccess(AddressableAssetGroup group, HashSet<AddressableAssetGroup> accessedSet)
    {
        if (accessedSet.Contains(group))
        {
            return;
        }
        accessedSet.Add(group);
        var childGroupSet = InternalGetGroupDirectDependency(group);
        foreach (var childGroup in childGroupSet)
        {
            InternalAccess(childGroup, accessedSet);
        }
    }

    HashSet<string> ResolveAssetsDependency(HashSet<Object> rootAssetSet)
    {
        var pathSet = new HashSet<string>();
        foreach (var one in rootAssetSet)
        {
            var path = AssetToPath(one);
            pathSet.Add(path);
        }
        var ret = ResolveAssetsDependency(pathSet);
        return ret;
    }

    HashSet<string> ResolveAssetsDependency(HashSet<string> rootAssetPathSet)
    {
        var ret = new HashSet<string>();
        var list = new List<string>(rootAssetPathSet);
        var allAssetPathList = AssetDatabase.GetDependencies(list.ToArray());
        foreach (var assetPath in allAssetPathList)
        {
            ret.Add(assetPath);
        }
        return ret;
    }

    HashSet<string> GetAllAssetPathHashSetFromGroupSet(HashSet<AddressableAssetGroup> groupSet)
    {
        var ret = new HashSet<string>();
        foreach (var g in groupSet)
        {
            var entryList = g.entries;
            foreach (var entry in entryList)
            {
                var assetPath = entry.AssetPath;
                ret.Add(assetPath);
            }
        }
        return ret;
    }

}
#endif
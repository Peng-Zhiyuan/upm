using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public static class IOUtil 
{
    public static void DeleteDirectoryIfExits(string path)
    {
        var b = Directory.Exists(path);
        if(b)
        {
            Directory.Delete(path, true);
        }
    }


    /// <summary>
    /// 同步目录中的内容
    /// </summary>
    /// <param name="sourceDir">源目录</param>
    /// <param name="targetDir">目标目录</param>
    /// <param name="ignoreKeywordList">忽略文件名关键词，再两边的目录中都不会进行处理</param>
    /// <param name="targetKeepRootItemList">目标目录保留项目，目标文件夹中不会删除这些项目</param>
    public static void SyncDir(string sourceDir, string targetDir, string[] ignoreKeywordList = null, string[] targetKeepRootItemList = null)
    {
        if(ignoreKeywordList == null)
        {
            ignoreKeywordList = new string[] { };
        }
        if(targetKeepRootItemList == null)
        {
            targetKeepRootItemList = new string[] { };
        }

        if(!Directory.Exists(sourceDir))
        {
            throw new Exception("[IOUtil] sourceDir " + sourceDir + " not exists");
        }
        if(!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        var sourceMap = CreatePathInfo(sourceDir, ignoreKeywordList);
        var targetMap = CreatePathInfo(targetDir, ignoreKeywordList);

        foreach (var kv in sourceMap)
        {
            var relativePath = kv.Key;
            var info = kv.Value;
            var isExsitsInTarget = targetMap.ContainsKey(relativePath);
            if (isExsitsInTarget)
            {
                // 两边都有，进行更新
                var from = info.path;
                var to = targetDir + relativePath;
                EnsureDirAndCopy(from, to);
            }
            else
            {
                // 目标目的没有，输出新文件
                var from = info.path;
                var to = targetDir + relativePath;
                EnsureDirAndCopy(from, to);
            }
        }

        var keepItemList = new List<string>(targetKeepRootItemList);
        // 计算在源中已删除的文件
        foreach (var kv in targetMap)
        {
            var relativePath = kv.Key;
            var rootItem = GetFirstItemName(relativePath);
            if (keepItemList.Contains(rootItem))
            {
                continue;
            }
            var info = kv.Value;
            var isExsitsInSource = sourceMap.ContainsKey(relativePath);
            if (!isExsitsInSource)
            {
                // 源中没有此文件，进行删除
                var path = info.path;
                File.Delete(path);
            }

        }

    }

    /// <summary>
    /// 删除内部任何空文件夹（好像与bug，部分空文件夹不会删除，需要调查）
    /// </summary>
    /// <param name="dir">根文件夹(如果这个文件夹为空也会删除)</param>
    /// <param name="ignoreExtension">这些后缀的文件会当作没有</param>
    public static void DeleteEmptyDir(string dir, List<string> ignoreExtension)
    {
        var subDirList = Directory.GetDirectories(dir, "*", SearchOption.TopDirectoryOnly);
        foreach (var subDir in subDirList)
        {
            DeleteEmptyDir(subDir, ignoreExtension);
        }
        var entryList = new List<string>(Directory.GetFileSystemEntries(dir));
        for (int i = entryList.Count - 1; i >= 0; i--)
        {
            var path = entryList[i];
            var extension = Path.GetExtension(path);
            if (ignoreExtension.Contains(extension))
            {
                entryList.RemoveAt(i);
            }
        }
        if (entryList.Count == 0)
        {
            Directory.Delete(dir, true);
        }
    }

    static string GetFirstItemName(string path)
    {
        var index = 0;
        if (path.StartsWith("/"))
        {
            index = 1;
        }
        var parts = path.Split('/');
        var frist = parts[index];
        return frist;
    }

    static void EnsureDirAndCopy(string from, string to)
    {
        var parent = Path.GetDirectoryName(to);
        if (!Directory.Exists(parent))
        {
            Directory.CreateDirectory(parent);
        }
        //Debug.Log("copy: " + from + " -> " + to);
        var extis = File.Exists(to);
        if (extis)
        {
            File.Delete(to);
        }
        File.Copy(from, to);
    }

    static bool MatchAnyKeyword(string test, string[] keywordList)
    {
        foreach(var one in keywordList)
        {
            var b = test.Contains(one);
            if(b)
            {
                return true;
            }
        }
        return false;
    }

    static Dictionary<string, PathInfo> CreatePathInfo(string rootDir, string[] ignoreKeywordList)
    {
        if(!Directory.Exists(rootDir))
        {
            return new Dictionary<string, PathInfo>();
        }

        var sourceMap = new List<string>(Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories));

        // 过滤
        for (int i = sourceMap.Count - 1; i >= 0; i--)
        {
            var path = sourceMap[i];
            var fileName = Path.GetFileName(path);
            var ignore = MatchAnyKeyword(fileName, ignoreKeywordList);
            if (ignore)
            {
                sourceMap.RemoveAt(i);
                continue;
            }

            sourceMap[i] = sourceMap[i].Replace("\\", "/");
        }

        var ret = CreateRelativePathToInfoDic(sourceMap, rootDir);
        return ret;
    }

    static Dictionary<string, PathInfo> CreateRelativePathToInfoDic(List<string> pathList, string rootDir)
    {
        var ret = new Dictionary<string, PathInfo>();
        foreach (var path in pathList)
        {
            var pathInfo = new PathInfo();
            pathInfo.path = path;
            var relativePath = path.Replace(rootDir, "");
            pathInfo.relativePath = relativePath;
            ret[relativePath] = pathInfo;
        }
        return ret;
    }
}

public class PathInfo
{
    public string path;
    public string relativePath;
}




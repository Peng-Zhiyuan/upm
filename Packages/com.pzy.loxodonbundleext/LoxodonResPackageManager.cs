using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Bundles;
using Loxodon.Framework.Contexts;
using Loxodon.Framework.Examples.Bundle;
using System.IO;
using System.Text;


public static class LoxodonResPackageManager
{
    public static async Task ProcessDownloadAsync(string baseUrl, string remoteBundleVersion, Func<float, Task<bool>> onAskDownloadCallback, Action<Progress> onProgressChangedCallback)
    {
        //var resUrl = EnvUtil.RemoteResCdn;
        //var dirPath = RemoteUtil.BundleDir;
        //var bundleVersion = remoteConfig.TryGet<string>("bundleVersion", "");
        //var bundlePath = $"{dirPath}/{remoteBundleVersion}";
        //var root = $"{resUrl}/{bundlePath}";
        var root = $"{baseUrl}/{BundlePlatformDir}/{remoteBundleVersion}";

        Debug.Log($"[ResPackageManager] root: {root}");
        var rootUrl = new Uri(root);
        var ossWebRequestDownloader = new UnityOSSWebRequestDownloader(rootUrl);

        var manifest = await DownloadManifestIfNeedAsync(remoteBundleVersion, ossWebRequestDownloader);
        await DownloadBundleIfNeedAsync(manifest, ossWebRequestDownloader, onAskDownloadCallback, onProgressChangedCallback);
        DeleteUnusedBundle(manifest);
    }

    public static string BundlePlatformDir
    {
        get
        {
            var platform = PlatformUtil.RuntimePlatformName;
            if (platform == "android")
            {
                return "bundle-android";
            }
            else if (platform == "ios")
            {
                return "bundle-ios";
            }
            else if (platform == "editor")
            {
                return "bundle-editor";
            }
            throw new Exception("unsupport target paltform: " + platform);
        }
    }

    public static void DeleteDownloadResPakcgae()
    {
        var dir = Application.persistentDataPath + "/" + BundleSetting.BundleRoot;
        if(!Directory.Exists(dir))
        {
            Debug.Log($"[ResPackageManager] there is no any res pcakge");
        }
        var filePathList = Directory.GetFiles(dir);
        var deletedCount = 0;
        var beforeFileCount = filePathList.Length;
        foreach(var filePath in filePathList)
        {
            File.Delete(filePath);
            Debug.Log($"[ResPackageManager] delete file: {filePath}");
            deletedCount++;
        }
        Debug.Log($"[ResPackageManager] {deletedCount} files deleted");
    }

    /// <summary>
    /// 获得已下载到本地的资源包的文件总数
    /// </summary>
    public static int DownloadResPakcgaeFileCount
    {
        get
        {
            var dir = Application.persistentDataPath + "/" + BundleSetting.BundleRoot;
            if(!Directory.Exists(dir))
            {
                return 0;
            }
            var filePathList = Directory.GetFiles(dir);
            var count = filePathList.Length;
            return count;
        }
    }

    /// <summary>
    /// 获得已下载到本地的资源包的大小 (字节)
    /// </summary>
    public static long DownloadResPakcgaeFileTotalSize
    {
        get
        {
            var dir = Application.persistentDataPath + "/" + BundleSetting.BundleRoot;
            if(!Directory.Exists(dir))
            {
                return 0;
            }
            var filePathList = Directory.GetFiles(dir);
            var totalSize = 0L;
            foreach(var path in filePathList)
            {
                var fileInfo = new FileInfo(path);
                totalSize += fileInfo.Length;
            }
            return totalSize;
        }
    }

    private static async Task<BundleManifest> DownloadManifestIfNeedAsync(string remoteVersion, UnityOSSWebRequestDownloader ossWebRequestDownloader)
    {
        //var remoteVersion = remoteConfig.TryGet<string>("bundleVersion", "");


        //var localManifest = GameBundleUtil.LoadLocalManifestInRemoteType();
        var localManifest = LoxodonBundleUtil.LoadLocalManifestInRemoteType();
        if (localManifest != null)
        {
            var localVersion = localManifest.Version;
            if (localVersion == remoteVersion)
            {
                return localManifest;
            }
        }


        var manifest = await DownloadManifestAsync(ossWebRequestDownloader);
        return manifest;
    }

    private static Task<BundleManifest> DownloadManifestAsync(UnityOSSWebRequestDownloader downloader) 
    {
        var loader = downloader.DownloadManifest(BundleSetting.ManifestFilename);
        var tcs = new TaskCompletionSource<BundleManifest>();
        loader.Callbackable().OnCallback(r =>
         {
            if(r.Exception!=null) 
            {
                tcs.SetException(r.Exception);
                return;
            }
            tcs.SetResult(r.Result);
        });

        return tcs.Task;
    }

    private static void DeleteUnusedBundle(BundleManifest manifest)
    {
        var version = manifest.Version;
        Debug.Log("[ResPakcgaeManager] delete file which not need of version: " + version);
        var bundleInfoList = manifest.GetAll();
        var fileNameDic = new Dictionary<string, bool>();
        foreach(var info in bundleInfoList)
        {
            var name = info.Filename;
            fileNameDic[name] = true;
        }
        var dir = Application.persistentDataPath + "/" + BundleSetting.BundleRoot;
        Debug.Log("[ResPakcgaeManager] persistent folder: " + dir);
        if(!Directory.Exists(dir))
        {
            Debug.Log($"[ResPackageManager] there is no any download res package");
            return;
        }
        var filePathList = Directory.GetFiles(dir);
        var deletedCount = 0;
        var beforeFileCount = filePathList.Length;
        foreach(var filePath in filePathList)
        {
            var name = System.IO.Path.GetFileName(filePath);
            if(name == "manifest.dat")
            {
                continue;
            }
            if(name.EndsWith(".bak"))
            {
                continue;
            }
            bool need;
            fileNameDic.TryGetValue(name, out need);
            if(!need)
            {
                File.Delete(filePath);
                Debug.Log($"[ResPackageManager] delete file: {filePath}");
                deletedCount++;
            }
        }
        Debug.Log($"[ResPackageManager] {deletedCount} files deleted, before file Count: " + beforeFileCount);
    }

    private static async Task DownloadBundleIfNeedAsync(BundleManifest manifest,  UnityOSSWebRequestDownloader ossWebRequestDownloader, Func<float, Task<bool>> onAskDownloadCallback, Action<Progress> onProgressChangedCallback)
    {			
        var bundleInfoList = await GetBundleInfoListAsync(ossWebRequestDownloader, manifest);
        float totalsize = ossWebRequestDownloader.GetDownLoadSize(bundleInfoList, UNIT.MB);

        Debug.Log($"[ResPakcageManager] totalsize: {totalsize}");
        var needDownload = totalsize > 0;
        if(needDownload) 
        {
            //var b = await AskDownloadBundleAsync(totalsize);
            var b = await onAskDownloadCallback(totalsize);
            if(!b)
            {
                Application.Quit();
            }
            //_uiDialog.SetActive(false);
            await DownloadBundles(ossWebRequestDownloader, bundleInfoList, onProgressChangedCallback);
        } 
        else
        {
            Debug.Log($"[ResPackageManager] all file in local");
        }
    }

    private static Task<bool> DownloadBundles(UnityOSSWebRequestDownloader downloader, List<BundleInfo> bundles, Action<Progress> onProgressChangedCallback) 
    {
        var result = downloader.DownloadBundles(bundles);
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        result.Callbackable().OnCallback(r => 
        {
            if(r.Exception!=null) 
            {
                tcs.SetException(r.Exception);
                return;
            }
            tcs.SetResult(r.Result);

        });
        
        result.Callbackable().OnProgressCallback(p => 
        {
            onProgressChangedCallback?.Invoke(p);
        });

        return tcs.Task;
    }

    private static Task<List<BundleInfo>> GetBundleInfoListAsync(UnityOSSWebRequestDownloader downloader, BundleManifest bundleManifest) 
    {
        var result = downloader.GetDownloadList(bundleManifest);
        var tcs = new TaskCompletionSource<List<BundleInfo>>();
        result.Callbackable().OnCallback(r => 
        {
            if(r.Exception != null) 
            {
                tcs.SetException(r.Exception);
                return;
            }
            tcs.SetResult(r.Result);

        });
        return tcs.Task;
    }


}
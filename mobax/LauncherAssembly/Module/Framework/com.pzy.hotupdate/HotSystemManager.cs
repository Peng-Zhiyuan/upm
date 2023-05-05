using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using System.Threading.Tasks;
using HybridCLR;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System;
using System.Reflection;
using Ionic.Zlib;
using UnityEngine.Scripting;
using System.Net;
using System.Net.Http;

public static class HotSystemManager 
{
    [Preserve]
    public static string RemoteAddressableDirPath
    {
        get
        {
            var subEnvUrl = Remote.Stuff.SubEnvUrl;
            var ret = $"{subEnvUrl}/Addressable";
            return ret;
        }
    }


    public static async Task InitalizeAsync(bool isUpdateAllowed, Func<float, List<DownloadEntry>, Task<bool>> onComfirmDownload, Action<float> onProgress, Action<DownloadStatus> onReportProgress)
    {
        onProgress.Invoke(0.1f);

        // 注册资源重定向
        if (!AddressableUtil.IsUseAssetDatabase)
        {
            // 如果开启了资源内置
            //var embed = bool.Parse(LauncherBuildInfo.Get("embed", "false"));
            //if (embed)
            //{
            //    await AddressableUtil.SetupEmbededBudleRedirectAsync();
            //}
            Debug.Log("AddressableUtil.SetupEmbededBudleRedirectAsync");
            await AddressableUtil.SetupEmbededBudleRedirectAsync();
            onProgress.Invoke(0.2f);
        }

        // 更新目录
        if (isUpdateAllowed)
        {
            // 更新资源目录
            Debug.Log("AddressableUtil.AddressableUpdateCatlog");
            await AddressableUtil.AddressableUpdateCatlog();
            onProgress.Invoke(0.3f);
        }

        // 下载缺失的 bundle
        var label = LauncherDownloadResLabel;
        Debug.Log("AddressableUtil.DownloadBundle");

        AddressableUtil.CanCacheType = false;
        var result = await AddressableUtil.DownloadBundle(label, onComfirmDownload, onReportProgress);
        if(result.status == ResDownloadStatus.NeedDownloadButUserRefused)
        {
            Application.Quit();
        }
        else if(result.status == ResDownloadStatus.DownloadingStarted)
        {
            var task = result.downloadTask;
            await task;
        }
        AddressableUtil.CanCacheType = true;

        var isHclrEnbaled = IsHclrEnabled;
        // 如果启用了 Hclr，先加载代码
        // 代码需要再任何其他资源加载前加载
        // 因为资源类型在热更程序集中
        if (isHclrEnbaled)
        {
            Debug.Log("HotSystemManager.LoadAssemblyAndAotMetadata"); 
            await HclrManager.TryLoadAotMetadataAsync();
            await LoadAssembly();
            onProgress.Invoke(0.5f);
        }

        // 初始化 Addressable
        Debug.Log("AddressableUtil.InitalizeAsync");
        onProgress.Invoke(0.6f);
        await AddressableUtil.InitalizeAsync();

        // 补充 unity 自动调用
        if (isHclrEnbaled)
        {
            Debug.Log("[LauncherManager] call unit auto init");
            await UnityAutoInitManager.AutoCallAsync();
        }
        // 
        onProgress.Invoke(0.8f);
        Debug.Log("HotBuildInfo.InitAsync");
        await HotBuildInfo.InitAsync();

        //onReportProgress(1f);
    }

    static string LauncherDownloadResLabel
    {
        get
        {
            var b = DeveloperLocalSettings.IsAheadLogic;
            if(b)
            {
                return "ahead";
            }
            else
            {
                return "any";
            }
        }
    }


    public static bool IsHclrEnabled
    {
        get
        {
            if (Application.isEditor)
            {
                return false;
            }

            var useHclr = LauncherBuildInfo.Get("useHclr", "false");
            if (useHclr == "true")
            {
                return true;
            }
            return false;

        }
    }



    static bool hotAssemblyAlreadyLoaded;
    static Exception preloadException;
    static async Task LoadAssembly()
    {
        if(hotAssemblyAlreadyLoaded)
        {
            if(preloadException != null)
            {
                throw preloadException;
            }
            else
            {
                return;
            }
        }
        
        Debug.Log($"[ResPackageManager] loading assembly data...");
        var data = await LoadAssemblyDataAsync();
        byte[] pdbData = null;

        // 符号文件似乎不生效
        //if (ShowDeveloperMsg)
        //{
        //    Debug.Log($"[ResPackageManager] pdb data loaded");
        //    pdbData = await GetCurrentPdbDataAsync();
        //}

        try
        {
            hotAssemblyAlreadyLoaded = true;
            Assembly.Load(data, pdbData);
        }
        catch(Exception e)
        {
            preloadException = e;
            throw;
        }
        Debug.Log($"[ResPackageManager] assembly loaded");
    }
    
    /// <summary>
    /// 在当前平台下 Addressable 系统内的平台名称
    /// </summary>
    private static string PlatformName
    {
        get
        {
            var platform = Application.platform;
            switch (platform)
            {
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return "Windows";
                case RuntimePlatform.Android:
                    return "Android";
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                // 添加其他平台的处理逻辑
                default:
                    throw new NotImplementedException("Unsupported platform: " + platform);
            }
        }
    
    }

    static async Task<byte[]> LoadAssemblyDataFromAddressableAsync()
    {
        var asset = await Addressables.LoadAssetAsync<TextAsset>("a.bytes").Task;
        var data = asset.bytes;
        return data;
    }

    static async Task<byte[]> LoadAssemblyDataAsync()
    {
        if(HotSystemConfig.overrideAssemblyData)
        {
            // 使用外部程序集
            var addressableDir = RemoteAddressableDirPath;
            var platform = PlatformName;
            var url = $"{addressableDir}/{PlatformName}/a.bytes";
            var data = await Request(url, false);
            return data;
        }
        else
        {
            var data = await LoadAssemblyDataFromAddressableAsync();
            return data;
        }

    }

    public async static Task<byte[]> Request(string url, bool allowFail)
    {


        Debug.Log($"[{nameof(WebResDownloader)}] LoadAsBytesAsync: {url}");

        var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
        var clinet = new HttpClient(handler);
        clinet.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        var result = await clinet.GetAsync(url);



        if (!result.IsSuccessStatusCode)
        {
            var msg = $"[{nameof(WebResDownloader)}] Http error in load {url}, code: {result.StatusCode}";
            //throw new GameException(ExceptionLevel.Dialog, msg);
            if (!allowFail)
            {
                throw new Exception(msg);
            }
            else
            {
                Debug.LogWarning(msg);
                return null;
            }
        }
        var data = await result.Content.ReadAsByteArrayAsync();
        Debug.Log($"[GetHttpByteData] Http data length : {data.Length}");
        return data;
    }

}

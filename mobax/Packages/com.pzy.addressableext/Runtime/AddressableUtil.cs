using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using System.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Text;
using CustomLitJson;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Reflection;

public static class AddressableUtil
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("AddressableExt/Clean Downloeded Bundles")]
#endif
    public static void CleanDownloadedBundles()
    {
        Caching.ClearCache();
    }

    public async static Task InitalizeAsync()
    {
        var op = Addressables.InitializeAsync(false);
        await WaitOperationAsync(op);
    }

    public static Assembly GetAssembly(string name)
    {
        var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblyList)
        {
            if (assembly.GetName().Name == name)
            {
                return assembly;
            }
        }
        return null;
    }

    public static string PlayModeScriptName
    {
        get
        {
            if (Application.isEditor)
            {
                var assembly = GetAssembly("AddressableExt.Editor");
                var type = assembly.GetType("AddressableEditorUtil");
                var proeprty = type.GetProperty("PlayModeScriptName");
                var name = proeprty.GetValue(null) as string;
                return name;
            }
            else
            {
                return null;
            }
        }
    }

    public static bool IsUseAssetDatabase
    {
        get
        {
            if (Application.isEditor)
            {
                var name = PlayModeScriptName;
                if (name == "Use Asset Database (fastest)")
                {
                    return true;
                }
                else if (name == "Simulate Groups (advanced)")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }


    public async static Task<long> GetTotalDownloadSizeAsyncInByte(string label)
    {
        var size = await Addressables.GetDownloadSizeAsync(label).Task;
        return size;
    }

    static async Task<bool> TryComfirmByUserAsync(Func<float, List<DownloadEntry>, Task<bool>> handller, float sizeM, List<DownloadEntry> downloadEntryList)
    {
        if (handller == null)
        {
            return true;
        }
        var result = await handller.Invoke(sizeM, downloadEntryList);
        return result;
    }

    static float ByteToM(long sizeInByte)
    {
        var sizeM = (float)sizeInByte / 1024f / 1024f;
        return sizeM;
    }


    public static Func<bool> simulateDownloadInEditorDelegate;
    static async Task<(ResDownloadStatus status, Task downloadTask)> DownloadBundleInAssetDatabaseCaseAsync(string label, Func<float, List<DownloadEntry>, Task<bool>> onComfirmDownload, Action<DownloadStatus> onDownloadStatucChanged)
    {
        var simulateDownload = simulateDownloadInEditorDelegate?.Invoke();
        if (simulateDownload != null && simulateDownload.Value == true)
        {
            // 模拟下载:
            // 总大小 100M
            long sizeByte = 100 * 1024 * 1024;
            var sizeM = ByteToM(sizeByte);
            var agree = await TryComfirmByUserAsync(onComfirmDownload, sizeM, new List<DownloadEntry>());
            if (agree)
            {
                Func<Task> action = async () =>
                {
                    var lastStatus = new DownloadStatus();
                    lastStatus.TotalBytes = sizeByte;
                    while (lastStatus.Percent < 1)
                    {
                        onDownloadStatucChanged(lastStatus);
                        // 模拟下载：
                        // 1M/s
                        lastStatus.DownloadedBytes += 1024 * 1024;
                        await Task.Delay(1000);
                    }
                    lastStatus.DownloadedBytes = lastStatus.TotalBytes;
                    onDownloadStatucChanged(lastStatus);
                };
                var downlaodTask = action.Invoke();
                return (ResDownloadStatus.DownloadingStarted, downlaodTask);
            }
            else
            {
                // 跳过
                return (ResDownloadStatus.NeedDownloadButUserRefused, null);
            }
        }
        else
        {
            return (ResDownloadStatus.NotNeedDownload, null);
        }
    }

    static async Task<(ResDownloadStatus status, Task downloadTask)> DownloadBundleInProductCaseAsync(string label, Func<float, List<DownloadEntry>, Task<bool>> onComfirmDownload, Action<DownloadStatus> onDownloadStatucChanged)
    {
        long sizeByte = await GetTotalDownloadSizeAsyncInByte(label);
        var entryList = AddressableInject.downloadEntry;

        if (sizeByte == 0)
        {
            Debug.Log("[Address] no bundle need to be download");
            return (ResDownloadStatus.NotNeedDownload, null);
        }

        var sizeM = ByteToM(sizeByte);
        Debug.Log($"[Address] totalDownloadSize: {sizeM:0.00} M({sizeByte} bytes)");
        var agree = false;
        if (sizeM < 1f)
        {
            agree = true;
        }
        else
        {
            agree = await TryComfirmByUserAsync(onComfirmDownload, sizeM, entryList);
        }
        if (!agree)
        {
            return (ResDownloadStatus.NeedDownloadButUserRefused, null);
        }

        Func<Task> action = async () =>
        {
            var downloadOperation = Addressables.DownloadDependenciesAsync(label, false);
            while (!downloadOperation.IsDone)
            {
                var status = downloadOperation.GetDownloadStatus();
                onDownloadStatucChanged(status);
                await Task.Delay(100);
            }
            var exception = downloadOperation.OperationException;
            if (exception != null)
            {
                throw exception;
            }

            var status2 = downloadOperation.GetDownloadStatus();
            onDownloadStatucChanged(status2);
            Addressables.Release(downloadOperation);
        };
        var downloadTask = action.Invoke();
        return (ResDownloadStatus.DownloadingStarted, downloadTask);

       
    }

    public static bool CanCacheType
    {
        get
        {
            return ContentCatalogData.CompactLocation.canCacheType;
        }
        set
        {
            ContentCatalogData.CompactLocation.canCacheType = value;
        }
    }

    public static async Task<(ResDownloadStatus status, Task downloadTask)> DownloadBundle(string label, Func<float, List<DownloadEntry>, Task<bool>> onComfirmDownload, Action<DownloadStatus> onDownloadStatucChanged)
    {
        var useAssetDatabase = IsUseAssetDatabase;
        if (useAssetDatabase)
        {
            var ret = await DownloadBundleInAssetDatabaseCaseAsync(label, onComfirmDownload, onDownloadStatucChanged);
            return ret;
        }
        else
        {
            var ret = await DownloadBundleInProductCaseAsync(label, onComfirmDownload, onDownloadStatucChanged);
            return ret;
        }
        
    }

    public static async Task AddressableUpdateCatlog()
    {
        var useAssetDatabase = IsUseAssetDatabase;
        if (useAssetDatabase)
        {
            Debug.Log("[Address] Use AsssetDatabase, skip UpdateCatalog");
            return;
        }
        var op1 = Addressables.CheckForCatalogUpdates(false);
        var catalogListNeedToUpdate = await WaitOperationAsync(op1);
        if (catalogListNeedToUpdate.Count == 0)
        {
            // 没有需要更新的目录
            Debug.Log("[Address] no catalog need to be update");
            return;
        }

        var op = Addressables.UpdateCatalogs(null, false);
        await WaitOperationAsync(op);
        Debug.Log("[Address] UpdateCatalog success");
    }

    public async static Task<T> WaitOperationAsync<T>(AsyncOperationHandle<T> op)
    {
        await op.Task;
        if (op.Status != AsyncOperationStatus.Succeeded)
        {
            var exception = op.OperationException;
            throw exception;
        }
        var ret = op.Result;
        Addressables.Release(op);
        return ret;
    }

    public static async Task SetupEmbededBudleRedirectAsync()
    {
        await LoadEmbededIndexAsync();
        Addressables.InternalIdTransformFunc = RedirectBundlePath;
    }

    static string RedirectBundlePath(IResourceLocation location)
    {
        if(location.Data is AssetBundleRequestOptions)
        {

            var filename = location.PrimaryKey;
            var isFileEmbed = IsFileEmbeded(filename);
            if (isFileEmbed)
            {
                //Debug.Log($"[Addressable] {location.PrimaryKey} found in emebded bundle");
                var embededPath = GetEmbedFilePath(filename);
                return embededPath;
            }
        }
        return location.InternalId;
    }

    static string GetEmbedFilePath(string filename)
    {
        var ret = $"{Addressables.RuntimePath}/Embeded/{filename}";
        return ret;
    }

    static string LoadIndexJsonFromEditorRuntimePath()
    {
        var path = $"{Addressables.RuntimePath}/Embeded/index.json";
        if(!File.Exists(path))
        {
            return "{}";
        }
        var json = File.ReadAllText(path);
        return json;
    }

    static async Task<string> LoadIndexJsonFromSteamingAsync()
    {
        var jsonPath = $"aa/Embeded/index.json";
        var json = await StreamingUtil.LoadStreamingAssetsAsync<string>(jsonPath);
        return json;

    }

    static string LoadIndexJsonFromResource()
    {
        var asset = Resources.Load<TextAsset>("embededBundleIndex");
        var json = asset.text;
        return json;
    }

    static async Task<string> LoadIndexJson()
    {
        if(Application.isEditor)
        {
            var ret = LoadIndexJsonFromEditorRuntimePath();
            return ret;
        }
        else
        {
            var ret = await LoadIndexJsonFromSteamingAsync();
            return ret;
        }
    }

    static async Task LoadEmbededIndexAsync()
    {
        var json = await LoadIndexJson();
        Debug.Log("[AddressableUtil] embeded bundle index: " + json);
        var list = JsonMapper.Instance.ToObject<List<string>>(json);
        embededBundleNameToTrueDic = new Dictionary<string, bool>();
        foreach (var one in list)
        {
            embededBundleNameToTrueDic[one] = true;
        }
    }

    static Dictionary<string, bool> embededBundleNameToTrueDic;

    public static bool IsFileEmbeded(string filename)
    {
        if(embededBundleNameToTrueDic == null)
        {
            throw new Exception("[AddressableUtil] embed index not load yet.");
        }
        var b = embededBundleNameToTrueDic.ContainsKey(filename);
        return b;
    }


 
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Loxodon.Framework.Asynchronous;
using Loxodon.Framework.Bundles;
using Loxodon.Framework.Contexts;
using UnityEngine;
using UnityEngine.SceneManagement;
using Sirenix.OdinInspector;
using IAsyncResult = Loxodon.Framework.Asynchronous.IAsyncResult;

public class AssetManager : StuffObject<AssetManager>
{
    /// <summary>
    /// 需要驱动：如何阻挡屏幕用户输入
    /// </summary>
    public static Action<bool, string> OnRequestBlock;

    [Sirenix.OdinInspector.ReadOnly]
    public Dictionary<string, IBundle> nameToLoadedBundleDic = new Dictionary<string, IBundle>();

    public IBundle GetBundleInLoadedDic(string bundleName)
    {
        if(!nameToLoadedBundleDic.ContainsKey(bundleName))
        {
            return null;
        }
        var bundle = nameToLoadedBundleDic[bundleName];
        return bundle;
    }

    /// <summary>
    /// 尝试加载所有 bundle
    /// 如果某个 bundle 已经被打开，则会跳过
    /// </summary>
    /// <param name="progressCallback"></param>
    /// <returns></returns>
    public async Task<int> TryLoadAllBundleAsync(Action<float> progressCallback)
    {
        var bundleNameList = this.GetAllBundleNames();
        var loadBundleCount = await this.LoadBundlesAsync(bundleNameList.ToArray(), progressCallback);
        return loadBundleCount;
    }


    Task<int> LoadBundlesAsync(string[] bundleNames, Action<float> onProgressCallback)
    {
        var tcs = new TaskCompletionSource<int>();
        var nameList = new List<string>();
        foreach (var name in bundleNames)
        {
            var loaded = this.IsBundleLoaded(name);
            if (loaded)
            {
                continue;
            }
            nameList.Add(name);
        }
        if (nameList.Count == 0)
        {
            tcs.SetResult(0);
            return tcs.Task;
        }
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var loader = resources.LoadBundle(nameList.ToArray(), 0);


        loader.Callbackable().OnProgressCallback(onProgressCallback);
        
        loader.Callbackable().OnCallback(r =>
        {
            if (r.Exception != null)
            {
                tcs.SetException(r.Exception);
                return;
            }
            var ret = r.Result;
            foreach (var bundle in ret)
            {
                var name = bundle.Name;
                nameToLoadedBundleDic[name] = bundle;
            }

            var loadBundleCount = ret.Length;
            tcs.SetResult(loadBundleCount);
        });

        return tcs.Task;
    }

    bool IsBundleLoaded(string bundleName)
    {
        if (this.nameToLoadedBundleDic.ContainsKey(bundleName))
        {
            return true;
        }
        return false;
    }


    //void AddToLoadedAssets(string bundleName, string assetPath, UnityEngine.Object asset)
    //{
    //    var useage = GetBundleUsage(bundleName);
    //    if(useage == null)
    //    {
    //        // 可能正在进行资源GC，bundle 已经被关闭，不缓存这个资源
    //        Debug.LogError("可能正在进行资源GC，bundle 已经被关闭，不缓存这个资源.");
    //        return;
    //    }
    //    useage.pathToLoadedAsset[assetPath] = asset;
    //}

    bool ThrowExceptionWhenGcLoading
    {
        get
        {
            return false;
        }
    }


    bool isRunningAssetGC = false;
    public async Task AssetGC(bool block = true)
    {
        if (isRunningAssetGC)
        {
            return;
        }


        Debug.Log("[AssetManager] 开始卸载未使用的资源");
        if(block)
        {
            //BlockManager.Show("AssetGC");
            OnRequestBlock?.Invoke(true, "AssetGC");
        }

        isRunningAssetGC = true;
        gcBlockedAsyncLoaderCount = 0;

        //var runningLoaderCount = this.pathToLoaderDic.Count;
        //Debug.Log($"[AssetManager] gc while running loader count {runningLoaderCount}");

        var runningLoaderCount = this.pathToLoaderDic.Count;
        Debug.Log($"[AssetManager] gc while running loader count {runningLoaderCount}, wait complete");
        if (runningLoaderCount > 0)
        {
            await WaitingAllRunningLoaderComplete();
        }

        // 关闭所有 bundle
        //BlockManager.Show("dispose all bundle");
        //OnRequestBlock?.Invoke(true, "dispose all bundle");
        Debug.Log("[AssetManager] dispose all bundle");
        foreach (var kv in nameToLoadedBundleDic)
        {
            var bundle = kv.Value;
            bundle.Dispose();
        }
        //BlockManager.Hide("dispose all bundle");
        //OnRequestBlock?.Invoke(false, "dispose all bundle");

        // 移除所有缓存引用
        nameToLoadedBundleDic.Clear();

        // gc
        // Resources.UnloadUnusedAssets();
        Debug.Log("[AssetManager] UnloadUnusedAssetsAsync");
        //BlockManager.Show("UnloadUnusedAssetsAsync");
        //OnRequestBlock?.Invoke(true, "UnloadUnusedAssetsAsync");
        await UnloadUnusedAssetsAsync();
        //BlockManager.Hide("UnloadUnusedAssetsAsync");
        //OnRequestBlock?.Invoke(false, "UnloadUnusedAssetsAsync");

        // 重新加载所有 bundle
        //var bundleNameList = this.GetAllBundleNames();
        //await LoadBundlesAsync(bundleNameList.ToArray(), null);
        Debug.Log("[AssetManager] TryLoadAllBundleAsync");
        //BlockManager.Show("TryLoadAllBundleAsync");
        //OnRequestBlock?.Invoke(true, "TryLoadAllBundleAsync");
        var loadBundleCount = await AssetManager.Stuff.TryLoadAllBundleAsync(null);
        //BlockManager.Hide("TryLoadAllBundleAsync");
        //OnRequestBlock?.Invoke(false, "TryLoadAllBundleAsync");

        isRunningAssetGC = false;
        Debug.Log($"[AssetManager] 资源清理完毕: {runningLoaderCount} loader running, {gcBlockedAsyncLoaderCount} new loader delayed, {loadBundleCount} bundle loaded");
        //BlockManager.Hide("AssetGC");
        OnRequestBlock?.Invoke(false, "AssetGC");
    }

    public async Task WaitingAllRunningLoaderComplete()
    {
        while(this.pathToLoaderDic.Count > 0)
        {
            //await TimeWaiterManager.Stuff.WaitAsync(200);
            await Task.Delay(200);
        }
    }

    public Task UnloadUnusedAssetsAsync()
    {
        //await AssetManager.Stuff.AssetGC(block);
        var tcs = new TaskCompletionSource<bool>();
        Debug.Log("[AssetManager] call Resources.UnloadUnusedAssets()");
        var operation = Resources.UnloadUnusedAssets();
        Debug.Log("[AssetManager] post call Resources.UnloadUnusedAssets()");
        operation.completed += a =>
        {
            tcs.SetResult(true);
        };
        return tcs.Task;
    }


    string GetBundleName(string assetPath)
    {
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var abResource = resources as AbstractResources;
        var pathInfo = abResource.ParsePath(assetPath);
        if(pathInfo == null)
        {
            throw new Exception("bundleInfo not found of assetPath: " + assetPath);
        }
        var bundleName = pathInfo.BundleName;
        return bundleName;
    }

    /// <summary>
    /// 获得所有 bundle 的名字
    /// </summary>
    /// <returns></returns>
    List<string> GetAllBundleNames()
    {
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var abResource = resources as AbstractResources;
        var bundleNameList = abResource.GetAllBundleNames();
        return bundleNameList;
    }

    ///// <summary>
    ///// 获得指定 bundle 中所有资源的 path 列表
    ///// </summary>
    ///// <param name="bundleName"></param>
    ///// <returns></returns>
    //public string[] GetAssetPathListOfBundle(string bundleName)
    //{
    //    var context = Context.GetApplicationContext();
    //    var resources = context.GetService<IResources>();
    //    var abResource = resources as AbstractResources;
    //    var assetPathList = abResource.GetBundleAssetNames(bundleName);
    //    return assetPathList;
    //}

    ///// <summary>
    ///// 尝试从缓存中获取
    ///// </summary>
    ///// <param name="bundleName"></param>
    ///// <param name="assetPath"></param>
    ///// <returns></returns>
    //UnityEngine.Object TryGetFromCache(string bundleName, string assetPath)
    //{
    //    BundleUsageInfo usageInfo;
    //    nameToLoadedBundleDic.TryGetValue(bundleName, out usageInfo);
    //    if(usageInfo == null)
    //    {
    //        return null;
    //    }
    //    UnityEngine.Object asset;
    //    usageInfo.pathToLoadedAsset.TryGetValue(assetPath, out asset);
    //    return asset;
    //}

    //UnityEngine.Object TryGetFromCache(string assetPath)
    //{
    //    var bundleName = GetBundleName(assetPath);
    //    var asset = TryGetFromCache(bundleName, assetPath);
    //    return asset;
    //}


    async Task WateGcCompleteAsync()
    {
        while(this.isRunningAssetGC)
        {
            //await TimeWaiterManager.Stuff.WaitAsync(200);
            await Task.Delay(200);
        }
    }

    int gcBlockedAsyncLoaderCount;

    /// <summary>
    /// 在后台线程加载资源,
    /// 如果资源已被加载，则会存缓存中直接获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public async Task<T> LoadAssetInBackgroundThreadAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        Debug.Log("[AssetManager] LoadAssetInBackgroundThreadAsync assetPath: " + assetPath);

        if(this.isRunningAssetGC)
        {
            gcBlockedAsyncLoaderCount++;
            await WateGcCompleteAsync();
        }


        if (this.ThrowExceptionWhenGcLoading)
        {
            if (isRunningAssetGC)
            {
                // pzy:
                // 在添加上GC等待后，从养成返回主界面仍然可能出现GC中加载资源
                // 暂时允许在这种情况下加载资源，但愿底层不会出错
                throw new Exception($"load res when asset gc is running. path: {assetPath}");
            }
        }



        //var cachedAsset = TryGetFromCache(assetPath);
        //if(cachedAsset != null)
        //{
        //    return cachedAsset as T;
        //}


        var result = await this.WrapLoadAssetAsync<T>(assetPath);
        if (result == null)
        {
            return null;
        }

        //var bundleName = this.GetBundleName(assetPath);
        //AddToLoadedAssets(bundleName, assetPath, result);
        return result;
    }

    /// <summary>
    /// 在后台线程加载资源,
    /// 如果资源已被加载，则会存缓存中直接获取
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, UnityEngine.Object>> LoadAssetListInBackgroundThreadAsync(List<string> assetPathList)
    {
        Debug.Log("[AssetManager] LoadAssetListInBackgroundThreadAsync assetPathList.count: " + assetPathList.Count);

        if (this.isRunningAssetGC)
        {
            gcBlockedAsyncLoaderCount++;
            await WateGcCompleteAsync();
        }

        if (ThrowExceptionWhenGcLoading)
        {
            if (isRunningAssetGC)
            {
                throw new Exception($"load res when asset gc is running.");
            }
        }

        //var cachedAsset = TryGetFromCache(assetPath);
        //if (cachedAsset != null)
        //{
        //    return cachedAsset as T;
        //}


        var result = await this.WrapLoadAssetListAsync(assetPathList);
        if (result == null)
        {
            return null;
        }

        //var bundleName = this.GetBundleName(assetPath);
        //AddToLoadedAssets(bundleName, assetPath, result);
        return result;
    }

    //int CancelAllNativeLoader()
    //{
    //    var count = 0;
    //    foreach(var kv in pathToLoaderDic)
    //    {
    //        var path = kv.Key;
    //        var loader = kv.Value;
    //        Debug.Log("[AssetManager] cancel loader id: " + path);
    //        loader.Cancel();
    //        count++;
    //    }
    //    Debug.Log($"[AssetManager] {count} loader canceled ");
    //    return count;
    //}

    public Dictionary<int, IAsyncResult> pathToLoaderDic = new Dictionary<int, IAsyncResult>();

    int loaderId = 0;
    private Task<T> WrapLoadAssetAsync<T>(string assetPath) where T : UnityEngine.Object
    {
        var tcs = new TaskCompletionSource<T>();
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var loader = resources.LoadAssetAsync<T>(assetPath);

        // store loader
        var loaderId = this.loaderId++;
        pathToLoaderDic[loaderId] = loader;

        var count = pathToLoaderDic.Count;
        //Debug.Log($"[AssetManager]     loader start for: {assetPath}, running loader count: {count}");

        loader.Callbackable().OnCallback(r =>
        {
            // remove loader
            pathToLoaderDic.Remove(loaderId);

            var count2 = pathToLoaderDic.Count;
            var exception = r.Exception;
            var result2 = r.Result;
            //Debug.Log($"[AssetManager]     loader complete for: {assetPath}, running loader count: {count2}, exception: {exception}, result: {result2}");

            

            if (r.Exception != null)
            {
                tcs.SetException(r.Exception);
                return;
            }
            else
            {
                var result = r.Result;
                tcs.SetResult(result);
            }
        });
        return tcs.Task;
    }

    private Task<Dictionary<string, UnityEngine.Object>> WrapLoadAssetListAsync(List<string> assetPathList) 
    {
        var tcs = new TaskCompletionSource<Dictionary<string, UnityEngine.Object>>();
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var array = assetPathList.ToArray();
        var loader = resources.LoadAssetsToMapAsync(array);

        // store loader
        var loaderId = this.loaderId++;
        pathToLoaderDic[loaderId] = loader;

        var count = pathToLoaderDic.Count;
        //Debug.Log($"[AssetManager]     loader start for multi asset, running loader count: {count}");

        loader.Callbackable().OnCallback(r =>
        {
            // remove loader
            pathToLoaderDic.Remove(loaderId);

            var count2 = pathToLoaderDic.Count;
            var exception = r.Exception;
            var result2 = r.Result;
            //Debug.Log($"[AssetManager]     loader complete for multi asset, running loader count: {count2}, exception: {exception}, result: {result2}");

            if (r.Exception != null)
            {
                tcs.SetException(r.Exception);
                return;
            }
            else
            {
                var result = r.Result;
                tcs.SetResult(result);
            }
        });
        return tcs.Task;
    }

    /// <summary>
    /// 前台加载资源：
    /// 1. 所在的 AssetBundle 必须已经加载。
    /// 2. 会造成画面卡顿，尽量避免使用。
    /// 3. 加载失败的情况会抛出异常
    /// 4. 如果资源已被加载过，则会从缓存中直接返回
    /// </summary>
    public T LoadAsset<T>(string assetPath) where T : UnityEngine.Object
    {
        if(this.ThrowExceptionWhenGcLoading)
        {
            if (isRunningAssetGC)
            {
                throw new Exception($"load res when asset gc is running. path: {assetPath}");
            }
        }

        //var cechedAsset = TryGetFromCache(assetPath);
        //if(cechedAsset != null)
        //{
        //    return cechedAsset as T;
        //}

        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var result = resources.LoadAsset<T>(assetPath);
        if (result == null)
        {
            throw new Exception($"[{nameof(AssetManager)}] LoadAsset: bundle asset exists but bundle not loaded, path: {assetPath}");
        }
        //var abResource = resources as AbstractResources;
        //var pathInfo = abResource.ParsePath(assetPath);
        //var bundleName = pathInfo.BundleName;
        //AddToLoadedAssets(bundleName, assetPath, result);
        return result;
    }



    public Task<Scene> LoadSceneAsync(string scenePath)
    {
        //if (this.ThrowExceptionWhenGcLoading)
        //{
        //    if (isRunningAssetGC)
        //    {
        //        throw new Exception($"load scene when asset gc is running. path: {scenePath}");
        //    }
        //}
        //else if (DialogWhenGcLoading)
        //{
        //    if (isRunningAssetGC)
        //    {
        //        Debug.LogError("load scene while gc");
        //    }
        //}

        var tcs = new TaskCompletionSource<Scene>();
        var context = Context.GetApplicationContext();
        var resources = context.GetService<IResources>();
        var result = resources.LoadSceneAsync(scenePath, LoadSceneMode.Additive);
        result.AllowSceneActivation = false;

        result.OnProgressCallback(p =>
        {
            //Debug.LogFormat("Loading {0}%", (p * 100));
        });

        result.OnStateChangedCallback(state =>
        {
            if (state == LoadState.Failed)
            {
                Debug.LogFormat("Loads scene '{0}' failure.Error:{1}", scenePath, result.Exception);
                tcs.SetException(result.Exception);
            }
            else if (state == LoadState.Completed)
            {
                Debug.LogFormat("Loads scene '{0}' completed.", scenePath);
                var scene = result.Result;
                tcs.SetResult(scene);
            }
            else if (state == LoadState.AssetBundleLoaded)
            {
                Debug.LogFormat("The AssetBundle has been loaded.");
            }
            else if (state == LoadState.SceneActivationReady)
            {
                Debug.LogFormat("Ready to activate the scene.");
                result.AllowSceneActivation = true;
            }
        });
        return tcs.Task;
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceProviders;
using System;
using UnityEngine.UI;

public partial class AddressableRes
{
    public static Dictionary<string, UnityEngine.Object> resDic = new Dictionary<string, UnityEngine.Object>();
    private static Dictionary<string, IList<UnityEngine.Object>> labelsDic = new Dictionary<string, IList<UnityEngine.Object>>();

    public static void releaseAll()
    {
        var data = resDic.GetEnumerator();
        while (data.MoveNext())
        {
            Addressables.Release(data.Current.Value);
        }
        resDic.Clear();
        var data2 = labelsDic.GetEnumerator();
        while (data2.MoveNext())
        {
            for (int i = 0; i < data2.Current.Value.Count; i++)
            {
                Addressables.Release(data2.Current.Value[i]);
            }
        }
        labelsDic.Clear();
    }

    public static void releaseObject(string name)
    {
        if (resDic.ContainsKey(name)
            && resDic[name] != null)
        {
            Addressables.Release(resDic[name]);
        }
        resDic.Remove(name);
    }

    public static void releaseObjects(List<string> names)
    {
        for (int i = 0; i < names.Count; i++)
        {
            if (resDic.ContainsKey(names[i]))
            {
                Addressables.Release(resDic[names[i]]);
            }
            resDic.Remove(names[i]);
        }
    }

    public void releaseInstance(ref GameObject obj)
    {
        if (obj != null)
        {
            Addressables.ReleaseInstance(obj);
        }
    }

    public void releaseAsset<T>(ref T obj) where T : class
    {
        if (obj != null)
            Addressables.Release(obj);
    }

    public static bool isAddressableResLoaded(string addressableName)
    {
        if (resDic.ContainsKey(addressableName))
        {
            return true;
        }
        return false;
    }

    public static TObject takeAddressableRes<TObject>(string addressableName) where TObject : UnityEngine.Object
    {
        if (resDic.ContainsKey(addressableName))
        {
            return resDic[addressableName] as TObject;
        }
        BattleLog.LogError("addressable res is not loaded:" + addressableName);
        return null;
    }

    public static async Task<TObject> loadAddressableResAsync<TObject>(string addressableName, bool loadDepends = true) where TObject : UnityEngine.Object
    {
        if (loadDepends)
        {
            var result = await downloadDependencies(addressableName);
            if (result)
            {
                TObject obj = await loadAddressableRes<TObject>(addressableName);
                return obj;
            }
            return null;
        }
        else
        {
            TObject obj = await loadAddressableRes<TObject>(addressableName);
            return obj;
        }
    }

    private static Task<TObject> loadAddressableRes<TObject>(string addressableName) where TObject : UnityEngine.Object
    {
        var tcs = new TaskCompletionSource<TObject>();
        if (resDic.ContainsKey(addressableName))
        {
            tcs.SetResult(resDic[addressableName] as TObject);
            return tcs.Task;
        }
        Addressables.LoadAssetAsync<TObject>(addressableName).Completed += (AsyncOperationHandle<TObject> op) =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                BattleLog.LogError("Cant find the res " + addressableName);
                tcs.SetResult(null);
                return;
            }
            resDic[addressableName] = op.Result;
            tcs.SetResult(op.Result);
        };
        return tcs.Task;
    }

    public static Task<IList<TObject>> loadAddressableLabelsRes<TObject>(List<string> labels) where TObject : UnityEngine.Object
    {
        var tcs = new TaskCompletionSource<IList<TObject>>();
        //if (labelsDic.ContainsKey(labels)) {
        //    tcs.SetResult(labelsDic[labels] as IList<TObject>);
        //    return tcs.Task;
        //}
        Addressables.LoadAssetsAsync<TObject>(labels, null, Addressables.MergeMode.None).Completed += (AsyncOperationHandle<IList<TObject>> op) =>
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                BattleLog.LogError("Cant find the labels " + labels);
                tcs.SetResult(null);
                return;
            }
            //labelsDic[labels] = op.Result;
            tcs.SetResult(op.Result as IList<TObject>);
        };
        return tcs.Task;
    }

    private static Task<bool> downloadDependencies(string addressableName)
    {
        var tcs = new TaskCompletionSource<bool>();
        Addressables.DownloadDependenciesAsync(addressableName).Completed += (AsyncOperationHandle op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                tcs.SetResult(true);
            }
            else
            {
                tcs.SetResult(false);
            }
        };
        return tcs.Task;
    }

    private Scene currentlyLoadingScene;

    /// <summary>
    /// Loads a given scene either in additive or single mode to the current scene.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    /// <param name="loadMode">Scene load mode.</param>
    public Task<Scene> LoadScene(string sceneName, LoadSceneMode loadMode)
    {
        currentlyLoadingScene = default;
        currentlyLoadingScene.name = "";
        Addressables.LoadSceneAsync(sceneName, loadMode).Completed += AddressablesManager_OnSceneLoadCompleted;
        if (string.IsNullOrEmpty(currentlyLoadingScene.name))
        {
            Task.Delay(1);
        }
        return Task.Run(() => currentlyLoadingScene);
    }

    private void AddressablesManager_OnSceneLoadCompleted(AsyncOperationHandle<SceneInstance> obj)
    {
        if (obj.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            currentlyLoadingScene = obj.Result.Scene;
        }
    }

    /// <summary>
    /// Loads a given scene either in additive or single mode to the current scene.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    /// <param name="loadMode">Scene load mode.</param>
    public async Task OnlyLoadScene(string sceneName, LoadSceneMode loadMode)
    {
        await Task.Run(() => Addressables.LoadSceneAsync(sceneName, loadMode));
    }

    /// <summary>
    /// Unloads a given scene from memory asynchronously.
    /// </summary>
    /// <param name="scene">Scene object to unload.</param>
    public async Task UnloadScene(SceneInstance scene)
    {
        AsyncOperationHandle<SceneInstance> sceneHandle = Addressables.UnloadSceneAsync(scene);
        await sceneHandle.Task;
    }
}

public partial class AddressableRes
{
    // -------------  HRY 2021.5.12 新增 -------------
    // ------------- 希望解决的问题：总结出资源的使用频率，得出一个预测模型
    // 【完成xlua功能后在推进】

    /// <summary>
    /// 资源容器数据模型
    /// 记录资源的Object
    /// 记录资源使用标记
    /// 记忆使用标记次数
    /// 得出使用时间：释放时间 - 加载时间
    /// </summary>
    public struct Container
    {
        public string Key; // 资源键

        public struct Mark
        {
            public string Name;
            /// <summary>
            /// 和，使用增加，释放减少
            /// </summary>
            public int Sum;
        }

        public UnityEngine.Object Src;
        public Mark[] Marks;
        public int Duration;
    }

    // ------------- 希望解决的问题：同步加载和异步加载并存
    // ------------- 同步加载：必须预热
    // ------------- 异步加载：必须等待
    public class Handler
    {
        public int type; // 用于判断结果，是异步还是同步
        public object locater; // 资源定位器，有可能是一个 异步句柄，有可能是一个资源
        public AsyncOperationHandle asyncOperation;
    }

    private static Dictionary<string, UnityEngine.Object> Caches = new Dictionary<string, UnityEngine.Object>(); // 单个加载缓存
    private static Dictionary<string, IList<UnityEngine.Object>> Caches2 = new Dictionary<string, IList<UnityEngine.Object>>(); // 多个加载缓存

    /// <summary>
    /// 批量预处理资源
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="onSuccess">当成功一个返回一个</param>
    public static Task<bool> PreloadAssets(List<string> keys, System.Action<UnityEngine.Object> onSuccess = null)
    {
        // 创建任务
        var taskSource = new TaskCompletionSource<bool>();
        // 异步加载资源
        var handler = Addressables.LoadAssetsAsync(keys, onSuccess, Addressables.MergeMode.None);
        handler.Completed += (handlerComplete) =>
        {
            if (handlerComplete.Status != AsyncOperationStatus.Succeeded)
            {
                string error = "";
                for (int i = 0; i < keys.Count; i++)
                {
                    error += $"[{keys[i]}]";
                }
                BattleLog.LogError($"Cant find the labels: {error}");
                BattleLog.LogError("加载失败了..........");
                // 任务完成（失败）
                taskSource.SetResult(false);
            }
            Caches2.Add(keys[0], handlerComplete.Result);
            BattleLog.Log($"任务完成，预先处理了 <color=#00ff00>{handlerComplete.Result.Count}</color> 个文件");
            // 任务完成（成功）
            taskSource.SetResult(true);
        };
        // 等待任务完成
        return taskSource.Task;
    }

    static Dictionary<string, UnityEngine.Object> loaded = new Dictionary<string, UnityEngine.Object>();

    /// <summary>
    /// 同步加载资源，先暂时这么弄
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static UnityEngine.Object LoadAsset(string key)
    {
        if (resDic.ContainsKey(key))
            return resDic[key];
        if (!loaded.ContainsKey(key))
        {
            loaded.Add(key, null);
        }
        UnityEngine.Object result = null;
        if (loaded[key] == null)
        {
            var handler = Addressables.LoadAssetAsync<UnityEngine.Object>(key);
            result = handler.WaitForCompletion();
            loaded[key] = result;
        }
        else
        {
            result = loaded[key];
        }
        //Addressables.Release(temp); // 这里不能立刻释放，需要考量释放时机
        return result;
    }

    public static UnityEngine.TextAsset LoadTextAsset(string key)
    {
        if (resDic.ContainsKey(key))
            return resDic[key] as UnityEngine.TextAsset;
        if (!loaded.ContainsKey(key))
        {
            loaded.Add(key, null);
        }
        UnityEngine.TextAsset result = null;
        if (loaded[key] == null)
        {
            var handler = Addressables.LoadAssetAsync<UnityEngine.TextAsset>(key);
            result = handler.WaitForCompletion();
            loaded[key] = result;
        }
        else
        {
            result = (UnityEngine.TextAsset)loaded[key];
        }
        //Addressables.Release(temp); // 这里不能立刻释放，需要考量释放时机
        return result;
    }

    public static T LoadAsset<T>(string key) where T : UnityEngine.Object
    {
        if (resDic.ContainsKey(key))
            return (T)resDic[key];
        //Debug.LogWarning("Load Asset " + key);
        if (!loaded.ContainsKey(key))
        {
            loaded.Add(key, null);
        }
        T result = null;
        if (loaded[key] == null)
        {
            var handler = Addressables.LoadAssetAsync<T>(key);
            result = handler.WaitForCompletion();
            loaded[key] = result;
        }
        else
        {
            result = (T)loaded[key];
        }
        return result;
    }

    public static void InstantiateAsync(string key, Transform parent, System.Action<UnityEngine.GameObject> onComplete)
    {
        var handler = Addressables.InstantiateAsync(key, Vector3.zero, Quaternion.identity, parent);
        handler.Completed += (operation) =>
        {
            GameObject clone = operation.Result;
            onComplete?.Invoke(clone);
        };
    }

    public static GameObject Instantiate(string key, Transform parent)
    {
        var handler = Addressables.InstantiateAsync(key, new InstantiationParameters(parent, false));
        return handler.WaitForCompletion();
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    /// <param name="keys"></param>
    /// <param name="onSuccess"> 当成功一个返回一个 </param>
    /// <returns></returns>
    public static Handler LoadAssetsAsync<TObject>(List<string> keys, System.Action<TObject> onSuccess = null)
    {
        //var tcs = new TaskCompletionSource<IList<TObject>>();
        ////if (labelsDic.ContainsKey(labels)) {
        ////    tcs.SetResult(labelsDic[labels] as IList<TObject>);
        ////    return tcs.Task;
        ////}

        //var handler = Addressables.LoadAssetsAsync(keys, onSuccess, Addressables.MergeMode.None);
        //handler.Completed += (handlerComplete) =>
        //{
        //    if (handlerComplete.Status != AsyncOperationStatus.Succeeded)
        //    {
        //        string error = "";
        //        foreach (var item in keys)
        //        {
        //            error += $"[{item}]";
        //        }
        //        Debug.LogError($"Cant find the labels: {error}");
        //        tcs.SetResult(null);
        //        return;
        //    }
        //    tcs.SetResult(handlerComplete.Result);
        //};
        //return tcs.Task;
        return null;
    }

    /// <summary>
    /// 同步加载，未处理资源释放！！！！！！
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Scene LoadScene(string name)
    {
        AsyncOperationHandle handler = Addressables.LoadSceneAsync(name, LoadSceneMode.Single);
        SceneInstance result = (SceneInstance)handler.WaitForCompletion(); // 拆箱了
        return result.Scene;
    }

    public static AsyncOperationHandle LoadSceneAsync(string name, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AsyncOperationHandle handler = Addressables.LoadSceneAsync(name, mode);
        return handler;
    }

    public static void LoadSceneAsyncHandler(string name, Action onComplete, LoadSceneMode mode = LoadSceneMode.Single)
    {
        AsyncOperationHandle handler = Addressables.LoadSceneAsync(name, mode);
        handler.Completed += (info) => { onComplete?.Invoke(); };
    }

    public static async void OnLoadSprite(Image image, string path)
    {
        Sprite sprite = await loadAddressableResAsync<Sprite>(path, false);
        if (sprite != null)
            image.sprite = sprite;
    }

    public static async void OnLoadFont(Text text, string path)
    {
        Font font = await loadAddressableResAsync<Font>(path, false);
        if (font != null)
            text.font = font;
    }
    

    public static TextAsset ResourcesLoadTextAssets(string path)
    {
        TextAsset txt = Resources.Load(path) as TextAsset;
        BattleLog.LogWarning("Resources TextAssets " + txt.bytes);
        return txt;
    }

    public static async Task<AvatarMask> OnLoadAvatarMask(string path)
    {
        AvatarMask avatar = await loadAddressableResAsync<AvatarMask>(path);
        return avatar;
    }

    public static async Task<RuntimeAnimatorController> OnLoadAnimatorController(string path)
    {
        RuntimeAnimatorController avatar = await loadAddressableResAsync<RuntimeAnimatorController>(path);
        return avatar;
    }

    public static async Task<AnimationClip> OnLoadClip(string path)
    {
        AnimationClip clip = await loadAddressableResAsync<AnimationClip>(path);
        return clip;
    }
}
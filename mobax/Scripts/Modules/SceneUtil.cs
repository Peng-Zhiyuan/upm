using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

public static class SceneUtil
{
    static Action onComplete;

    public static void WaitLoadComplete(Action onComplete)
    {
        if (SceneUtil.onComplete != null)
        {
            throw new Exception("[SceneUtil] onComplete alreay set");
        }
        SceneUtil.onComplete = onComplete;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var temp = SceneUtil.onComplete;
        SceneUtil.onComplete = null;
        temp.Invoke();
    }

    public static Task UnloadSceneAsync(string name)
    {
        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.UnloadSceneAsync(name);
        op.completed += (a) => { tcs.SetResult(true); };
        return tcs.Task;
    }

    public static Task LoadSceneAsync(string name, LoadSceneMode mode)
    {
        var tcs = new TaskCompletionSource<bool>();
        var op = SceneManager.LoadSceneAsync(name, mode);
        op.completed += (a) =>
        {
            tcs.SetResult(true);
            var aa = name.Replace(".unity", "");
            WwiseEventManager.SendEvent(TransformTable.SceneLoad, aa);
        };
        return tcs.Task;
    }

    public static Task<SceneInstance> AddressableLoadSceneAsync(string name, LoadSceneMode mode)
    {
        var tcs = new TaskCompletionSource<SceneInstance>();
        var op = Addressables.LoadSceneAsync($"{name}.unity", mode);
        op.Completed += (a) =>
        {
            tcs.SetResult(a.Result);
            WwiseEventManager.SendEvent(TransformTable.SceneLoad, name);
        };
        return tcs.Task;
    }

    public static Task AddressableUnloadSceneAsync(SceneInstance sceneInstance)
    {
        var tcs = new TaskCompletionSource<bool>();
        var op = Addressables.UnloadSceneAsync(sceneInstance);
        op.Completed += (a) =>
        {
            RenderSettings.skybox = null;
            LightmapSettings.lightmaps = new LightmapData[0];
            var activeScene = SceneManager.GetActiveScene().name;
            WwiseEventManager.SendEvent(TransformTable.SceneLoad, activeScene);
            tcs.SetResult(true);
        };
        return tcs.Task;
    }
}
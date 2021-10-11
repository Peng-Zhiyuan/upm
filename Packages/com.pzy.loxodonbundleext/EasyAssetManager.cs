using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using Loxodon.Framework.Bundles;

public class EasyAssetManager : StuffObject<EasyAssetManager>
{
    private void Awake()
    {
        nameToPathDic["gameroot.unity"] = "Game/Res/$scenes/GameRoot.unity";
    }

    public Dictionary<string, string> nameToPathDic = new Dictionary<string, string>();
    public Dictionary<string, string> pathToNameDic = new Dictionary<string, string>();
    public Dictionary<string, List<string>> bundleNameToAssetNameDic = new Dictionary<string, List<string>>();

    Regex regex = new Regex("^assets/", RegexOptions.IgnoreCase);

    void Record(string bundleName, string assetPath)
    {
        var fileName = System.IO.Path.GetFileName(assetPath);
        fileName = fileName.ToLower();

        if (nameToPathDic.ContainsKey(fileName))
        {
            Debug.LogError($"[EasyAsset] 重复名称警告 : {fileName}");
        }

        var fixedPath = regex.Replace(assetPath, "");
        nameToPathDic[fileName] = fixedPath;
        pathToNameDic[fixedPath] = fileName;

        RecordBundle(bundleName, fileName);
    }

    void RecordBundle(string bundleName, string assetName)
    {
        List<string> list;
        bundleNameToAssetNameDic.TryGetValue(bundleName, out list);
        if(list == null)
        {
            list = new List<string>();
            bundleNameToAssetNameDic[bundleName] = list;
        }
        list.Add(assetName);
    }

    public List<string> GetAssetNameListByBundle(string bundleName)
    {
        bundleName = bundleName.ToLower();
        var list = bundleNameToAssetNameDic[bundleName];
        return list;
    }


    public bool IsExsists(string name)
    {
        name = name.ToLower();
        var exsists = this.nameToPathDic.ContainsKey(name);
        return exsists;
    }

    string GetPath(string name)
    {
        name = name.ToLower();
        string path;
        this.nameToPathDic.TryGetValue(name, out path);
        if(path == null)
        {
            throw new Exception($"No asset name found in EasyLoader : {name}");
        }
        return path;
    }

    List<string> GetPathList(List<string> nameList)
    {
        var ret = new List<string>();
        foreach(var name in nameList)
        {
            var path = GetPath(name);
            ret.Add(path);
        }
        return ret;
    }

    string GetName(string path)
    {
        var name = pathToNameDic[path];
        return name;
    }

    public void BuildNameToPathCacheFromLoadedBundleList()
    {
        var nameToBundleInfoDic = AssetManager.Stuff.nameToLoadedBundleDic;
        foreach (var kv in nameToBundleInfoDic)
        {
            var bundleName = kv.Key;
            var bundle = kv.Value;
            string[] pathList = bundle.GetBundleAssetNames();
            if (pathList.Length == 0)
            {
                continue;
            }


            //Debug.Log($"[EasyAssetManager] cache asset path list from bundle: {bundleName}");
            foreach (var path in pathList)
            {
                Record(bundleName, path);
            }
        }
    }

    /// <summary>
    /// 前台加载资源：
    /// 1. 所在的 AssetBundle 必须已经加载。
    /// 2. 会造成画面卡顿，尽量避免使用。
    /// 3. 加载失败的情况会抛出异常
    /// 4. 如果资源已被加载过，则会从缓存中直接返回
    /// </summary>
    public T LoadAsset<T>(string assetName, bool allowNotFound = false) where T : UnityEngine.Object
    {
        if(allowNotFound)
        {
            var exists = this.IsExsists(assetName);
            if(!exists)
            {
                return null;
            }
        }

        var path = GetPath(assetName);
        var asset = AssetManager.Stuff.LoadAsset<T>(path);
        return asset;
    }


    /// <summary>
    /// 在后台线程加载资源,
    /// 如果资源已被加载，则会存缓存中直接获取
    /// </summary>
    public async Task<T> LoadAssetInBackgroundThreadAsync<T>(string assetName, bool allowNotFound = false) where T : UnityEngine.Object
    {
        if (allowNotFound)
        {
            var exists = this.IsExsists(assetName);
            if (!exists)
            {
                return null;
            }
        }

        var path = GetPath(assetName);
        var asset = await AssetManager.Stuff.LoadAssetInBackgroundThreadAsync<T>(path);
        return asset;
    }

    /// <summary>
    /// 在后台线程加载资源,
    /// 如果资源已被加载，则会存缓存中直接获取
    /// </summary>
    public async Task<Dictionary<string, UnityEngine.Object>> LoadAssetListInBackgroundThreadAsync(List<string> assetNameList)
    {
        var pathList = GetPathList(assetNameList);
        var pathToAssetDic = await AssetManager.Stuff.LoadAssetListInBackgroundThreadAsync(pathList);
        var nameToAssetDic = new Dictionary<string, UnityEngine.Object>();
        foreach(var kv in pathToAssetDic)
        {
            var path = kv.Key;
            var asset = kv.Value;
            var name = GetName(path);
            nameToAssetDic[name] = asset;
        }

        return nameToAssetDic;
    }

    //public async Task LoadAllBundles(Action<float> progressCallback)
    //{
    //    var bundleNameList = AssetManager.Stuff.GetAllBundleNames();
    //    await AssetManager.Stuff.LoadBundlesAsync(bundleNameList.ToArray(), progressCallback);
    //}

    /// <summary>
    /// 以追加的方式，加载场景
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public async Task<Scene> LoadSceneAsync(string assetName)
    {
        var path = GetPath(assetName);
        var scene = await AssetManager.Stuff.LoadSceneAsync(path);
        return scene;
    }
}

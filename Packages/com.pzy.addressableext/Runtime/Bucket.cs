using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Object = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.ResourceManagement.ResourceLocations;
using Sirenix.OdinInspector;

public class Bucket
{
    public string name;
    [ShowInInspector]
    public Dictionary<string, Object> addressToAssetDic = new Dictionary<string, Object>();

    public Bucket(string name)
    {
        this.name = name;
    }

    public void ReleaseAll()
    {
        var count = addressToAssetDic.Count;
        foreach (var kv in addressToAssetDic)
        {
            var address = kv.Key;
            var asset = kv.Value;
            Addressables.Release(asset);
        }
        Debug.Log($"[bucket] bucketd: {this.name} released {count} reference");
    }



    public async Task<T> GetOrAquireAsync<T>(string address, bool allowedNull = false) where T : Object
    {
        var cached = this.Get<T>(address, true);
        if(cached == null)
        {
            await AquireAddressAsync<T>(address, true);
            cached = this.Get<T>(address, true);
            if(cached == null)
            {
                if(!allowedNull)
                {
                    throw new Exception($"[bucket] address {address} not exists");
                }
                else
                {
                    return null;
                }
            }
        }
        return cached;
    }

    public async Task AquireIfNeedAsync<T>(string address) where T : Object
    {
        await GetOrAquireAsync<T>(address);
    }

    /// <summary>
    /// 确保指定资源的引用已被获取，这可以用来进行预引用(加载)资源
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task AquireIfNeedAsync(string address) 
    {
        await GetOrAquireAsync<Object>(address);
    }

    public async Task<Object> GetOrAquireAsync(string address) 
    {
        var ret = await GetOrAquireAsync<Object>(address);
        return ret;
    }

    public async Task<Object> GetOrAquireSpriteAsync(string address)
    {
        var ret = await GetOrAquireAsync<Sprite>(address);
        return ret;
    }

    Dictionary<string, Task> addressToIsAquiring = new Dictionary<string, Task>();


    public async Task AquireByLabelAsync<T>(string label) where T : Object
    {
        var locationList = await Addressables.LoadResourceLocationsAsync(label).Task;
        // 移除已获得引用的 address
        for(int i = locationList.Count - 1; i >=0; i--)
        {
            var location = locationList[i];
            var address = location.PrimaryKey;
            var alreadyAquired = this.IsAddressAquired(address);
            if(alreadyAquired)
            {
                locationList.RemoveAt(i);
            }
        }

        var taskList = new List<Task>();
        foreach(var location in locationList)
        {
            var address = location.PrimaryKey;
            var task = AquireAddressAsync<T>(address, false, location);
            taskList.Add(task);
        }
        await Task.WhenAll(taskList);
    }

    /// <summary>
    /// 以 address 加载资源
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="address"></param>
    /// <param name="allowedEmpty">是否允许找不到</param>
    /// <param name="preGotLocation">如果提前查询到了 address 对应的 location，在此传入以优化计算</param>
    /// <returns></returns>
    async Task AquireAddressAsync<T>(string address, bool allowedEmpty = false, IResourceLocation preGotLocation = null) where T : Object
    {
        //var isInitialized = EnvService.isAddressableInitailized;
        //if(!isInitialized)
        //{
        //    throw new Exception("[Bucket] Addressable not initalized");
        //}

        //var asset = await AddressableRes.AquireAsync<T>(address);
        var processingTask = DictionaryUtil.TryGet(addressToIsAquiring, address, null);
        if(processingTask == null)
        {
            Task<T> task = null;
            if(preGotLocation != null)
            {
                task = Addressables.LoadAssetAsync<T>(preGotLocation).Task;
            }
            else
            {
                task = Addressables.LoadAssetAsync<T>(address).Task;
            }
            addressToIsAquiring[address] = task;
            var asset = await task;
            addressToIsAquiring[address] = null;
            if(asset == null)
            {
                if (!allowedEmpty)
                {
                    throw new Exception($"[Bucket] address: {address} not exsits");
                }
                else
                {
                    return;
                }
            }
            var alreadyContains = this.addressToAssetDic.ContainsKey(address);
            if (alreadyContains)
            {
                throw new Exception($"[Bucket] bucket: {this.name} already contains a asset addressed: {address}");
            }
            this.addressToAssetDic[address] = asset;
            return;
        }
        else
        {
            await processingTask;
        }
    }

    /// <summary>
    /// 是否已获得指定 address 的引用
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public bool IsAddressAquired(string address)
    {
        var b = this.addressToAssetDic.ContainsKey(address);
        return b;
    }

    public List<Object> GetAll()
    {
        var list = new List<Object>();
        foreach(var kv in this.addressToAssetDic)
        {
            var address = kv.Key;
            var asset = kv.Value;
            list.Add(asset);
        }
        return list;
    }

    public Object Get(string address, bool isAllowedNull = false)
    {
        var ret = Get<Object>(address, isAllowedNull);
        return ret;
    }

     public T Get<T>(string address, bool isAllowedNull = false) where T : Object
     {
        var asset = DictionaryUtil.TryGet(addressToAssetDic, address, null);
        if(asset == null)
        {
            if(isAllowedNull)
            {
                return null;
            }
            else
            {
                throw new Exception($"[Bucket] bucket: {this.name} not contains cached address: {address}");
            }
        }
        var ret = asset as T;
        if(ret == null)
        {
            var assetType = asset.GetType();
            var requireType = typeof(T);
            throw new Exception($"[Bucket] cached {address} is type: {assetType}, which cannot cast to type: {requireType}");
        }
        return ret;
    }
}

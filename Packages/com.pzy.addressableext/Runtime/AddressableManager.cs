using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Object = UnityEngine.Object;
using System;
using System.IO;

public class AddressableManager : StuffObject<AddressableManager>
{

    static Dictionary<string, AsyncOperationHandle<IList<Object>>> labelAssets = new Dictionary<string, AsyncOperationHandle<IList<Object>>>();
    static Dictionary<string, AsyncOperationHandle> addressAssets = new Dictionary<string, AsyncOperationHandle>();

    // static Dictionary<string, List<string>> _labelInfoDic;
    // static Dictionary<string, List<string>> labelInfoDic{
    //     get{
    //         if(_labelInfoDic == null){
    //             var strAsset = AddressableManager.GetAsset<TextAsset>("AssetLabels.json");
    //             var str = strAsset.text;
    //             _labelInfoDic = JsonUtil.JsonStr2LabelDictionary(str);
    //         }
    //         return _labelInfoDic;
    //     }
    // }

    /// <summary>
    /// 加载并缓存资产
    /// </summary>
	public async static Task<T> LoadAssetAsync<T>(string address) where T : Object
	{
        // 句柄被保存在哈希表里，直接返回该句柄的资源内容
        if(addressAssets.ContainsKey(address)){
            return addressAssets[address].Result as T;
        }
        if(address.EndsWith(".unity")){
            Debug.LogWarning($"[AddressableManager] You can not load a unity scene [{address}] by method : LoadAssetAsync<>");
            return null;
        }
        // 加载资源句柄，同一个资源只应加载一次句柄
        var result = Addressables.LoadAssetAsync<Object>(address);
        // Debug.Log($" -{address}");
        // 保存资源句柄
        addressAssets[address] = result;
        // Debug.Log(result.Status);
        // result.Completed += (obj) =>{
        //     Debug.Log(obj);
        //     Debug.Log("complete");
        // };
        var ret = await result.Task;
        // Debug.Log(result.Status);
        return ret as T;
	}
    
    /// <summary>
    /// 加载已经加载过的资产
    /// </summary>
	public static T GetAsset<T>(string address, bool allowNull = false) where T : Object
	{
        var ret = Addressables.LoadAssetAsync<Object>(address);
        if(ret.Status != AsyncOperationStatus.Succeeded){
            // 未加载资源。Release句柄
            
            Addressables.Release(ret);
            if(allowNull){
                Debug.LogWarning($"[AddressableManager] Asset address : [{address}] not loaded yet : {ret.Status}");
                return null;
            }
            else{
                throw new Exception($"[AddressableManager] Asset address : [{address}] not loaded yet : {ret.Status}");
            }
        }
        var result = ret.Result as T;
        // 获取资源后Release临时获得的句柄
        Addressables.Release(ret);
        return result;
	}
    
    public static void ReleaseAsset(string address){
        // Release句柄哈希表中的句柄
        if(addressAssets.ContainsKey(address)){
            var handle = addressAssets[address];
            addressAssets.Remove(address);
            Addressables.Release(handle);
        }
        else{
            Debug.LogWarning($"[AddressableManager] Asset : [{address}] not loaded");
        }
    }

    public static bool IsAssetLoaded(string address){
        var handle = Addressables.LoadAssetAsync<Object>(address);
        if(handle.Status == AsyncOperationStatus.Succeeded){
            // 释放临时句柄
            Addressables.Release(handle);
            return true;
        }
        // 释放临时句柄
        Addressables.Release(handle);
        return false;
    }

    public async static Task<IList<Object>> LoadAssetsByLabelAsync(string label)
    {

        var taskHandle = PreLoadAssetsByLabel(label);
        if(taskHandle.Status == AsyncOperationStatus.Succeeded){
            return taskHandle.Result;
        }
        var objs = await taskHandle.Task;
        return objs;
    }
    
    public static AsyncOperationHandle<IList<Object>> PreLoadAssetsByLabel(string label)
    {
        // Addressables.LoadResourceLocationsAsync("");
        if(labelAssets.ContainsKey(label)){
            return labelAssets[label];
        }
        Debug.Log($"[AddressableManager] Load by label {label}");
        var labelList = new List<string> {label};
        var handle = Addressables.LoadAssetsAsync<Object>(labelList, null, Addressables.MergeMode.Intersection);
        // var objs = await handle.Task;
        handle.Completed += (obj) => {
            Debug.Log($"[AddressableManager] Label {label} loaded");
        };
        labelAssets[label] = handle;
        return handle;
    }
    
    public static void UnloadAssetByLabelAsync(string label)
    {
        if(labelAssets.ContainsKey(label)){
            var objs = labelAssets[label];
            Addressables.Release(objs);
            labelAssets.Remove(label);

            // 自动卸载单个资源
            // if(labelInfoDic.ContainsKey(label)){
            //     foreach(var addr in labelInfoDic[label]){
            //         if(addressAssets.ContainsKey(addr)){
            //             ReleaseAsset(addr);
            //         }
            //     }
            // }
            // else{
            //     Debug.LogWarning($"[AddressableManager] Label {label} is not managed");
            // }
        }
    }

}

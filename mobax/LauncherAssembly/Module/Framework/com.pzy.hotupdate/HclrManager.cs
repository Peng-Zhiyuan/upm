using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Threading.Tasks;
using CustomLitJson;
using HybridCLR;

public static class HclrManager
{

    public static bool hotAssemblyAlreadyLoaded;

    //public static async Task TryLoadDynamicAssemblyFromSteamingAsync()
    //{
    //    if (hotAssemblyAlreadyLoaded)
    //    {
    //        return;
    //    }

    //    var address = $"Assembly-CSharp.dll.bytes";
    //    var data = await StreamingUtil.LoadStreamingAssetsAsync<byte[]>("Assembly-CSharp.dll.bytes");
    //    Assembly.Load(data);
    //    hotAssemblyAlreadyLoaded = true;
    //    Debug.Log($"[HclrManager] {address} loaded from Addresable");
    //}


    public static bool aotMetadataAlreadyLoaded;
    public static async Task TryLoadAotMetadataAsync()
    {

        if (aotMetadataAlreadyLoaded)
        {
            return;
        }

        if (Application.isEditor)
        {
            Debug.Log("[AotMetadataManager] skip aot metadata loading in editor");
            return;
        }

        Debug.Log("[AotMetadataManager] load aot dll from streaming assets");
        // 从安卓 assets 中读取
        var index = "AotAssembly/index.json";
        var json = await StreamingUtil.LoadStreamingAssetsAsync<string>(index);
        Debug.Log("index: " + json);
        var jd = JsonMapper.Instance.ToObject(json);
        foreach (var element in jd)
        {
            var name = element.ToString();
            var path = "AotAssembly/" + name;
            var data = await StreamingUtil.LoadStreamingAssetsAsync<byte[]>(path);
            unsafe
            {
                fixed (byte* p = data)
                {
                    Debug.Log("[AotMetadataManager] loading " + name);
                    RuntimeApi.LoadMetadataForAOTAssembly((IntPtr)p, data.Length);
                }
            }
        }
        aotMetadataAlreadyLoaded = true;
    }

    /*
     *        
     *  [DllImport(dllName, EntryPoint = "il2cpp_gc_get_used_size")]
        public static extern long il2cpp_gc_get_used_size();

        [DllImport(dllName, EntryPoint = "il2cpp_gc_get_heap_size")]
        public static extern long il2cpp_gc_get_heap_size();
    */
}



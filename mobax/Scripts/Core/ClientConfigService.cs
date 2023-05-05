using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Linq;


public class ClientConfigService : Service
{
    Dictionary<string, TextAsset> addressToAssetDic = new Dictionary<string, TextAsset>();

    public override void OnCreate()
    {
        // 资源已提前加载
        var buket = BucketManager.Stuff.Conf;
        var dic = buket.addressToAssetDic;
        foreach(var kv in dic)
        {
            var address = kv.Key;
            var asset = kv.Value as TextAsset;
            addressToAssetDic[address] = asset;
        }

        ClientConfigManager.OnLoadTableTextAsset = this.LoadTableTextAsset;
    }

    TextAsset LoadTableTextAsset(string tableName)
    {
        //var path = $"clientConfigRes/{tableName}";
        //var textAsset = Resources.Load(path) as TextAsset;
        //return textAsset;
        var address = $"{tableName}.txt";
        var asset = addressToAssetDic[address];
        return asset;
    }
    
}
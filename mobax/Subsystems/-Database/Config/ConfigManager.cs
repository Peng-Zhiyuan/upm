using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using CustomLitJson;

public class ConfigManager : StuffObject<ConfigManager>
{
    [ShowInInspector, ReadOnly]
    List<ConfigInfo> infoList = new List<ConfigInfo>();

    [ShowInInspector, ReadOnly]
    Dictionary<ConfigType, List<ConfigInfo>> typeToInfoListDic = new Dictionary<ConfigType, List<ConfigInfo>>();

    [ShowInInspector, ReadOnly]
    Dictionary<string, ConfigInfo> idToInfoDic = new Dictionary<string, ConfigInfo>();
    public async Task SyncAsync()
    {
        var language = LocalizationManager.Stuff.Language;
        this.infoList.Clear();
        var arg = new JsonData();
        arg["language"] = language;
        var infoList = await ApiUtil.FetchByPage(ConfigApi.FetchAsync, 0, 0, arg);
        this.AddRange(infoList);
    }

    void AddRange(List<ConfigInfo> infoList)
    {
        foreach(var info in infoList)
        {
            this.Add(info);
        }
    }

    void Add(ConfigInfo info)
    {
        this.AddToList(info);
        this.AddToTypeIndex(info);
        this.AddToIdIndex(info);
    }

    void AddToList(ConfigInfo info)
    {
        this.infoList.Add(info);
    }

    void AddToTypeIndex(ConfigInfo info)
    {
        var type = info.AType;
        var list = this.typeToInfoListDic.GetOrCreateList(type);
        list.Add(info);
    }

    void AddToIdIndex(ConfigInfo info)
    {
        var id = info.id;
        this.idToInfoDic[id] = info;
    }

    public List<ConfigInfo> Get(ConfigType type)
    {
        var ret = new List<ConfigInfo>();
        var b = typeToInfoListDic.TryGetValue(type, out List<ConfigInfo> infoList);
        if(!b)
        {
            return ret;
        }
        foreach(var one in infoList)
        {
            if(one.IsValid)
            {
                ret.Add(one);
            }
        }
        return ret;
    }

    public ConfigInfo Get(string id)
    {
        var info = this.idToInfoDic.TryGet(id, null);
        return info;
    }
    
}

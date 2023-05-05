using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

public class GiftDatabase
{
    [ShowInInspector, ReadOnly]
    Dictionary<string, GiftInfo> idToInfoDic = new Dictionary<string, GiftInfo>();

    public async Task ResetByFetchAllAsync()
    {
        this.Clean();
        var infoList = await ApiUtil.FetchByPage(GiftApi.RequestAsync);
        this.AddRange(infoList);
    }

    List<GiftInfo> GetByBetweenTime(long startTimeSec, long endTimeSec)
    {
        var ret = new List<GiftInfo>();
        foreach (var kv in idToInfoDic)
        {
            var info = kv.Value;
            var update = info.update;
            if (update >= startTimeSec
                && update < endTimeSec)
            {
                ret.Add(info);
            }
        }
        return ret;
    }

    public GiftInfo Get(string id)
    {
        var ret = idToInfoDic[id];
        return ret;
    }

    public List<GiftInfo> GetAll(GiftSource source)
    {
        var ret = new List<GiftInfo>();
        foreach (var kv in idToInfoDic)
        {
            var id = kv.Key;
            var info = kv.Value;
            ret.Add(info);
        }
        return ret;
    }

    void AddRange(List<GiftInfo> infoList)
    {
        foreach (var info in infoList)
        {
            this.AddInfo(info);
        }
    }

    void AddInfo(GiftInfo info)
    {
        var id = info.id;
        var status = info.status;
        if (status == OrderStatus.Complete)
        {
            return;
        }
        this.idToInfoDic[id] = info;
    }

    public void UpdateInfo(GiftInfo info)
    {
        if (idToInfoDic.ContainsKey(info.id))
        {
            idToInfoDic[info.id] = info;
        }
    }

    void Clean()
    {
        this.idToInfoDic.Clear();
    }

    public int Count
    {
        get 
        {
            return this.idToInfoDic.Count; 
        }
    }

    public async Task SubmitAllIggGiftAsync()
    {
        var idList = new List<string>();
        foreach (var kv in this.idToInfoDic)
        {
            var id = kv.Key;
            var info = kv.Value;
            if (info.status != OrderStatus.Complete)
            {
                if (info.source == GiftSource.Igg)
                {
                    idList.Add(id);
                }
            }
        }
        if(idList.Count == 0)
        {
            return;
        }
        var successIdList = await GiftApi.SubmitAsync(idList);
        foreach (var id in successIdList)
        {
            this.idToInfoDic.Remove(id);
        }
    }
}
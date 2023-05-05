using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Sirenix.OdinInspector;
using CustomLitJson;

public class OrderDatabase 
{
    [ShowInInspector]
    Dictionary<string, OrderInfo> idToInfoDic = new Dictionary<string, OrderInfo>();

    [ShowInInspector]
    Dictionary<int, List<OrderInfo>> productIdToInfoDic = new Dictionary<int, List<OrderInfo>>();

    public static event Action StatusUpdated;

    public long time = 0;
    public async Task ResetByFetchAllAsync()
    {
        this.Clean();
        time = 0;
        await this.SyncAdditionalAsync();
    }

    /// <summary>
    /// 增量更新
    /// </summary>
    /// <returns></returns>
    public async Task SyncAdditionalAsync()
    {
        var jd = new JsonData();
        jd["update"] = time;
        var now = Clock.TimestampSec;
        var infoList = await ApiUtil.FetchByPage(OrderApi.FetchAsync, 0, 0, jd);
        this.AddInfoList(infoList);
        time = now;
    }


    void AddInfoList(List<OrderInfo> infoList)
    {
        foreach(var info in infoList)
        {
            this.AddInfo(info);
        }
    }

    public void AddInfo(OrderInfo info)
    {
        this.AddToDicIndex(info);
        this.AddToProductIdIndex(info);

        StatusUpdated?.Invoke();
    }

    void AddToProductIdIndex(OrderInfo info)
    {
        var id = info.item;
        var list = this.productIdToInfoDic.GetOrCreateList(id);
        list.Add(info);
    }

    void AddToDicIndex(OrderInfo info)
    {
        var id = info.id;
        this.idToInfoDic[id] = info;
    }

    public bool IsExists(string orderId)
    {
        var info = this.Get(orderId);
        if(info == null)
        {
            return false;
        }
        return true;
    }

    public OrderInfo Get(string orderId)
    {
        var b = idToInfoDic.TryGet(orderId, null);
        return b;
    }

    public List<OrderInfo> GetAll()
    {
        var ret = new List<OrderInfo>();
        foreach(var kv in this.idToInfoDic)
        {
            var info = kv.Value;
            ret.Add(info);
        }
        return ret;
    }

    public int Count(int paymentId)
    {
        var b = productIdToInfoDic.TryGetValue(paymentId, out var infoList);
        if(b)
        {
            return infoList.Count;
        }
        return 0;
    }

    void Clean()
    {
        this.idToInfoDic.Clear();
    }

}

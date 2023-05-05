using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Sirenix.OdinInspector;

public class MailDatabase 
{
    [ShowInInspector]
    Dictionary<string, MailInfo> instanceIdToInfoDic = new Dictionary<string, MailInfo>();
    long lastUpdateTime = 0;

    void AddInfoList(MailInfo[] infoList)
    {
        foreach(var info in infoList)
        {
            var id = info._id;
            this.instanceIdToInfoDic[id] = info;
        }
    }

    public void Remove(string id)
    {
        this.instanceIdToInfoDic.Remove(id);
    }

    public async Task UpdateByFetchNewModifiedSinceLastUpdateAsync(bool isBlock = true)
    {
        var msg = await MailApi.RequestAllAsync(this.lastUpdateTime, isBlock);
        var list = msg.data;
        var timestamp = msg.time;
        if(list != null)
        {
            this.AddInfoList(list);
        }
        this.lastUpdateTime = timestamp;
    }

    public List<MailInfo> GetAllMailInfo()
    {
        var ret = new List<MailInfo>();
        foreach(var kv in this.instanceIdToInfoDic)
        {
            var info = kv.Value;
            if(info.status == MailState.Deleted)
            {
                continue;
            }
            ret.Add(info);
        }
        return ret;
    }

    public int Count
    {
        get
        {
            var ret = this.instanceIdToInfoDic.Count;
            return ret;
        }
    }

}

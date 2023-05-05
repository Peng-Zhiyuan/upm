using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System;

public class GameTaskDatabase 
{
    [ShowInInspector]
    Dictionary<int, GameTaskInfo> chainIdOrTaskRowIdToInfoDic = new Dictionary<int, GameTaskInfo>();

    public async Task ResetByFetchAllAsync()
    {
        var infoList = await ApiUtil.FetchByPage(GameTaskApi.FetchAsync);
        this.AddList(infoList);
    }

    public void AddList(List<GameTaskInfo> list)
    {
        if (list != null)
        {
            foreach (var info in list)
            {
                this.Add(info);
            }
        }
    }

    public void Add(GameTaskInfo info)
    {
        var chainIdOrTaskId = info.key;
        this.chainIdOrTaskRowIdToInfoDic[chainIdOrTaskId] = info;
    }

    public void ModifyValue(int chainIdOrTaskRowId, int value)
    {
        var info = DictionaryUtil.TryGet(this.chainIdOrTaskRowIdToInfoDic, chainIdOrTaskRowId, null);
        if(info != null)
        {
            info.val = value;
        }
    }

    long NowTimestampSec
    {
        get
        {
            return Clock.TimestampSec;
        }

    }

    public GameTaskInfo GetByChainIdOrTaskRowId(int chainIdOrTaskRowId)
    {
        var info = DictionaryUtil.TryGet(this.chainIdOrTaskRowIdToInfoDic, chainIdOrTaskRowId, null);
        if (info == null)
        {
            return null;
        }
        var expireMs = info.expire;
        if (expireMs == 0 || expireMs == -1)
        {
            return info;
        }
        var nowSec = this.NowTimestampSec;
        var nowMs = nowSec * 1000;
        if (expireMs >= nowMs)
        {
            this.chainIdOrTaskRowIdToInfoDic[chainIdOrTaskRowId] = null;
            return null;
        }
        return info;
    }

    public GameTaskInfo Get(int chainIdOrTaskRowId)
    {
        this.chainIdOrTaskRowIdToInfoDic.TryGetValue(chainIdOrTaskRowId, out var info);
        if (info == null)
        {
            return null;
        }
        if(!info.HasExpire)
        {
            return info;
        }
        var now = NowTimestampSec;
        var expire = info.expire;
        if (now >= expire)
        {
            return null;
        }
        else
        {
            return info;
        }
    }

    public bool IsNonChainTaskSubmited(int taskId)
    {
        var info = this.Get(taskId);
        if (info == null)
        {
            return false;
        }
        if(info.val != taskId)
        {
            return false;
        }
        return true;
    }

    public int DailyRewardBitBucket
    {
        get
        {
            var id = 7271000;
            var info = this.Get(id);
            if(info == null)
            {
                return 0;
            }
            return info.val;
        }
    }

    public bool IsDailyLievenessRewardReceived(int dailyRewardId)
    {
        if(dailyRewardId >= 32)
        {
            throw new Exception("[GameTaskDatabase] bit bucket index can not biger than 32");
        }
        var bitBucket = (ulong)this.DailyRewardBitBucket;
        var ret = BitUtil.IsBitSet(bitBucket, dailyRewardId);
        return ret;

    }
}

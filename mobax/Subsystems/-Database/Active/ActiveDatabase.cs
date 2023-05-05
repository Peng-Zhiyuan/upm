using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System;

public class ActiveDatabase
{
    [ShowInInspector]
    Dictionary<string, ActiveInfo> idToInfoDic = new Dictionary<string, ActiveInfo>();

    public async Task SyncAsync()
    {
        idToInfoDic.Clear();
        var infoList = await ApiUtil.FetchByPage(ActiveApi.FetchAsync);
        this.AddList(infoList);
    }

    void AddList(List<ActiveInfo> list)
    {
        if (list != null)
        {
            foreach (var row in list)
            {
                var info = row;
                this.Add(info);
            }
        }
    }

    public ActiveInfo GetActiveInfo(string key)
    {
        if (idToInfoDic.ContainsKey(key))
        {
            return idToInfoDic[key];
        }
        return null;
    }

    public void Add(ActiveInfo info)
    {
        var id = info.iid;
        this.idToInfoDic[id] = info;
    }

    public static bool IsBitSet(uint bitBucket, int indexBaseFrom0)
    {
        if (indexBaseFrom0 >= 32)
        {
            throw new Exception("[BitUtil] bit index out of range (0 ~ 32)");
        }
        return (bitBucket & (1UL << indexBaseFrom0)) != 0;
    }

    public static bool IsBitSet(uint[] bitBucket, int indexBaseFrom0)
    {
        if (bitBucket == null)
        {
            return false;
        }
        var index = indexBaseFrom0 / 32;
        var indexInBitBucket = indexBaseFrom0 % 32;
        if (index >= bitBucket.Length)
        {
            return false;
        }
        var bitBucketValue = bitBucket[index];
        return IsBitSet(bitBucketValue, indexInBitBucket);
    }

    public bool HasFlag(string activeId, int indexBase0)
    {
        if (!this.idToInfoDic.ContainsKey(activeId))
        {
            return false;
        }
        var info = this.idToInfoDic[activeId];
        var bitBucket = info.val;
        if (bitBucket == null)
        {
            return false;
        }
        var ret = IsBitSet(bitBucket, indexBase0);
        return ret;
    }

    public bool IsSuperMailReceived(string id)
    {
        var ret = this.HasFlag(id, 0);
        return ret;
    }

    public bool IsSupoerMailDeleted(string id)
    {
        var ret = this.HasFlag(id, 1);
        return ret;
    }
}
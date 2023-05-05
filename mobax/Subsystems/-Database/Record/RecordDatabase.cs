using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using CustomLitJson;

public class RecordDatabase 
{
    [ShowInInspector]
    Dictionary<int, RecordInfo> dateDic = new Dictionary<int, RecordInfo>();

    void Clean()
    {
        this.dateDic.Clear();
    }

    void Remove(int id)
    {
        this.dateDic.Remove(id);
    }

    public async Task ResetByFetchAllAsync()
    {
        this.Clean();
        var infoList = await ApiUtil.FetchByPage(RecordApi.FetchAsync);
        this.AddInfoList(infoList);
    }

    RecordInfo GetInfoById(int id)
    {
        return DictionaryUtil.TryGet(this.dateDic, id, null);
    }

    public void ApplyTransaction(DatabaseTransaction transaction)
    {
        var type = transaction.t;
        if (type == TransactionType.New)
        {
            // r 是一个数组，但是应该总只有一个元素，其元素是一个 record info
            var array = transaction.r;
            var jd = array[0];
            var info = JsonUtil.JsonDataToObject<RecordInfo>(jd);
            this.AddInfo(info);
        }
        else
        {
            var path = transaction.k;
            var value = transaction.r;
            var id = transaction.id;
            var info = GetInfoById(id);
            DataTreeUpdater.Update(info, path, value);
        }

    }

    void AddInfoList(List<RecordInfo> list)
    {
        if(list == null)
        {
            return;
        }
        foreach(var info in list)
        {
            this.AddInfo(info);
        }
    }

    void AddInfo(RecordInfo info)
    {
        var id = info.id;
        this.dateDic[id] = info;
    }

    public int GetValue(int id)
    {
        var info = DictionaryUtil.TryGet(this.dateDic, id, null);
        if (info == null)
        {
            return 0;
        }
        var ret = info.val;
        return ret;
    }
}

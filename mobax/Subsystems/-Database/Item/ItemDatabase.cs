using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;

public class ItemDatabase
{
    [ShowInInspector] Dictionary<string, ItemInfo> instanceIdToItemInfoDic = new Dictionary<string, ItemInfo>();

    [ShowInInspector] Dictionary<int, List<ItemInfo>> rowIdToItemInfoListDic = new Dictionary<int, List<ItemInfo>>();

    [ShowInInspector] Dictionary<int, List<ItemInfo>> itypeToItemInfoListDic = new Dictionary<int, List<ItemInfo>>();

    public async Task ResetByFetchAllAsync()
    {
        this.Clean();
        var infoList = await ApiUtil.FetchByPage(ItemApi.FetchAsync);
        this.AddItemInfoList(infoList);
    }

    void AddItemInfoList(List<ItemInfo> itemList)
    {
        if (itemList == null)
        {
            return;
        }

        foreach (var info in itemList)
        {
            this.AddItemInfo(info);
        }
    }

    void AddItemInfo(ItemInfo info)
    {
        this.AddToInstanceDic(info);
        this.AddToRowIdDic(info);
        this.AddToITypeIdDic(info);

        // 触发道具变动的回调
        this.InvokeItemTypeAdd(info);
        this.InvokeItemChange(info);
    }

    void AddToInstanceDic(ItemInfo info)
    {
        var instanceId = info._id;
        this.instanceIdToItemInfoDic[instanceId] = info;
    }

    void AddToRowIdDic(ItemInfo info)
    {
        var rowId = info.id;
        var list = DictionaryUtil.GetOrCreateList(this.rowIdToItemInfoListDic, rowId);
        list.Add(info);
    }

    static int? GetIType(int rowId)
    {
        var row = StaticDataRuntime.GetRow(rowId, null, true);
        if (row == null) return null;
        var (itype, _) = ReflectionUtil.TryGetPropertyValue<int>(row, "IType");
        return itype;
    }

    void AddToITypeIdDic(ItemInfo info)
    {
        var rowId = info.id;
        var itype = GetIType(rowId);
        if (itype == null)
        {
            // 这个数据没有 itype 吗？
            return;
        }

        var list = DictionaryUtil.GetOrCreateList(this.itypeToItemInfoListDic, itype.Value);
        list.Add(info);
    }

    void Clean()
    {
        this.instanceIdToItemInfoDic.Clear();
        this.rowIdToItemInfoListDic.Clear();
        this.itypeToItemInfoListDic.Clear();
    }

    void RemoveItemInfo(ItemInfo info)
    {
        this.RemoveFromInstanceIdDic(info);
        this.RemoveFromRowIdDic(info);
        this.RemoveFromITypeDic(info);
    }

    void RemoveFromInstanceIdDic(ItemInfo info)
    {
        var instanceId = info._id;
        this.instanceIdToItemInfoDic.Remove(instanceId);
    }

    void RemoveFromRowIdDic(ItemInfo info)
    {
        var rowId = info.id;
        var list = DictionaryUtil.GetOrCreateList(this.rowIdToItemInfoListDic, rowId);
        list.Remove(info);
    }

    void RemoveFromITypeDic(ItemInfo info)
    {
        var rowId = info.id;
        var itype = GetIType(rowId);
        if (itype == null)
        {
            return;
        }

        var list = DictionaryUtil.GetOrCreateList(this.itypeToItemInfoListDic, itype.Value);
        list.Remove(info);
    }

    public void ApplyTransaction(DatabaseTransaction transaction)
    {
        var type = transaction.t;
        if (type == TransactionType.Add || type == TransactionType.Sub || type == TransactionType.Set)
        {
            this.DoUpdate(transaction);
        }
        else if (type == TransactionType.Delete)
        {
            this.DoDelete(transaction);
        }
        else if (type == TransactionType.New)
        {
            var r = transaction.r;
            if (r.IsArray)
            {
                // r 是 itemInfo 的数组
                var list = r.ToList();
                foreach (var jd in list)
                {
                    var info = JsonUtil.JsonDataToObject<ItemInfo>(jd);
                    this.AddItemInfo(info);
                }
            }
            else
            {
                // r 是 itemInfo
                var info = JsonUtil.JsonDataToObject<ItemInfo>(r);
                this.AddItemInfo(info);
            }
        }
        else if (type == TransactionType.AutoDecomposition)
        {
            // eg: {"_id":"","id":15001,"t":6,"k":"","v":1,"b":15,"r":null}
            // 已经被分解的物品不会修改数据库
        }

        this.ProcessChnaged();
    }

    void DoUpdate(DatabaseTransaction transaction)
    {
        var instanceId = transaction._id;
        var info = this.GetItemInfoByInstanceId(instanceId);
        if (info == null)
        {
            // 理论上不会走到这里
            Debug.LogError($"[ItemDatabase] try update item info, but not exists. (instanceId: {instanceId})");
            return;
        }

        if (transaction.k == "*")
        {
            // 根据 r 对象的字段来确定修改的字段
            var jd = transaction.r;
            var dic = jd.EnsureDictionary();
            var keyList = dic.Keys;
            foreach (string key in keyList)
            {
                var value = jd[key];
                DataTreeUpdater.Update(info, key, value);
            }
        }
        else
        {
            var path = transaction.k;
            var value = transaction.r;
            DataTreeUpdater.Update(info, path, value);
        }

        // 物品的修改
        this.InvokeItemChange(info);

        // 需要确认此逻辑是否还存在
        // // 某些物品，当堆叠为0时，服务器会删除这些对象
        // if (itemInfo.val == 0)
        // {
        //     let typeRow = TsProtoStaticData.ITypesTable.TryGet(itemInfo.bag);
        //     // TODO:当前不确定是否仍需要此字段
        //     // if (typeRow.isClear)
        //     // {
        //     //     this.RemoveItemInfo(itemInfo);
        //     // }
        // }
    }

    void DoDelete(DatabaseTransaction transaction)
    {
        var instanceId = transaction._id;
        var info = this.GetItemInfoByInstanceId(instanceId);
        this.RemoveItemInfo(info);
        this.InvokeItemChange(info);
    }

    /// <summary>
    /// 或许所有物品的列表副本
    /// </summary>
    /// <returns></returns>
    public List<ItemInfo> GetAll()
    {
        var ret = new List<ItemInfo>();
        foreach (var kv in this.instanceIdToItemInfoDic)
        {
            var info = kv.Value;
            ret.Add(info);
        }

        return ret;
    }

    public ItemInfo GetItemInfoByInstanceId(string instanceId)
    {
        var ret = this.instanceIdToItemInfoDic[instanceId];
        return ret;
    }

    public List<ItemInfo> GetItemInfoListByRowId(int rowId)
    {
        var list = DictionaryUtil.GetOrCreateList(this.rowIdToItemInfoListDic, rowId);
        return list;
    }

    public List<ItemInfo> GetItemInfoListByIType(int itype)
    {
        var list = DictionaryUtil.GetOrCreateList(this.itypeToItemInfoListDic, itype);
        return list;
    }

    public ItemInfo GetFirstItemInfoOfRowId(int rowId)
    {
        var itemList = this.GetItemInfoListByRowId(rowId);
        if (itemList.Count == 0)
        {
            return null;
        }
        else
        {
            return itemList[0];
        }
    }

    public void SetCount(string instanceId, int count)
    {
        var itemInfo = this.GetItemInfoByInstanceId(instanceId);
        if (itemInfo != null)
        {
            itemInfo.val = count;
        }
    }

    /// <summary>
    /// 获取第一个指定 rowId 物品的堆叠数量
    /// 用于肯定只有一个的物品（例：金币，经验）
    /// 对于可能有多个的物品（例：武器），不要使用这个方法
    /// * 对于门票类型，会自动计算恢复量
    /// </summary>
    /// <param name="rowId"></param>
    /// <returns></returns>
    public int GetHoldCount(int rowId)
    {
        var info = this.GetFirstItemInfoOfRowId(rowId);
        var isTicekt = TicketUtil.IsTicket(rowId);
        if (!isTicekt)
        {
            if (info == null)
            {
                return 0;
            }
            else
            {
                return info.val;
            }
        }
        else
        {
            // 门票类型
            if (info == null)
            {
                var max = TicketUtil.GetMax(rowId);
                return max;
            }
            else
            {
                var staticCount = info.val;
                var lastCaculateTimeSec = info.attach.ToLong();
                var nowMs = Clock.TimestampMs;
                var timezone = Clock.Timezone;
                var count = TicketUtil.GetHoldCount(rowId, staticCount, lastCaculateTimeSec * 1000, nowMs, timezone);
                return count;
            }
        }
    }

    public long dataVersion;
    public event Action Changed;


    void ProcessChnaged()
    {
        // 更新数据版本
        var timestanp = Database.RequestTimestap.Invoke();
        this.dataVersion = timestanp;

        // 通知数据变更
        this.Changed?.Invoke();
    }

    #region 物品更新的字典

    private Dictionary<int, Action<int>> _itemChangedMap = new Dictionary<int, Action<int>>();
    private Dictionary<int, Action<ItemInfo>> _itemTypeAddedMap = new Dictionary<int, Action<ItemInfo>>();

    public void AddItemChange(int itemId, Action<int> handler)
    {
        if (!_itemChangedMap.ContainsKey(itemId))
        {
            _itemChangedMap.Add(itemId, handler);
        }
        else
        {
            _itemChangedMap[itemId] -= handler;
            _itemChangedMap[itemId] += handler;
        }
    }

    public void RemoveItemChange(int itemId, Action<int> handler)
    {
        if (_itemChangedMap.ContainsKey(itemId))
        {
            _itemChangedMap[itemId] -= handler;
        }
    }

    public void AddItemTypeChange(int bagType, Action<ItemInfo> handler)
    {
        if (!_itemTypeAddedMap.ContainsKey(bagType))
        {
            _itemTypeAddedMap.Add(bagType, handler);
        }
        else
        {
            _itemTypeAddedMap[bagType] -= handler;
            _itemTypeAddedMap[bagType] += handler;
        }
    }

    public void RemoveItemTypeChange(int bagType, Action<ItemInfo> handler)
    {
        if (_itemTypeAddedMap.ContainsKey(bagType))
        {
            _itemTypeAddedMap[bagType] -= handler;
        }
    }

    private void InvokeItemChange(ItemInfo itemInfo)
    {
        if (_itemChangedMap.TryGetValue(itemInfo.id, out var changeHandler))
        {
            if (null != changeHandler) changeHandler(itemInfo.id);
        }
    }

    private void InvokeItemTypeAdd(ItemInfo itemInfo)
    {
        if (_itemTypeAddedMap.TryGetValue(itemInfo.bag, out var addHandler))
        {
            if (null != addHandler) addHandler(itemInfo);
        }
    }

    #endregion
}
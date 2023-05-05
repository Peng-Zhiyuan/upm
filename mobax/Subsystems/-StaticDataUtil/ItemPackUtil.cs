using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemPackUtil 
{
    /// <summary>
    /// 如果是物品组，换算为拆开后的物品
    /// </summary>
    /// <param name="itemOrPack"></param>
    /// <returns></returns>
    public static List<VirtualItem> TryUnpack(VirtualItem itemOrPack)
    {
        var id = itemOrPack.id;
        var isPack = IsItemPack(id);
        if (!isPack)
        {
            var ret = new List<VirtualItem>();
            ret.Add(itemOrPack);
            return ret;
        }
        else
        {
            var packCount = itemOrPack.val;
            var ret = GetItem(id, packCount);
            return ret;
        }
    }

    public static List<VirtualItem> TryUnpack(List<VirtualItem> itemOrPackList)
    {
        var packList = new List<VirtualItem>();
        var itemList = new List<VirtualItem>();
        for(int i = itemOrPackList.Count - 1; i >= 0; i--)
        {
            var itemOrPack = itemOrPackList[i];
            var isPack = IsItemPack(itemOrPack.id);
            if(isPack)
            {
                packList.Add(itemOrPack);
            }
            else
            {
                itemList.Add(itemOrPack);
            }
        }
        if(packList.Count == 0)
        {
            return itemList;
        }
        foreach(var pack in packList)
        {
            var list = TryUnpack(pack);
            itemList.AddRange(list);
        }
        return itemList;
    }

    public static bool IsItemPack(int id)
    {
        var isPack = StaticData.ItemPacksTable.ContainsKey(id);
        return isPack;
    }

    public static List<VirtualItem> GetItem(int itemPackId, int packCount)
    {
        var ret = new List<VirtualItem>();
        var array = StaticData.ItemPacksTable[itemPackId];
        var list = array.Colls;
        foreach(var row in list)
        {
            var itemId = row.Key;
            var count = row.Num;
            var item = new VirtualItem();
            item.id = itemId;
            item.val = count * packCount;
            ret.Add(item);
        }
        return ret;
    }
}

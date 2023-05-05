using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PackUtil
{

    public static void UnPack(int id, int num, List<VirtualItem> itemList, bool recursion = false)
    {
        if (recursion)
        {
            UnPackRecursion(id, num, itemList);
        }
        else 
        {
            UnPackDirectly(id, num, itemList);
        }
    }
    public static void UnPackDirectly(int id, int num, List<VirtualItem> itemList)
    {
        if (StaticData.ItemPacksTable.ContainsKey(id))
        {
            var list = StaticData.ItemPacksTable[id].Colls;
            for (int i = 0; i < list.Count; i++)
            {
                var vi = new VirtualItem();
                vi.id = list[i].Key;
                vi.val = list[i].Num * num;
                itemList.Add(vi);
            }

        }
        else if (StaticData.ItemGroupTable.ContainsKey(id))
        {
            var list = StaticData.ItemGroupTable[id].Colls;
            for (int i = 0; i < list.Count; i++)
            {
                var vi = new VirtualItem();
                vi.id = list[i].Key;
                vi.val = list[i].Num * num;
                itemList.Add(vi);
            }
        }
        else
        {
            var vi = new VirtualItem();
            vi.id = id;
            vi.val = num;
            itemList.Add(vi);
        }
    }

    public static void UnPackRecursion(int id, int num,  List<VirtualItem> itemList)
    {
        if (StaticData.ItemPacksTable.ContainsKey(id))
        {
            var list = StaticData.ItemPacksTable[id].Colls;
            for (int i = 0; i < list.Count; i++)
            {
                UnPackRecursion(list[i].Key, list[i].Num * num, itemList);
            }

        }
        else if (StaticData.ItemGroupTable.ContainsKey(id))
        {
            var list = StaticData.ItemGroupTable[id].Colls;
            for (int i = 0; i < list.Count; i++)
            {
                UnPackRecursion(list[i].Key, list[i].Num * num, itemList);
            }
        }
        else 
        {
            var vi = new VirtualItem();
            vi.id = id;
            vi.val = num;
            itemList.Add(vi);
        }
    }

}

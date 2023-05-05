using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LitJson;
using UnityEngine;

public class CommonItem
{
    public int Id;
    public int? Num;
    public int Qlv;
    public string Icon;
    public string Name;
}

public static class ItemUtil
{
    public static CommonItem ConvertCommonItem(int id)
    {
        object itemRow = StaticDataRuntime.GetRow(id);
        var (itemName, _) = ReflectionUtil.TryGetPropertyValue<string>(itemRow, "Name");
        var (itemIcon, _) = ReflectionUtil.TryGetPropertyValue<string>(itemRow, "Icon");
        var (itemNum, _) = ReflectionUtil.TryGetPropertyValue<int>(itemRow, "Num");
        var commonItem = new CommonItem();
        commonItem.Id = id;
        commonItem.Icon = itemIcon;
        commonItem.Name = itemName;
        commonItem.Num = itemNum;
        return commonItem;
    }

    public static ItemRow GetRow(int rowId)
    {
        var row = StaticData.ItemTable.TryGet(rowId);
        return row;
    }

    public static int GetHoldCount(int rowId)
    {
        return Database.Stuff.itemDatabase.GetHoldCount(rowId);
    }

    /// <summary>
    /// 
    /// 可以支持三种方式的判断<br />
    /// ItemUtil.IsEnough(itemId, count);<br />
    /// ItemUtils.IsEnough({id: itemId, num: count});<br />
    /// ItemUtils.IsEnough({id: itemId, num: count}[]);<br />
    /// </summary>
    /// <param name="items"></param>
    /// <returns></returns>
    public static bool IsEnough(ICostItem items)
    {
        return IsEnough(items.Id, items.Num);
    }


    public static bool IsEnough<T>(List<T> items) where T : ICostItem
    {
        var isEnough = true;
        for (int i = 0; i < items.Count; i++)
        {
            if (!IsEnough(items[i]))
            {
                isEnough = false;
                break;
            }
        }

        return isEnough;
    }
    
    public static bool IsEnough(Dictionary<int, int> costMap)
    {
        var isEnough = true;
        foreach (var kv in costMap)
        {
            if (!IsEnough(kv.Key, kv.Value))
            {
                isEnough = false;
                break;
            }
        }

        return isEnough;
    }

    public static bool IsEnough(int itemId, int num = 1)
    {
        var have = GetHoldCount(itemId);
        if (have < num)
        {
            Debug.LogWarning($"[itemId={itemId}] count:{have}/{num}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 消耗2种道具的哪一种都可以
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="aliasItemId">候选道具</param>
    /// <param name="num"></param>
    /// <returns></returns>
    public static bool IsEnough(int itemId, int aliasItemId, int num)
    {
        var have = GetHoldCount(itemId) + GetHoldCount(aliasItemId);
        if (have < num)
        {
            Debug.LogWarning($"[itemId={itemId} or {aliasItemId}] count:{have}/{num}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 判断物品是否不足 并检测是否跳转充值
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="num"></param>
    /// <param name="jump">是否跳转 - 暂未实现</param>
    /// <param name="showTip">开启提示</param>
    /// <returns></returns>
    public static async Task<bool> IsEnoughAsync(int itemId, int num, bool showTip = true, bool jump = true)
    {
        var have = GetHoldCount(itemId);
        var result = false;
        var itemName = "";
        // 免费钻石
        if (itemId == ItemId.FreeDiamond)
        {
            if (have < num)
            {
                var paidDiamondHave = GetHoldCount(ItemId.PaidDiamond);
                var addNum = num - have;
                if (paidDiamondHave >= addNum)
                {
                    itemName = GetLocalizedName(ItemId.PaidDiamond);
                    result = await Dialog.AskAsync("",
                        LocalizationManager.Stuff.GetText("common_PaidDiamondAdd", addNum, itemName));
                    if (!result) return false;

                    // TODO:这里要看服务器是不是也支持 - 后续最好是直接能转化差价
                    // return true;
                    await UIEngine.Stuff.ForwardOrBackToAsync<ShopPageV2>("diamond");
                    return false;
                }

                itemName = GetLocalizedName(ItemId.FreeDiamond);
                result = await Dialog.AskAsync("",
                    LocalizationManager.Stuff.GetText("common_FreeDiamondNotEnough", itemName));
                if (!result) return false;

                await UIEngine.Stuff.ForwardOrBackToAsync<ShopPageV2>("point");
                return false;
            }

            return true;
        }

        // 点券钻石
        if (itemId == ItemId.PaidDiamond)
        {
            if (have < num)
            {
                itemName = GetLocalizedName(ItemId.PaidDiamond);
                result = await Dialog.AskAsync("",
                    LocalizationManager.Stuff.GetText("common_FreeDiamondNotEnough", itemName));
                if (!result) return false;

                await UIEngine.Stuff.ForwardOrBackToAsync<ShopPageV2>("point");
                return false;
            }

            return true;
        }

        // 普通物品如果开启Jump则跳转，如果开启提示则showTip
        if (have < num)
        {
            // if (jump)
            // {
            //     
            // }
            if (showTip)
            {
                ToastManager.ShowLocalize("common_itemNotEnough");
            }

            Debug.LogWarning($"[itemId={itemId}] count:{have}/{num}");
            return false;
        }

        return true;
    }

    public static bool CheckNewItem(DatabaseTransaction cache)
    {
        return cache.t == TransactionType.Add || cache.t == TransactionType.New;
    }

    public static string GetLocalizedName(int itemId)
    {
        var desc = StaticDataUtil.GetAnyFieldOfAnyRow<string>(itemId, "Name");
        desc = LocalizationManager.Stuff.GetText(desc);
        return desc;
    }

    public static string GetIconAddress(int itemId)
    {
        //var row = StaticDataUtil.GetAnyRow(itemId) as ICommonItem;
        //if (row == null)
        //{
        //    return null;
        //}
        var icon = StaticDataUtil.GetAnyFieldOfAnyRow<string>(itemId, "Icon");
        //var icon = row.Icon;
        var iconAddress = $"{icon}.png";
        return iconAddress;
    }

    public static string GetLocalizedDes(int itemId)
    {
        var desc = StaticDataUtil.GetAnyFieldOfAnyRow<string>(itemId, "Desc");
        desc = LocalizationManager.Stuff.GetText(desc);
        return desc;
    }
}
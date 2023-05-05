using System;
using System.Collections.Generic;

public static class CostReporter
{
    private static readonly int[] reportIds =
    {
        ItemId.Gold,
        ItemId.FreeDiamond,
        ItemId.PaidDiamond,
    };
    
    /// <summary>
    /// 消耗上报
    /// </summary>
    /// <param name="costItems">消耗内容</param>
    /// <param name="gets">备注获得了什么</param>
    /// <typeparam name="T"></typeparam>
    public static void Report<T>(List<T> costItems, string gets = "") where T : ICostItem
    {
        foreach (var costItem in costItems)
        {
            Report(costItem.Id, costItem.Num, gets);
        }
    }
    
    /// <summary>
    /// 消耗上报
    /// </summary>
    /// <param name="costMap">消耗字典</param>
    /// <param name="gets">备注获得了什么</param>
    public static void Report(Dictionary<int, int> costMap, string gets = "")
    {
        foreach (var kv in costMap)
        {
            Report(kv.Key, kv.Value, gets);
        }
    }

    /// <summary>
    /// 消耗上报
    /// </summary>
    /// <param name="itemId"></param>
    /// <param name="count"></param>
    /// <param name="gets">备注获得了什么</param>
    public static void Report(int itemId, int count, string gets = "")
    {
        // 如果不是需要上报的，就忽略
        if (Array.IndexOf(reportIds, itemId) < 0) return;

        var costName = ItemUtil.GetLocalizedName(itemId);
        TrackManager.SendVirtualCurrency(costName, "" + itemId, costName, count, gets);
    }
}
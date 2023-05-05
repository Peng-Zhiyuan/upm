/* Created:Loki Date:2023-03-01*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using CustomLitJson;
using UnityEngine;

public class GiftTriggerManager : Singleton<GiftTriggerManager>
{
    private DateTime FlagCheckTime;

    public override void Init()
    {
        FlagCheckTime = Clock.Now;
    }

    public void SetGiftCheckTime(DateTime _flagCheckTime)
    {
        FlagCheckTime = _flagCheckTime;
    }

    /// <summary>
    /// 获取最新礼包信息
    /// </summary>
    /// <returns></returns>
    public GiftInfo GetNewGiftInfo()
    {
        Debug.LogWarning("Gift Check Tim " + FlagCheckTime.ToLongTimeString());
        List<GiftInfo> lst = Database.Stuff.giftDatabase.GetAll(GiftSource.Trigger);
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].status == OrderStatus.Complete
                || (lst[i].expire != 0 && Clock.ToDateTime(lst[i].expire) < Clock.Now))
            {
                continue;
            }
            Debug.LogWarning("Gift Check Tim " + lst[i].id + "   " + Clock.ToDateTime(lst[i].update).ToLongTimeString());
            if (Clock.ToDateTime(lst[i].update) > FlagCheckTime)
            {
                return lst[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 获取激活礼包列表
    /// </summary>
    /// <returns></returns>
    public List<GiftInfo> GetActiveGiftLst()
    {
        List<GiftInfo> tempLst = new List<GiftInfo>();
        List<GiftInfo> lst = Database.Stuff.giftDatabase.GetAll(GiftSource.Trigger);
        for (int i = 0; i < lst.Count; i++)
        {
            if (lst[i].status == OrderStatus.Complete
                || (lst[i].expire != 0 && Clock.ToDateTime(lst[i].expire) < Clock.Now))
            {
                continue;
            }
            tempLst.Add(lst[i]);
        }
        return tempLst;
    }

    public async Task BtnBuyGiftTrigger(string giftID)
    {
        var response = await GiftApi.PurchseAsync(giftID);
        Database.Stuff.giftDatabase.UpdateInfo(response.Item1);
        UiUtil.ShowRewardByCache(response.Item2);
    }

    // public async Task<(GiftInfo, List<JsonData>)> DebugAddGift(string id)
    // {
    //     var jd = new JsonData();
    //     jd["id"] = id;
    //     var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<GiftInfo>(ServerType.Game, "debug/gift/create", jd);
    //     return (ret, cache);
    // }
}
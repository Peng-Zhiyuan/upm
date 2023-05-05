using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public class OrderApi 
{
    public static async Task<NetPage<OrderInfo>> FetchAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<OrderInfo>>(ServerType.Game, "business/order/getter", pageArg);
        return ret;
    }

    /// <summary>
    /// 订单发货
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public static async Task<OrderInfo> SubmitAsync(string orderId, DisplayType autoShowReward)
    {
        var jd = new JsonData();
        jd["id"] = orderId;
        var netMsg = await NetworkManager.Stuff.CallAsync<OrderInfo>(ServerType.Game, "business/order/submit", jd, null, true, isAutoShowReward: autoShowReward);
        return netMsg;

    }
}

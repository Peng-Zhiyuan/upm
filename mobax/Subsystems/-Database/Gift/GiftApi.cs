using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public class GiftApi
{
    public static async Task<NetPage<GiftInfo>> RequestAsync(JsonData arg)
    {
        var data = await NetworkManager.Stuff.CallAsync<NetPage<GiftInfo>>(ServerType.Game, "business/gift/getter", arg);
        return data;
    }

    public static async Task<List<string>> SubmitAsync(List<string> idList)
    {
        var jd = NativeArrayToJdArry(idList);
        var data = await NetworkManager.Stuff.CallAsync<List<string>>(ServerType.Game, "business/gift/submit", jd, null, false, DisplayType.Show);
        return data;
    }

    static JsonData NativeArrayToJdArry(List<string> idList)
    {
        var jd = new JsonData();
        jd.SetJsonType(JsonType.Array);
        foreach (var one in idList)
        {
            jd.Add(one);
        }
        return jd;
    }

    public static async Task<(GiftInfo, List<JsonData>)> PurchseAsync(string giftID)
    {
        var jd = new JsonData();
        jd["id"] = giftID;
        var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<GiftInfo>(ServerType.Game, "business/gift/purchase", jd);
        return (ret, cache);
    }
}
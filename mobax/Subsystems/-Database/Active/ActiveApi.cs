using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;

public class ActiveApi
{
    public static async Task<NetPage<ActiveInfo>> FetchAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<ActiveInfo>>(ServerType.Game, "getter/active", pageArg);
        return ret;
    }

    public static async Task<ActiveInfo> SuperMailSubmit(string id, DisplayType isAutoShowReward)
    {
        var jd = new JsonData();
        jd["id"] = id;
        var ret = await NetworkManager.Stuff.CallAsync<ActiveInfo>(ServerType.Game, "active/supmail/submit", jd, isAutoShowReward: isAutoShowReward);
        return ret;
    }

    public static async Task<(ActiveInfo, List<JsonData>)> SuperMailDelete(string id)
    {
        var jd = new JsonData();
        jd["id"] = id;
        var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<ActiveInfo>(ServerType.Game, "active/supmail/delete", jd);
        return (ret, cache);
    }

    public static async Task<List<JsonData>> EliminatePrize(int id)
    {
        var jd = new JsonData { ["id"] = id };
        var (_, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<ActiveInfo>(ServerType.Game, "active/evaluate", jd);
        return cache;
    }

    public static async Task<ActiveInfo> QuestionAsync()
    {
        var ret = await NetworkManager.Stuff.CallAsync<ActiveInfo>(ServerType.Game, "active/question", null, null, true, DisplayType.Show);
        return ret;
    }

    public static async Task<ActiveInfo> DiscardAsync()
    {
        var ret = await NetworkManager.Stuff.CallAsync<ActiveInfo>(ServerType.Game, "active/follow", null, null, true, DisplayType.Show);
        return ret;
    }

    public static async Task<int> SuperSaleInfo(int saleID)
    {
        var jd = new JsonData();
        jd["id"] = saleID;
        var ret = await NetworkManager.Stuff.CallAsync<int>(ServerType.Game, "active/sale/getter", jd);
        return ret;
    }

    public static async Task<(ActiveInfo, List<JsonData>)> SuperSaleSubmit(int saleGoodsID)
    {
        var jd = new JsonData();
        jd["id"] = saleGoodsID;
        var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<ActiveInfo>(ServerType.Game, "active/sale/submit", jd);
        return (ret, cache);
    }

    public static async Task<(ActiveInfo, List<JsonData>)> ActivityTaskSubmit(int rookieTaskID)
    {
        var jd = new JsonData();
        jd["id"] = rookieTaskID;
        var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<ActiveInfo>(ServerType.Game, "active/rookie/submit", jd);
        return (ret, cache);
    }

    public static async Task<(ActiveInfo, List<JsonData>)> ActivityScroungeWeekly()
    {
        var jd = new JsonData();
        var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<ActiveInfo>(ServerType.Game, "active/scrounge/weekly", jd);
        return (ret, cache);
    }
    public static async Task<(ActiveInfo, List<JsonData>)> ActivityScroungeMonthly()
    {
        var jd = new JsonData();
        var (ret, cache) = await NetworkManager.Stuff.CallAndGetCacheAsync<ActiveInfo>(ServerType.Game, "active/scrounge/monthly", jd);
        return (ret, cache);
    }
}
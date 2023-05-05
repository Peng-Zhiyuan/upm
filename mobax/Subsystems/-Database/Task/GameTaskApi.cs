using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;
using Spine;

public static class GameTaskApi
{
    public static async Task<NetPage<GameTaskInfo>> FetchAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<GameTaskInfo>>(ServerType.Game, "task/getter", pageArg);
        return ret;
    }

    public static async Task<GameTaskInfo> SubmitChainAsync(int rowId, DisplayType displayType)
    {
        var param = new JsonData();
        param["id"] = rowId;
        var ret = await NetworkManager.Stuff.CallAsync<GameTaskInfo>(ServerType.Game, "task/submit", param, isAutoShowReward: displayType);
        return ret;
    }

    public static async Task<GameTaskInfo> SubmitDailyAsync(int rowId, DisplayType displayType)
    {
        var param = new JsonData();
        param["id"] = rowId;
        var ret = await NetworkManager.Stuff.CallAsync<GameTaskInfo>(ServerType.Game, "task/daily/submit", param, isAutoShowReward: displayType);
        return ret;
    }

    public static async Task<ActiveInfo> SubmitDailyRewardAsync(int rowId, DisplayType displayType)
    {
        var param = new JsonData();
        param["id"] = rowId;
        var ret = await NetworkManager.Stuff.CallAsync<ActiveInfo>(ServerType.Game, "task/daily/reward", param, null, true, displayType);
        return ret;
    }

    public static async Task<JsonData> SubmitTowerDailyRewardAsync(int towerID, DisplayType displayType)
    {
        var param = new JsonData();
        param["id"] = towerID;
        var ret = await NetworkManager.Stuff.CallAsync<JsonData>(ServerType.Game, "task/daily/tower", param, null, true, displayType);
        return ret;
    }

    public static async Task<GameTaskInfo> SubmitWeeklyAsync(int rowId, DisplayType displayType)
    {
        var param = new JsonData();
        param["id"] = rowId;
        var ret = await NetworkManager.Stuff.CallAsync<GameTaskInfo>(ServerType.Game, "task/weekly/submit", param, isAutoShowReward: displayType);
        return ret;
    }

    public static async Task<ActiveInfo> SubmitWeeklyRewardAsync(int rowId, DisplayType displayType)
    {
        var param = new JsonData();
        param["id"] = rowId;
        var ret = await NetworkManager.Stuff.CallAsync<ActiveInfo>(ServerType.Game, "task/weekly/reward", param, null, true, displayType);
        return ret;
    }

    // pzy:
    // 应当移到别处
    public static async Task<GameTaskInfo> ChatAsync()
    {
        var ret = await NetworkManager.Stuff.CallAsync<GameTaskInfo>(ServerType.Game, "task/events/chat", null, null, false);
        return ret;
    }

    // pzy:
    // 应当移到别处
    public static async Task<GameTaskInfo> FollowAsync(int count)
    {
        var param = new JsonData();
        param["val"] = count;
        var ret = await NetworkManager.Stuff.CallAsync<GameTaskInfo>(ServerType.Game, "task/events/follow", param, null, false);
        return ret;
    }



}

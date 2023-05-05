using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;

public static class HeroCircuitApi
{
    /** 拼图升级 */
    public static async Task LevelUp(string id, int lv)
    {
        var param = new JsonData
        {
            ["id"] = id,
            ["lv"] = lv
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "puzzle/up", param);
        TrackManager.CustomReport("Puzzle_Levelup", new Dictionary<string, string>()
        {
            {"Id", $"{id}"},
        });
    }

    /** 拼图突破 */
    public static async Task Break(string id)
    {
        var param = new JsonData
        {
            ["id"] = id,
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "puzzle/break", param);
        TrackManager.CustomReport("Puzzle_Break", new Dictionary<string, string>()
        {
            {"Id", $"{id}"},
        });
    }
    
    /** 某个hero的拼图统统卸下来 */
    public static async Task Clear(string heroInstanceId)
    {
        var param = new JsonData
        {
            ["id"] = heroInstanceId
        };
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "puzzle/clear", param);
    }

    /** 拼图合成 */
    public static async Task<(JsonData, List<JsonData>)> Compose(string ids, string lockedId)
    {
        var param = new JsonData
        {
            ["ids"] = ids,
            ["selectId"] = lockedId,
        };
        var ret = await NetworkManager.Stuff.CallAndGetCacheAsync<JsonData>(ServerType.Game, "puzzle/compose", param);
        TrackManager.CustomReport("Puzzle_Advance", new Dictionary<string, string>()
        {
            {"Id", $"{ids}"},
        });
        return ret;
    }

    public static async Task<(JsonData, List<JsonData>)> Compose(string[] idArr, string lockedId)
    {
        return await Compose(string.Join(",", idArr), lockedId);
    }

    /** 拼图分解 */
    public static async Task Resolve(List<string> items)
    {
        var param = new JsonData
        {
            ["ids"] = string.Join(",", items),
        };
        await NetworkManager.Stuff.CallAsync<JsonData>(ServerType.Game, "puzzle/resolve", param, isAutoShowReward: DisplayType.Show);
        TrackManager.CustomReport("Puzzle_Resolve", new Dictionary<string, string>()
        {
            {"ResolveNum", $"{items.Count}"},
        });
    }

    public static async Task<bool> Set(int heroId, Dictionary<string, JsonData> opMap, List<string> offArr)
    {
        var param = new JsonData
        {
            ["heroid"] = heroId,
        };
        if (null != opMap)
        {
            param["items"] = JsonUtil.ToJsonData(opMap);
        }

        if (null != offArr)
        {
            param["off"] = JsonUtil.ToJsonData(offArr);
        }

        var ret = await NetworkManager.Stuff.CallAllowLogicFailAsync<JsonData>(ServerType.Game, "puzzle/set", param);

        if (ret.IsSuccess)
        {
            var putStr = null != opMap ? string.Join(",", opMap.Keys) : "";
            var offStr = null != offArr ? string.Join(",", offArr) : "";
            TrackManager.CustomReport("Puzzle_Put", new Dictionary<string, string>()
            {
                {"heroid", $"{heroId}"},
                {"on", $"{putStr}"},
                {"off", $"{offStr}"},
            });
        }
        return ret.IsSuccess;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System.Threading.Tasks;

public static class ArenaApi 
{
    /// <summary>
    /// 获取我的竞技场信息
    /// </summary>
    /// <param name="k"></param>
    /// <param name="v"></param>
    public static async Task<ArenaInfo> RequestInfoAsync()
    {
        var ret = await NetworkManager.Stuff.CallAsync<ArenaInfo>(ServerType.Game, "arena/info");
        return ret;
    }

    /// <summary>
    /// 购买挑战次数
    /// </summary>
    /// <param name="count">当前第几次购买</param>
    /// <returns></returns>
    public static async Task PurchaseTicketAsync(int count)
    {
        var jd = new JsonData();
        jd["num"] = count;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "arena/purchase", jd);
    }

    /// <summary>
    /// 购买挑战次数
    /// </summary>
    /// <param name="count">当前第几次刷新</param>
    /// <returns></returns>
    public static async Task<List<ArenaEnemyInfo>> RefreshAsync(int count, List<string> ignoreIdList)
    {


        var jd = new JsonData();
        jd["num"] = count;




        if (ignoreIdList != null)
        {
            var jd2 = new JsonData();
            jd2.SetJsonType(JsonType.Array);
            for (int i = 0; i < ignoreIdList.Count; i++)
            {
                var one = ignoreIdList[i];
                jd2.Add(one);
                //jd[i] = one;
            }
            jd["ignore"] = jd2;
        }


        var ret = await NetworkManager.Stuff.CallAsync<List<ArenaEnemyInfo>>(ServerType.Game, "arena/refresh", jd);
        return ret;
    }

    /// <summary>
    /// 获取对手
    /// </summary>
    /// <returns></returns>
    public static async Task<List<ArenaEnemyInfo>> RequestPlayersAsync(List<string> ignoreIdList)
    {
        var jd = new JsonData();
        jd.SetJsonType(JsonType.Array);
        if (ignoreIdList != null)
        {
            for (int i = 0; i < ignoreIdList.Count; i++)
            {
                var one = ignoreIdList[i];
                jd.Add(one);
            }
        }

        var obj = new JsonData();
        obj["ignore"] = jd;

        var ret = await NetworkManager.Stuff.CallAsync<List<ArenaEnemyInfo>>(ServerType.Game, "arena/players", obj);
        return ret;
    }


    /// <summary>
    /// 历史最高分数
    /// </summary>
    /// <returns></returns>
    public static async Task<ArenaInfo> RequestHighestAsync()
    {
        var ret = await NetworkManager.Stuff.CallAsync<ArenaInfo>(ServerType.Game, "arena/highest");
        return ret;
    }

    /// <summary>
    /// 历史赛季记录, pageIndex 从 1 开始
    /// </summary>
    /// <returns></returns>
    public static async Task<NetPage<ArenaInfo>> RequestHistroyAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<ArenaInfo>>(ServerType.Game, "arena/history", pageArg);
        return ret;
    }

    /// <summary>
    /// 领取积分奖励
    /// </summary>
    /// <returns>修改后的位桶</returns>
    public static async Task<ulong> RequestReceiveSocreReward(int id)
    {
        var jd = new JsonData();
        jd["id"] = id;
        var ret = await NetworkManager.Stuff.CallAsync<ulong>(ServerType.Game, "arena/reward", jd, isAutoShowReward: DisplayType.Show);
        return ret;
    }

    /// <summary>
    /// 排行榜
    /// </summary>
    /// <param name="pageArg"></param>
    /// <returns></returns>
    public static async Task<NetPage<ArenaEnemyInfo>> RequestRanklistAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<ArenaEnemyInfo>>(ServerType.Game, "socialize/arena/range", pageArg);
        return ret;
    }

    /// <summary>
    /// 查询我的排名
    /// </summary>
    /// <returns></returns>
    public static async Task<int> RequestMyRankAsync()
    {
        var jd = new JsonData();
        jd["uid"] = Database.Stuff.roleDatabase.Me._id;
        var ret = await NetworkManager.Stuff.CallAsync<int>(ServerType.Game, "socialize/arena/rank", jd);
        return ret;
    }
}

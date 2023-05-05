using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using BattleEngine.Logic;
using CustomLitJson;
using ProtoBuf;

public static class BattleApi
{
    public static async Task<CreateRoomResponse> CreateRoomAsync(params string[] allowedUidList)
    {
        var uidString = string.Join(",", allowedUidList);
        //Debug.LogError("CreateRoomAsync:" + uidString);
        var arg = new JsonData();
        arg["uids"] = uidString;
        var response = await NetworkManager.Stuff.CallAsync<CreateRoomResponse>(ServerType.Game, "battle/create", arg);
        return response;
    }

    public enum BattleKind
    {
        /// <summary>
        /// 主线关卡
        /// </summary>
        Pve = 1,

        /// <summary>
        /// 竞技场
        /// </summary>
        Arena = 2,

        /// <summary>
        /// 记忆回廊
        /// </summary>
        Memory = 3,

        /// <summary>
        /// 爬塔
        /// </summary>
        Tower = 4,
    }

    public static async Task<BattlePlayer> RequestPlayerAsync(string uid)
    {
        var param = new JsonData();
        param["pid"] = uid;
        var response = await NetworkManager.Stuff.CallAsync<BattlePlayer>(ServerType.Game, "battle/player", param);
        return response;
    }

    public static async Task<BattlePlayer> RequestFriendAssistAsync(string friendUid)
    {
        var param = new JsonData();
        param["pid"] = friendUid;
        var response = await NetworkManager.Stuff.CallAsync<BattlePlayer>(ServerType.Game, "battle/assist", param);
        if (response.heroes.Count > 0)
        {
            var hero = response.heroes[0];
            TrackManager.CustomReport("Help", hero.id + "", hero.lv + "");
        }
        return response;
    }

    public static async Task<BattlePlayer> RequestDreamscapeAssistAsync(int heroID)
    {
        var param = new JsonData();
        param["pid"] = heroID;
        var response = await NetworkManager.Stuff.CallAsync<BattlePlayer>(ServerType.Game, "battle/memory", param);
        return response;
    }

    /// <summary>
    /// 创建关卡战斗
    /// </summary>
    /// <param name="kind">战斗类型</param>
    /// <param name="formationIndex">使用阵容，留空使用默认</param>
    /// <param name="opponentUidString">对手 uid，多个对手使用逗号字符串</param>
    /// <param name="allowedUidList"></param>
    /// <returns></returns>
    public static async Task<CreateBattleResponse> CreateBattle(BattleKind kind, EFormationIndex ? formationIndex, List<string> opponentUidList, bool isDebug, string assistUID = "", int heroid = -1)
    {
        var uidString = string.Join(",", opponentUidList);
        var param = new JsonData();
        param["kind"] = (int)kind;
        param["target"] = uidString;
        if (formationIndex != null)
        {
            param["corps"] = (int)formationIndex;
        }
        if (isDebug)
        {
            param["debug"] = 1;
        }
        else
        {
            param["debug"] = 0;
        }
        if (heroid != -1)
        {
            param["tutor"] = heroid;
        }
        if (BattleUtil.fast_enter)
        {
            param["debug"] = 1;
        }
        if (!string.IsNullOrEmpty(assistUID))
        {
            var isFriend = SocializeManager.Stuff.IsFriend(assistUID) || SocializeManager.Stuff.IsFollow(assistUID);
            param["assist"] = assistUID;
            param["assistType"] = isFriend ? 1 : 2; // 陌生人2

            // pzy:
            // 好友需要填亲密度
            if (isFriend)
            {
                var qinmidu = SocializeManager.Stuff.GetIntimacy(assistUID);
                param["assistVal"] = qinmidu;
            }
        }

        // pzy:
        // 这里允许服务器拒绝没意义
        // 反而影响显示报错信息
        //var response = await NetworkManager.Stuff.CallAllowLogicFailAsync<CreateBattleResponse>(ServerType.Game, "battle/create",param);
        //if (response.IsSuccess)
        //{
        //    return ((NetMsg<CreateBattleResponse>) response).data;
        //}
        //return null;
        var data = await NetworkManager.Stuff.CallAsync<CreateBattleResponse>(ServerType.Game, "battle/create", param);
        return data;
    }

    //public static async Task<CreateBattleResponse> CreateBattleAsync(int battleType, int gkId, params string[] allowedUidList)
    //{
    //    var uidString = string.Join(",", allowedUidList);
    //    var arg = new JsonData();
    //    arg["bt"] = battleType;
    //    arg["id"] = gkId;
    //    arg["uids"] = uidString;
    //    if (DeveloperLocalSettings.IsDevelopmentMode)
    //    {
    //        arg["debug"] = 1;
    //    }
    //    else
    //    {
    //        arg["debug"] = 0;
    //    }
    //    var response = await NetworkManager.Stuff.RequestAssertSuccessAsync<CreateBattleResponse>(ServerType.Game, "stage/create", arg);
    //    return response;
    //}

    public static async Task<JsonData> PVEBattleResultAsync(BattleInfo battleInfo)
    {
        var arg = new JsonData();
        arg["win"] = battleInfo.win;
        arg["id"] = battleInfo.id;
        arg["corps"] = battleInfo.corps;
        arg["data"] = battleInfo.data;
        var response = await NetworkManager.Stuff.CallAsync<JsonData>(ServerType.Game, "stage/result", arg);
        return response;
    }

    /// <summary>
    /// 战斗结算
    /// </summary>
    /// <param name="battleId">战斗 id， 由创建战斗的接口获取</param>
    /// <param name="winnerCamp">获胜放阵营（1/2），在创建战斗接口中查询自己的阵营</param>
    /// <returns></returns>
    public static async Task<(ArenaInfo info, List<JsonData> cache)> ArenaResultAsync(string battleId = null, int? winnerCamp = null)
    {
        var jd = new JsonData();
        if (battleId != null)
        {
            jd["id"] = battleId;
        }
        if (winnerCamp != null)
        {
            jd["winner"] = winnerCamp;
        }
        if (BattleUtil.fast_enter)
        {
            jd["debug"] = 1;
        }
        if (BattleDataManager.Instance.ItemRecord.Count > 0)
        {
            var data = new JsonData();
            foreach (var VARIABLE in BattleDataManager.Instance.ItemRecord)
            {
                data[VARIABLE.Key.ToString()] = VARIABLE.Value;
            }
            jd["sub"] = data;
        }
        var response = await NetworkManager.Stuff.CallAndGetCacheAsync<ArenaInfo>(ServerType.Game, "battle/result", jd);
        return response;
    }
}
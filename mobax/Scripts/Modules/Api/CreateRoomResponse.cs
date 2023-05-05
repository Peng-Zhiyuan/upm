using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CustomLitJson;
using ProtoBuf;

public struct CreateRoomResponse
{
    public string Id;
    public string Address;
}

public class CreateBattleResponse
{
    // 战斗id
    public string id;

    /// <summary>
    /// 服务器地址
    /// </summary>
    public string address;

    /// <summary>
    /// 战斗的状态(比如：匹配中，战斗中...)
    /// </summary>
    public string status;

    /// <summary>
    /// 过期时间
    /// </summary>
    public string expire;

    /// <summary>
    /// 战斗类型
    /// </summary>
    public string kind;

    /// <summary>
    /// 额外数据,可以解析为不同的结构?（助战/记忆回廊)
    /// </summary>
    public JsonData attach;

    /// <summary>
    /// 玩家信息
    /// </summary>
    public List<BattlePlayerInfo> players;

    public int GetCampByUid(string uid)
    {
        foreach (var one in this.players)
        {
            if (one.uid == uid)
            {
                return one.camp;
            }
        }
        throw new Exception($"not found uid `{uid}` in battle info");
    }
}

public class BattlePlayerInfo
{
    /// <summary>
    /// 阵营
    /// </summary>
    public int camp;

    /// <summary>
    /// 所使用阵容的 id
    /// </summary>
    public int corps;

    /// <summary>
    /// 用户 uid
    /// </summary>
    public string uid;
}

//public class BattleResultResponse
//{
//    public string Id;
//}

/// <summary>
/// 战斗数据
/// </summary>
/// param data {kill:击杀数,time:战斗使用时间(秒),coins:获得金币数量}
public struct BattleInfo
{
    public int win;
    public int id;
    public int corps;
    public JsonData data;
}

public class BattlePlayer
{
    public string uid;
    public string name;
    public OnlineStatus online;
    public int Seed;
    public List<BattleHero> heroes;
    public List<int> corps;
}

public class BattleHero
{
    public string oid;
    public int id;
    public int lv;
    public int[] Attr;
    public Dictionary<int, int> Skill;
    public int star;
}
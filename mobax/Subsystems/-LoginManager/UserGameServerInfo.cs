using System.Collections;
using System.Collections.Generic;
using CustomLitJson;
using UnityEngine;

public class UserGameServerInfo
{
    /// <summary>
    /// 角色数据的简介，只包含少数字段
    /// </summary>
    public LoginRoleInfo[] roles;
    //public GameServerCookie cookie;
    public ServerBattleInfo battle;

    /// <summary>
    /// 开服时间
    /// </summary>
    public long ServerTime;

    // +0800
    public string TimeZone;
}

public class GameServerCookie
{
    public string key;
    public string val;
}

public class ServerBattleInfo
{
    public string id;
    public int kind;
    public int status;
    public JsonData attach;
}

//public class QueueServerInfo
//{
//    public int port;
//    public string url;
//}


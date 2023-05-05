using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using CustomLitJson;

public class RoleApi
{
    public static async Task<RoleInfo> FetchMyselfAsync()
    {
        var ret = await NetworkManager.Stuff.CallAsync<RoleInfo>(ServerType.Game, "getter/role");
        return ret;
    }

    /// <summary>
    /// 查询多个用户的简单信息
    /// 一定报错 "_id", "name", "lv", "sid", "icon" 字段
    /// </summary>
    /// <param name="uids"></param>
    /// <param name="keys">额外字段</param>
    /// <returns></returns>
    public static async Task<RoleInfo[]> RequestRoleInfo(List<string> uids, List<string> keys, bool isBlock)
    {
        var arg = new JsonData();
        arg["uid"] = string.Join(",", uids);
        arg["fields"] = string.Join(",", keys);
        var ret = await NetworkManager.Stuff.CallAsync<RoleInfo[]>(ServerType.Game, "getter/players", arg, null, isBlock);
        return ret;
    }

    /// <summary>
    /// 查询单个用户的详细信息
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="isBlock"></param>
    /// <returns></returns>
    public static async Task<RoleInfo> RequestSocialInfo(string uid, bool isBlock)
    {
        var arg = new JsonData();
        arg["pid"] = uid;
        var ret = await NetworkManager.Stuff.CallAsync<RoleInfo>(ServerType.Game, "player/getinfo", arg, null, isBlock);
        return ret;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;
using GPC.Helper.Jingwei.Script.Common;

public static class GameServerApi
{
    /// <summary>
    /// 向服务器选择角色，服务器会保存当前角色
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public static async Task<LoginRoleInfo> SelectRoleAsync(string uid)
    {
        var arg = new JsonData();
        arg["uid"] = uid;
        var roleData =
            await NetworkManager.Stuff.CallAsync<LoginRoleInfo>(ServerType.Game, "role/select", arg);
        return roleData;
    }

    public static async Task<CreateRoleResponse> CreateRoleAsync(string nickName, string iconUrl)
    {
        var arg = new JsonData();
        arg["name"] = nickName;
        arg["icon"] = iconUrl;

        if(IggSdkManager.Stuff.IsIggChannel)
        {
            arg["gameid"] = LauncherIggSdkManager.Stuff.GameId;
            arg["udid"] = KungfuInstance.Get().UDID;
            arg["client_version"] = GameUtil.Version;
        }


        var roleData = await NetworkManager.Stuff.CallAsync<CreateRoleResponse>(ServerType.Game, "role/create", arg);
        return roleData;
    }

    public static async Task<UserGameServerInfo> LoginGameServerAsync(string guid, string access)
    {
        var arg = new JsonData();
        arg["guid"] = guid;
        arg["access"] = access;

        if (IggSdkManager.Stuff.IsIggChannel)
        {
            arg["gameid"] = LauncherIggSdkManager.Stuff.GameId;
            arg["udid"] = KungfuInstance.Get().UDID;
            arg["client_version"] = GameUtil.Version;
        }

        var msg = await NetworkManager.Stuff.CallAsync<UserGameServerInfo>(ServerType.Game, "login", arg);
        return msg;
    }

    public static async Task RenameAsync(string name)
    {
        var arg = new JsonData();
        arg["name"] = name;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "setting/role", arg);
    }
    
    public static async Task ResetHeadFrameAsync(int id)
    {
        var arg = new JsonData();
        arg["headframe"] = id;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "setting/role", arg);
    }
    
    /// <summary>
    /// 签名
    /// </summary>
    /// <param name="content"></param>
    public static async Task SignAsync(string content)
    {
        var arg = new JsonData();
        arg["describe"] = content;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "setting/role", arg);
    }
}
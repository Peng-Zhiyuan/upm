using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class LoginManager : StuffObject<LoginManager>
{
    public LoginSession session;

    public static event Action<ServerPushType> ServerPushArrival;
    public static void InvokServerPushArrival(ServerPushType type)
    {
        ServerPushArrival?.Invoke(type);
    }


    async Task LoginPlatformServer()
    {

        Debug.Log("[LoginManager] login platform");
        this.session.userPlatformInfo = await ChannelManager.Channel.LoginAsync();
        var access = this.session.userPlatformInfo.token;

        Debug.Log("[LoginManager] request server list");
        var idToGameServerinfoObj = await PlatformApi.RequestGameServerInfoAsync(access);
        this.session.idToServerInfoDic = idToGameServerinfoObj;

        Debug.Log("[LoginManager] select game server");
        var sidFilter = EnvManager.GetConfigOfFinalEnv("sidFilter");
        Debug.Log("sidFilter: " + sidFilter);

        // 过滤合法的 sid 字典
        var validateSidToServerDic = new Dictionary<int, GameServerInfo>();
        foreach(var kv in this.session.idToServerInfoDic)
        {
            var sid = kv.Key;
            var info = kv.Value;
            if(LoginUtil.IsMatchedSidFilter(sid, sidFilter))
            {
                validateSidToServerDic[sid] = info;
            }
        }
        this.session.selectedServerId = await this.SelectServerId(this.session.userPlatformInfo, validateSidToServerDic);
        Debug.Log("[LoginManager] select game server id: " + this.session.selectedServerId);


    }

    void Update()
    {
        this.session?.Update();
    }

    public void Logout()
    {
        this.session = null;
        OnSessionReleased?.Invoke();
    }

    public async Task LoginAsync(Func<Task> showLoadingPageDelegate, Action<float> percentUpdateDelegate)
    {
        try
        {
            this.session = new LoginSession();

            await ChannelManager.Channel.OnLoginStartAsync();

            await this.LoginPlatformServer();
            await this.LoginGameServer();

            // 处理时区
            var timeZoneString = this.session.userGameServerInfo.TimeZone;
            // like "+0800";
            int timeZone = int.Parse(timeZoneString);
            var timeZoneHour = timeZone / 100;
            Clock.Timezone = timeZoneHour;

            await showLoadingPageDelegate?.Invoke();

            // 等待时区换算完成再更新database -- xinwusai
            await OnFetchUserData?.Invoke(percent =>
            {
                percentUpdateDelegate?.Invoke(percent);
            });

            ChannelManager.Channel.OnLoginComplete();
            
        }
        catch
        {
            this.session = null;
            OnSessionReleased?.Invoke();
            throw;
        }
    }

    async Task LoginGameServer()
    {
        var guid = this.session.userPlatformInfo.guid;
        var access = this.session.userPlatformInfo.token;

        //var isPostAccess = ChannelManager.Channel.IsLoginGameServerPostAccess;
        //if(!isPostAccess)
        //{
        //    access = "";
        //}

        var userInfo = await GameServerApi.LoginGameServerAsync(guid, access);
        this.session.userGameServerInfo = userInfo;
        //this.session.cookie = userInfo.cookie;

        // 角色处理
        LoginRoleInfo roleData;
        if(this.session.userGameServerInfo.roles == null || this.session.userGameServerInfo.roles.Length == 0)
        {
            // 没角色就创建
            var createRoleResponse = await GameServerApi.CreateRoleAsync(guid, "");
            roleData = createRoleResponse.role;
        }
        else
        {
            // 有角色就选择
            roleData = this.session.userGameServerInfo.roles[0];
        }


        var nowSec = Clock.TimestampSec;
        if(LoginRoleUtil.IsForbiden(roleData, nowSec))
        {
            throw new GameException(ExceptionFlag.None, "account forbiden", "ACCOUNT_FORBIDEN");
        }

        this.session.selectedRoleData = roleData;

        Debug.Log("[LoginManager] game sever login success");
    }

    public static Action OnSessionReleased;

    public static Func<Action<float>, Task> OnFetchUserData;

    public static Func<Task<string>> OnInputUsername;

    public static Func<UserPlatformInfo, Dictionary<int, GameServerInfo>, Task<int>> onSelectServer;
    async Task<int> SelectServerId(UserPlatformInfo userPlatformInfo, Dictionary<int, GameServerInfo> idToServerInfoDic)
    {
        if(onSelectServer != null)
        {
            var sid = await onSelectServer.Invoke(userPlatformInfo, idToServerInfoDic);
            return sid;
        }
        else
        {
            Debug.Log("[LoginManager] onSelectServer not set, auto select sid");
            var sid = AutoSelectServerId(userPlatformInfo, idToServerInfoDic);
            return sid;
        }
    }


    static int AutoSelectServerId(UserPlatformInfo userPlatformInfo, Dictionary<int, GameServerInfo> idToServerInfoDic)
    {
        // 如果有以前登录的选取记录，选择上一次进入的区
        if (userPlatformInfo.gzone.Length > 0)
        {
            var lastIndex = userPlatformInfo.gzone.Length - 1;
            var lastServerId = userPlatformInfo.gzone[lastIndex];
            if (idToServerInfoDic[lastServerId] != null)
            {
                return lastServerId;
            }
        }


        //UIEngine.Stuff.Forward("SelectServerPage", idToServerInfoDic);

        // 否则选择选择最后一个开放的区
        var serverList = new List<int>();
        foreach (var kv in idToServerInfoDic)
        {
            var key = kv.Key;
            var serverInfo = kv.Value;
            if (serverInfo.online == 2)
            {
                continue;
            }

            var serverId = serverInfo.sid;
            serverList.Add(serverId);
        }

        if (serverList.Count != 0)
        {
            var selectedServerId = serverList[serverList.Count - 1];
            return selectedServerId;
        }

        throw new Exception("[LoginManager] cannot auto select select game server");
    }
}

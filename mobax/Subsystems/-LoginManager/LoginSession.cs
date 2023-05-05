using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class LoginSession 
{
    public UserPlatformInfo userPlatformInfo;
    public Dictionary<int, GameServerInfo> idToServerInfoDic;
    public int selectedServerId;
    public GameServerCookie cookie;
    public GameServerInfo SelectedGameServerInfo
    {
        get
        {
            var ret =this.idToServerInfoDic[this.selectedServerId];
            return ret;
        }
    }

    public UserGameServerInfo userGameServerInfo;

    /// <summary>
    /// 最后创建的角色的 id
    /// </summary>
    public string LatestCreatedRoleId
    {
        get
        {
            var roleId = this.userGameServerInfo.roles[this.userGameServerInfo.roles.Length - 1]._id;
            return roleId;
        }
    }

    public LoginRoleInfo selectedRoleData;



    bool beatRequesting;
    float delta;
    public void Update()
    {
        if(beatRequesting)
        {
            return;
        }
        if(this.cookie == null)
        {
            return;
        }
        var delta = Time.deltaTime;
        this.delta += delta;
        if(this.delta >= 5)
        {
            this.delta = 0;
            this.Beat();
        }
    }

    public int heatBeatCount;
    public int headBeadFialCount;

    async void Beat()
    {
        var enbaleHeartBeat = HotLocalSettings.IsHeartBeat;
        if(!enbaleHeartBeat)
        {
            return;
        }

        try
        {
            beatRequesting = true;
            heatBeatCount++;
            var data = await NetworkManager.Stuff.CallAsync<ServerPushType[]>(ServerType.Game, "notify", isBlock: false, repreatWhenNetError: false);
            await ProcessNotifyAsync(data);

            var giftCount = Database.Stuff.giftDatabase.Count;
            if(giftCount > 0)
            {
                await Database.Stuff.giftDatabase.SubmitAllIggGiftAsync();
            }
        }
        catch(Exception e)
        {
            // 对于标记了登出的异常，需要抛出
            // 其他异常静默处理
            var gameException = e as GameException;
            if(gameException != null)
            {
                if (gameException.flag == ExceptionFlag.Logout)
                {
                    throw;
                }
            }

        }
        finally
        {
            beatRequesting = false;
        }
    }

    async Task ProcessNotifyAsync(ServerPushType[] magicList)
    {
        if(magicList == null)
        {
            return;
        }
        Debug.Log($"[LoginSession] server push {string.Join(", ", magicList)}");
        foreach(var id in magicList)
        {
            if(id == ServerPushType.PlayerGift)
            {
                await Database.Stuff.giftDatabase.ResetByFetchAllAsync();
            }
            else if(id == ServerPushType.PlayerMail)
            {
                await Database.Stuff.mailDatabase.UpdateByFetchNewModifiedSinceLastUpdateAsync();
            }
            else if(id == ServerPushType.OrderChnaged)
            {
                await Database.Stuff.orderDatabase.SyncAdditionalAsync();
                await OrderUtil.TrySubmitAllOrderIfNeedAsync();
            }
            else
            {
                LoginManager.InvokServerPushArrival(id);
            }
        }
          
    }
}


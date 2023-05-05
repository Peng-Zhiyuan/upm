using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class LoginService : Service
{
    public override void OnCreate()
    {
        LoginManager.onSelectServer = this.OnSelectServer;
        //LoginManager.OnInputUsername = this.OnInputUsername;
        LoginManager.OnFetchUserData = this.OnFetchUserData;
        LoginManager.OnSessionReleased = this.OnSessionReleased;
    }

    void OnSessionReleased()
    {
        Database.DestroyStuff();
    }


    async Task OnFetchUserData(Action<float> reportPercentHandler)
    {
        //var taskList = new List<Task>();
        var taskRunner = new TaskRunner();
        taskRunner.Add(Database.Stuff.FetchAllAsync());
        //taskList.Add(EndpointManager.Stuff.SyncAsync());
        taskRunner.Add(ConfigManager.Stuff.SyncAsync());
        taskRunner.Add(SocializeManager.Stuff.SocializeListAsync());
        taskRunner.Add(SocializeManager.Stuff.FansListAsync());
        taskRunner.Add(SocializeManager.Stuff.SocializeAssistListAsync());

        await taskRunner.WhenAll(percent =>
        {
            reportPercentHandler?.Invoke(0.3f * percent);
        });
        reportPercentHandler?.Invoke(0.3f);

        await BPManager.Stuff.FetchBpDataAsync();

        SocializeManager.Stuff.InitZeroHourTime();
        SocializeManager.Stuff.AddSocializeListener();
        ResourceCopyManager.Stuff.CacheResourceCopyDic();

        HeroManager.Instance.TryUploadAllMyHeroPower();

        var isChatEnabled = HotLocalSettings.IsChatEnabled;
        if (isChatEnabled)
        {
            ChatManager.Stuff.TryInitalizeThenConnectAsync();
        }

        reportPercentHandler?.Invoke(0.5f);

        var taskRunner2 = new TaskRunner();
        taskRunner2.Add(GuildManager.Stuff.RecommendGuildListAsync());
        taskRunner2.Add(GuildManager.Stuff.GuildMemberListAsync());
        taskRunner2.Add(GuildManager.Stuff.ApplyGuildIdListAsync());
        taskRunner2.Add(GuildManager.Stuff.FetchGuildRankAsync());
        taskRunner2.Add(GuildManager.Stuff.ApplyPersonUIDListAsync());
        taskRunner2.Add(GuildManager.Stuff.FetchGuildMessageAsync());
        await taskRunner2.WhenAll(percent =>
        {
            reportPercentHandler?.Invoke(0.9f * percent);
        });

        reportPercentHandler?.Invoke(0.9f);

        //SdkNoticeManager.Stuff.FetchInBackground();
    }

    async Task<int> OnSelectServer(UserPlatformInfo info, Dictionary<int, GameServerInfo> sidToGameServerInfoDic)
    {
        var autoSelectGameServer = EnvManager.GetConfigOfFinalEnv("autoSelectGameServer");
        if (autoSelectGameServer == "true")
        {
            int? firstId = null;
            foreach (var kv in sidToGameServerInfoDic)
            {
                firstId = kv.Key;
                break;
            }

            if (firstId == null)
            {
                throw new GameException(ExceptionFlag.None, "no game server");
            }

            return firstId.Value;
        }
        else
        {
            var bucket = BucketManager.Stuff.Main;
            await bucket.AquireIfNeedAsync<GameObject>("SelectServerFloating.prefab");
            var f = UIEngine.Stuff.ShowFloatingImediatly<SelectServerFloating>();

            var lastedSelectedSid = LocalCache.GetInt("selected_sid");
            f.Reset(sidToGameServerInfoDic, lastedSelectedSid);
            var sid = await f.WaitResultAsync();
            UIEngine.Stuff.RemoveFloating<SelectServerFloating>();
            LocalCache.SetInt("selected_sid", sid);
            return sid;
        }
    }
}
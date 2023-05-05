using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;
using UnityEngine;

using Peapod.ADSDK;
using Peapod.ADSDK.Base;
using GPC.ADSDK.Firebase;
using GPC.ADSDK.Appsflyer.Tracker;
using Firebase.Analytics;
using GPC.Helper.Jingwei.Script.Common;



using System.Linq;
using GPC.Core.Error;
using GPC.Core.VO;
using GPC.Foundation;
using GPC.Foundation.Views;
using GPC.Framework;
using GPC.Framework.VO;
using GPC.Helper.Jingwei.Script.Account;
using GPC.Helper.Jingwei.Script.AgreementSigning;
using GPC.Helper.Jingwei.Script.Common;
using GPC.Helper.Jingwei.Script.Common.Unity;
using GPC.Helper.Jingwei.Script.Common.Utils;
using GPC.Helper.Jingwei.Script.Common.View;
using GPC.Helper.Jingwei.Script.Init;
using GPC.Helper.Jingwei.Script.Init.AppconfBoard;
using GPC.Helper.Jingwei.Script.Init.bean;
using GPC.Helper.Jingwei.Script.Purchase;
using GPC.Modules.Account;
using GPC.Modules.Account.UI;
using GPC.Modules.AppConf.VO;
using GPC.Modules.EventCollection.VO;
using GPC.Modules.Payment.Primary.VO;
using GPC.Modules.Payment.VO;

using UnityEngine.UI;


public class IggChannel : Channel
{
    public override string Alias
    {
        get
        {
            return "Igg";
        }
    }


    public override void OnLoginComplete()
    {
        TrackManager.ReportDayIndexEventIfNeed();
        //var roleId = Database.Stuff.roleDatabase.Me._id;
        //var ppsUserId = PpsManager.userId;
        //GPCPlayer player = new GPCPlayer(ppsUserId);
        //// 配置玩家的 char id (角色 ID)。
        //player.SetCharId(roleId);
        //PeapodSDK.SetPlayer(player);
        //// 设置玩家的 UserId。市调游戏 User Id 由 PappusSDK 获取。
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    FirebaseAnalytics.SetUserId(roleId);
        //}
    }

    public override async Task OnLoginStartAsync()
    {
        IggSdkManager.Stuff.Init();

        if(LanguageManager.Language == LanguageManager.Language_Default)
        {
            LanguageManager.Language = LanguageManager.Language_Default;
        }
    }



    public override async Task<UserPlatformInfo> LoginAsync(string accountIfNeed = null)
    {
        var session = LauncherIggSdkManager.Stuff.initResult.session;
        var token = session.GetAccesskey();
        var iggId = session.GetUserId();
        var deviceId = KungfuInstance.Get().UDID;
        var gameId = LauncherIggSdkManager.Stuff.GameId;

        var arg = new JsonData();
        arg["gameid"] = gameId;
        arg["udid"] = deviceId;
        arg["iggid"] = iggId;
        arg["accessToken"] = token;
        var msg = await NetworkManager.Stuff.CallAsync<UserPlatformInfo>(ServerType.Platform, "login", arg);
        //var msg = new UserPlatformInfo();
        //msg.guid = "1444319087";

        return msg;

    }

    public override async Task PurchaseAsync(int itemId)
    {
        await IggSdkManager.Stuff.PurchaseAsync(itemId);
    }

}
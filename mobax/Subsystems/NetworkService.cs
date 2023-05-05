using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using CustomLitJson;

[ServiceDescription("驱动 NetworkManager")]
public class NetworkService : Service 
{
    public override void OnCreate()
    {
        NetworkManager.addExtraParamHandler += OnAddExtraParam;
        NetworkManager.onAnyRoutingSuccess = OnPreAnyRequestSuccessed;
        NetworkManager.anyRequestSuspended = OnAnyRequestSuspended;
        NetworkManager.OnGetUrl = OnGetUrl;
        NetworkManager.changeRequestBlock = OnChangeRequestBlock;
        NetworkManager.isFateErrorHandler = IsFateError;
    }

    void OnChangeRequestBlock(bool b)
    {
        if(b)
        {
            BlockManager.Stuff.AddBlock("Network");
        }
        else
        {
            BlockManager.Stuff.RemoveBlock("Network");
        }
    }

    string GameUrl
    {
        get
        {
            var serverInfo = LoginManager.Stuff.session.SelectedGameServerInfo;
            var url = serverInfo.address;
            return url;
        }
    }

    static string FixUrl(string url)
    {
        if (url.StartsWith("http://") ||url.StartsWith("https://"))
        {
            return url;
        }
        else
        {
            return "http://" + url;
        }
    }

    public string OnGetUrl(ServerType serverType, string api)
    {
        if (api.StartsWith("/"))
        {
            Debug.LogError("api 不要 / 开头:" + api);
        }

        if (serverType == ServerType.Platform)
        {
            var platformUrl = EnvManager.GetConfigOfFinalEnv("platform");
            var branch = EnvManager.Branch;
            var url = $"{platformUrl}/{branch}/{api}";
            return url;
        }
        else if(serverType == ServerType.Game)
        {
         
            var gameUrl = this.GameUrl;
            var url = $"{gameUrl}/{api}";
            url = FixUrl(url);
            return url;
        }
        else if(serverType == ServerType.Extra)
        {
            return api;
        }
        throw new Exception("[NetworkManager] serverType: " + serverType + " not implememnt yet");
    }

    static bool isAsking;
    public async void OnAnyRequestSuspended()
    {
        if(isAsking)
        {
            return;
        }
        isAsking = true;
        try
        {
            var msg = "net_error_retry".Localize();
            await Dialog.ConfirmAsync("", msg);

            NetworkManager.Stuff.RepostAllSuspendedRequest();
        }
        finally
        {
            isAsking = false;
        }
    }

    public void OnAddExtraParam (NetworkRoutine routing)
    {
        var session = LoginManager.Stuff.session;
        if (session != null)
        {
            //var userGameServerInfo = session.userGameServerInfo;
            var cookie = session.cookie;
            if (cookie != null)
            {
                var key = cookie.key;
                var value = cookie.val;
                routing.urlParam[key] = value;
                //Debug.Log ("OnAddExtraParam: " + key + "=>" + value);
            }
            var api = routing.url;
            if (api.EndsWith("login"))
            {
                var sid = LoginManager.Stuff.session.selectedServerId;
                routing.urlParam["sid"] = sid.ToString();
            }


        }
    }

 

    public bool IsFateError(string errorCode)
    {
        if(errorCode == GameErrorCode.Server_Disconnected)
        {
            return true;
        }
        else if(errorCode == GameErrorCode.Server_SignInElsewhere)
        {
            return true;
        }
        else if(errorCode == GameErrorCode.Server_NeedRelogin)
        {
            return true;
        }
        return false;
    }



    public void OnPreAnyRequestSuccessed (NetworkResult result, NetworkRoutine routing) 
    {
        if(result.msgWithDataTypeIsJsonData == null)
        {
            return;
        }

        // 更新 coockie
        var cookie = result.msgWithDataTypeIsJsonData.cookie;
        if(cookie != null)
        {
            LoginManager.Stuff.session.cookie = cookie;
        }

        // 更新时间
        var timestampSec = result.msgWithDataTypeIsJsonData.time;
        var timestampMs = timestampSec * 1000;
        if(timestampSec != 0)
        {
            Clock.UpdateTimestamp(timestampMs);
        }

        var displayType = routing.isAutoShowReward;

        // 更新数据库
        var netmsg = result.msgWithDataTypeIsJsonData;
        if (netmsg.cache != null)
        {
            var beforeSnapshot = CreateSnapshot();

            var transactionList = GetDatabaseTransactionListFromNetMsg(netmsg);
            var record = displayType == DisplayType.Cache || displayType == DisplayType.Show;
            foreach (var t in transactionList)
            {
                Database.Stuff.ApplyTransaction(t, timestampMs, record);
            }

            var postSnapshot = CreateSnapshot();
            ProcessChanging(beforeSnapshot, postSnapshot, routing.goods);
        }

        // 自动显示奖励
        if (displayType == DisplayType.Show)
        {
            UiUtil.CleanAndShowAllCachedReward();
        }
    }

    void ProcessChanging(Snapshot before, Snapshot post, string goods)
    {
        // 等级购买
        if (before.level != null && post.level != null && before.level != post.level)
        {
            SystemOpenManager.Stuff.RefreshLevelUpChanged(before.level.Value, post.level.Value);
        }

        // 打点
        var pointDelta = post.point - before.point;
        var diamondDelta = post.diamond - before.diamond;
        var coinDelta = post.coin - before.coin;

        if (pointDelta < 0)
        {
            var name = StaticDataUtil.GetAnyFieldOfAnyRow<string>(ItemId.PaidDiamond, "Name").Localize();
            TrackManager.SendVirtualCurrency(name, ItemId.PaidDiamond.ToString(), "", pointDelta, goods);
        }
        else if(pointDelta > 0)
        {
            //var name = StaticDataUtil.GetAnyFieldOfAnyRow<string>(ItemId.PaidDiamond, "Name").Localize();
            //TrackManager.SendEarnVirtualCurrency(name, pointDelta);
        }

        //if (diamondDelta < 0)
        //{
        //    var name = StaticDataUtil.GetAnyFieldOfAnyRow<string>(ItemId.FreeDiamond, "Name").Localize();
        //    TrackManager.SendVirtualCurrency(name, ItemId.FreeDiamond.ToString(), "", diamondDelta, goods);
        //}

        //if (coinDelta < 0)
        //{
        //    var name = StaticDataUtil.GetAnyFieldOfAnyRow<string>(ItemId.Gold, "Name").Localize();
        //    TrackManager.SendVirtualCurrency(name, ItemId.Gold.ToString(), "", coinDelta, goods);
        //}
    }

    static Snapshot CreateSnapshot()
    {
        var ret = new Snapshot();
        ret.level = Database.Stuff.roleDatabase.Me?.lv;
        ret.point = Database.Stuff.itemDatabase.GetHoldCount(ItemId.PaidDiamond);
        ret.diamond = Database.Stuff.itemDatabase.GetHoldCount(ItemId.FreeDiamond);
        ret.coin = Database.Stuff.itemDatabase.GetHoldCount(ItemId.Gold);
        return ret;
    }

    struct Snapshot
    {
        public int? level;
        public int point;
        public int diamond;
        public int coin;
    }

    static List<DatabaseTransaction> GetDatabaseTransactionListFromNetMsg(NetMsg<JsonData> netMsg)
    {
        var transactionListJson = netMsg.cache;
        var transactionList = new List<DatabaseTransaction>();
        foreach (var jd in transactionListJson)
        {
            var transaction = JsonUtil.JsonDataToObject<DatabaseTransaction>(jd);
            transactionList.Add(transaction);
        }
        return transactionList;
    }

    //public void OnChangeRequestBlock (bool show) {
    //    if (show) {
    //        //FlowerManager.Show ("NETWORK_REQUEST");
    //        //UIEngine.Instance.ShowFloating("FlowerFloating");
    //    } else {
    //        //FlowerManager.Hide ("NETWORK_REQUEST");
    //        //UIEngine.Instance.HideFloating("FlowerFloating");
    //    }

    //}
}
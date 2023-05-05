using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;
using UnityEngine;


public class DevChannel : Channel 
{
    public override string Alias
    {
        get
        {
            return "Dev";
        }
    }


    static string OverrideGustId
    {
        get
        {
            var ret = PlayerPrefs.GetString("overrideGustId", "");
            return ret;
        }
        set
        {
            PlayerPrefs.SetString("overrideGustId", value);
        }
    }



    public override async Task<UserPlatformInfo> LoginAsync(string branch = null) 
    {
        var page = await UIEngine.Stuff.ShowFloatingAsync<DevLoginPage>();
        var guid = await page.WaitResultAsync();


        var arg = new JsonData();
        //arg["username"] = guestGuid;
        arg["guid"] = guid;
        var msg = await NetworkManager.Stuff.CallAsync<UserPlatformInfo> (ServerType.Platform, "login", arg);

        return msg;
    }

    public override async Task PurchaseAsync(int itemId)
    {
        throw new GameException(ExceptionFlag.None, "开发者登录不支持购买", "NOT_SUPPORTED");
        try
        {
            BlockManager.Stuff.AddBlock("purchase");
            var itemIdString = itemId.ToString();
            var orderId = await DebugApi.CreateOrderAsync(itemId);
            await OrderUtil.WaitForSubmitAsync(orderId, DisplayType.Show);
        }
        finally
        {
            BlockManager.Stuff.RemoveBlock("purchase");
        }
    }

    //public async Task<string> ProcessPaymentAsync (PaymentRow row, string payBackPage = "", bool useHasBonusDes = false) {

    //    var arg = new Dictionary<string, string> ();
    //    arg["id"] = row.id.ToString ();
    //    arg["sid"] = 1. ToString ();
    //    arg["uid"] = RoleDatabase.Instance.Me._id;
    //    var msg = await NetworkManager.Instance.RequestAssertSuccessGetMsgAsync<CustomLitJson.JsonData> (ServerType.Platform, "/order/create", arg, true, DeveloperLocalSettings.SelectedBranchOfCurrentEnv);
    //    string orderId = msg.ret["orderid"].ToString ();

    //    var arg2 = new Dictionary<string, string> ();
    //    arg2["orderid"] = msg.ret["orderid"].ToString ();
    //    arg2["attach"] = msg.ret["attach"].ToString ();
    //    await NetworkManager.Instance.RequestAssertSuccessGetMsgAsync<CustomLitJson.JsonData> (ServerType.Platform, "/order/notify", arg2, true, DeveloperLocalSettings.SelectedBranchOfCurrentEnv);
    //    //await RequestPaymentResultThenShowUIAsync (id, orderId);        
    //    return orderId;
    //}


    // async ProcessPaymentAsync(row: PaymentRow, payBackPage: string, useHasBonusDes: boolean)
    // {
    //     NetworkRegister.RegisterCommand("CreateOrder", ServerType.Platform, "/order/create")
    //     NetworkRegister.RegisterCommand("Notify", ServerType.Platform, "/order/notify")
    //     let orderId = await this.GuestCreateOrderAndNotify(row.id)
    //     await GameUtil.RequestPaymentResultThenShowUIAsync(orderId)
    // }

    // private async GuestCreateOrderAndNotify(id: number)
    // {
    //     let itemRow = StaticDataManager.Root.payment[id]
    //     let arg = {}
    //     // common param
    //     arg["id"] = id
    //     arg["sid"] = LogiedServerInfo.serverInfo._id
    //     arg["uid"] = RoleDatabase.Instance.Me._id

    //     let branch = GameUtil.Branch
    //     let ret = await NetworkManager.RequestMustSuccessAsync("CreateOrder", arg, true, true, branch)
    //     console.log("guest cretor order ret: ", ret);
    //     let orderId = ret["orderid"]
    //     let attach = ret["attach"]

    //     let arg2 = {}
    //     arg2["orderid"] = orderId
    //     arg2["attach"] = attach

    //     await NetworkManager.RequestMustSuccessGetMsgAsync("Notify", arg2, true, true, "guest")
    //     return orderId
    // }

}
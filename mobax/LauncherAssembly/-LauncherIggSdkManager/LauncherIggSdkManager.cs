using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GPC.Framework;
using GPC.Core.Configuration;
using GPC.Core;
using GPC.Core.VO;
using System;
using GPC.Framework.Listener;
using GPC.Modules.AppConf.VO;
using GPC.Framework.VO;


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
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using CustomLitJson;
using System;

public class LauncherIggSdkManager : LauncherStuffObject<LauncherIggSdkManager>, GPCGameDelegate
{
    [ShowInInspector]
    public LauncherIggSdkUtil.InitResult initResult;

    public async Task InitAsync()
    {
        //开启 SDK 内部日志
        GPCSDKRunningTimeConfig.ENABLE_LOG = true;

        //由于支付模块日志较多，因此特别使用一个开关控制日志输出，支付问题或者调试支付功能时可一同开启支付模块日志。
        GPCSDKRunningTimeConfig.PAYMENT_ITEMS_LOG = true;

        var gameId = ResolveAllPlatformGameId();
        here:
        this.initResult = await LauncherIggSdkUtil.InitAsync(this, gameId.androidGameId, gameId.iosGameId);
        if(this.initResult.status == LauncherIggSdkUtil.InitResultStatus.Retry)
        {
            goto here;
        }
        if (initResult.status != LauncherIggSdkUtil.InitResultStatus.LoginSuccess)
        {
            throw new Exception("[IggSdkManager] sdk login fail:" + initResult.error);
        }
        var isDev = DeveloperLocalSettings.IsDevelopmentMode;

        if(!isDev)
        {
            await LauncherIggSdkUtil.ShowOperationPannelIfNeedAsync(initResult.appconf);
        }

        InitHelper.SharedInstance().onProductListener = OnProductReceived;
        await ProcessPaymentRegisterAsync();
        await ShowAgreementIfNeedAsync();
        this.BindTshUnreadCount();
        Peapod.ADSDK.PeapodSDK.TrackUninstall();
    }


    public Dictionary<string, string> androidLanguageToGameIdDic = new Dictionary<string, string>()
    {
        ["en"] = "11410102021",
        ["cn"] = "11411902021",
        ["tw"] = "11410202021",
        ["jp"] = "11410802021",
    };
    public Dictionary<string, string> iosLanguageToGameIdDic = new Dictionary<string, string>()
    {
        ["en"] = "11410103031",
        ["cn"] = "11411903031",
        ["tw"] = "11410203031",
        ["jp"] = "11410803031",
    };

    public string GameId
    {
        get
        {
            var gameId = ResolveAllPlatformGameId();
            if (Application.platform == RuntimePlatform.Android || Application.isEditor)
            {
                return gameId.androidGameId;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return gameId.iosGameId;
            }
            throw new Exception("[IggSdkManager] unsupport platform");
        }
    }

    (string androidGameId, string iosGameId) ToAllPlatformGameId(string languageCode)
    {
        var androidGameId = this.androidLanguageToGameIdDic[languageCode];
        var iosGameId = this.iosLanguageToGameIdDic[languageCode];
        return (androidGameId, iosGameId);
    }

    (string androidGameId, string iosGameId) ResolveAllPlatformGameId()
    {
        var language = LauncherLocalizatioManager.Stuff.Language;
        var gameId = ToAllPlatformGameId(language);
        return gameId;
    }

    public static int tshUnreadCount;
    public static Action tshUnraedCountChanged;

    public void BindTshUnreadCount()
    {
        KungfuInstance.Get().GetPreparedOperations().TicketService().SetDialogMode(false);
        KungfuInstance.Get().GetPreparedOperations().TicketService().SetUnreadMessageCountDelegate(count =>
        {
            tshUnreadCount = count;
            tshUnraedCountChanged?.Invoke();
        });
    }


    Task<bool> ShowAgreementIfNeedAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        InitHelper.CheckAgreementOnInitFlowComplete(() =>
        {
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    public static Action<List<GPCGameItem>> onProductReceivedDelegate;

    void OnProductReceived(List<GPCGameItem> infoList)
    {
        onProductReceivedDelegate?.Invoke(infoList);
    }

    Task ProcessPaymentRegisterAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        var startupListener = new InitHelper.OnStartupListener(() =>
        {
            Debug.Log("[IggSdkManager] ProcessPaymentRegisterAsync startupListener");
            tcs.TrySetResult(true);
        });
        var purchaseListener = new InitHelper.OnPurchaseListener(OnPaymentSuccess, OnPaymentFailed,
            OnPaymentCancel, OnGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItem);
        var subscriptionListener = new InitHelper.OnSubscriptionPurchaseListener(OnItemAlreadySubscribed,
            OnSubscriptionShouldMakeRecurringPaymentsInstead);

        this.initResult.startupStepInvoke.Invoke(startupListener, purchaseListener, subscriptionListener);
        return tcs.Task;
    }

    public static Action<GPCPurchaseResult, GPCIAPPurchase> onPaymentSuccessDelegate;

    /// <summary>
    /// 支付成功通知
    /// </summary>
    /// <param name="result"></param>
    public void OnPaymentSuccess(GPCPurchaseResult sdkResult, GPCIAPPurchase purchaseInfo)
    {
        onPaymentSuccessDelegate?.Invoke(sdkResult, purchaseInfo);
    }

    public static Action<string> onPaymentFailedDelegate;

    /// <summary>
    /// 支付失败通知，研发处理提示语的时候请带上错误码显示。
    /// </summary>
    /// <param name="errorCode"></param>
    public void OnPaymentFailed(string errorCode)
    {
        onPaymentFailedDelegate.Invoke(errorCode);
    }

    public static Action onPaymentCancelDelegate;
    /// <summary>
    /// 用户取消了购买
    /// </summary>
    public void OnPaymentCancel()
    {
        onPaymentCancelDelegate?.Invoke();
    }


    public static Action<GPCGameItem> onGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItemDelegate;

    /// <summary>
    /// 快速购买同一商品的错误回调。
    /// </summary>
    /// <param name="item"></param>
    public void OnGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItem(GPCGameItem item)
    {
        onGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItemDelegate.Invoke(item);
    }

    public static Action onItemAlreadySubscribedDelegate;
    /// <summary>
    /// 商品已订阅，不能重复订阅
    /// </summary>
    public void OnItemAlreadySubscribed()
    {
        onItemAlreadySubscribedDelegate.Invoke();
    }

    public static Action<GPCGameItem> onSubscriptionShouldMakeRecurringPaymentsInsteadDelegate;

    /// <summary>
    /// 订阅类商品转消耗
    /// </summary>
    public void OnSubscriptionShouldMakeRecurringPaymentsInstead(GPCGameItem gameItem)
    {
        onSubscriptionShouldMakeRecurringPaymentsInsteadDelegate.Invoke(gameItem);
    }

    public static Func<GPCGameCharacter> getRoleInfoDelegate;
    public static Func<GPCGameServer> getServerInfoDelegate;

    GPCGameCharacter GPCGameDelegate.GetGameCharacter()
    {
        var info = getRoleInfoDelegate.Invoke();
        return info;
    }

    GPCGameServer GPCGameDelegate.GetGameServer()
    {
        var info = getServerInfoDelegate.Invoke();
        return info;
    }

    public void ShowTshUi()
    {
        GPC.Helper.Jingwei.Script.TSH.TSHHelper.SharedInstance().ShowPanel();
    }
}

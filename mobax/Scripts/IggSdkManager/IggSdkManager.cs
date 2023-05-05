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
using GPC.Modules.AgreementSigning.VO;



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
using GPC.Modules.Survey;

// cdkey 参数
// key:c163b4b1d9c61e5301f68ha9c9d3da5
// mp_id：77

public class IggSdkManager : StuffObject<IggSdkManager>
{
    public List<GPCGameItem> itemList;

    public void Init()
    {
        LauncherIggSdkUtil.onBannedDelegate = this.OnBanned;
        LauncherIggSdkUtil.onLoggedInDelegate = this.OnLoggedIn;

        LauncherIggSdkManager.getRoleInfoDelegate = this.GetRoleInfo;
        LauncherIggSdkManager.getServerInfoDelegate = this.GetServerInfo;

        LauncherIggSdkManager.onPaymentSuccessDelegate = this.OnPaymentSuccess;
        LauncherIggSdkManager.onPaymentFailedDelegate = this.OnPaymentFailed;
        LauncherIggSdkManager.onPaymentCancelDelegate = this.OnPaymentCancel;
        LauncherIggSdkManager.onGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItemDelegate = this.OnGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItem;
        LauncherIggSdkManager.onItemAlreadySubscribedDelegate = this.OnItemAlreadySubscribed;
        LauncherIggSdkManager.onSubscriptionShouldMakeRecurringPaymentsInsteadDelegate = this.OnSubscriptionShouldMakeRecurringPaymentsInstead;

        this.BindTshUnreadCount();
        this.BindProductList();

        //AgreementSigningUIHelper.SharedInstance().GetTermsOfSubscription((title, url) =>
        //{
        //    subTermTitle = title;
        //    subTermUrl = url;
        //});
        this.GetTermsOfSubscription(set =>
        {
            agrementSet = set;
        });


    }

    public static GPCAssignedAgreements agrementSet;

    public static string subTermTitle;
    public static string subTermUrl = "https://policies.legendgamesonline.com/view?id=562&game_id=11410102021"; // 需要有默认值

    // 禁言
    public bool IsShutup
    {
        get
        {
            var ts = LoginManager.Stuff.session.selectedRoleData.shutup;
            if(ts == -1)
            {
                return true;
            }
            if(ts == 0)
            {
                return false;
            }
            var now = Clock.TimestampSec;
            if(now < ts)
            {
                return true;
            }
            return false;

        }
    }

    public void CleanSdkSession()
    {
        GPCSession.currentSession?.InvalidateCurrentSession();
    }

    /// <summary>
    /// 账号被封号的统一触发回调
    /// </summary>
    /// <param name="expiredSession">被封的账号的会话信息</param>
    async void OnBanned(GPCSession expiredSession)
    {
        Debug.Log("[IggSdkManager] OnBanned:" + expiredSession);
        var msg = "m1_sdk_account_forbiden".Localize(); // 账号已被封禁
        await Dialog.ConfirmAsync("", msg);
        Application.Quit();
    }

    /// <summary>
    /// 切换账号/会话过期后重新登录后触发回调
    /// 注意：首次进入游戏以游客身份的登录将会回调 OnLoginSuccess，不会触发此回调
    /// </summary>
    /// <param name="session">账号的会话信息</param>
    async void OnLoggedIn(GPCSession session)
    {
        Debug.Log("[IggSdkManager] OnLoggedIn:" + session);
        if(LoginManager.Stuff.session == null)
        {
            // 正常登录
        }
        else
        {
            // 游戏内切换账号
            var msg = "m1_sdk_switch_account".Localize(); // 已切换账号，请重启游戏
            await Dialog.ConfirmAsync("", msg);
            Application.Quit();
        }
    }

    public string IggId
    {
        get
        {
            var id = LauncherIggSdkManager.Stuff.initResult.session.GetUserId();
            return id;
        }
    }

    public bool IsIggChannel
    {
        get
        {
            if(ChannelManager.Channel is IggChannel)
            {
                return true;
            }
            return false;
        }
    }


    public void ShowGuide()
    {
        if(!this.IsIggChannel)
        {
            Dialog.Confirm("", "仅在 igg 登录时支持");
            return;
        }
        var url = LauncherIggSdkManager.Stuff.initResult.appconfBean.url.guideUrl;
        IggSdkManager.Stuff.OpenNativeBrowser(url);
    }

    public void BindProductList()
    {
        this.itemList = DataCenter.SharedInstance().products;
        LauncherIggSdkManager.onProductReceivedDelegate = (itemList) =>
        {
            this.itemList = itemList;
        };
    }


    public void BindTshUnreadCount()
    {
        this.SyncUnreadInfo();
        LauncherIggSdkManager.tshUnraedCountChanged = () =>
        {
            this.SyncUnreadInfo();
        };
    }

    void SyncUnreadInfo()
    {
        TshUnreadCount.Value = LauncherIggSdkManager.tshUnreadCount;
    }


    public ObservableValue<int> TshUnreadCount = new ObservableValue<int>();

    /// <summary>
    /// 显示服务条款
    /// </summary>
    public void ShowTerm()
    {
        if (!this.IsIggChannel)
        {
            Dialog.Confirm("", "仅在 igg 登录时可用");
            return;
        }
        AgreementSigningUIHelper.SharedInstance().ShowAssignedAgreements(new AgreementSigningUIHelper.InformResultListener(() =>
        {

        }));
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


    /// <summary>
    /// 支付成功通知
    /// </summary>
    /// <param name="result"></param>
    public void OnPaymentSuccess(GPCPurchaseResult sdkResult, GPCIAPPurchase purchaseInfo)
    {
        var result = new PurchaseResult();
        result.status = PaymentResultStatus.Sucess;
        result.sdkResult = sdkResult;
        result.purchaseInfo = purchaseInfo;
        paymentTcs?.TrySetResult(result);
        paymentTcs = null;
    }

    /// <summary>
    /// 支付失败通知，研发处理提示语的时候请带上错误码显示。
    /// </summary>
    /// <param name="errorCode"></param>
    public void OnPaymentFailed(string errorCode)
    {
        var result = new PurchaseResult();
        result.status = PaymentResultStatus.Fail;
        result.errorCode = errorCode;
        paymentTcs?.TrySetResult(result);
        paymentTcs = null;
    }

    /// <summary>
    /// 用户取消了购买
    /// </summary>
    public void OnPaymentCancel()
    {
        var result = new PurchaseResult();
        result.status = PaymentResultStatus.UserCancel;
        paymentTcs?.TrySetResult(result);
        paymentTcs = null;
    }

    /// <summary>
    /// 快速购买同一商品的错误回调。
    /// </summary>
    /// <param name="item"></param>
    public void OnGPCPurchaseFailedDueToThePendingPurchaseOfTheSameItem(GPCGameItem item)
    {
        var result = new PurchaseResult();
        result.status = PaymentResultStatus.PendingPurchaseOfTheSameItem;
        paymentTcs?.TrySetResult(result);
        paymentTcs = null;
    }

    /// <summary>
    /// 商品已订阅，不能重复订阅
    /// </summary>
    public void OnItemAlreadySubscribed()
    {
        var result = new PurchaseResult();
        result.status = PaymentResultStatus.FailDueToAlreadySubscribed;
        paymentTcs?.TrySetResult(result);
        paymentTcs = null;
    }

    /// <summary>
    /// 订阅类商品转消耗
    /// </summary>
    public void OnSubscriptionShouldMakeRecurringPaymentsInstead(GPCGameItem gameItem)
    {
        var result = new PurchaseResult();
        result.status = PaymentResultStatus.FailAndShouldUseCostItemInstead;
        paymentTcs?.TrySetResult(result);
        paymentTcs = null;
    }

    GPCGameCharacter GetRoleInfo()
    {
        Debug.Log("[IggSdkManager] provide user info");
        if(LoginManager.Stuff.session == null)
        {
            throw new Exception("[IggSdkManager] session not created yet.");
        }
        var roleInfo = Database.Stuff.roleDatabase.Me;
        var uid = roleInfo._id;
        var username = roleInfo.name;
        var level = roleInfo.lv;
        var ch = new GPCGameCharacter(uid);
        ch.SetCharName(username);
        ch.SetLevel(level.ToString());
        return ch;
    }

    GPCGameServer GetServerInfo()
    {
        Debug.Log("[IggSdkManager] provide server info");
        if (LoginManager.Stuff.session == null)
        {
            throw new Exception("[IggSdkManager] session not created yet.");
        }
        var sid = LoginManager.Stuff.session.selectedServerId;
        var ret = new GPCGameServer(sid);
        return ret;
    }

    public enum PaymentResultStatus
    {
        Sucess,
        Fail,
        UserCancel,

        /// <summary>
        /// 同一商品存在未处理完的支持
        /// </summary>
        PendingPurchaseOfTheSameItem,
        LimitForDeviceAndUser,
        LimitForDevice,
        LimitForUser,
        LimitForRunOutOfQuota,
        LimitForOther,
        PurchaseNotReady,

        /// <summary>
        /// 重复订阅
        /// </summary>
        FailDueToAlreadySubscribed,
        FailAndShouldUseCostItemInstead,
    }

    public class PurchaseResult
    {
        public PaymentResultStatus status;

        /// <summary>
        /// 当状态为 LimitForRunOutOfQuota 时有值
        /// </summary>
        public int otherLimitCode;

        /// <summary>
        /// 当状态为 success 时有值
        /// </summary>
        public GPCPurchaseResult sdkResult;

        /// <summary>
        /// 当状态为 fail 时有值
        /// </summary>
        public string errorCode;

        /// <summary>
        /// 订单信息
        /// </summary>
        public GPCIAPPurchase purchaseInfo;
    }

    TaskCompletionSource<PurchaseResult> paymentTcs;

    Task<PurchaseResult> PayAsync(GPCGameItem item, string roleId, int serverId)
    {
        if(Application.isEditor)
        {
            var msg = "编辑器中不可用";
            throw new GameException(ExceptionFlag.None, msg);
        }

        if(paymentTcs != null)
        {
            var msg = "payment_pre_payment_not_complete".Localize();
            throw new GameException(ExceptionFlag.None, msg);
        }
        this.paymentTcs = new TaskCompletionSource<PurchaseResult>();
        InAppPurchaseHelper.OnPayOrSubscribeLimitationListener.OnLimitedForDeviceAndUser onLimitForDeviceAndUser = () =>
        {
            var result = new PurchaseResult();
            result.status = PaymentResultStatus.LimitForDeviceAndUser;
            paymentTcs?.SetResult(result);
            paymentTcs = null;
        };

        InAppPurchaseHelper.OnPayOrSubscribeLimitationListener.OnLimitedForDevice onLimitForDevice = () =>
        {
            var result = new PurchaseResult();
            result.status = PaymentResultStatus.LimitForDevice;
            paymentTcs?.SetResult(result);
            paymentTcs = null;
        };

        InAppPurchaseHelper.OnPayOrSubscribeLimitationListener.OnLimitedForUser onLimitForUser = () =>
        {
            var result = new PurchaseResult();
            result.status = PaymentResultStatus.LimitForUser;
            paymentTcs?.SetResult(result);
            paymentTcs = null;
        };

        InAppPurchaseHelper.OnPayOrSubscribeLimitationListener.OnLimitedForRunOutOfQuota onLimitForRunOutOfQuota = () =>
        {
            var result = new PurchaseResult();
            result.status = PaymentResultStatus.LimitForRunOutOfQuota;
            paymentTcs?.SetResult(result);
            paymentTcs = null;
        };

        InAppPurchaseHelper.OnPayOrSubscribeLimitationListener.OnLimitedForOther onLimitedForOther = (int limit) =>
        {
            var result = new PurchaseResult();
            result.status = PaymentResultStatus.LimitForOther;
            result.otherLimitCode = limit;
            paymentTcs?.SetResult(result);
            paymentTcs = null;
        };

        InAppPurchaseHelper.OnPayOrSubscribeLimitationListener.OnPurchaseNotReady onPurchaseNotReady = () =>
        {
            var result = new PurchaseResult();
            result.status = PaymentResultStatus.PurchaseNotReady;
            paymentTcs?.SetResult(result);
            paymentTcs = null;
        };

        InAppPurchaseHelper.SharedInstance().PayOrSubscribeTo(item, roleId, serverId, new InAppPurchaseHelper.OnPayOrSubscribeLimitationListener(onLimitForDeviceAndUser, onLimitForDevice, onLimitForUser, onLimitForRunOutOfQuota, onLimitedForOther, onPurchaseNotReady));
        return paymentTcs.Task;
    }

    public GPCGameItem GetSdkItem(string itemId)
    {
        var item = IggSdkManager.Stuff.itemList?.Find(item =>
        {
            var id = item.ID;
            if (id == itemId)
            {
                return true;
            }
            return false;
        });
        Debug.Log("[GetSdkItem] itemId: " + itemId + " found");
        return item;
    }

    public bool IsSdkItemHasPromitionPrice(string itemId)
    {
        var originPriceAvaliable = this.IsSdkItemPriceInfoAvaliable(itemId, true);
        if (!originPriceAvaliable)
        {
            return false;
        }

        var sdkItem = this.GetSdkItem(itemId);
        var b = sdkItem?.promotionPricing?.IsInStorePromotionPriceAvailable();
        var ret = b ?? false;
        Debug.Log("[PurchaseUtil] id: " + itemId + ", promotion info: " + ret);
        return ret;
    }

    public bool IsSdkItemPriceInfoAvaliable(string itemId, bool mustUseStoragePrice = true)
    {
        //mustUseStoragePrice = true;
        var item = GetSdkItem(itemId);
        if (item == null)
        {
            return false;
        }
        var storagePriceAvailable = item.GetPricing().IsInStorePriceAvailable();
        var standardProceAvaliable = item.GetPricing().IsStandardPriceAvailable(null);

        if(mustUseStoragePrice)
        {
            return storagePriceAvailable;
        }
        else
        {
            return storagePriceAvailable || standardProceAvaliable;
        }
    }


    string PaymentPlatformName
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return "Google Play";
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return "Apple Store";
            }
            else
            {
                return "m1_payment_platform".Localize();
            }
        }
    }

    public async Task PurchaseAsync(int itemId)
    {
        try
        {

            //// pzy: 测试代码
            //{
            //    var code = GameErrorCode.FailAndShouldUseCostItemInstead;
            //    var e = new GameException(ExceptionFlag.None, "payment returns wrong status: " + code, code);
            //    throw e;
            //}


            BlockManager.Stuff.AddBlock("purchase");
            var itemIdString = itemId.ToString();
            var isAvaliable = this.IsSdkItemPriceInfoAvaliable(itemIdString, true);
            if (!isAvaliable)
            {
                var platformName = this.PaymentPlatformName;
                var msg = "m1_payment_not_avaliable".Localize(platformName);
                Dialog.Confirm("", msg);
                throw new GameException(ExceptionFlag.Silent, "sdk price info not avaliable");
            }

            var sdkItem = this.GetSdkItem(itemIdString);
            var roleId = Database.Stuff.roleDatabase.Me._id;
            var serverId = LoginManager.Stuff.session.SelectedGameServerInfo.sid;
            var result = await this.PayAsync(sdkItem, roleId, serverId);
    
            if (result.status == PaymentResultStatus.Sucess)
            {
                var orderId = result.purchaseInfo.orderId;
                await OrderUtil.WaitForSubmitAsync(orderId, DisplayType.Show);
            }
            else if (result.status == PaymentResultStatus.UserCancel)
            {
                // 用户取消
                throw new GameException(ExceptionFlag.Silent, "user cancel");
            }
            else if (result.status == PaymentResultStatus.Fail)
            {
                var sdkError = result.errorCode;
                var code = "SDK_" + sdkError;
                var e = new GameException(ExceptionFlag.None, "sdk returns error code: " + sdkError, code);
                throw e;
            }
            else if (result.status == PaymentResultStatus.FailAndShouldUseCostItemInstead)
            {
                var errorStatus = result.status;
                var code = GameErrorCode.FailAndShouldUseCostItemInstead;
                var e = new GameException(ExceptionFlag.None, "payment returns wrong status: " + errorStatus, code);
                throw e;
            }
            else
            {
                var errorStatus = result.status;
                var code = "PAYMENT_" + errorStatus;
                var e = new GameException(ExceptionFlag.None, "payment returns wrong status: " + errorStatus, code);
                throw e;
            }
        }
        finally
        {
            BlockManager.Stuff.RemoveBlock("purchase");
        }


    }

    public void ShowAccountUi()
    {
        if (!this.IsIggChannel)
        {
            Dialog.Confirm("", "仅在 igg 登录时支持该功能");
            return;
        }
        GPC.Helper.Jingwei.Script.Account.AccountUIHelper.SharedInstance().ShowProfileManagementPanel();
    }

    public void ShowTshUi()
    {
        if(Application.isEditor)
        {
            Dialog.Confirm("", "编辑器不支持该功能");
            return;
        }
        GPC.Helper.Jingwei.Script.TSH.TSHHelper.SharedInstance().ShowPanel();
    }

    public void OpenNativeBrowser(string url)
    {
        GPC.Helper.Jingwei.Script.Common.Utils.GPCNativeUtils.ShareInstance().OpenBrowser(url);
    }

    public void OpenHelp()
    {
        if(!this.IsIggChannel)
        {
            Dialog.Confirm("", "仅在 igg 登录时支持该功能");
            return;
        }
        var url = LauncherIggSdkManager.Stuff.initResult.appconfBean.url.guideUrl;
        if(string.IsNullOrEmpty(url))
        {
            Dialog.Confirm("", LocalizationManager.Stuff.GetText("m1_feature_not_yet_available"));
            return;
        }
        IggSdkManager.Stuff.OpenNativeBrowser(url);
    }

    public void GameRating()
    {
        if (!this.IsIggChannel)
        {
            Dialog.Confirm("", "仅在 igg 登录时支持该功能");
            return;
        }
        GPC.Helper.Jingwei.Script.AppRating.AppRatingUIHelper.SharedInstance().RequestReview(null);
    }

    public Task<GPCSurveyResult> ShowSurveyAsync(string surveyKey)
    {
        var tcs = new TaskCompletionSource<GPCSurveyResult>();

        var survey = KungfuInstance.Get().GetPreparedSurvey();
        survey.Open(surveyKey, true, result =>
        {
            //if (result == GPCSurveyResult.Complete)
            //{
            //    // 完成问卷
            //    Debug.Log("--- Complete");
            //}
            //else if (result == GPCSurveyResult.NextTime)
            //{
            //    // 打开“问卷”前的确认弹窗， 用户选择 “下次再说”。
            //    // 再次触发"问卷"的时机，由游戏自行控制。
            //    Debug.Log("--- NextTime");
            //}
            //else if (result == GPCSurveyResult.Quit)
            //{
            //    // 未完成，用户在答题中途退出
            //    // 再次触发"问卷"的时机，由游戏自行控制。
            //    Debug.Log("--- Quit");
            //}
            //else if (result == GPCSurveyResult.Error)
            //{
            //    // 获取 SSOToken 失败, 无法打开问卷
            //    // 再次触发"问卷"的时机，由游戏自行控制。
            //    Debug.LogError("--- 打开问卷失败，获取 SSO token 失败");
            //}
            tcs.SetResult(result);
        });
        return tcs.Task;
    }

    public virtual void GetTermsOfSubscription(Action<GPCAssignedAgreements> callBack)
    {
        KungfuInstance.Get().GetPreparedAgreementSigningUI().RequestAssignedAgreements(
            delegate (GPCError error, GPCAssignedAgreements agreements)
            {
                 // 一定要处理非空判断 agreements != null && agreements.Agreements != null && agreements.Agreements.Count > 0，在测试过程中有可能遇到后台没配置或网络请求失败的情况下，都有可能返回空
                if (agreements != null && agreements.Agreements != null && agreements.Agreements.Count > 0)
                {
                    var subscriptionAgreement = agreements.Get((int)GPCAgreementType.TermsOfService);
                    callBack(agreements);
                }
                else
                {
                    callBack(null);
                }
            }
        );
    }
}

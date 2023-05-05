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
using Artees.UnitySemVer;

public static class LauncherIggSdkUtil
{
    public class InitResult
    {
        public InitHelper.StartupStepInvoke startupStepInvoke;
        public AppconfBean appconfBean;
        public GPCPrimaryAppConf appconf;
        public GPCSession session;
        public GPCError error;
        public InitResultStatus status;
    }

    public enum InitResultStatus
    {
        LoginSuccess,
        LoginFailed,
        Retry,
    }



    static TaskCompletionSource<InitResult> initTcs;
    static InitResult initResult;

    /// <summary>
    /// SDK 初始化，获取游戏 appconfig。
    /// </summary>
    public static Task<InitResult> InitAsync(GPCGameDelegate gameInfoProvider, string androidGameId, string iosGameId)
    {
        initTcs = new TaskCompletionSource<InitResult>();
        initResult = new InitResult();
        //组装 Android 平台下的所有的 Game ID 和默认的 Game ID
        var androidGameID = androidGameId;

        GameIDBundle androidGameIDBundle = new GameIDBundle(androidGameID, null);

        //组装 iOS 平台下的所有的 Game ID 和默认的 Game ID
        var iOSGameID = iosGameId;
        GameIDBundle iOSGameIDBundle = new GameIDBundle(iOSGameID, null);



        //设置InitHelper监听，包括 usdk初始化监听，账号状态监听，快速登录监听
        var onInitListener = new InitHelper.OnInitListener(
            new InitHelper.OnInitSDKListener(OnInitSDKSuccess, OnInitSDKCompleteWithLocalAppConfig,
                OnInitSDKCompleteToLoadGameDefaultConfig),
            new InitHelper.OnAccountStateListener(OnLoggedIn, OnBanned),
            new InitHelper.OnQuickLoginListener(OnLoginSuccess, OnLoginFailed));



        InitHelper.SharedInstance().Init("IggSdkCanvas", androidGameIDBundle, iOSGameIDBundle, onInitListener, gameInfoProvider);
        return initTcs.Task;
    }


    /// <summary>
    /// 自动登录失败
    /// </summary>
    /// <param name="error"></param>
    static void OnLoginFailed(GPCError error)
    {
        Debug.Log("[IggSdkManager] OnLoginFailed:" + error);
        initResult.error = error;
        initResult.status = InitResultStatus.LoginFailed;
        initTcs.SetResult(initResult);
    }


    /// <summary>
    /// 自动登录成功回调
    /// </summary>
    /// <param name="invoke">自动登录完后的下一步要执行的动作</param>
    /// <param name="session">登录完成后的，账号会话信息</param>
    static void OnLoginSuccess(InitHelper.StartupStepInvoke invoke, GPCSession session)
    {
        Debug.Log("[IggSdkManager] OnLoginSuccess");
        //User ID登录成功后，需检测维护等状态
        initResult.startupStepInvoke = invoke;
        initResult.session = session;
        initResult.status = InitResultStatus.LoginSuccess;
        initTcs.SetResult(initResult);
    }

    public static Action<GPCSession> onBannedDelegate;
    public static Action<GPCSession> onLoggedInDelegate;

    /// <summary>
    /// 账号被封号的统一触发回调
    /// </summary>
    /// <param name="expiredSession">被封的账号的会话信息</param>
    static async void OnBanned(GPCSession expiredSession)
    {
        Debug.Log("[IggSdkManager] OnBanned:" + expiredSession);
        if (onBannedDelegate != null)
        {
            onBannedDelegate.Invoke(expiredSession);
        }
        else
        {
            var dialog = LauncherUiManager.Stuff.Show<LauncherDialog>();
            var msg = LauncherLocalizatioManager.Get("account_forbiden");
            dialog.Set(msg, true);
            await dialog.WaitCompleteAsync();
            Application.Quit();
        }
    }

    /// <summary>
    /// 切换账号/会话过期后重新登录后触发回调
    /// 注意：首次进入游戏以游客身份的登录将会回调 OnLoginSuccess，不会触发此回调
    /// </summary>
    /// <param name="session">账号的会话信息</param>
    static void OnLoggedIn(GPCSession session)
    {
        Debug.Log("[IggSdkManager] OnLoginIn");
        onLoggedInDelegate?.Invoke(session);

        //User ID登录成功后，需检测维护等状态
        initResult.status = InitResultStatus.Retry;
        initTcs.TrySetResult(initResult);
    }


    /// <summary>
    /// 从服务端获取 AppConfig 成功回调。
    /// </summary>
    /// <param name="stepInvoke">如果游戏不在维护状态，就可以调用这个执行游戏的自动登录。</param>
    /// <param name="primaryConfig">appconfig 配置信息</param>
    /// <param name="serverTime">当前服务端时间</param>
    static void OnInitSDKSuccess(GPCPrimaryAppConf primaryConfig, GPCEasternStandardTime serverTime)

    {
        //服务端获取 AppConfig ，需要保存下来，在自动登录成功后，检测运营面板逻辑时使用
        initResult.appconf = primaryConfig;
        initResult.appconfBean = Newtonsoft.Json.JsonConvert.DeserializeObject<AppconfBean>(primaryConfig.GetRawString());
    }

    /// <summary>
    /// 从服务端获取 AppConfig 失败，SDK 会返回本地缓存的 appconfig 这边是返回缓存的 appconfig 回调。
    /// </summary>
    /// <param name="stepInvoke">如果游戏不在维护状态，就可以调用这个执行游戏的自动登录。</param>
    /// <param name="primaryConfig">封装了 GPCPrimaryAppConf 这个 GPCPrimaryAppConf 只是本地缓存的，可以从这边获取  appconfig 配置信息 </param>
    /// <param name="clientTime">当前手机时间</param>
    static void OnInitSDKCompleteWithLocalAppConfig(GPCPrimaryAppConfBackup primaryConfig, GPCEasternStandardTime clientTime)
    {
        initResult.appconf = primaryConfig.appconf;
        initResult.appconfBean = Newtonsoft.Json.JsonConvert.DeserializeObject<AppconfBean>(primaryConfig.appconf.GetRawString());
    }

    /// <summary>
    /// 从服务端获取失败，并且 SDK 没有获取到本地缓存的 appconfig 这时候游戏要根据游戏上配置的默认 appconfig 进游戏
    /// ，这个主要配置信息是游戏的服务端 ip 跟端口情况（能影响进游戏的都默认写死一份到游戏的默认配置里）。
    /// </summary>
    /// <param name="stepInvoke">可以调用这个执行游戏的自动登录。</param>
    static void OnInitSDKCompleteToLoadGameDefaultConfig()
    {
        Debug.Log("[IggChannel] no sdk config got");
    }


    static TaskCompletionSource<bool> tcsOperationPanel;
    public static Task ShowOperationPannelIfNeedAsync(GPCPrimaryAppConf appconf)
    {
        if(tcsOperationPanel != null)
        {
            throw new Exception("tcsOperation already exsits");
        }
        tcsOperationPanel = new TaskCompletionSource<bool>();
        var ret = tcsOperationPanel;
        AppConfPanel.Find().Init(EnterGameByMaintainServerIds, IsForceAppUpdate, IsAppUpdate, refreshGame);
        AppConfPanel.Find().ShowDialog(appconf, OnOperationPannelCompelte);
        return ret.Task;
    }

    public static void OnOperationPannelCompelte()
    {
        tcsOperationPanel.SetResult(true);
        tcsOperationPanel = null;
    }

    /// <summary>
    /// 根据 AppConf 配置的“维护的服务器”，将维护服务器数据返回给游戏侧，由游戏侧判断是否可进入游戏。
    /// </summary>
    /// <param name="updateVersion"></param>
    /// <returns>true 进入游戏； false 不进入游戏</returns>
    public static bool EnterGameByMaintainServerIds(string serverIds)
    {
        return false;
    }

    /// <summary>
    /// 游戏判断是否需要强制更新
    /// </summary>
    /// <param name="updateVersion">AppConf获取到的版本号</param>
    /// <returns>true 强制更新 ，false 不强制更新</returns>
    public static bool IsForceAppUpdate(string updateVersion)
    {
        var thisV = SemVer.Parse(Application.version);
        var toV = SemVer.Parse(updateVersion);
        if(thisV < toV)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 游戏判断是否提示更新
    /// </summary>
    /// <param name="updateVersion">AppConf获取到的版本号</param>
    /// <returns>true 提示更新 ，false 不提示更新</returns>
    public static bool IsAppUpdate(string updateVersion)
    {
        var thisV = SemVer.Parse(Application.version);
        var toV = SemVer.Parse(updateVersion);
        if (thisV < toV)
        {
            return true;
        }
        return false;
    }


    /// <summary>
    /// 维护面板，刷新按钮事件
    /// </summary>
    private static void refreshGame()
    {
        InitHelper.SharedInstance().LoadPrimaryConfig(delegate (GPCPrimaryAppConf appconfig, GPCEasternStandardTime time)
        {
            AssistMonoBehaviour.SharedInstance().AddRunnable(() =>
            {
                AppConfPanel.Find().ShowDialog(appconfig, OnOperationPannelCompelte);
            });
           

        }, delegate (GPCPrimaryAppConfBackup backup, GPCEasternStandardTime time, GPCError error)
        {

        });
    }
}

using System.Collections;
using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;
using Peapod.ADSDK;
using Peapod.ADSDK.Base;
using GPC.ADSDK.Firebase;
using GPC.ADSDK.Appsflyer.Tracker;
using Firebase.Analytics;
using GPC.ADSDK.Facebook;

public static class TrackManager
{
    //https://g.igg.com/E6A0T8
    // AppsFlyer: IGG 所有游戏都是用同一个 Dev Key：WEYqZmRBi6ZmFww2esj28Y

    // peapod: http://peapod-sdk-unity-integration.rtd.skyunion.net/en/latest/guides/research_ad_entrepot.html

    static string appsflyerDevKey = "WEYqZmRBi6ZmFww2esj28Y";
    static string appleId = "6444820641";

    public static void Initalize()
    {
        Debug.Log("[TrackManager] Initalize");
        // 反初始化，避免重复调用
        PeapodSDK.UnInitialize();
        // 配置 firebase 平台。
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // firebase 只在安卓/ios上支持
                var enableFirebase = GameManifestManager.Get("enableFirebase", "true");
                if (enableFirebase == "true")
                {
                    PeapodSDK.AttachVendor(new GPCADVendorFirebase());
                }
            }
        }

        // appsflayer
        {
            // 配置 appsflyer 平台，需传入 appsflyer 平台对应的 devkey
            // 与游戏在 iTunes Connect 后台的 Apple ID。
            GPCADVendorAppsflyer appsflyer = new GPCADVendorAppsflyer(appsflyerDevKey, appleId);
            // 开启 appsflyer 运行日志打印，该开关仅为了 debug 方便，研发发包的时候要设置为 false。
            var enableAppsflyerDebug = bool.Parse(GameManifestManager.Get("enableAppsflyerDebug", "false"));
            appsflyer.SetIsDebug(enableAppsflyerDebug);
            PeapodSDK.AttachVendor(appsflyer);
        }

        // facebook
        {
            PeapodSDK.AttachVendor(new GPCADVendorFacebook());
        }

        Debug.Log("[TrackManager] call peapod sdk initalize()");
        var gameId = LauncherIggSdkManager.Stuff.GameId;
        PeapodSDK.Initialize(gameId);
        Debug.Log("[TrackManager] call peapod sdk initalize() complete");
    }

    static (Dictionary<string, int> intDic, Dictionary<string, string> stringDic) FiltArgsType(
        Dictionary<string, string> dic)
    {
        var intDic = new Dictionary<string, int>();
        var stringDic = new Dictionary<string, string>();
        foreach (var kv in dic)
        {
            var key = kv.Key;
            var value = kv.Value;
            var isInt = int.TryParse(value, out var intValue);
            if (isInt)
            {
                intDic[key] = intValue;
            }
            else
            {
                stringDic[key] = value.ToString();
            }
        }

        return (intDic, stringDic);
    }

    public static void LevelUp(string heroName, long level)
    {
        if (!IsTrackOpen)
        {
            return;
        }

        PeapodSDK.SendLevelUp(heroName, level);
    }

    public static async void ReportJoinGroupIfNeed(string groupId)
    {
        if (!IsTrackOpen)
        {
            return;
        }

        if (Database.Stuff.roleDatabase.Me.reported > 0)
        {
            return;
        }

        var eventName = "join_group";
        var b = IsEventSent(eventName);
        if (b)
        {
            return;
        }

        SetEventSent(eventName);

        Database.Stuff.roleDatabase.Me.reported++;
        PeapodSDK.SendJoinGroup(groupId);
        await GuildApi.ReportAsync();
    }

    // 参数一：支出虚拟货币的名称（比如：金币、宝石、代币等），可由研发根据自身游戏货币类型做选择，选择后备注好虚拟货币类型，同步给广告部即可。
    // 参数二：消耗物id。
    // 参数三：支出虚拟货币的名称类型，比如：金币、宝石、代币等。
    // 参数四：支出虚拟货币的数量数字类型(double)
    // 参数五：（选填）支出虚拟货币后获得商品的名称，广告部分析数据不依赖这个参数信息，主要用于研发自己对虚保消耗去做定向分析时有帮助。
    public static void SendVirtualCurrency(string name, string id, string type, double count, string goods)
    {
        if (!IsTrackOpen)
        {
            return;
        }

        PeapodSDK.SendSpendVirtualCurrency(name, id, type, count, goods);
    }

    // 虚拟货币增加
    // 上报的数量不包含赠送的数量
    // 如果礼包中不是纯粹只包含代码，则不上报
    public static void SendEarnVirtualCurrency(string name, int count)
    {
        PeapodSDK.SendEarnVirtualCurrency(name, count);
    }

    public static void UnlockAchievement(string id)
    {
        if (!IsTrackOpen)
        {
            return;
        }

        PeapodSDK.SendUnlockAchievement(id);
    }

    /// <summary>
    /// 自定义事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="paramDic"></param>
    public static void CustomReport(string eventName, Dictionary<string, string> paramDic = null)
    {
        if (!IsTrackOpen)
        {
            return;
        }

        Debug.Log("[TrackManager] report: " + eventName);

        if (paramDic == null)
        {
            paramDic = new Dictionary<string, string>();
        }


        // add igg id
        if (IggSdkManager.Stuff.IsIggChannel)
        {
            var session = LauncherIggSdkManager.Stuff.initResult.session;
            var iggId = session.GetUserId();
            paramDic["userid"] = iggId;
        }
        else
        {
            paramDic["userid"] = "0";
        }

        // add role id
        var gameSession = LoginManager.Stuff.session;
        if (gameSession != null)
        {
            var uid = gameSession.selectedRoleData._id;
            paramDic["role_uid"] = uid;
        }

        // add platform
        paramDic["platform"] = Application.platform.ToString();

        // send to sdk
        PeapodTrack(eventName, paramDic);
    }

    //static void GameSelfTrack(string eventName, Dictionary<string, string> paramDic)
    //{
    //    var (intDic, stringDic) = FiltArgsType(paramDic);
    //    TrackApi.StatisticsInBackground(eventName, intDic, stringDic);
    //}

    static Dictionary<string, object> StringStringDicToStringObjectDic(Dictionary<string, string> paramDic)
    {
        var ret = new Dictionary<string, object>();
        foreach (var kv in paramDic)
        {
            var key = kv.Key;
            var value = kv.Value;
            ret[key] = value;
        }

        return ret;
    }

    static void PeapodTrack(string eventName, Dictionary<string, string> paramDic)
    {
        var stringObjectDic = StringStringDicToStringObjectDic(paramDic);
        PeapodSDK.SendEvent(eventName, stringObjectDic);
    }

    /// <summary>
    /// 自定义事件
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="parameters"></param>
    public static void CustomReport(string eventName, params string[] parameters)
    {
        var paramDic = new Dictionary<string, string>();
        for (var index = 0; index < parameters.Length; index += 2)
        {
            if (index + 1 >= parameters.Length)
            {
                break;
            }

            var key = parameters[index];
            var val = parameters[index + 1];
            paramDic.Add(key, val);
        }

        CustomReport(eventName, paramDic);
    }

    static bool IsTrackOpen
    {
        get
        {
            if (Application.isEditor)
            {
                return false;
            }

            if (!IggSdkManager.Stuff.IsIggChannel)
            {
                return false;
            }

            var track = EnvManager.GetConfigOfFinalEnv("track");
            if (track == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    //public static void SignUp()
    //{
    //    // 发送 SIGN_UP 事件，需要在获取 UserId 成功之后
    //    CustomReport(MarketResearchEventName.SIGN_UP);
    //}

    // 触发时机：每次游戏启动后，获得uid后触发。
    public static void GameStart()
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：设备内存大小，单位 MB
        var size = SystemInfo.systemMemorySize.ToString();
        extraParams.Add(MarketResearchEventParam.GameStartDeviceMemoryParam, size);
        CustomReport(MarketResearchEventName.GAME_START, extraParams);
    }

    public static void ReportDayIndexEventIfNeed()
    {
        var registerTime = Database.Stuff.roleDatabase.Me.logon;
        var registerDate = Clock.ToDateTime(registerTime).Date;
        var nowDate = Clock.Now.Date;
        var span = nowDate - registerDate;
        var spanDays = span.Days;
        var dayIndex = spanDays + 1;

        if (needReportDaysIndex.Contains(dayIndex))
        {
            ReportDayIndexEventIfNeed(dayIndex);
        }
    }


    static HashSet<int> needReportDaysIndex = new HashSet<int> {2, 3, 4, 5, 6, 7, 14, 30, 60, 90, 120, 150, 180};

    static void ReportDayIndexEventIfNeed(int dayIndex)
    {
        var eventName = $"DAY{dayIndex}_RETENTION";
        var b = RoleLocalCache.GetInt(eventName, 0);
        if (b == 1)
        {
            return;
        }

        //Debug.Log("[TrackManager] dayIndex: " + eventName);
        CustomReport(eventName);
        RoleLocalCache.SetInt(eventName, 1);
    }

    public static void SetEventSent(string eventName)
    {
        var key = $"event.{eventName}";
        RoleLocalCache.SetInt(key, 1);
    }

    public static bool IsEventSent(string eventName)
    {
        var key = $"event.{eventName}";
        return RoleLocalCache.GetInt(key, 0) == 1;
    }


    public static void SendGeneralEvents()
    {
        // 要求安卓系统版本高于某个版本号，才上传对应的系统版本事件，这边配置成高于安卓 6。
        PeapodSDK.ExposeFromAndroidVersion(6);
        // 要求 iOS 系统版本高于某个版本号，才上传对应的系统版本事件，这边配置成高于 iOS 10。
        PeapodSDK.ExposeFromiOSVersion(10);

        var session = LauncherIggSdkManager.Stuff.initResult.session;
        var iggId = session.GetUserId();
        PeapodSDK.SendGeneralEvents(iggId);
    }

    public static void ReportDeviceInfo(int frameCount)
    {
        var extraParams = new Dictionary<string, string>();

        extraParams.Add("currentQuality", DeveloperLocalSettings.GraphicQuality.ToString());
        extraParams.Add("frameCount", frameCount.ToString());
        extraParams.Add("operatingSystem", SystemInfo.operatingSystem);
        extraParams.Add("systemMemorySize", SystemInfo.systemMemorySize.ToString());

        extraParams.Add("processorType", SystemInfo.processorType);
        extraParams.Add("processorFrequency", SystemInfo.processorFrequency.ToString());
        extraParams.Add("processorCount", SystemInfo.processorCount.ToString());


        extraParams.Add("graphicsDeviceName", SystemInfo.graphicsDeviceName);
        extraParams.Add("graphicsDeviceVersion", SystemInfo.graphicsDeviceVersion.ToString());
        extraParams.Add("graphicsMemorySize", SystemInfo.graphicsMemorySize.ToString());


        CustomReport("DeviceInfo", extraParams);
    }

    public static void Wish(string heroUse, int heroResult)
    {
        var extraParams = new Dictionary<string, string>();
        extraParams.Add("heroUse", heroUse);
        extraParams.Add("heroResult", heroResult.ToString());
        CustomReport("wish", extraParams);
    }

    // 触发时机：每一段对话点击时触发。
    public static void Story(string chatper)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：表示对话所属章节。Ex：1 - 表示第一章剧情
        extraParams.Add(MarketResearchEventParam.StoryChapter, chatper);
        CustomReport(MarketResearchEventName.STORY, extraParams);
    }

    // 触发时机：玩家点击剧情跳过时触发。
    public static void StorySkip(string chatper)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：表示对话所属章节。Ex：1 - 表示第一章剧情
        extraParams.Add(MarketResearchEventParam.StorySkipChapter, chatper);
        CustomReport(MarketResearchEventName.STORY_SKIP, extraParams);
    }


    public static void TutorialBegin()
    {
        if (!IsTrackOpen)
        {
            return;
        }

        var eventName = "tutorialBegin";
        if (IsEventSent(eventName))
        {
            return;
        }

        SetEventSent(eventName);

        PeapodSDK.SendTutorialBegin();
    }

    public static void ReportTutorial(string step)
    {
        if (string.IsNullOrEmpty(step)) return;
        var split = step.Split(';');
        if (split.Length <= 0) return;

        foreach (var s in split)
        {
            if (IsEventSent(s))
            {
                continue;
            }

            SetEventSent(s);
            CustomReport(s, s, step);
        }
    }

    // 触发时机：玩家教程每一步成功触发。
    public static void TutorialComplete(string step, bool complete)
    {
        if (!IsTrackOpen)
        {
            return;
        }

        var eventName = "tutorialComplete";
        if (IsEventSent(eventName))
        {
            return;
        }

        SetEventSent(eventName);

        PeapodSDK.SendTutorialComplete("", step, complete);
    }


    // 触发时机：进入关卡战斗界面触发。
    public static void BattleStart(string stage)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：战斗关卡。可以由研发定义，例如 "1" 则代表第一关
        extraParams.Add(MarketResearchEventParam.BattleStartMission, stage);
        CustomReport(MarketResearchEventName.BATTLE_START, extraParams);
    }

    // 触发时机：战斗胜利时触发。
    public static void BattleWin(string stage, BattleSenceRecord record)
    {
        var extraParams = record.GetTrackDic();
        extraParams.Add("mission", stage);
        CustomReport("Battle_Win", extraParams);
    }

    // 触发时机：战斗失败时触发。
    public static void BattleLose(string stage, BattleSenceRecord record)
    {
        var extraParams = record.GetTrackDic();
        extraParams.Add("mission", stage);
        CustomReport("Battle_Lose", extraParams);
    }

    // 触发时机：战斗逃跑时触发（若游戏没有战斗逃跑功能可忽略此事件）。
    public static void BattleExit(string stage)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：战斗关卡。可以由研发定义，例如 "1" 则代表第一关
        extraParams.Add(MarketResearchEventParam.BattleExitMission, stage);
        CustomReport(MarketResearchEventName.BATTLE_EXIT, extraParams);
    }

    // 触发时机：PVP 战斗开始触发。
    public static void PvpStart(string opponentId, string rankId)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：对手id
        extraParams.Add(MarketResearchEventParam.PvpStartOpponent, opponentId);
        // 参数二：阶位id。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpStartRank, rankId);
        CustomReport(MarketResearchEventName.PVP_START, extraParams);
    }

    // 触发时机：PVP 战斗胜利触发。
    public static void PvpWin(string opponentId, string rankId, string scoreDelta)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：对手id
        extraParams.Add(MarketResearchEventParam.PvpWinOpponent, opponentId);
        // 参数二：阶位id。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpWinRank, rankId);
        // 参数三：积分变化，例如10或者-10。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpWinPoint, scoreDelta);
        CustomReport(MarketResearchEventName.PVP_WIN, extraParams);
    }

    // 触发时机：PVP 战斗失败触发
    public static void PvpLose(string opponentId, string rankId, string scoreDelta)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：对手id
        extraParams.Add(MarketResearchEventParam.PvpWinOpponent, opponentId);
        // 参数二：阶位id。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpWinRank, rankId);
        // 参数三：积分变化，例如10或者-10。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpWinPoint, scoreDelta);
        CustomReport(MarketResearchEventName.PVP_LOSE, extraParams);
    }

    // 触发时机：PVP 战斗逃跑（若游戏没有战斗逃跑功能可忽略此事件）
    public static void PvpExit(string opponentId, string rankId, string scoreDelta)
    {
        var extraParams = new Dictionary<string, string>();
        // 参数一：对手id
        extraParams.Add(MarketResearchEventParam.PvpWinOpponent, opponentId);
        // 参数二：阶位id。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpWinRank, rankId);
        // 参数三：积分变化，例如10或者-10。（可选参数）
        extraParams.Add(MarketResearchEventParam.PvpWinPoint, scoreDelta);
        CustomReport(MarketResearchEventName.PVP_EXIT, extraParams);
    }
}
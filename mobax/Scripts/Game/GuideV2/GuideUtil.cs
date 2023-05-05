using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

// 战斗界面UI
public enum EGuideBattleUIType
{
    [LabelText("战术道具1")] TacticItem1,
    [LabelText("战术道具2")] TacticItem2,
    [LabelText("战术道具3")] TacticItem3,
    [LabelText("退出按钮")] BackBtn,
    [LabelText("守护按钮1")] DefenceBtn1,
    [LabelText("守护按钮2")] DefenceBtn2,
    [LabelText("守护按钮3")] DefenceBtn3,
    [LabelText("集火怪物组")] FocusMonsters,
    [LabelText("自动战斗按钮")] AutoBtn,
    [LabelText("职业介绍")] JobIntro,
}

// 主界面UI
public enum EMainPageUIType
{
    [LabelText("派遣按钮")] DispatchBtn = 1,
    [LabelText("公会按钮")] GuildBtn,
    [LabelText("好友按钮")] FriendBtn,
    [LabelText("帮助按钮")] HelpBtn,
    [LabelText("战斗按钮")] BattleBtn,
    [LabelText("商城按钮")] ShopBtn,
    [LabelText("关卡按钮")] StageBtn,
    [LabelText("BP按钮")] BpBtn,
    [LabelText("任务按钮")] TaskBtn,
    [LabelText("活动按钮")] ActivityBtn,
    [LabelText("小游戏按钮")] MiniGameBtn,
}

/// <summary>
/// 存放一些接口调用
/// </summary>
public static class GuideUtil
{
    public static Vector3 LastPos;

    public static void RecordLastPos(Vector3 pos)
    {
        LastPos = pos;
    }

    #region ---战斗UI相关---

    private static Dictionary<string, EGuideBattleUIType> _battleUITypeDic =
        new Dictionary<string, EGuideBattleUIType>()
        {
            {"战术道具1", EGuideBattleUIType.TacticItem1},
            {"战术道具2", EGuideBattleUIType.TacticItem2},
            {"战术道具3", EGuideBattleUIType.TacticItem3},
            {"退出按钮", EGuideBattleUIType.BackBtn},
            {"守护按钮1", EGuideBattleUIType.DefenceBtn1},
            {"守护按钮2", EGuideBattleUIType.DefenceBtn2},
            {"守护按钮3", EGuideBattleUIType.DefenceBtn3},
            {"集火怪物组", EGuideBattleUIType.FocusMonsters},
            {"自动战斗按钮", EGuideBattleUIType.AutoBtn},
            {"职业介绍", EGuideBattleUIType.JobIntro},
        };

    public static void ShowBattleUI(string arg)
    {
        var r = ParseBattleUITypes(arg);
        if (r.Count <= 0) return;
        UiUtil.ShowBattleUI(r);
    }

    public static void HideBattleUI(string arg)
    {
        var r = ParseBattleUITypes(arg);
        if (r.Count <= 0) return;
        UiUtil.HideBattleUI(r);
    }

    // 解析所有的类型
    static List<EGuideBattleUIType> ParseBattleUITypes(string arg)
    {
        var t = arg.Split(';');
        var r = new List<EGuideBattleUIType>();

        foreach (var tt in t)
        {
            if (_battleUITypeDic.ContainsKey(tt))
            {
                r.Add(_battleUITypeDic[tt]);
            }
        }

        return r;
    }

    #endregion

    #region ---主界面UI相关---

    private static Dictionary<string, EMainPageUIType> _mainPageTypeDic = new Dictionary<string, EMainPageUIType>()
    {
        {"派遣按钮", EMainPageUIType.DispatchBtn},
        {"公会按钮", EMainPageUIType.GuildBtn},
        {"好友按钮", EMainPageUIType.FriendBtn},
        {"帮助按钮", EMainPageUIType.HelpBtn},
        {"战斗按钮", EMainPageUIType.BattleBtn},
        {"商城按钮", EMainPageUIType.ShopBtn},
        {"关卡按钮", EMainPageUIType.StageBtn},
        {"BP按钮", EMainPageUIType.BpBtn},
        {"任务按钮", EMainPageUIType.TaskBtn},
        {"活动按钮", EMainPageUIType.ActivityBtn},
        {"小游戏按钮", EMainPageUIType.MiniGameBtn},
    };

    public static void ShowMainPageUI(string arg)
    {
        var r = ParseMainPageUITypes(arg);
        if (r.Count <= 0) return;
        UiUtil.ShowOrHideMainPageUI(r, true);
    }

    public static void HideMainPageUI(string arg)
    {
        var r = ParseMainPageUITypes(arg);
        if (r.Count <= 0) return;
        UiUtil.ShowOrHideMainPageUI(r, false);
    }

    // 解析所有的类型
    static List<EMainPageUIType> ParseMainPageUITypes(string arg)
    {
        var t = arg.Split(';');
        var r = new List<EMainPageUIType>();

        foreach (var tt in t)
        {
            if (_mainPageTypeDic.ContainsKey(tt))
            {
                r.Add(_mainPageTypeDic[tt]);
            }
        }

        return r;
    }

    #endregion

    #region ---好友相关---

    #endregion

    #region ---检测相关---

    public static bool IsAddSystemFriend()
    {
        var isCompleted = Database.Stuff.roleDatabase.Me.GetGuide("108", -1) >= 1;
        return GuideManagerV2.Stuff.IsExecutingForceGuide && !isCompleted;
    }

    public static bool IsCircuitCompleted()
    {
        var isCompleted = Database.Stuff.roleDatabase.Me.GetGuide("circuit", -1) >= 1;
        return isCompleted;
    }

    public static bool IsTaskGuide()
    {
        return GuideManagerV2.Stuff.ExecutingForceGuideId == int.Parse("111");
        var isCompleted = Database.Stuff.roleDatabase.Me.GetGuide("111", -1) >= 1;
        return GuideManagerV2.Stuff.IsExecutingForceGuide && !isCompleted;
    }

    public static bool IsForbidden(string extraParams)
    {
        return extraParams == "禁止按钮操作";
    }

    /// <summary>
    /// 是否需要显示幫助
    /// </summary>
    /// <returns></returns>
    public static bool CheckShowStageGuide()
    {
        return !IsGuideCompleted() && IsNeedToShowStageGuide() && HotLocalSettings.IsGuideV2Enabled;
    }

    /// <summary>
    /// 是否完成公会引导
    /// </summary>
    /// <returns></returns>
    static bool IsGuideCompleted()
    {
        var isCompleted = Database.Stuff.roleDatabase.Me.GetGuide("GoToStage", -1) >= 1;
        return isCompleted;
    }

    public static bool IsForceEnd()
    {
        var isCompleted = Database.Stuff.roleDatabase.Me.GetGuide("111", -1) >= 1;
        return isCompleted;
    }

    public static bool IsNeedToShowStageGuide()
    {
        var isCompleted = Database.Stuff.roleDatabase.Me.GetGuide("MainPageHelp", -1) >= 1;
        return isCompleted;
    }

    #endregion
}
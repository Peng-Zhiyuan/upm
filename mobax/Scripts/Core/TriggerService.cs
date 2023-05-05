using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;
using BattleEngine.Logic;

public class TriggerService : Service
{
    public override void OnCreate()
    {
        Expression.OnResolveVaribale = OnResolveVariable;
        ScriptEventExecutingUtil.ExecuteCommand = OnExecuteCommandLine;
    }

    async Task<object> OnExecuteCommandLine(string command, List<ResolveResult> argList)
    {
        if (command == "dialog")
        {
            var args1 = argList[0];
            var msg = args1.stringValue;
            await Dialog.ConfirmAsync("", msg);
            return null;
        }
        else if (command == "battle")
        {
            var arg = argList[0].stringValue;
            await StartBattleAsync(arg);
            return null;
        }
        else if (command == "addHp")
        {
            var heroId = argList[0].intValue;
            var value = argList[1].intValue;
            BattleUtil.AddHp(heroId, value);
            GameEventCenter.Broadcast(GameEvent.UpdateHpMp);
            return null;
        }
        else if (command == "addMp")
        {
            var heroId = argList[0].intValue;
            var value = argList[1].intValue;
            BattleUtil.AddMp(heroId, value);
            GameEventCenter.Broadcast(GameEvent.UpdateEnergy);
            return null;
        }
        else if (command == "navigate")
        {
            var pageName = argList[0].stringValue;
            var pageArg = ListUtil.TryGet(argList, 1)?.AnyValue;

            // 首次跳转至主界面需要forward
            if (pageName == nameof(MainPage))
            {
                await UIEngine.Stuff.ForwardOrBackToAsync<MainPage>();
            }
            else
            {
                await UIEngine.Stuff.ForwardOrBackToAsync(pageName, pageArg);
            }

            return null;
        }
        else if (command == "delay")
        {
            var ms = argList[0].intValue;
            await Task.Delay(ms);
            return null;
        }
        else if (command == "saveFormation")
        {
            var ms = argList[0].stringValue;
            await SaveFormationAsync(ms);
            return null;
        }
        else if (command == "plot")
        {
            var plotId = argList[0].intValue;
            //await PlotPipelineManager.Stuff.PlayPlotAsync(plotId, DeveloperLocalSettings.IsSkipPlot);
            await PlotPipelineUtil.PlayPlotAsync(plotId);
            return null;
        }
        else if (command == "pause")
        {
            BattleManager.Instance.BtnPause(true);
            return null;
        }
        else if (command == "replay")
        {
            BattleManager.Instance.BtnPause(false);
            return null;
        }
        else if (command == "waitPage")
        {
            var pageName = argList[0].stringValue;
            await WaitPageAsync(pageName);
            return null;
        }
        else if (command == "waitFloating")
        {
            var pageName = argList[0].stringValue;
            await WaitFloatingAsync(pageName);
            return null;
        }
        // else if (command == "waitRemovePage")
        // {
        //     var pageName = argList[0].stringValue;
        //     await WaitRemovePageAsync(pageName);
        // }
        else if (command == "block")
        {
            ShowCheckBlockFloating();
            return null;
        }
        else if (command == "removeBlock")
        {
            RemoveBlockFloating();
            return null;
        }
        else if (command == "click")
        {
            var path = argList[0].stringValue;
            var hint = ListUtil.TryGet(argList, 1)?.stringValue;
            var extraParam = ListUtil.TryGet(argList, 2)?.stringValue;
            await ClickGuideAsync(path, hint, extraParams: extraParam);
            UIEngine.Stuff.RemoveFloating<GuideFloating>();
            return null;
        }
        else if (command == "clickWithNoBg")
        {
            var path = argList[0].stringValue;
            var hint = ListUtil.TryGet(argList, 1)?.stringValue;
            var extraParam = ListUtil.TryGet(argList, 2)?.stringValue;
            await ClickGuideWithNoBgAsync(path, hint, extraParams: extraParam);
            UIEngine.Stuff.RemoveFloating<GuideFloating>();
            return null;
        }
        else if (command == "clickJustShow")
        {
            var path = argList[0].stringValue;
            var hint = ListUtil.TryGet(argList, 1)?.stringValue;
            var extraParam = ListUtil.TryGet(argList, 2)?.stringValue;
            await ClickGuideWithNoBgAsync(path, hint, extraParams: extraParam);
            UIEngine.Stuff.RemoveFloating<GuideFloating>();
            return null;
        }
        // 隐藏战斗UI
        else if (command == "hideBattleUI")
        {
            var arg = argList[0].stringValue;
            GuideUtil.HideBattleUI(arg);
            return null;
        }
        // 显示战斗UI
        else if (command == "showBattleUI")
        {
            var arg = argList[0].stringValue;
            GuideUtil.ShowBattleUI(arg);
            return null;
        }
        // 隐藏主界面UI
        else if (command == "hideMainPageUI")
        {
            var arg = argList[0].stringValue;
            GuideUtil.HideMainPageUI(arg);
            return null;
        }
        // 显示主界面UI
        else if (command == "showMainPageUI")
        {
            var arg = argList[0].stringValue;
            GuideUtil.ShowMainPageUI(arg);
            return null;
        }
        else if (command == "drag")
        {
            var path = argList[0].stringValue;
            var hint = argList[1].stringValue;
            await ClickGuideAsync(path, hint, true);
            UIEngine.Stuff.RemoveFloating<GuideFloating>();
            return null;
        }
        else if (command == "battle.setBackPage")
        {
            var pageName = argList[0].stringValue;
            BattlePipline.SetBackPage(pageName);
            return null;
        }
        else if (command == "triggerCompleted")
        {
            GuideManagerV2.Stuff.SetTriggerGuideCompleteToData();
            return null;
        }
        else if (command == "stepCompleted")
        {
            GuideManagerV2.Stuff.SetForceStepGuideCompleteToData();
            return null;
        }

        else if (command == "drawCard.setGuide")
        {
            var value = argList[0].boolValue;
            DrawCardPoolPage.isMarkedGuideDrawCard = value;
            return null;
        }
        else if (command == "movie")
        {
            var movieId = 0;
            if (argList == null || argList.Count <= 0 || argList[0] == null)
            {
                movieId = 1;
            }
            else
            {
                movieId = argList[0].intValue;
                if (movieId == 0)
                {
                    movieId = 1;
                }
            }

            var moviePage = await UIEngine.Stuff.ForwardAsync<MovieSubtitlePage>(movieId);
            await moviePage.WaitCompleteAsync();
            return null;
        }
        else if (command == "ensureBattleQuit")
        {
            await SureNoBattleAsync();
            return null;
        }
        else if (command == "reportTutorialBegin")
        {
            TrackManager.TutorialBegin();
            return null;
        }
        else if (command == "reportTutorial")
        {
            var str = argList[0].stringValue;
            TrackManager.ReportTutorial(str);
            return null;
        }
        else if (command == "reportTutorialEnd")
        {
            TrackManager.TutorialComplete("", true);
            return null;
        }
        else if (command == "checkItemEnough")
        {
            var itemStr = argList[0].stringValue;
            var b = CheckItemEnoughAsync(itemStr);
            return b;
        }
        else if (command == "checkCircuitReadyOn")
        {
            var b = CheckCircuitReadyOn();
            return b;
        }
        else if (command == "checkStageNotPass")
        {
            var stageId = int.Parse(argList[0].stringValue);
            var b = CheckStageNotPassAsync(stageId);
            return b;
        }
        else if (command == "checkTowerNotPass")
        {
            var b = CheckTowerNotPass();
            return b;
        }
        else if (command == "exception")
        {
            var b = await ExceptionAsync();
            return b;
        }
        else if (command == "exceptionNotBack")
        {
            var b = await ExceptionNotBackAsync();
            return b;
        }

        throw new Exception("unsuuport");
    }

    // 检测是否完成过拼图
    static bool CheckCircuitReadyOn()
    {
        return false;
        // var heroInfo = HeroDisplayer.Hero ?? HeroDisplayer.Default;
        // return HeroCircuitManager.GetCircuits(heroInfo.HeroId).Count <= 0;
    }

    static bool CheckTowerNotPass()
    {
        return Database.Stuff.roleDatabase.Me.tower <= 0;
    }

    static bool CheckItemEnoughAsync(string itemStr)
    {
        var str = itemStr.Split(';');
        var itemId = int.Parse(str.First());
        var num = 1;
        if (str.Length > 1)
        {
            num = int.Parse(str.Last());
        }

        return ItemUtil.IsEnough(itemId, num);
    }

    static async Task<bool> ExceptionAsync()
    {
        await GuideManagerV2.Stuff.SetGuideValueToDataAndBackToMainPage();

        return false;
    }

    static async Task<bool> ExceptionNotBackAsync()
    {
        await GuideManagerV2.Stuff.SetAllGuideValueToDataAndNotBackToMainPage();

        return false;
    }

    static bool CheckStageNotPassAsync(int stageId)
    {
        return !StageManager.Stuff.IsClose(stageId);
    }

    static Task SureNoBattleAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        if (!Battle.Instance.IsBattleServiceResponse)
        {
            tcs.SetResult(true);
        }
        else
        {
            Action d = null;
            d = () =>
            {
                Battle.BattleExit -= d;
                tcs.SetResult(true);
            };
            Battle.BattleExit += d;
        }

        return tcs.Task;
    }

    static void ShowCheckBlockFloating()
    {
        UIEngine.Stuff.ShowFloating<GuideBlockFloating>(layer: UILayer.GuideBlockLayer);
    }

    static void RemoveBlockFloating()
    {
        UIEngine.Stuff.RemoveFloating<GuideBlockFloating>();
    }

    static async Task SaveFormationAsync(string arg)
    {
        var temp = arg.Split(new char[] {';'});
        var tempHeroes = temp[0].Split(',');
        var tempItems = temp[1].Split(',');
        var heroes = new List<int>();
        var items = new List<int>();
        for (int i = 0; i < tempHeroes.Length; i++)
        {
            var heroId = int.Parse(tempHeroes[i]);
            if (ItemUtil.IsEnough(heroId))
            {
                heroes.Add(heroId);
            }
            else
            {
                heroes.Add(0);
            }
        }

        var tempIds = new List<int>();
        if (heroes.Count < 9)
        {
            for (int i = 0; i < 9 - heroes.Count; i++)
            {
                tempIds.Add(0);
            }
        }

        heroes.AddRange(tempIds);
        for (int i = 0; i < tempItems.Length; i++)
        {
            items.Add(int.Parse(tempItems[i]));
        }

        await FormationUtil.SaveFormationAndRefreshDatabase(EFormationIndex.Normal1, heroes, items);
    }

    static async Task StartBattleAsync(string arg)
    {
        var temp = arg.Split(new char[] {';'});
        var stageId = int.Parse(temp[0]);
        var tempHeroes = temp[1].Split(',');
        var tempItems = temp[2].Split(',');
        var heroes = new List<int>();
        var items = new List<int>();
        for (int i = 0; i < tempHeroes.Length; i++)
        {
            heroes.Add(int.Parse(tempHeroes[i]));
        }

        var tempIds = new List<int>();
        if (heroes.Count < 9)
        {
            for (int i = 0; i < 9 - heroes.Count; i++)
            {
                tempIds.Add(0);
            }
        }

        heroes.AddRange(tempIds);
        for (int i = 0; i < tempItems.Length; i++)
        {
            items.Add(int.Parse(tempItems[i]));
        }

        Debug.Log("guide ---> 准备进入战斗，关卡id为" + stageId);

        await FormationUtil.SaveFormationAndRefreshDatabase(EFormationIndex.Normal1, heroes, items);
        //这里设置战斗返回的界面
        //BattlePipline.SetBackPage("StageMainPage");
        BattleUtil.EnterPveBattle(stageId, EFormationIndex.Normal1);
    }

    Task ClickGuideAsync(string path, string hint, bool drag = false, string extraParams = "")
    {
        var tcs = new TaskCompletionSource<bool>();

        var guideFloatingInfo = new GuideFloatingInfo
        {
            DelayDuration = 0f,
            AimPath = path,
            ClonePath = drag ? "needDrag" : "",
            SpecialArg = "",
            EndEvent = "",
            Offset = Vector3.zero,
            IsHideMask = false,
            Hint = hint,
            HintDir = 2,
            Bgm = "",
            ExtraParam = extraParams,
        };
        GuideFloating.OnComplete = () => { tcs.SetResult(true); };
        var guideFloating = UIEngine.Stuff.ShowFloatingImediatly<GuideFloating>();
        guideFloating.OnShow(null);
        guideFloating.SetInfo(guideFloatingInfo);
        guideFloating.transform.SetAsLastSibling();
        return tcs.Task;
    }

    Task ClickGuideWithNoBgAsync(string path, string hint, bool drag = false, string extraParams = "")
    {
        var tcs = new TaskCompletionSource<bool>();

        var guideFloatingInfo = new GuideFloatingInfo
        {
            DelayDuration = 0f,
            AimPath = path,
            ClonePath = drag ? "needDrag" : "",
            SpecialArg = "",
            EndEvent = "",
            Offset = Vector3.zero,
            IsHideMask = true,
            Hint = hint,
            HintDir = 2,
            Bgm = "",
            ExtraParam = extraParams,
        };
        GuideFloating.OnComplete = () => { tcs.SetResult(true); };
        var guideFloating = UIEngine.Stuff.ShowFloatingImediatly<GuideFloating>();
        guideFloating.SetInfo(guideFloatingInfo);
        guideFloating.transform.SetAsLastSibling();
        return tcs.Task;
    }

    Task WaitPageAsync(string pageName)
    {
        var tcs = new TaskCompletionSource<bool>();
        var top = UIEngine.Stuff.Top;
        if (top.name == pageName)
        {
            tcs.SetResult(true);
            return tcs.Task;
        }

        UIEngine.RegisterOnetimePageChangedListner(pageName, () => { tcs.SetResult(true); });
        return tcs.Task;
    }

    Task WaitFloatingAsync(string pageName)
    {
        var tcs = new TaskCompletionSource<bool>();
        var top = UIEngine.Stuff.FindFloating(pageName);
        if (top != null && top.name == pageName)
        {
            tcs.SetResult(true);
            return tcs.Task;
        }

        UIEngine.RegisterOnetimePageChangedListner(pageName, () => { tcs.SetResult(true); });
        return tcs.Task;
    }
    // Task WaitRemovePageAsync(string pageName)
    // {
    //     var tcs = new TaskCompletionSource<bool>();
    //     var top = UIEngine.Stuff.Top;
    //     if (top.name != pageName)
    //     {
    //         tcs.SetResult(true);
    //         return tcs.Task;
    //     }
    //
    //     return UIEngine.Stuff.RemoveFromStackAsync(pageName);
    //     // UIEngine.RegisterOnetimePageChangedListner(pageName, () => { tcs.SetResult(true); });
    //     // return tcs.Task;
    // }

    ResolveResult OnResolveVariable(string variable)
    {
        var result = new ResolveResult();
        if (variable == "$page")
        {
            result.stringValue = UIEngine.Stuff.Top.name;
            result.type = ResultValueType.String;
        }
        else if (variable == "$stageGuideMark")
        {
            var value = ResolveStageGuideMark();
            result.intValue = value;
            result.type = ResultValueType.Int;
        }
        else if (variable == "$StageId")
        {
            var stageId = TryFindCurrentStageId() ?? -1;
            result.intValue = stageId;
            result.type = ResultValueType.Int;
        }
        else
        {
            throw new Exception("[ScriptEventService] unsupport variable: " + variable);
        }

        return result;
    }

    static int ResolveStageGuideMark()
    {
        var stageId = TryFindCurrentStageId();
        if (stageId == null)
        {
            return -1;
        }

        var stageRow = StaticData.StageTable[stageId.Value];
        var value = stageRow.guideType;
        return value;
    }

    static int? TryFindCurrentStageId()
    {
        if (Battle.Instance.IsBattleServiceResponse)
        {
            var stageId = Battle.Instance.CopyId;
            return stageId;
        }

        {
            var page = UIEngine.Stuff.FindPage<BattleResultWinPage>();
            if (page != null)
            {
                return page.currentStageRow.Id;
            }
        }
        {
            var page = UIEngine.Stuff.FindPage<StageMainPage>();
            if (page != null)
            {
                return page.BestStageId;
            }
        }
        //throw new Exception("[Script] $stageGuideMark: only avaiable when BattleResultWinPage or StageMainPage in stack");
        return null;
    }
}
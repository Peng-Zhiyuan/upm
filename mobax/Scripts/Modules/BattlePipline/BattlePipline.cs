using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using BattleEngine.Logic;
using BattleEngine.View;
using BattleSystem.ProjectCore;

public static class BattlePipline
{
    private static string from;

    /// <summary>
    /// 请求创建战斗协议
    /// </summary>
    public static async Task<CreateBattleResponse> RequestCreateBattleResponse(BattleCoreParam param)
    {
        CreateBattleResponse BattleResponse;
        var pveParam = param.pveParam;
        if (param.mode == BattleModeType.PveMode)
        {
            var pveMode = pveParam;
            Debug.Log("进入关卡：" + pveMode.CopyId);
            BattleResponse = await BattleApi.CreateBattle(BattleApi.BattleKind.Pve, pveMode.FormationIndex, new List<string> { pveMode.CopyId + "" }, false, pveMode.AssistUID);
        }
        else if (param.mode == BattleModeType.Gold)
        {
            var pveMode = pveParam;
            var stageRow = StaticData.StageTable.TryGet(pveMode.CopyId);
            var assistId = stageRow.Main == (int)EStageMainlineType.MainLine ? pveMode.AssistUID : "";
            Debug.Log("进入关卡：" + pveMode.CopyId);
            BattleResponse = await BattleApi.CreateBattle(BattleApi.BattleKind.Pve, pveMode.FormationIndex, new List<string> { pveMode.CopyId + "" }, false, assistId);
        }
        else if (param.mode == BattleModeType.Arena)
        {
            var arenaParam = param.modeParam as ArenaModeParam;
            var targetUid = arenaParam.targetUid;
            BattleResponse = await BattleApi.CreateBattle(BattleApi.BattleKind.Arena, null, new List<string> { targetUid }, false);
        }
        else if (param.mode == BattleModeType.Dreamscape)
        {
            var dreamEscape = param.modeParam as DreamscapeBattleParam;
            BattleResponse = await BattleApi.CreateBattle(BattleApi.BattleKind.Memory, null, new List<string> { dreamEscape.memMarkId + "" }, false, "", dreamEscape.heroid);
            // var memoryInfo = JsonUtil.JsonDataToObject<DreamscapeInfo>(BattleResponse.attach);
            // DreamscapeManager.UpdateInfo(memoryInfo);
        }
        else if (param.mode == BattleModeType.TowerNormal
                 || param.mode == BattleModeType.TowerFixed)
        {
            var pveMode = pveParam;
            Debug.Log("进入爬塔：" + pveMode.CopyId);
            BattleResponse = await BattleApi.CreateBattle(BattleApi.BattleKind.Tower, pveMode.FormationIndex, new List<string> { pveMode.CopyId + "" }, false);
        }
        else
        {
            var pveMode = pveParam;
            var stageRow = StaticData.StageTable.TryGet(pveMode.CopyId);
            var assistId = stageRow.Main == (int)EStageMainlineType.MainLine ? pveMode.AssistUID : "";
            Debug.Log("进入关卡：" + pveMode.CopyId);
            BattleResponse = await BattleApi.CreateBattle(BattleApi.BattleKind.Pve, pveMode.FormationIndex, new List<string> { $"{pveMode.CopyId}" }, false, assistId);
        }
        RefreshAssistInfo(BattleResponse, pveParam);
        return BattleResponse;
    }

    public static async Task GoBack()
    {
        // pzy:
        // 需要一种方式指定战斗结束后不要跳转任何界面
        // 因为调用方会自行处理
        // 补充：建议完全取消设置 from 的逻辑，改为对调用方进行回调
        if (from == "None"
            || from == null)
        {
            await UIEngine.Stuff.RemoveFromStack<PlotPage>();
            await UIEngine.Stuff.RemoveFromStack<BattlePage>();
            await UIEngine.Stuff.RemoveFromStack<BattleResultWinPage>();
            await UIEngine.Stuff.RemoveFromStack<BattleResultFailPage>();
            await UIEngine.Stuff.RemoveFromStack<ArenaBattleDetailPage>();
        }
        if (!GuideManagerV2.Stuff.IsExecutingForceGuide)
        {
            if (string.IsNullOrEmpty(from))
            {
                UiUtil.BackToMainGroupThenReplace<MainPage>();
            }
            else
            {
                await UIEngine.Stuff.ForwardOrBackToAsync(from);
                from = null;
            }
        }
        await Battle.Instance.DestroyBattleInstanceAsync();
    }

    public static void SetBackPage(string name)
    {
        from = name;
    }

    public static void RefreshAssistInfo(CreateBattleResponse battleResponse, PveModeParam pveModeParam)
    {
        if (battleResponse == null
            || pveModeParam == null
            || pveModeParam.CopyId == 0
            || battleResponse.attach == null)
        {
            return;
        }
        StageRow stageRow = StaticData.StageTable.TryGet(pveModeParam.CopyId);
        if (stageRow == null
            || stageRow.Main != (int)EStageMainlineType.MainLine)
        {
            return;
        }
        var parseAssist = JsonUtil.JsonDataToObject<SocializeAssistInfo>(battleResponse.attach);
        if (parseAssist != null)
        {
            SocializeManager.Stuff.AssistInfo = parseAssist;
        }
    }
}
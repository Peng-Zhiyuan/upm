using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using BattleSystem.ProjectCore;
using UnityEngine.SceneManagement;
using BattleEngine.Logic;
using BattleSystem.Core;
using CustomLitJson;

[BattleModeClass(BattleModeType.PveMode)]
public class PveMode : Mode
{
    public override string GetPageName()
    {
        return "BattlePage";
    }

    public override async Task BattleReady()
    {
        await this.battle.SpawnMonsters(true);
    }

    public async override Task BattleResult(bool isWin, List<BattleEngine.Logic.BattlePlayerRecord> myTeamRecrod, List<BattleEngine.Logic.BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        if (!BattleLogicManager.Instance.IsReport)
        {
            var arg = new JsonData() { ["kill"] = BattleLogicManager.Instance.BattleData.KillNum, ["time"] = Mathf.FloorToInt(BattleTimeManager.Instance.CurrentBattleTime) };
            var battleInfo = new BattleInfo
            {
                            win = BattleStateManager.Instance.GameResult == eBattleResult.Victory ? 1 : 2,
                            id = Battle.Instance.CopyId,
                            corps = Battle.Instance.FormationIndex, //TODO 编队 
                            data = arg
            };
            StageManager.Stuff.ReqStageResult(battleInfo);
        }
    }

    public override void SendZoneResult()
    {
        var remTime = BattleTimeManager.Instance.BattleTime - BattleTimeManager.Instance.CurrentBattleTime;
        SeaBattleManager.SetBattleResult(new SeaBattleBattleResult { win = BattleResultManager.Instance.IsBattleWin, leftTime = (int)remTime, passNum = 0, lossHp = 0 });
    }
}
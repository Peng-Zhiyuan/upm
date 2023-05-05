using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleSystem.ProjectCore;
using CustomLitJson;
using UnityEngine;

/// <summary>
/// 角色本参数
/// </summary>
public class RoleCopyModeParam { }

[BattleModeClass(BattleModeType.Role)]
public class RoleMode : PveMode
{
    public async override Task BattleResult(bool isWin, List<BattlePlayerRecord> myTeamRecrod, List<BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        var arg = new JsonData() { ["kill"] = BattleLogicManager.Instance.BattleData.KillNum, ["time"] = Mathf.FloorToInt(BattleTimeManager.Instance.CurrentBattleTime) };
        ///英雄副本永远是胜利
        var battleInfo = new BattleInfo { win = BattleStateManager.Instance.GameResult == eBattleResult.Victory ? 1 : 2, id = Battle.Instance.CopyId, corps = Battle.Instance.FormationIndex, data = arg };
        StageManager.Stuff.ReqStageResult(battleInfo);
    }
}
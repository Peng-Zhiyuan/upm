/* Created:Loki Date:2023-03-16*/

using System.Collections.Generic;
using BattleSystem.ProjectCore;
using System.Threading.Tasks;
using BattleEngine.Logic;
using CustomLitJson;
using UnityEngine;

[BattleModeClass(BattleModeType.TowerFixed)]
public class TowerFixedMode : FixedMode
{
    public override BattleModeType ModeType
    {
        get { return BattleModeType.TowerFixed; }
    }

    public async override Task BattleResult(bool isWin, List<BattlePlayerRecord> myTeamRecrod, List<BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        if (!BattleLogicManager.Instance.IsReport)
        {
            var arg = new JsonData() { ["kill"] = BattleLogicManager.Instance.BattleData.KillNum, ["time"] = Mathf.FloorToInt(BattleTimeManager.Instance.CurrentBattleTime) };
            TowerModeParam towerModeParam = (TowerModeParam)Battle.Instance.param.modeParam;
            PveModeParam pveModeParam = (PveModeParam)Battle.Instance.param.pveParam;
            var battleInfo = new BattleInfo
            {
                            win = isWin ? 1 : 2,
                            id = towerModeParam.TowerID,
                            corps = (int)pveModeParam.FormationIndex, //TODO 编队 
                            data = arg
            };
            StageManager.Stuff.ReqStageResult(battleInfo);
        }
    }

    public override List<MapWaveDataObject> GetWaveDataList()
    {
        var modeParam = (TowerModeParam)this.battle.param.modeParam;
        TowerRow towerRow = StaticData.TowerTable.TryGet(modeParam.TowerID);
        if (towerRow == null)
        {
            BattleLog.LogError("Cant find the tower");
            return base.GetWaveDataList();
        }
        var resultWaveData = base.GetWaveDataList();
        return ModeUtil.ReplaceMonsterInfo(resultWaveData, new List<int>(towerRow.mosterGroups));
    }
}
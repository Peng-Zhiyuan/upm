using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BattleEngine.Logic;
using BattleEngine.View;
using BattleSystem.ProjectCore;
using CustomLitJson;
using UnityEngine;
using Random = UnityEngine.Random;

[BattleModeClass(BattleModeType.Gold)]
public class GoldMode : PveMode
{
    public override async Task<bool> PreloadRes(BattleCoreParam param, Action<float> onProgress,List<int> otherHero = null)
    {
        await base.PreloadRes(param, onProgress);
        await BattleGoldDropManager.Stuff.PreLoadFXRes();
        return true;
    }

    public override async Task OnPreCreateBattleGameObjectAsync()
    {
        await base.OnPreCreateBattleGameObjectAsync();
    }

    public override async Task BattleReady()
    {
        await base.BattleReady();
        if (!Battle.Instance.IsFight)
            return;
        int copyID = GetCopyId();
        var stageRow = StaticData.StageTable.TryGet(copyID);
        if (stageRow == null)
        {
            Debug.LogError("获取不到指定copyId =>" + copyID);
            return;
        }
        DropRuleData ruleData = new DropRuleData();
        ruleData.FxStepTime = stageRow.GoldFxStep;
        ruleData.FxStepCount = stageRow.GoldFxStepCout;
        ruleData.HpToGoldKey = stageRow.HPChangeGoldKey;
        ruleData.HpToGoldValue = stageRow.HPChangeGoldValue;
        BattleGoldDropManager.Instance.InitDropActor(BattleLogicManager.Instance.BattleData, ruleData);
    }

    public override async Task OnWaveEnd()
    {
        await base.OnWaveEnd();
    }

    public override List<MapWaveDataObject> GetWaveDataList()
    {
        PveModeParam modeParam = battle.param.pveParam;
        var stageRow = StaticData.StageTable.TryGet(this.GetCopyId());
        if (stageRow == null)
        {
            Debug.LogError("获取不到指定copyId =>" + this.GetCopyId());
            return new List<MapWaveDataObject>();
        }
        var stageType = (EPveMapType)stageRow.mapeType;
        if (stageType.Equals(EPveMapType.RogueLike))
        {
            var mapGenerator = MapGenerateCore.Instance;
            return mapGenerator.MapAgent.MapWaveDataList;
        }
        var resultWaveData = new List<MapWaveDataObject>();
        // 这里需要修改为指定的怪物组
        foreach (var waveData in modeParam.WaveDataList)
        {
            resultWaveData.Add(new MapWaveDataObject()
                            {
                                            WaveId = waveData.WaveId,
                                            MainHeroList = waveData.MainHeroList,
                                            SubHeroList = waveData.SubHeroList,
                                            AssistData = waveData.AssistData,
                                            LeadData = waveData.LeadData,
                                            MonsterList = this.ReplaceMonsterId(waveData.MonsterList),
                            }
            );
        }
        return resultWaveData;
    }

    private List<MapWaveModelData> ReplaceMonsterId(List<MapWaveModelData> monsterModel)
    {
        var result = new List<MapWaveModelData>();
        var modeParam = (GoldCopyModeParam)this.battle.param.modeParam;
        if (modeParam.MonsterGroup == null
            || modeParam.MonsterGroup.Count <= 0) return monsterModel;
        var randomMonsterGroupId = modeParam.MonsterGroup[Random.Range(0, modeParam.MonsterGroup.Count)];
        var monsterIds = new List<int>();
        var monsterGroupRow = StaticData.PortMonsterTable.TryGet(randomMonsterGroupId);
        if (monsterGroupRow.Boss > 0)
        {
            monsterIds.Add(monsterGroupRow.Boss);
        }
        if (monsterGroupRow.Monsters != null
            && monsterGroupRow.Monsters.Length > 0)
        {
            monsterIds.AddRange(monsterGroupRow.Monsters);
        }
        for (var i = 0; i < monsterModel.Count; i++)
        {
            var monster = monsterModel[i];
            if (i > monsterIds.Count - 1) continue;
            result.Add(new MapWaveModelData()
                            {
                                            Id = monsterIds[i],
                                            Dir = monster.Dir,
                                            Index = monster.Index,
                                            Pos = monster.Pos,
                                            RotationY = monster.RotationY
                            }
            );
        }
        return result;
    }

    public async override Task BattleResult(bool isWin, List<BattlePlayerRecord> myTeamRecrod, List<BattlePlayerRecord> enemyTeamRecord, float duration)
    {
        bool isDouble = false;
        if (battle.param != null
            && battle.param.modeParam != null)
        {
            GoldCopyModeParam param = battle.param.modeParam as GoldCopyModeParam;
            isDouble = param.IsDouble;
        }
        var arg = new JsonData() { ["kill"] = BattleLogicManager.Instance.BattleData.KillNum, ["time"] = Mathf.FloorToInt(BattleTimeManager.Instance.CurrentBattleTime), ["coins"] = BattleGoldDropManager.Stuff.GetGoldNum() * (isDouble ? 2 : 1), };
        ///金币副本永远是胜利
        var battleInfo = new BattleInfo
        {
                        win = 1,
                        id = Battle.Instance.CopyId,
                        corps = Battle.Instance.FormationIndex, //TODO 编队 
                        data = arg
        };
        StageManager.Stuff.ReqStageResult(battleInfo);
    }
}
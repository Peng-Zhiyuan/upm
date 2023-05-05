/* Created:Loki Date:2023-02-09*/

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using BattleSystem.ProjectCore;
    using UnityEngine;

    public class BattleServerUtil
    {
        public static async Task<BattleServerData> CreateBattleServerData(BattleCoreParam battleCoreParam, EFormationIndex formationIndex, List<BattleHero> battleHeroLst = null)
        {
            BattleServerData serverData = new BattleServerData();
            serverData.BattleSeed = Clock.Now.Millisecond;
            serverData.BattleKey = "TestBattleServer";
            serverData.pathData = await LoadPathData(battleCoreParam.pveParam);
            List<MapWaveDataObject> MapWavList = await GetMapWavDataObject(battleCoreParam.pveParam);
            if (battleCoreParam.mode == BattleModeType.TowerFixed
                || battleCoreParam.mode == BattleModeType.TowerNormal)
            {
                var modeParam = (TowerModeParam)battleCoreParam.modeParam;
                TowerRow towerRow = StaticData.TowerTable.TryGet(modeParam.TowerID);
                if (towerRow != null)
                {
                    MapWavList = ModeUtil.ReplaceMonsterInfo(MapWavList, new List<int>(towerRow.mosterGroups));
                }
                else
                {
                    BattleLog.LogError("Cant find the tower " + modeParam.TowerID);
                }
            }
            Dictionary<int, List<BattleHero>> mainHeroDic = new Dictionary<int, List<BattleHero>>();
            if (battleHeroLst != null)
            {
                EFormationIndex teamIndex = formationIndex;
                if (teamIndex == EFormationIndex.None)
                {
                    teamIndex = FormationUtil.GetDefaultFormationIndex();
                }
                var formationInfoData = Database.Stuff.FormationDatabase.GetFormationInfo(teamIndex);
                var heros = FormationUtil.ParseHeroIdList(formationInfoData.heroes);
                List<List<BattleHero>> parseHeroList = new List<List<BattleHero>>();
                for (int i = 0; i < heros.Count; i++)
                {
                    List<BattleHero> tempList = new List<BattleHero>();
                    for (int j = 0; j < heros[i].Count; j++)
                    {
                        if (heros[i][j] != 0)
                        {
                            BattleHero hero = battleHeroLst.Find(it => it.id == heros[i][j]);
                            if (hero != null)
                            {
                                tempList.Add(hero);
                            }
                        }
                    }
                    parseHeroList.Add(tempList);
                }
                for (int i = 0; i < parseHeroList.Count; i++)
                {
                    if (parseHeroList[i].Count == 0
                        || parseHeroList[i][0] == null)
                    {
                        continue;
                    }
                    mainHeroDic[parseHeroList[i][0].id] = parseHeroList[i];
                }
            }
            else
            {
                mainHeroDic = GetMainFormationHero(formationIndex);
            }
            var data = mainHeroDic.GetEnumerator();
            while (data.MoveNext())
            {
                for (int i = 0; i < data.Current.Value.Count; i++)
                {
                    if (i == 0)
                    {
                        serverData.atkHeroLst.Add(data.Current.Value[i]);
                        serverData.atkSubHeroDic.Add(data.Current.Value[i].id, new List<BattleHero>());
                    }
                    else
                    {
                        serverData.atkSubHeroDic[data.Current.Value[0].id].Add(data.Current.Value[i]);
                    }
                }
            }
            int waveIndex = 0;
            for (int m = 0; m < MapWavList.Count; m++)
            {
                waveIndex = m + 1;
                serverData.atkHeroIFFDic[waveIndex] = GetWaveIFFLst(MapWavList[m].MainHeroList, true);
                serverData.defHeroIFFDic[waveIndex] = GetWaveIFFLst(MapWavList[m].MonsterList, false);
                serverData.defHeroDic[waveIndex] = GetMonsterBattleHero(MapWavList[m].MonsterList);
            }
            serverData.checkData = GetResultCheckCondition(battleCoreParam.pveParam.CopyId);
            int tempTime = (int)BattleUtil.GetGlobalK(GlobalK.NORMAL_BATTLETIME_35);
            var stageinfo = StaticData.StageTable.TryGet(Battle.Instance.CopyId);
            if (stageinfo != null)
            {
                tempTime = stageinfo.endTime;
            }
            serverData.FinishFrame = Mathf.FloorToInt(tempTime * BattleLogicDefine.LogicSecFrame);
            return serverData;
        }

        public static Dictionary<int, List<BattleHero>> GetMainFormationHero(EFormationIndex formationIndex, int mainHeroID = -1)
        {
            Dictionary<int, List<BattleHero>> dic = new Dictionary<int, List<BattleHero>>();
            EFormationIndex teamIndex = formationIndex;
            if (teamIndex == EFormationIndex.None)
            {
                teamIndex = FormationUtil.GetDefaultFormationIndex();
            }
            var formationInfoData = Database.Stuff.FormationDatabase.GetFormationInfo(teamIndex);
            var heros = FormationUtil.ParseHeroIdList(formationInfoData.heroes);
            for (int i = 0; i < heros.Count; i++)
            {
                for (int j = 0; j < heros[i].Count; j++)
                {
                    if (heros[i][j] == 0)
                    {
                        continue;
                    }
                    HeroInfo heroInfo = HeroManager.Instance.GetHeroInfo(heros[i][j]);
                    BattleHero hero = BattleUtil.HeroInfoToBattleHero(heroInfo, HeroCircuitManager.GetSkills(heroInfo.HeroId));
                    if (j == 0)
                    {
                        dic[hero.id] = new List<BattleHero>() { hero };
                    }
                    else
                    {
                        int mainHeroConfigID = heros[i][0];
                        dic[mainHeroConfigID].Add(hero);
                    }
                }
            }
            return dic;
        }

        public static List<BattleHero> GetMonsterBattleHero(List<MapWaveModelData> _MapWavList)
        {
            List<BattleHero> monsterLst = new List<BattleHero>();
            for (int i = 0; i < _MapWavList.Count; i++)
            {
                BattleItemInfo iteminfo = BattleUtil.CreateActorItemInfo(_MapWavList[i].Id, 1, true);
                BattleHero monster = new BattleHero();
                monster.oid = iteminfo._id;
                monster.id = iteminfo.id;
                monster.lv = iteminfo.lv;
                monster.Attr = iteminfo.att;
                monster.Skill = iteminfo.skillsDic;
                monsterLst.Add(monster);
            }
            return monsterLst;
        }

        public static List<ActorIFF> GetWaveIFFLst(List<MapWaveModelData> MapDataLst, bool isAtk)
        {
            List<ActorIFF> waveIFFLst = new List<ActorIFF>();
            for (int i = 0; i < MapDataLst.Count; i++)
            {
                ActorIFF iff = new ActorIFF();
                iff.PosIndex = i;
                iff.pos = MapDataLst[i].Pos;
                var dir = Quaternion.AngleAxis(StageBattleUtil.GetMapRotationY(MapDataLst[i]), Vector3.up) * Vector3.forward;
                iff.dir = dir;
                iff.CampID = isAtk ? BattleConst.ATKCampID : BattleConst.DEFCampID;
                iff.TeamID = isAtk ? BattleConst.ATKTeamID : BattleConst.DEFTeamID;
                waveIFFLst.Add(iff);
            }
            return waveIFFLst;
        }

        public static async Task<List<MapWaveDataObject>> GetMapWavDataObject(PveModeParam pveModeParam)
        {
            StageRow stageRow = StaticData.StageTable.TryGet(pveModeParam.CopyId);
            if (stageRow == null)
            {
                BattleLog.LogError("获取不到指定copyId =>" + pveModeParam.CopyId);
                return new List<MapWaveDataObject>();
            }
            var stageType = (EPveMapType)stageRow.mapeType;
            if (stageType.Equals(EPveMapType.RogueLike))
            {
                var mapGenerator = MapGenerateCore.Instance;
                return mapGenerator.MapAgent.MapWaveDataList;
            }
            if (stageType.Equals(EPveMapType.MemoryMode))
            {
                return ModeUtil.ResetRandomMonster(pveModeParam);
            }
            return pveModeParam.WaveDataList;
        }

        public static List<BattleResultCheckData> GetResultCheckCondition(int stageID)
        {
            List<BattleResultCheckData> checkData = new List<BattleResultCheckData>();
            var stageinfo = StaticData.StageTable.TryGet(stageID);
            if (stageinfo != null)
            {
                if (!string.IsNullOrEmpty(stageinfo.Condition))
                {
                    string[] strs = stageinfo.Condition.Split(',');
                    foreach (var VARIABLE in strs)
                    {
                        string[] infos = VARIABLE.Split(':');
                        int type = Int32.Parse(infos[0]);
                        if (type == (int)BATTLE_RESULT_TYPE.DEFEND_TARGET)
                        {
                            checkData.Add(new BattleResultCheckData() { resultType = (BATTLE_RESULT_TYPE)type, Param = Int32.Parse(infos[1]) });
                        }
                        else if (type == (int)BATTLE_RESULT_TYPE.KILL_TARGET)
                        {
                            checkData.Add(new BattleResultCheckData() { resultType = (BATTLE_RESULT_TYPE)type, Param = Int32.Parse(infos[1]) });
                        }
                        else if (type == (int)BATTLE_RESULT_TYPE.DEFEND_TIME)
                        {
                            checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.DEFEND_TIME, Param = Int32.Parse(infos[1]) * 1000 });
                        }
                        else
                        {
                            checkData.Add(new BattleResultCheckData() { resultType = (BATTLE_RESULT_TYPE)(Int32.Parse((infos[0]))), Param = Int32.Parse(infos[1]) });
                        }
                    }
                }
                checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.TEAM_DOOM, Param = BattleLogicManager.Instance.BattleData });
                checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.TIMEEND, Param = stageinfo.endTime * 1000 });
                checkData.Add(new BattleResultCheckData() { resultType = BATTLE_RESULT_TYPE.OUTCHECKRESULT, });
            }
            return checkData;
        }

        public static async Task<BattlePathData> LoadPathData(PveModeParam pveParam)
        {
            var stageRow = StaticData.StageTable.TryGet(pveParam.CopyId);
            var mapType = (EPveMapType)stageRow.mapeType;
            var sceneId = ((EPveMapType)stageRow.mapeType).Equals(EPveMapType.Pve) || ((EPveMapType)stageRow.mapeType).Equals(EPveMapType.MemoryMode) ? stageRow.mapeId : stageRow.mapId;
            if (mapType == EPveMapType.RogueLike
                || mapType == EPveMapType.PveRogueLike)
            {
                PathFindingManager.Instance.CreateRogueLikeMap(mapType, pveParam.MapParts, pveParam.EnvironmentParts, pveParam.MapEffects, pveParam.SceneId, pveParam.FormationIndex);
            }
            var graphInfoList = PathFindingManager.Instance.GetPathfindingGraph(mapType, sceneId);
            // 获得寻路网格数据
            BattlePathData pathData = new BattlePathData();
            foreach (var graphInfo in graphInfoList)
            {
                var address = graphInfo.graphAddress;
                var offset = graphInfo.offset;
                var rotation = graphInfo.rotation;
                var graphTextAsset = await BucketManager.Stuff.Battle.GetOrAquireAsync<TextAsset>(address, true);
                if (graphTextAsset == null)
                {
                    throw new Exception($"[ProjectBattleCore] graph address '{address}' not found");
                }
                var data = graphTextAsset.bytes;
                pathData.graphAssetList.Add(data);
                pathData.offsetList.Add(offset);
                pathData.rotationList.Add(rotation);
            }
            return pathData;
        }

        public static void DebuLogBattleInfo()
        {
            BattleLog.LogWarning("[SERVER_BATTLE] Finish Frame : " + BattleLogicManager.Instance.CurrentFrame);
            BattleLog.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~Attack~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            var atkActorLst = BattleLogicManager.Instance.BattleData.atkActorLst;
            for (int i = 0; i < atkActorLst.Count; i++)
            {
                BattleLog.LogWarning("[SERVER_BATTLE] " + atkActorLst[i].ConfigID + "  MaxHP : " + atkActorLst[i].CurrentHealth.MaxValue + " currentHp ：" + atkActorLst[i].CurrentHealth.Value);
            }
            BattleLog.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~Defence~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            var defActorLst = BattleLogicManager.Instance.BattleData.defActorLst;
            for (int i = 0; i < defActorLst.Count; i++)
            {
                BattleLog.LogWarning("[SERVER_BATTLE] " + defActorLst[i].ConfigID + "  MaxHP : " + defActorLst[i].CurrentHealth.MaxValue + " currentHp ：" + defActorLst[i].CurrentHealth.Value);
            }
        }
    }
}
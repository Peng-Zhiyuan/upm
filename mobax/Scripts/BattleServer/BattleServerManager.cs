using System;
using System.Threading.Tasks;
using BattleEngine.Manager;
using BattleSystem.ProjectCore;

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public class BattleServerManager : Singleton<BattleServerManager>
    {
        private bool isInitManager = false;
        private CoreEngine ServerCoreEngine;

        private int waveIndex = 1;
        private Dictionary<int, List<CombatActorEntity>> waveMonsterDic = new Dictionary<int, List<CombatActorEntity>>();
        private BattleServerData serverData;
        private BattleCoreParam battleCoreParam;

        public void InitManager(BattleServerData data, BattleCoreParam _battleCoreParam)
        {
            serverData = data;
            battleCoreParam = _battleCoreParam;
            waveIndex = 1;
            BattleLogicManager.Instance.InitLogicManager(data.BattleKey, data.BattleSeed, true);
            BattleLogicManager.Instance.FinishFrame = data.FinishFrame;
            BattleLogicManager.Instance.IsDefAddSkillCD = true;
            InitBattleModel();
            CreateBattleData(data);
            CreateAStarPath(data.pathData);
            CreateBattleResultData(data.checkData);
            isInitManager = true;
        }

        public void QuitServerManager()
        {
            if (PathFindingManager.HasInstance()
                && PathFindingManager.Instance._AstarPathCore != null)
            {
                CoreEngine.Destory(PathFindingManager.Instance._AstarPathCore.coreObject);
            }
            PathFindingManager.DestroyInstance();
            BattleLogicManager.DestroyInstance();
            Entity.Destroy(MasterEntity.Instance);
            MasterEntity.Destroy();
            BattleEventManager.DestroyInstance();
            BattleTimeManager.DestroyInstance();
            SkillPassiveAbilityManager.DestroyInstance();
            TimerMgr.Instance.RemoveType(TimerType.Battle);
            LogicFrameTimerMgr.Instance.RemoveAll();
            Time.timeScale = 1.0f;
            BattleResultManager.Instance.ClearData();
            EventManager.Instance.ClearAllListener();
            DestroyInstance();
        }

        public async Task<BattleServerResultData> ExecuteBattleVerify()
        {
            if (!isInitManager)
            {
                return null;
            }
            try
            {
                for (int i = 0; i < BattleLogicManager.Instance.FinishFrame; i++)
                {
                    LogicState result = BattleLogicManager.Instance.Update(BattleLogicDefine.LogicSecTime);
                    if (result == LogicState.Playing)
                    {
                        MasterEntity.Instance.LogicUpdate(BattleLogicDefine.LogicSecTime);
                    }
                    else if (result == LogicState.End)
                    {
                        if (RefreshWaveMonsterData(waveIndex + 1))
                        {
                            waveIndex += 1;
                            BattleLogicManager.Instance.IsBattleEnd = false;
                            continue;
                        }
                        break;
                    }
                }
                BattleLog.LogWarning("BattleEnd Is Win " + BattleResultManager.Instance.IsBattleWin);
                isInitManager = false;
                BattleServerResultData resultData = new BattleServerResultData();
                resultData.isWin = BattleResultManager.Instance.IsBattleWin;
                resultData.battleServerData = this.serverData;
                resultData.battleCoreParam = battleCoreParam;
                return resultData;
            }
            catch (Exception e)
            {
                BattleLog.LogWarning(e.Message);
                return null;
            }
        }

        /// <summary>
        /// 初始化模块
        /// </summary>
        private void InitBattleModel()
        {
            ServerCoreEngine = new CoreEngine(this);
            AIManager.Initialize();
            MasterEntity.Create();
            Entity.Create<CombatContext>();
            BattleEventManager.Instance.ClearEvent();
        }

        private void CreateBattleData(BattleServerData data)
        {
            for (int i = 0; i < data.atkHeroLst.Count; i++)
            {
                BattleItemInfo itemInfo = BattleUtil.CreateActorItemInfo(data.atkHeroLst[i]);
                CombatActorEntity actorEntity = BattleUtil.CreateCombatActorUnit(itemInfo, data.atkHeroIFFDic[waveIndex][i]);
                BattleLogicManager.Instance.BattleData.AddActorData(actorEntity);
                if (data.atkSubHeroDic.ContainsKey(itemInfo.ConfigID))
                {
                    List<BattleHero> actorSubLst = data.atkSubHeroDic[itemInfo.ConfigID];
                    for (int j = 0; j < actorSubLst.Count; j++)
                    {
                        BattleItemInfo subItemInfo = BattleUtil.CreateActorItemInfo(actorSubLst[j]);
                        CombatActorEntity subActorEntity = BattleUtil.CreateCombatActorUnit(subItemInfo, data.atkHeroIFFDic[waveIndex][i]);
                        subActorEntity.SetLifeState(ACTOR_LIFE_STATE.Assist);
                        subActorEntity.PosIndex = BattleConst.SSPAssistPosIndexStart;
                        actorEntity.LinkerUIDLst.Add(subActorEntity.UID);
                        BattleLogicManager.Instance.BattleData.AddActorData(actorEntity);
                    }
                }
            }
            var monsterData = data.defHeroDic.GetEnumerator();
            while (monsterData.MoveNext())
            {
                if (!waveMonsterDic.ContainsKey(monsterData.Current.Key))
                {
                    waveMonsterDic[monsterData.Current.Key] = new List<CombatActorEntity>();
                }
                List<ActorIFF> defHeroIFFLst = data.defHeroIFFDic[monsterData.Current.Key];
                for (int i = 0; i < monsterData.Current.Value.Count; i++)
                {
                    BattleItemInfo itemInfo = BattleUtil.CreateActorItemInfo(monsterData.Current.Value[i]);
                    CombatActorEntity actorEntity = BattleUtil.CreateCombatActorUnit(itemInfo, defHeroIFFLst[i]);
                    waveMonsterDic[monsterData.Current.Key].Add(actorEntity);
                }
            }
            RefreshWaveMonsterData(waveIndex);
        }

        private void CreateAStarPath(BattlePathData data)
        {
            var astarManager = PathFindingManager.Instance.AstarPathCore;
            astarManager.LoadGraphAdditional(data.graphAssetList, data.offsetList, data.rotationList);
        }

        private void CreateBattleResultData(List<BattleResultCheckData> checkLst)
        {
            BattleResultManager.Instance.CreateResultData(checkLst);
        }

        private bool RefreshWaveMonsterData(int waveIndex)
        {
            if (waveMonsterDic.ContainsKey(waveIndex)
                && serverData.atkHeroIFFDic.ContainsKey(this.waveIndex))
            {
                List<ActorIFF> actorIffLst = serverData.atkHeroIFFDic[this.waveIndex];
                for (int i = 0; i < actorIffLst.Count; i++)
                {
                    CombatActorEntity actorEntity = BattleLogicManager.Instance.BattleData.atkActorLst[i];
                    if (actorEntity.IsCantSelect)
                    {
                        continue;
                    }
                    actorEntity.Born(actorIffLst[i].pos, actorIffLst[i].dir, actorEntity.battleItemInfo.scale);
                }
                var lst = waveMonsterDic[waveIndex];
                for (int i = 0; i < lst.Count; i++)
                {
                    BattleLogicManager.Instance.BattleData.AddActorData(lst[i]);
                }
                return true;
            }
            return false;
        }
    }
}
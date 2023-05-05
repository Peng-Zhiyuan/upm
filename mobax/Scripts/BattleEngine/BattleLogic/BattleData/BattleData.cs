namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    /// <summary>
    /// 战斗数据上下文
    /// </summary>
    public sealed class BattleData
    {
        public string battleKey;

        private Dictionary<int, List<CombatActorEntity>> waveCombatActorDic = new Dictionary<int, List<CombatActorEntity>>();

        /// <summary>
        /// UID-演员
        /// </summary>
        public Dictionary<string, CombatActorEntity> allActorDic = new Dictionary<string, CombatActorEntity>();
        public List<CombatActorEntity> allActorLst = new List<CombatActorEntity>();
        /// <summary>
        /// 队伍索引ID-演员
        /// </summary>
        public Dictionary<int, List<CombatActorEntity>> atkActorDic = new Dictionary<int, List<CombatActorEntity>>();
        /// <summary>
        /// 队伍索引ID-演员
        /// </summary>
        public Dictionary<int, List<CombatActorEntity>> defActorDic = new Dictionary<int, List<CombatActorEntity>>();
        /// <summary>
        /// 攻击防御所有演员信息 
        /// </summary>
        public List<CombatActorEntity> atkActorLst = new List<CombatActorEntity>();
        public List<CombatActorEntity> defActorLst = new List<CombatActorEntity>();

        public List<BulletEntity> bulletList = new List<BulletEntity>();
        public int finishFrame; //完成帧

        public void Init()
        {
            ActorDistanceMgr = new BattleDistanceManager();
            OccupyPositionMgr = new BattleOccupyPosManager();
            finishFrame = 1000000;
            allActorDic.Clear();
            allActorLst.Clear();
            atkActorDic.Clear();
            defActorDic.Clear();
            atkActorLst.Clear();
            defActorLst.Clear();
            bulletList.Clear();
            waveCombatActorDic.Clear();
        }

        public void ClearData()
        {
            ActorDistanceMgr = null;
            OccupyPositionMgr = null;
            allActorDic.Clear();
            allActorLst.Clear();
            atkActorDic.Clear();
            defActorDic.Clear();
            atkActorLst.Clear();
            defActorLst.Clear();
            bulletList.Clear();
            waveCombatActorDic.Clear();
        }

        public void InitActorData(List<CombatActorEntity> teamEntityLst)
        {
            for (int i = 0; i < teamEntityLst.Count; i++)
            {
                AddActorData(teamEntityLst[i]);
            }
        }

        public void AddSummonActorData(CombatActorEntity summonActorEntity)
        {
            AddActorData(summonActorEntity);
        }
        
        public void AddActorData(CombatActorEntity actorEntity)
        {
            if (actorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            actorEntity.MaxOTNum.AddFinalAddModifier(new FloatModifier() { Value = 0 });
            if (!allActorDic.ContainsKey(actorEntity.UID))
            {
                allActorDic.Add(actorEntity.UID, actorEntity);
                allActorLst.Add(actorEntity);
                if (!waveCombatActorDic.ContainsKey(Battle.Instance.Wave))
                {
                    waveCombatActorDic.Add(Battle.Instance.Wave, new List<CombatActorEntity>() { actorEntity });
                }
                else
                {
                    waveCombatActorDic[Battle.Instance.Wave].Add(actorEntity);
                }
            }
            if (actorEntity.isAtker)
            {
                if (atkActorDic.ContainsKey(actorEntity.TeamKey))
                {
                    atkActorDic[actorEntity.TeamKey].Add(actorEntity);
                }
                else
                {
                    atkActorDic.Add(actorEntity.TeamKey, new List<CombatActorEntity>() { actorEntity });
                }
                atkActorLst.Add(actorEntity);
                for (int i = 0; i < defActorLst.Count; i++)
                {
                    actorEntity.InitPushAttackList(defActorLst[i]);
                    actorEntity.AddOTNum(defActorLst[i].UID, defActorLst[i].InitOTNum);
                    defActorLst[i].InitPushAttackList(actorEntity);
                    defActorLst[i].AddOTNum(actorEntity.UID, actorEntity.InitOTNum);
                }
            }
            else
            {
                if (defActorDic.ContainsKey(actorEntity.TeamKey))
                {
                    defActorDic[actorEntity.TeamKey].Add(actorEntity);
                }
                else
                {
                    defActorDic.Add(actorEntity.TeamKey, new List<CombatActorEntity>() { actorEntity });
                }
                defActorLst.Add(actorEntity);
                for (int i = 0; i < atkActorLst.Count; i++)
                {
                    actorEntity.InitPushAttackList(atkActorLst[i]);
                    actorEntity.AddOTNum(atkActorLst[i].UID, atkActorLst[i].InitOTNum);
                    atkActorLst[i].InitPushAttackList(actorEntity);
                    atkActorLst[i].AddOTNum(actorEntity.UID, actorEntity.InitOTNum);
                }
            }
        }

        public CombatActorEntity GetActorEntity(string uid)
        {
            if (allActorDic.ContainsKey(uid))
            {
                return allActorDic[uid];
            }
            return null;
        }

        public void ResetInitOTNum()
        {
            var data = allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.CurrentHealth.Value <= 0)
                {
                    continue;
                }
                data.Current.Value.MaxOTNum.AddFinalAddModifier(new FloatModifier() { Value = data.Current.Value.MaxOTNum.Value * -1 });
                InitOTNum(data.Current.Value);
            }
        }

        public void InitOTNum(CombatActorEntity actorEntity)
        {
            var data = allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.CurrentHealth.Value <= 0)
                {
                    continue;
                }
                if (data.Current.Value.isAtker == actorEntity.isAtker)
                {
                    continue;
                }
                actorEntity.InitPushAttackList(data.Current.Value);
                actorEntity.AddOTNum(data.Current.Value.UID, data.Current.Value.InitOTNum);
            }
        }

        /// <summary>
        /// 获取队伍演员信息
        /// </summary>
        /// <param name="TeamKey">队伍唯一索引ID</param>
        /// <returns></returns>
        public List<CombatActorEntity> GetTeamList(int TeamKey)
        {
            if (atkActorDic.ContainsKey(TeamKey))
            {
                return atkActorDic[TeamKey];
            }
            else if (defActorDic.ContainsKey(TeamKey))
            {
                return defActorDic[TeamKey];
            }
            return null;
        }

        public List<CombatActorEntity> GetEnemyLst(CombatActorEntity actor)
        {
            if (actor.targetTeamKey > 0)
            {
                if (actor.targetTeamKey < (BattleConst.DEFCampID * 1000)
                    && atkActorDic.ContainsKey(actor.targetTeamKey))
                {
                    return atkActorDic[actor.targetTeamKey];
                }
                else if (actor.targetTeamKey >= (BattleConst.DEFCampID * 1000)
                         && defActorDic.ContainsKey(actor.targetTeamKey))
                {
                    return defActorDic[actor.targetTeamKey];
                }
            }
            return actor.isAtker ? defActorLst : atkActorLst;
        }

        public List<BattlePlayerRecord> GetAtkRecordLst()
        {
            var ret = new List<BattlePlayerRecord>();
            for (int i = 0; i < atkActorLst.Count; i++)
            {
                if (atkActorLst[i].PosIndex == BattleConst.PlayerPosIndex
                    || atkActorLst[i].PosIndex == BattleConst.FriendPosIndex
                    || atkActorLst[i].PosIndex > BattleConst.SummonPosIndexStart)
                {
                    continue;
                }
                ret.Add(atkActorLst[i].battleItemInfo.battlePlayerRecord);
            }
            return ret;
        }

        public List<BattlePlayerRecord> GetDefRecordLst()
        {
            var ret = new List<BattlePlayerRecord>();
            for (int i = 0; i < defActorLst.Count; i++)
            {
                if (defActorLst[i].CurrentLifeState == ACTOR_LIFE_STATE.LookAt)
                {
                    continue;
                }
                ret.Add(defActorLst[i].battleItemInfo.battlePlayerRecord);
            }
            return ret;
        }

        public void ResetActorData()
        {
            var data = allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.CurrentHealth.Value <= 0)
                {
                    continue;
                }
                data.Current.Value.ResetInfoData();
            }
        }

        public bool HasEnemy(bool atk)
        {
            if (atk)
            {
                var data = defActorDic.GetEnumerator();
                while (data.MoveNext())
                {
                    for (int i = 0; i < data.Current.Value.Count; i++)
                    {
                        if (data.Current.Value[i].CurrentHealth.Value > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                var data = atkActorDic.GetEnumerator();
                while (data.MoveNext())
                {
                    for (int i = 0; i < data.Current.Value.Count; i++)
                    {
                        if (data.Current.Value[i].CurrentHealth.Value > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public int KillNum
        {
            get
            {
                int killNum = 0;
                for (int i = 0; i < defActorLst.Count; i++)
                {
                    if (defActorLst[i].IsDead)
                    {
                        killNum += 1;
                    }
                }
                return killNum;
            }
        }

#region 战斗人物距离运算
        public BattleDistanceManager ActorDistanceMgr;

        public void ClearDistanceData()
        {
            ActorDistanceMgr.ClearData();
        }

        public float GetActorDistance(CombatActorEntity selfActor, CombatActorEntity targetActor)
        {
            return ActorDistanceMgr.CalActorDistance(selfActor, targetActor);
        }
#endregion

#region 战斗坑位管理
        public BattleOccupyPosManager OccupyPositionMgr;

        public void ClearOccupyData()
        {
            OccupyPositionMgr.ClearData();
        }

        public bool IsCalActorOccupy(CombatActorEntity actorEntity)
        {
            if (OccupyPositionMgr.ExitOccupyPosition(actorEntity))
            {
                return true;
            }
            return false;
        }

        public void PushActorOccupyVector(CombatActorEntity actorEntity)
        {
            OccupyPositionMgr.PushOccupyPosition(actorEntity);
        }
#endregion
    }
}
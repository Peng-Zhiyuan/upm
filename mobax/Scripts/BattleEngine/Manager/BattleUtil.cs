namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using System;
    using UnityEngine;

    public static partial class BattleUtil
    {
        public static float GetGlobalK(GlobalK type)
        {
            string str = StaticData.CommonTable.TryGet((int)type).Value;
            return Convert.ToSingle(str);
        }

        /// <summary>
        /// 计算伤害
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="defender"></param>
        /// <param name="skillID"></param>
        public static int DoDamage(CombatActorEntity caster, CombatActorEntity defender, BehitData tmp_data, SkillRow skillRow, float percent = 1f, bool isHitAnim = false, int hitIndex = 1)
        {
            if (defender.IsDead)
                return 0;
            tmp_data.casterId = caster.UID;
            tmp_data.defendId = defender.UID;
            tmp_data.skillRow = skillRow;
            tmp_data.isHurtAnim = isHitAnim;
            tmp_data.HitIndex = hitIndex;
            Formula.CacDamage(tmp_data, caster, defender, skillRow, 1);
            if (percent < 1f)
            {
                tmp_data.damage = Mathf.FloorToInt(tmp_data.damage * percent);
            }
            return tmp_data.damage;
        }

        /// <summary>
        /// 获取当前目标
        /// 技能->集火->普攻
        /// 治疗单独走目标
        /// </summary>
        /// <param name="battleData"></param>
        /// <param name="actorEntity"></param>
        public static CombatActorEntity GetTargetActorEntity(BattleData battleData, CombatActorEntity actorEntity, bool isNeedRefresh = false)
        {
            if (actorEntity.skillTargetKeys.Count > 0)
            {
                CombatActorEntity inputTarget = battleData.allActorDic[actorEntity.skillTargetKeys[0]];
                if (inputTarget.CurrentHealth.Value > 0
                    && !inputTarget.IsCantSelect)
                {
                    return inputTarget;
                }
            }
            if (!string.IsNullOrEmpty(actorEntity.ATKFoucusUID))
            {
                CombatActorEntity inputTarget = battleData.allActorDic[actorEntity.targetKey];
                if (inputTarget.CurrentHealth.Value > 0
                    && !inputTarget.IsCantSelect)
                {
                    return inputTarget;
                }
            }
            if (actorEntity.IsDefFocus)
            {
                if (string.IsNullOrEmpty(actorEntity.DefendTargetUID))
                {
                    BattleLog.LogError("Defend Focus cant find defend UID");
                }
                else
                {
                    CombatActorEntity defendActorEntity = battleData.GetActorEntity(actorEntity.DefendTargetUID);
                    CombatActorEntity inputTarget = GetDefendActorToTarget(battleData, defendActorEntity);
                    if (inputTarget != null)
                    {
                        return inputTarget;
                    }
                }
            }
            CombatActorEntity targetActor = actorEntity.GetAttackTargetEntity();
            if (targetActor == null
                && !string.IsNullOrEmpty(actorEntity.targetKey)
                && !isNeedRefresh)
            {
                targetActor = battleData.allActorDic[actorEntity.targetKey];
                if (targetActor.IsCantSelect)
                    targetActor = null;
            }
            return targetActor;
        }

        public static CombatActorEntity GetDefendActorToTarget(BattleData battleData, CombatActorEntity defendActorEntity)
        {
            List<CombatActorEntity> enemyList = battleData.GetEnemyLst(defendActorEntity);
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (enemyList[i].IsCantSelect
                    || !enemyList[i].targetKey.Equals(defendActorEntity.UID))
                {
                    continue;
                }
                return enemyList[i];
            }
            return null;
        }

        public static bool IsCanExecuteSPSkill(CombatActorEntity actorEntity)
        {
            if (actorEntity.IsCantSelect
                || actorEntity.IsWaitSPSkillState
                || actorEntity.SPSKL == null
                || actorEntity.CurrentMp.Value < actorEntity.CurrentMp.MaxValue
                || BattleControlUtil.IsForbidAttack(actorEntity, SKILL_TYPE.SPSKL))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取当前技能对象
        /// </summary>
        /// <param name="battleData"></param>
        /// <param name="actorEntity"></param>
        /// <param name="isNeedRefresh"></param>
        /// <returns></returns>
        public static List<CombatActorEntity> GetSkillTargetsActorEntity(BattleData battleData, CombatActorEntity actorEntity, bool isNeedRefresh = false)
        {
            List<CombatActorEntity> lst = new List<CombatActorEntity>();
            for (int i = 0; i < actorEntity.skillTargetKeys.Count; i++)
            {
                lst.Add(battleData.allActorDic[actorEntity.skillTargetKeys[i]]);
            }
            return lst;
        }

        /// <summary>
        /// 获取技能所影响的对象信息
        /// </summary>
        /// <param name="actorEntity"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static List<CombatActorEntity> GetSkillEffectTargets(CombatActorEntity actorEntity, SKILL_AFFECT_TARGET_TYPE targetType)
        {
            List<CombatActorEntity> targetActors = new List<CombatActorEntity>();
            if (targetType == SKILL_AFFECT_TARGET_TYPE.Self)
            {
                targetActors.Add(actorEntity);
                return targetActors;
            }
            List<CombatActorEntity> tempActors = new List<CombatActorEntity>();
            if (targetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
            {
                tempActors = BattleLogicManager.Instance.BattleData.GetEnemyLst(actorEntity);
            }
            else if (targetType == SKILL_AFFECT_TARGET_TYPE.Team)
            {
                tempActors = BattleLogicManager.Instance.BattleData.GetTeamList(actorEntity.TeamKey);
            }
            else
            {
                tempActors = BattleLogicManager.Instance.BattleData.allActorLst;
            }
            for (int i = 0; i < tempActors.Count; i++)
            {
                if (tempActors[i].IsCantSelect)
                {
                    continue;
                }
                targetActors.Add(tempActors[i]);
            }
            return targetActors;
        }

        /// <summary>
        /// 是否在攻击范围
        /// </summary>
        /// <param name="originActor"></param>
        /// <param name="targetActor"></param>
        /// <returns></returns>
        public static bool IsInAttackRange(BattleData battleData, CombatActorEntity originActor, CombatActorEntity targetActor)
        {
            float atkDis = originActor.GetCurrentAtkDistance() + originActor.GetTouchRadiu() + targetActor.GetTouchRadiu();
            float dis = battleData.GetActorDistance(originActor, targetActor);
            if (Math.Round(dis, 2) <= Math.Round(atkDis, 2))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否在普攻范围
        /// </summary>
        /// <param name="originActor"></param>
        /// <param name="targetActor"></param>
        /// <returns></returns>
        public static bool IsInNormalAttackRange(BattleData battleData, CombatActorEntity originActor, CombatActorEntity targetActor)
        {
            float atkDis = originActor.GetNormalAtkDistance() + originActor.GetTouchRadiu() + targetActor.GetTouchRadiu();
            float dis = battleData.GetActorDistance(originActor, targetActor);
            if (Math.Round(dis, 2) <= Math.Round(atkDis, 2))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否敌方死亡
        /// </summary>
        /// <param name="data"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static bool isEnemyDied(BattleData data, CombatActorEntity actor)
        {
            if (!string.IsNullOrEmpty(actor.targetKey)
                && data.allActorDic[actor.targetKey].CurrentHealth.Value <= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 是否敌方队伍灭亡
        /// </summary>
        /// <param name="data"></param>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static bool IsEnemyTeamDead(BattleData data, CombatActorEntity actor)
        {
            if (actor.targetTeamKey == -1)
            {
                return false;
            }
            List<CombatActorEntity> enemyList = new List<CombatActorEntity>();
            if (actor.isAtker
                && data.defActorDic.ContainsKey(actor.targetTeamKey))
            {
                enemyList = data.defActorDic[actor.targetTeamKey];
            }
            else if (data.atkActorDic.ContainsKey(actor.targetTeamKey))
            {
                enemyList = data.atkActorDic[actor.targetTeamKey];
            }
            for (int i = 0; i < enemyList.Count; i++)
            {
                if (enemyList[i].CurrentHealth.Value > 0)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 是否在警戒区域
        /// </summary>
        /// <param name="atkEntity"></param>
        /// <param name="defEntity"></param>
        /// <returns></returns>
        public static bool IsInAlertRange(CombatActorEntity atkEntity, CombatActorEntity defEntity)
        {
            if (atkEntity.alertRange == 0)
            {
                return true;
            }
            float dis = MathHelper.ActorDistance(atkEntity, defEntity);
            if (atkEntity.alertRange * atkEntity.alertRange < dis)
            {
                return false;
            }
            return true;
        }

        public static bool isSameTeamCommander(BattleData data, CombatActorEntity actorEntity)
        {
            if (actorEntity.isLeader
                || !data.allActorDic.ContainsKey(actorEntity.CommanderID))
            {
                return true;
            }
            CombatActorEntity commanderEntity = data.allActorDic[actorEntity.CommanderID];
            if (actorEntity.targetTeamKey != commanderEntity.targetTeamKey)
            {
                return false;
            }
            return true;
        }

        public static int GetTeamCommanderTargetTeam(BattleData data, CombatActorEntity actorEntity)
        {
            if (!data.allActorDic.ContainsKey(actorEntity.CommanderID))
            {
                return -1;
            }
            if (actorEntity.isLeader)
            {
                return actorEntity.targetTeamKey;
            }
            return data.allActorDic[actorEntity.CommanderID].targetTeamKey;
        }

        public static bool IsTeamOver(List<CombatActorEntity> lst)
        {
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i] == null)
                {
                    continue;
                }
                if (lst[i].CurrentHealth.Value > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static int GetOtherCamp(BattleData battleContext, CombatActorEntity actor, out List<CombatActorEntity> actors)
        {
            actors = new List<CombatActorEntity>();
            int camp = actor.isAtker ? 0 : 1;
            var data = battleContext.allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.isAtker != actor.isAtker)
                {
                    actors.Add(data.Current.Value);
                }
            }
            return 0;
        }

        public static int FilterDeath(ref List<CombatActorEntity> actors)
        {
            int lastIndex = actors.Count - 1;
            for (int i = lastIndex; i >= 0; i--)
            {
                var actor = actors[i];
                if (actor == null
                    && actor.CurrentHealth.Value <= 0)
                {
                    actors.RemoveAt(i);
                }
            }
            return 0;
        }

        public static int FilterSelectable(ref List<CombatActorEntity> actors)
        {
            int lastIndex = actors.Count - 1;
            for (int i = lastIndex; i >= 0; i--)
            {
                var actor = actors[i];
                if (!actor.IsCantSelect)
                {
                    actors.RemoveAt(i);
                }
            }
            return 0;
        }

        public static int GetCaptain(BattleData battleContext, CombatActorEntity actor, out CombatActorEntity captain)
        {
            if (actor.isLeader)
            {
                captain = actor;
            }
            else
            {
                captain = battleContext.allActorDic[actor.CommanderID];
            }
            return 0;
        }

        public static int SearchTeam(BattleData battleContext, int team, out List<CombatActorEntity> actors)
        {
            actors = new List<CombatActorEntity>();
            var target = battleContext.atkActorDic.GetEnumerator();
            while (target.MoveNext())
            {
                if (target.Current.Key == team)
                {
                    actors = target.Current.Value;
                }
            }
            return 0;
        }

        //组移动—聚合
        public static Vector3 MoveGroupFroce(CombatActorEntity originActor, List<CombatActorEntity> neighborList, bool isCenter = true)
        {
            float alignmentWeight = 1.0f;
            float separationWeight = 1.0f;
            float cohesionWeight = 1.0f;
            Vector3 alignmentForce = Vector3.zero; //队列的力
            Vector3 separationForce = Vector3.zero; //分离的力
            Vector3 cohesionForce = Vector3.zero; //聚集的力
            Vector3 avgDir = Vector3.zero;
            Vector3 sumForce = Vector3.zero;
            Vector3 dir = Vector3.zero;
            Vector3 center = Vector3.zero;
            for (int i = 0; i < neighborList.Count; i++)
            {
                dir = originActor.GetPositionXZ() - neighborList[i].GetPositionXZ();
                if (dir.magnitude > 0.1f)
                    separationForce += dir.normalized / dir.magnitude;
                avgDir += neighborList[i].GetForward();
                center += neighborList[i].GetPositionXZ();
            }
            if (neighborList.Count > 0)
            {
                separationForce *= separationWeight;
                sumForce += separationForce;
                avgDir /= neighborList.Count;
                alignmentForce = avgDir - originActor.GetForward();
                alignmentForce *= alignmentWeight;
                sumForce += alignmentForce;
                if (isCenter)
                {
                    center /= neighborList.Count;
                    Vector3 dirToCenter = center - originActor.GetPositionXZ();
                    cohesionForce += dirToCenter;
                    cohesionForce *= cohesionWeight;
                    sumForce += cohesionForce;
                }
            }
            return sumForce;
        }

        /// <summary>
        ///  查找当前角色的敌对角色的攻击力最高的某一个角色，距离不限
        /// </summary>
        /// <param name="battleData"></param>
        public static CombatActorEntity GetHighestATKActor(this BattleData battleData, CombatActorEntity from)
        {
            var data = battleData.allActorDic.GetEnumerator();
            CombatActorEntity targetActor = null;
            // 对比全部的角色攻击力
            while (data.MoveNext())
            {
                CombatActorEntity tempActor = data.Current.Value;
                if (from.Equals(tempActor))
                {
                    continue;
                }
                if (!tempActor.Alive())
                {
                    continue;
                }
                if (tempActor.isAtker == from.isAtker)
                {
                    continue;
                }
                if (targetActor == null)
                {
                    targetActor = tempActor;
                    continue;
                }
                if (tempActor.AttrData.Att_Attack > targetActor.AttrData.Att_Attack)
                {
                    targetActor = tempActor;
                }
            }
            return targetActor;
        }

        public static CombatActorEntity GetNearestTarget(this BattleData battleData, CombatActorEntity from, float limitRange = 0)
        {
            var data = from.isAtker ? battleData.defActorLst : battleData.atkActorLst;
            CombatActorEntity targetActor = null;
            float distance = -1;
            if (limitRange > 0)
            {
                distance = limitRange * limitRange;
            } // 如果存在距离显示
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].CurrentHealth.Value <= 0
                    || data[i] == null
                    || data[i].IsCantSelect)
                {
                    continue;
                }

                // 距离筛选
                float tempDistance = MathHelper.ActorDistance(data[i], from);
                if (tempDistance < distance
                    || distance == -1)
                {
                    targetActor = data[i];
                    distance = tempDistance;
                }
            }
            return targetActor;
        }

        public static CombatActorEntity GetNearbyActorEntity(this BattleData battleData, CombatActorEntity actorObject, Vector3 targetPos)
        {
            var data = battleData.allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.CurrentHealth.Value <= 0
                    && data.Current.Value.UID == actorObject.UID)
                    continue;
                float tempDistance = MathHelper.DoubleDistanceVect3(targetPos, data.Current.Value.GetPositionXZ());
                if (tempDistance < data.Current.Value.GetAiLocRadiu() * data.Current.Value.GetAiLocRadiu())
                {
                    return data.Current.Value;
                }
            }
            return null;
        }

        public static List<CombatActorEntity> GetNeighborList(BattleData battleData, CombatActorEntity actorEntity)
        {
            var allActorData = battleData.allActorDic.GetEnumerator();
            List<CombatActorEntity> _neighborList = new List<CombatActorEntity>();
            float distanc = 0.0f;
            float distanceLimit = 0.0f;
            while (allActorData.MoveNext())
            {
                if (allActorData.Current.Value.UID == actorEntity.UID
                    || allActorData.Current.Value.CurrentHealth.Value <= 0
                    || allActorData.Current.Value.Action_actorActionState == ACTOR_ACTION_STATE.Move)
                {
                    continue;
                }
                distanceLimit = allActorData.Current.Value.GetAiLocRadiu() + actorEntity.GetAiLocRadiu();
                distanc = MathHelper.ActorDistance(actorEntity, allActorData.Current.Value);
                if ((Math.Round(distanc, 2) <= Math.Round(distanceLimit, 2))
                    || float.IsNaN(distanc))
                {
                    _neighborList.Add(allActorData.Current.Value);
                }
            }
            return _neighborList;
        }

        public static Vector3 GetCheckTargetPos(BattleData battleData, CombatActorEntity actorEntity, CombatActorEntity targetActorEntity, float radiusOffset = 0.0f)
        {
            Vector3 targetCrossPos = Vector3.zero;
            float distanceLimit = 0.0f;
            float distanc = 0.0f;
            Vector3 centerPos = targetActorEntity.GetPositionXZ();
            Vector3 tempPos = Vector3.zero;
            float angle = 0.0f;
            int angleOffset = BattleLogicManager.Instance.Rand.RandomVaule(30, 90);
            float radius = actorEntity.GetTouchRadiu() + targetActorEntity.GetTouchRadiu() + radiusOffset;
            List<Vector3> checkList = new List<Vector3>();
            int calculateNum = 20;
            while (true)
            {
                float x = centerPos.x + radius * Mathf.Cos(angle); //* angleOffset
                float z = centerPos.z + radius * Mathf.Sin(angle); //* angleOffset
                tempPos = new Vector3(x, 0, z);
                bool isBreak = false;
                var allActorData = battleData.allActorDic.GetEnumerator();
                while (allActorData.MoveNext())
                {
                    if (allActorData.Current.Value.UID == actorEntity.UID
                        || allActorData.Current.Value.CurrentHealth.Value <= 0
                        || allActorData.Current.Value.Action_actorActionState == ACTOR_ACTION_STATE.Move
                        || allActorData.Current.Value.PosIndex == BattleConst.PlayerPosIndex)
                    {
                        continue;
                    }
                    distanceLimit = allActorData.Current.Value.GetAiLocRadiu() + actorEntity.GetAiLocRadiu();
                    distanc = MathHelper.DoubleDistanceVect3(tempPos, allActorData.Current.Value.GetPositionXZ());
                    if (distanc <= distanceLimit * distanceLimit
                        || float.IsNaN(distanc))
                    {
                        if (battleData.IsCalActorOccupy(allActorData.Current.Value))
                        {
                            isBreak = true;
                            break;
                        }
                    }
                }
                if (!isBreak)
                {
                    checkList.Add(tempPos);
                }
                angle += angleOffset;
                if (angle >= 360)
                {
                    if (checkList.Count > 0)
                    {
                        break;
                    }
                    angleOffset = BattleLogicManager.Instance.Rand.RandomVaule(30, 90);
                    angle = angleOffset;
                    radius += actorEntity.GetAiLocRadiu();
                }
                calculateNum--;
                if (calculateNum <= 0)
                {
                    break;
                }
            }
            distanceLimit = 0;
            if (checkList.Count > 0)
            {
                int index = BattleLogicManager.Instance.Rand.RandomVaule(0, checkList.Count);
                index = Mathf.Min(index, checkList.Count - 1);
                targetCrossPos = checkList[index];
            }
            // for (int i = 0; i < checkList.Count; i++)
            // {
            //     distanc = MathHelper.DoubleDistanceVect3(actorEntity.GetPositionXZ(), checkList[i]);
            //     if (distanc < distanceLimit * distanceLimit
            //         || distanceLimit == 0)
            //     {
            //         targetCrossPos = checkList[i];
            //         distanceLimit = distanc;
            //     }
            // }
            // if (!BattleUtil.IsInMap(targetCrossPos))
            // {
            //     targetCrossPos = BattleUtil.GetWalkablePos(targetCrossPos);
            // }
            return targetCrossPos;
        }

#region Boss战斗指令查询
        public static int IsFocusBossPart(this BattleData data, string actorID, string partID, params object[] obj)
        {
            if (!data.allActorDic.ContainsKey(actorID))
            {
                return 0;
            }
            var actor = data.allActorDic.GetEnumerator();
            while (actor.MoveNext())
            {
                if (actor.Current.Value.UID == actorID)
                {
                    continue;
                }
                if (actor.Current.Value.targetKey != actorID
                    || actor.Current.Value.targetPartkey != partID)
                {
                    return 0;
                }
            }
            return 1;
        }

        ///hpPer : 百分比：100，80写法
        public static int IsHPLower(this BattleData data, string actorID, int hpPer)
        {
            if (!data.allActorDic.ContainsKey(actorID))
            {
                return 0;
            }
            var actor = data.allActorDic[actorID];
            if (actor.CurrentHealth.PercentHealth(hpPer) > actor.CurrentHealth.Value)
            {
                return 1;
            }
            return 0;
        }
#endregion

        public static void RefreshOTNumFromCureAction(string cureReciver, string cureCreater, int num, bool isCurerAtker)
        {
            if (num <= 0)
            {
                return;
            }
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (data.Current.Value.CurrentHealth.Value <= 0)
                {
                    continue;
                }
                if (data.Current.Value.isAtker == isCurerAtker)
                {
                    continue;
                }
                CombatActorEntity target = data.Current.Value.GetAttackTargetEntity();
                if (target == null)
                {
                    continue;
                }
                if (target.UID == cureReciver)
                {
                    data.Current.Value.AddOTNum(cureCreater, num);
                }
            }
        }

        public static void RefreshOTNumFromDamageAction(string damageReciver, int num, bool isDamagerAtker)
        {
            if (num <= 0)
            {
                return;
            }
            var data = isDamagerAtker ? BattleLogicManager.Instance.BattleData.atkActorDic.GetEnumerator() : BattleLogicManager.Instance.BattleData.defActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                for (int i = 0; i < data.Current.Value.Count; i++)
                {
                    if (data.Current.Value[i].CurrentHealth.Value <= 0)
                    {
                        continue;
                    }
                    data.Current.Value[i].AddOTNum(damageReciver, num);
                }
            }
        }

        /// <summary>
        /// 仇恨转移
        /// </summary>
        /// <param name="owner"></param>
        public static void TriggerTaunted(string owner)
        {
            var data = BattleLogicManager.Instance.BattleData.allActorDic[owner].isAtker ? BattleLogicManager.Instance.BattleData.defActorDic.GetEnumerator() : BattleLogicManager.Instance.BattleData.atkActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                for (int i = 0; i < data.Current.Value.Count; i++)
                {
                    data.Current.Value[i].SetOTToTheTop(owner);
                }
            }
        }

        public static void EndTaunted(string owner)
        {
            var data = BattleLogicManager.Instance.BattleData.allActorDic[owner].isAtker ? BattleLogicManager.Instance.BattleData.defActorDic.GetEnumerator() : BattleLogicManager.Instance.BattleData.atkActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                for (int i = 0; i < data.Current.Value.Count; i++)
                {
                    data.Current.Value[i].ClearModifierOTNum(owner);
                }
            }
        }

        public static string MaxOTEntity(bool isAttker = true)
        {
            CombatActorEntity entity = null;
            float otNum = 0;
            var data = isAttker ? BattleLogicManager.Instance.BattleData.defActorDic.GetEnumerator() : BattleLogicManager.Instance.BattleData.atkActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                for (int i = 0; i < data.Current.Value.Count; i++)
                {
                    if (data.Current.Value[i].CurrentHealth.Value <= 0)
                    {
                        continue;
                    }
                    float num = data.Current.Value[i].GetFirstOTNum();
                    if (num > otNum)
                    {
                        entity = data.Current.Value[i].GetAttackTargetEntity();
                        otNum = num;
                    }
                }
            }
            if (entity == null
                || otNum <= 0)
            {
                return "";
            }
            else
            {
                return entity.UID;
            }
        }

        /// <summary>
        /// 是否在地图内
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool IsInMap(Vector3 pos)
        {
            return PathUtil.IsWalkable(PathFindingManager.Instance.AstarPathCore, pos);
        }

        /// <summary>
        /// 获取最近能走位置
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static Vector3 GetWalkablePos(Vector3 pos)
        {
            return PathUtil.GetNearestWalkablePos(PathFindingManager.Instance.AstarPathCore, pos);
        }

        /// <summary>
        /// 获取BUFF效果对象
        /// </summary>
        /// <param name="OwnerEntity">施法者</param>
        /// <param name="skillTargetEntity">技能目标对象</param>
        /// <param name="buffEffectTarget">效果类型</param>
        /// <returns></returns>
        public static CombatActorEntity GetBuffTarget(CombatActorEntity OwnerEntity, CombatActorEntity skillTargetEntity, BUFF_EFFECT_TARGET buffEffectTarget)
        {
            if (buffEffectTarget == BUFF_EFFECT_TARGET.SELF)
            {
                return OwnerEntity;
            }
            else if (buffEffectTarget == BUFF_EFFECT_TARGET.CURRENT_TARGET)
            {
                return skillTargetEntity;
            }
            else if (buffEffectTarget == BUFF_EFFECT_TARGET.ALL_ENEMY_MIN_HP)
            {
                List<CombatActorEntity> allEnemy = BattleLogicManager.Instance.BattleData.GetEnemyLst(OwnerEntity);
                CombatActorEntity tempCombatActorEntity = null;
                for (int j = 0; j < allEnemy.Count; j++)
                {
                    if (allEnemy[j].IsCantSelect)
                    {
                        continue;
                    }
                    if (tempCombatActorEntity == null
                        || tempCombatActorEntity.CurrentHealth.Percent() > allEnemy[j].CurrentHealth.Percent())
                    {
                        tempCombatActorEntity = allEnemy[j];
                    }
                }
                return tempCombatActorEntity;
            }
            else if (buffEffectTarget == BUFF_EFFECT_TARGET.ALL_ENEMY_MAX_HP)
            {
                List<CombatActorEntity> allEnemy = BattleLogicManager.Instance.BattleData.GetEnemyLst(OwnerEntity);
                CombatActorEntity tempCombatActorEntity = null;
                for (int j = 0; j < allEnemy.Count; j++)
                {
                    if (allEnemy[j].IsCantSelect)
                    {
                        continue;
                    }
                    if (tempCombatActorEntity == null
                        || tempCombatActorEntity.CurrentHealth.Percent() < allEnemy[j].CurrentHealth.Percent())
                    {
                        tempCombatActorEntity = allEnemy[j];
                    }
                }
                return tempCombatActorEntity;
            }
            else if (buffEffectTarget == BUFF_EFFECT_TARGET.MIN_HP_SELF_TEAM)
            {
                List<CombatActorEntity> selfTeam = BattleLogicManager.Instance.BattleData.GetTeamList(OwnerEntity.TeamKey);
                CombatActorEntity tempCombatActorEntity = null;
                for (int j = 0; j < selfTeam.Count; j++)
                {
                    if (selfTeam[j].IsCantSelect)
                    {
                        continue;
                    }
                    if (tempCombatActorEntity == null
                        || tempCombatActorEntity.CurrentHealth.Percent() > selfTeam[j].CurrentHealth.Percent())
                    {
                        tempCombatActorEntity = selfTeam[j];
                    }
                }
                return tempCombatActorEntity;
            }
            return null;
        }

        /// <summary>
        /// 添加怒气值
        /// </summary>
        /// <param name="configID"></param>
        /// <param name="val"></param>
        public static void AddMp(int configID, int val)
        {
            var actorLst = BattleLogicManager.Instance.BattleData.allActorLst;
            for (int i = 0; i < actorLst.Count; i++)
            {
                if (actorLst[i].ConfigID == configID)
                {
                    actorLst[i].AddMp(val);
                }
            }
        }

        public static void AddHp(int configID, int val)
        {
            var actorLst = BattleLogicManager.Instance.BattleData.allActorLst;
            for (int i = 0; i < actorLst.Count; i++)
            {
                if (actorLst[i].ConfigID == configID)
                {
                    actorLst[i].AddHp(val);
                }
            }
        }

#region 战场英雄数据结构搭建
        public static BattlePlayer CreateTestHero(int heroId, int lv = 1)
        {
            BattlePlayer player = new BattlePlayer();
            BattleHero hero = new BattleHero();
            hero.id = heroId;
            hero.lv = lv;
            ItemInfo itemInfo = new ItemInfo();
            itemInfo._id = "Test_Friend_Hero";
            itemInfo.id = hero.id;
            itemInfo.lv = 1;
            BattleItemInfo battleInfo = new BattleItemInfo(itemInfo, null);
            int[] tempList = new int[battleInfo.att.Length];
            for (int i = 0; i < tempList.Length; i++)
            {
                tempList[i] = (int)battleInfo.att[i];
            }
            hero.Attr = tempList;
            player.heroes = new List<BattleHero>();
            player.heroes.Add(hero);
            return player;
        }

        public static BattleHero HeroInfoToBattleHero(HeroInfo heroInfo, List<int> puzzleSkillLst)
        {
            BattleHero battleHero = new BattleHero();
            battleHero.oid = heroInfo.InstanceId;
            battleHero.id = heroInfo.HeroId;
            battleHero.lv = heroInfo.Level;
            battleHero.Attr = new int[(int)HeroAttr.MaxNum - 1];
            for (int i = 1; i < (int)HeroAttr.MaxNum; i++)
            {
                battleHero.Attr[i - 1] = heroInfo.GetAttribute((HeroAttr)i);
            }
            battleHero.Skill = new Dictionary<int, int>();
            int[] skillLst = heroInfo.SkillIds;
            for (int i = 0; i < skillLst.Length; i++)
            {
                var skillLv = heroInfo.GetSkillLevel(skillLst[i]);
                battleHero.Skill[skillLst[i]] = skillLv;
            }
            foreach (var kv in heroInfo.StarSkillMap)
            {
                battleHero.Skill[kv.Key] = kv.Value;
            }
            if (puzzleSkillLst != null)
            {
                for (int i = 0; i < puzzleSkillLst.Count; i++)
                {
                    battleHero.Skill[puzzleSkillLst[i]] = 1;
                }
            }
            return battleHero;
        }

        public static BattleItemInfo CreateActorItemInfo(int rowId, int level, bool isMonster)
        {
            BattleItemInfo battleItemInfo = new BattleItemInfo(StrBuild.Instance.ToStringAppend("hero_", rowId.ToString(), "_", System.Guid.NewGuid().ToString()), rowId);
            battleItemInfo.lv = level;
            bool isBoss = false;
            float scale = 1.0f;
            if (isMonster)
            {
                MonsterRow monster = StaticData.MonsterTable.TryGet(rowId);
                battleItemInfo.id = monster.heroID;
                battleItemInfo.lv = monster.monsterLv;
                battleItemInfo.warnRange = monster.warnRange;
                var attrGroupArryRow = StaticData.AttrGroupTable.TryGet(monster.attrGroup);
                AttrGroupRow row = attrGroupArryRow.Colls.Find(it => it.Rank == monster.monsterLv);
                if (row != null)
                {
                    battleItemInfo.att = new int[row.Attrs.Length + 1];
                    for (int i = 0; i < row.Attrs.Length; i++)
                    {
                        battleItemInfo.att[i + 1] = row.Attrs[i];
                    }
                }
                isBoss = monster.Sort == 2;
                scale = monster.Scale;
            }
            else
            {
                HeroRow heroRow = StaticData.HeroTable.TryGet(rowId);
                if (heroRow != null)
                {
                    battleItemInfo.att = new int[heroRow.Attrs.Length + 1];
                    for (int i = 0; i < heroRow.Attrs.Length; i++)
                    {
                        battleItemInfo.att[i + 1] = heroRow.Attrs[i];
                    }
                }
            }
            List<int> skillList = battleItemInfo.GetHeroSkill();
            for (int i = 0; i < skillList.Count; i++)
            {
                battleItemInfo.skillsDic[skillList[i]] = 1;
            }
            battleItemInfo.isBoss = isBoss;
            battleItemInfo.scale = scale;
            return battleItemInfo;
        }

        public static BattleItemInfo CreateActorItemInfo(BattleHero data)
        {
            if (data == null)
            {
                return null;
            }
            BattleItemInfo battleItemInfo = new BattleItemInfo(data.oid, data.id);
            battleItemInfo.lv = data.lv;
            battleItemInfo.att = new int[data.Attr.Length + 1];
            for (int i = 0; i < data.Attr.Length; i++)
            {
                battleItemInfo.att[i] = data.Attr[i];
            }
            List<int> skillList = battleItemInfo.GetHeroSkill();
            int skillLv = 1;
            for (int i = 0; i < skillList.Count; i++)
            {
                data.Skill.TryGetValue(skillList[i], out skillLv);
                battleItemInfo.skillsDic[skillList[i]] = Mathf.Max(1, skillLv);
            }
            battleItemInfo.isBoss = false;
            battleItemInfo.scale = 1.0f;
            return battleItemInfo;
        }

        public static BattleItemInfo CreateActorItemInfo(HeroInfo heroInfo, List<int> puzzleSkillLst)
        {
            BattleItemInfo battleItemInfo = new BattleItemInfo(heroInfo);
            int[] skillLst = heroInfo.SkillIds;
            for (int i = 0; i < skillLst.Length; i++)
            {
                var skillLv = heroInfo.GetSkillLevel(skillLst[i]);
                battleItemInfo.skillsDic[skillLst[i]] = skillLv;
            }
            foreach (var kv in heroInfo.StarSkillMap)
            {
                battleItemInfo.skillsDic[kv.Key] = kv.Value;
            }
            if (puzzleSkillLst != null)
            {
                for (int i = 0; i < puzzleSkillLst.Count; i++)
                {
                    battleItemInfo.skillsDic[puzzleSkillLst[i]] = 1;
                }
            }
            return battleItemInfo;
        }

        public static CombatActorEntity CreateCombatActorUnit(BattleItemInfo battleItemInfo, ActorIFF iff)
        {
            CombatActorEntity entity = Entity.Create<CombatActorEntity>();
            HeroRow heroRow = StaticData.HeroTable.TryGet(battleItemInfo.id);
            if (heroRow == null)
            {
                BattleLog.LogError("Can't find the Hero ID " + battleItemInfo.id);
                return null;
            }
            entity.Weak = heroRow.Weak;
            entity.InitBattleInfo(battleItemInfo);
            entity.SetTeamInfo(iff.CampID, iff.TeamID);
            entity.BornCharacters(iff.pos, iff.dir, battleItemInfo.scale, StaticData.HeroTable[entity.ConfigID]);
            entity.PosIndex = iff.PosIndex;
            var skillData = battleItemInfo.skillsDic.GetEnumerator();
            while (skillData.MoveNext())
            {
                SkillRow skillRow = SkillUtil.GetSkillItem(skillData.Current.Key, skillData.Current.Value);
                if (skillRow != null)
                {
                    entity.AttachSkill(skillRow);
                }
            }
            entity.AttachSkillGroup();
            return entity;
        }
#endregion
    }
}
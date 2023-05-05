namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public static class BattleControlUtil
    {
#region Buff State Forbid Control
        public static List<BuffAbility> GetCombatActorEntityForbidBuff(CombatActorEntity actorEntity)
        {
            var data = actorEntity.TypeIdBuffs.GetEnumerator();
            List<BuffAbility> resultLst = new List<BuffAbility>();
            while (data.MoveNext())
            {
                if (data.Current.Value.buffRow.controlType == (int)ACTION_CONTROL_TYPE.None)
                {
                    continue;
                }
                resultLst.Add(data.Current.Value);
            }
            return resultLst;
        }

        /// <summary>
        /// 禁止攻击
        /// </summary>
        /// <param name="actorEntity">人物数据</param>
        /// <param name="skillType">技能类型</param>
        /// <returns></returns>
        public static bool IsForbidAttack(CombatActorEntity actorEntity, SKILL_TYPE skillType = SKILL_TYPE.ATK)
        {
            //逻辑状态检查

            //Buff 检查
            if ((skillType == SKILL_TYPE.SPSKL || skillType == SKILL_TYPE.SSPMove)
                && !IsManualAttack(actorEntity))
            {
                return true;
            }
            else if (skillType == SKILL_TYPE.SSP
                     && !IsAISkillAttack(actorEntity))
            {
                return true;
            }
            else if (!IsAINormalAttack(actorEntity))
            {
                return true;
            }
            return false;
        }

        public static bool IsForbidMove(CombatActorEntity actorEntity, bool IsInputMove = false)
        {
            //逻辑状态检查

            //Buff 检查
            if (IsInputMove)
            {
                return !IsManualMove(actorEntity);
            }
            else
            {
                return !IsAIMove(actorEntity);
            }
        }

        public static bool IsForbidOperateTarget(CombatActorEntity actorEntity, CombatActorEntity targetEntity)
        {
            //逻辑状态检查

            //Buff 检查
            if (actorEntity.TeamKey != targetEntity.TeamKey)
            {
                return !IsOperableTargetEnemyTeam(actorEntity);
            }
            return !IsOperableTargetSelfTeam(actorEntity);
        }

        private static bool IsForbidBuffState(CombatActorEntity actorEntity, ACTION_CONTROL_STATE_TYPE sateType)
        {
            var data = actorEntity.TypeIdBuffs.GetEnumerator();
            List<BuffAbility> lst = new List<BuffAbility>();
            while (data.MoveNext())
            {
                if (data.Current.Value.buffRow.controlType == (int)ACTION_CONTROL_TYPE.None)
                {
                    continue;
                }
                BuffLevel2StateRow row = StaticData.BuffLevel2StateTable[((int)data.Current.Value.buffRow.controlType)];
                if (row.States[(int)sateType] == 0)
                    return true;
            }
            return false;
        }

        private static bool IsManualAttack(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.ManualAttack);
        }

        private static bool IsAINormalAttack(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.AINormalAttack);
        }

        private static bool IsAISkillAttack(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.AISkillAttack);
        }

        private static bool IsManualMove(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.ManualMove);
        }

        private static bool IsAIMove(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.AIMove);
        }

        private static bool IsOperableTargetSelfTeam(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.OperateSelfTeam);
        }

        private static bool IsOperableTargetEnemyTeam(CombatActorEntity actorEntity)
        {
            return !IsForbidBuffState(actorEntity, ACTION_CONTROL_STATE_TYPE.OperateEnemyTeam);
        }
#endregion

#region Buff State Reject
        /// <summary>
        /// 控制状态刷新
        /// </summary>
        /// <param name="actorEntity">人物数据</param>
        /// <param name="action_control_type">控制类型</param>
        /// <returns></returns>
        public static bool RefreshBuffControlState(CombatActorEntity actorEntity, ACTION_CONTROL_TYPE action_control_type)
        {
            if (action_control_type == ACTION_CONTROL_TYPE.None)
            {
                BattleLog.LogError(" control is null error");
                return false;
            }
            List<BuffInvalidRow> invalidLst = StaticData.BuffInvalidTable.ElementList;
            BuffRejectRow row = StaticData.BuffRejectTable[((int)action_control_type)];
            List<int> invalidLst2 = new List<int>();
            List<int> rejectLst = new List<int>();
            for (int i = 0; i < row.Rejects.Length; i++)
            {
                if (row.Rejects[i] == 1)
                {
                    rejectLst.Add(i + 1);
                }
                else if (row.Rejects[i] == 0)
                {
                    invalidLst2.Add(i + 1);
                }
            }
            var data = actorEntity.TypeIdBuffs.GetEnumerator();
            List<BuffAbility> removeBuffAbility = new List<BuffAbility>();
            BuffAbility tempBuffAbility = null;
            bool isInvalid = false;
            while (data.MoveNext())
            {
                tempBuffAbility = data.Current.Value;
                if (StaticData.BuffInvalidTable.ContainsKey((int)tempBuffAbility.buffRow.controlType))
                {
                    BuffInvalidRow invalidRow = StaticData.BuffInvalidTable[(int)tempBuffAbility.buffRow.controlType];
                    if ((int)action_control_type < invalidRow.Invalids.Length
                        && invalidRow.Invalids[(int)action_control_type] == 1)
                    {
                        isInvalid = true;
                        break;
                    }
                }
                if (invalidLst2.Contains((int)tempBuffAbility.buffRow.controlType))
                {
                    isInvalid = true;
                    break;
                }
                if (rejectLst.Contains((int)tempBuffAbility.buffRow.controlType))
                {
                    removeBuffAbility.Add(data.Current.Value);
                }
            }
            if (isInvalid)
            {
                return false;
            }
            for (int i = 0; i < removeBuffAbility.Count; i++)
            {
                removeBuffAbility[i].EndAbility();
            }
            return true;
        }
#endregion
    }
}
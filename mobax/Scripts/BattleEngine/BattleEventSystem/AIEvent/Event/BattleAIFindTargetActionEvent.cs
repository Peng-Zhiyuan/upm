namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public sealed class BattleAIFindTargetActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIFindTargetActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            if (!string.IsNullOrEmpty(actorEntity.targetKey))
            {
                CombatActorEntity targetEntityKey = battleData.allActorDic[actorEntity.targetKey];
                if (targetEntityKey.CurrentHealth.Value <= 0)
                {
                    actorEntity.ClearTargetInfo();
                }
            }
            CombatActorEntity targetEntity = BattleUtil.GetTargetActorEntity(battleData, actorEntity);
            if (targetEntity == null)
            {
                targetEntity = FindNearestTarget(actorEntity);
            }
#region 找最近的目标
            if (targetEntity != null)
            {
                if (actorEntity.WarningRange > 0)
                {
                    float distance = battleData.GetActorDistance(actorEntity, targetEntity);
                    if (distance > actorEntity.WarningRange)
                    {
                        return;
                    }
                }
                string targetPartKey = "";
                actorEntity.SetAutoTargetInfo(targetEntity, targetPartKey);
                if (BattleUtil.IsInAttackRange(battleData, actorEntity, targetEntity))
                {
                    Vector3 forwardDir = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
                    actorEntity.SetForward(forwardDir);
                    actorEntity.SetActionState(ACTOR_ACTION_STATE.ATK);
                }
            }
#endregion
        }

        public CombatActorEntity FindNearestTarget(CombatActorEntity actorEntity)
        {
            List<CombatActorEntity> targetActors = BattleUtil.GetSkillEffectTargets(actorEntity, SKILL_AFFECT_TARGET_TYPE.Enemy);
            if (targetActors.Count <= 0)
            {
                return null;
            }
            targetActors.Sort(delegate(CombatActorEntity x, CombatActorEntity y)
                            {
                                float dis1 = MathHelper.ActorDistance(actorEntity, x) - x.GetTouchRadiu() * x.GetTouchRadiu();
                                float dis2 = MathHelper.ActorDistance(actorEntity, y) - y.GetTouchRadiu() * y.GetTouchRadiu();
                                if (dis1 > dis2)
                                    return 1;
                                else
                                    return -1;
                            }
            );
            return targetActors[0];
        }
    }
}
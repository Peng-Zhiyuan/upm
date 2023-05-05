using UnityEngine;
using System.Collections.Generic;

namespace BattleEngine.Logic
{
    public sealed class BattleSkillFindFarestTargetActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "SkillFindFarestTargetActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleSkillEventData data = obj as BattleSkillEventData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.ReadySkill == null)
            {
                return;
            }
#region 锁敌范围
            List<CombatActorEntity> targetActors = BattleUtil.GetSkillEffectTargets(actorEntity, actorEntity.ReadySkill.SkillConfigObject.AffectTargetType);
            if (targetActors.Count <= 0)
            {
                return;
            }
            targetActors.Sort(delegate(CombatActorEntity x, CombatActorEntity y)
                            {
                                float dis1 = MathHelper.ActorDistance(actorEntity, x) - x.GetTouchRadiu() * x.GetTouchRadiu();
                                float dis2 = MathHelper.ActorDistance(actorEntity, y) - y.GetTouchRadiu() * y.GetTouchRadiu();
                                if (dis1 > dis2)
                                    return -1;
                                else
                                    return 1;
                            }
            );
#endregion
#region 找最远的目标
            CombatActorEntity targetEntity = targetActors[0];
            Vector3 forward = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
            actorEntity.SetForward(forward);
            List<CombatActorEntity> targets = new List<CombatActorEntity>();
            for (int i = 0; i < actorEntity.ReadySkill.SkillConfigObject.lockDownTargetNum; i++)
            {
                if (i >= targetActors.Count)
                {
                    break;
                }
                targets.Add(targetActors[i]);
            }
            actorEntity.SetSkillTargetInfos(targets);
#endregion
        }
    }
}
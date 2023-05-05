using UnityEngine;
using System.Collections.Generic;

namespace BattleEngine.Logic
{
    public sealed class BattleSkillFindTargetFromAttributeActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "SkillFindTargetFromAttributeActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleSkillEventData data = obj as BattleSkillEventData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
#region 锁敌范围
            List<CombatActorEntity> targetActors = BattleUtil.GetSkillEffectTargets(actorEntity, actorEntity.ReadySkill.SkillConfigObject.AffectTargetType);
            if (targetActors.Count <= 0)
            {
                return;
            }
            SortLst(ref targetActors, data.attrType, data.isMax);
#endregion;
#region 找血量最少的目标
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

        private void SortLst(ref List<CombatActorEntity> enemys, AttrType attr, bool isMax)
        {
            if (attr == AttrType.HP)
            {
                enemys.Sort(delegate(CombatActorEntity x, CombatActorEntity y)
                                {
                                    if (x.CurrentHealth.Percent() > y.CurrentHealth.Percent())
                                        return isMax ? -1 : 1;
                                    else
                                        return isMax ? 1 : -1;
                                }
                );
            }
            else
            {
                enemys.Sort(delegate(CombatActorEntity x, CombatActorEntity y)
                                {
                                    if (x.AttrData.GetValue(attr) > y.AttrData.GetValue(attr))
                                        return isMax ? -1 : 1;
                                    else
                                        return isMax ? 1 : -1;
                                }
                );
            }
        }
    }
}
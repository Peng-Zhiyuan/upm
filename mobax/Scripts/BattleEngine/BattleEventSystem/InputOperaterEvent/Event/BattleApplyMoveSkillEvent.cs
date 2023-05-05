/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class BattleApplyMoveSkillEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplyMoveSkillEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            Vector3 targetPos = Vector3.zero;
            if (!battleData.allActorDic.ContainsKey(data.originID))
            {
                return;
            }
            if (!string.IsNullOrEmpty(data.targetID)
                && battleData.allActorDic.ContainsKey(data.targetID))
            {
                CombatActorEntity targetActor = battleData.allActorDic[data.targetID];
                targetPos = targetActor.GetPosition();
            }
            else
            {
                targetPos = data.targetPos;
            }
            CombatActorEntity originActor = battleData.allActorDic[data.originID];
            if (BattleControlUtil.IsForbidAttack(originActor, SKILL_TYPE.SSPMove))
            {
                return;
            }
            if (originActor.CurrentHealth.Value >= 0
                && originActor.Action_actorActionState != ACTOR_ACTION_STATE.Dead)
            {
                if (originActor.CurrentSkillExecution != null)
                {
                    originActor.CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, () => { originActor.SpellMoveSkill(targetPos); });
                }
                else
                {
                    originActor.SpellMoveSkill(targetPos);
                }
                originActor.LastInputMoveTime = 0;
            }
        }
    }
}
/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    public sealed class BattleApplyItemSkillEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplyItemSkillEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.originID))
            {
                return;
            }
            CombatActorEntity originActor = battleData.allActorDic[data.originID];
            if (BattleControlUtil.IsForbidAttack(originActor, SKILL_TYPE.ItemSKL))
            {
                return;
            }
            if (originActor.CurrentHealth.Value >= 0
                && originActor.Action_actorActionState != ACTOR_ACTION_STATE.Dead)
            {
                CombatActorEntity targetActor = battleData.allActorDic[data.targetID];
                originActor.SetAutoTargetInfo(targetActor);
                if (originActor.CurrentSkillExecution != null)
                {
                    originActor.CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, () => { originActor.SpellItemSkill(System.UInt32.Parse(data.skillID.ToString())); }, true);
                }
                else if (battleData.HasEnemy(originActor.isAtker))
                {
                    originActor.SpellItemSkill(System.UInt32.Parse(data.skillID.ToString()));
                }
                originActor.LastInputMoveTime = 0;
            }
        }
    }
}
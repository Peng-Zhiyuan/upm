/* Created:Loki Date:2022-09-26*/

namespace BattleEngine.Logic
{
    public sealed class BattleApplyTurnOffEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplyTurnOffEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity leaveActor = battleData.GetActorEntity(data.originID);
            if (leaveActor == null
                || leaveActor.IsDead)
            {
                return;
            }
            System.Action action = delegate() { leaveActor.SetLifeState(ACTOR_LIFE_STATE.Substitut); };
            if (leaveActor.CurrentSkillExecution != null
                && !leaveActor.CurrentSkillExecution.SkillAbility.SkillConfigObject.CanbeDragged)
            {
                leaveActor.CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, action);
            }
            else
            {
                action();
            }
        }
    }
}
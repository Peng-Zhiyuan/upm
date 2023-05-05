/* Created:Loki Date:2022-09-26*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class BattleApplyTurnOnEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplyTurnOnEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity leaveActor = battleData.GetActorEntity(data.originID);
            if (leaveActor == null)
            {
                return;
            }
            CombatActorEntity joinActor = null;
            if (string.IsNullOrEmpty(data.targetID))
            {
                List<CombatActorEntity> team = battleData.GetTeamList(leaveActor.TeamKey);
                for (int i = 0; i < team.Count; i++)
                {
                    if (!team[i].IsSubstitut()
                        || team[i].IsDead)
                    {
                        continue;
                    }
                    joinActor = team[i];
                    break;
                }
            }
            else
            {
                joinActor = battleData.GetActorEntity(data.targetID);
            }
            if (joinActor == null)
            {
                return;
            }
            System.Action action = delegate()
            {
                int tempPosIndex = leaveActor.PosIndex;
                leaveActor.PosIndex = joinActor.PosIndex;
                joinActor.PosIndex = tempPosIndex;
                joinActor.SetPosition(data.targetPos);
                joinActor.SetForward(data.targetPos.normalized);
                joinActor.ClearTargetInfo();
                joinActor.SetLifeState(ACTOR_LIFE_STATE.Alive);
#if !SERVER
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    GameEventCenter.Broadcast(GameEvent.BattleHeroUpdate, leaveActor, joinActor);
                    EventManager.Instance.SendEvent<CombatActorEntity>("OnHeroTurnOnPoint", joinActor);
                }
#endif
            };
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
/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class BattleCureHurtEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "CureHurtEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity cureTargetData = battleData.allActorDic[data.targetID];
            if (cureTargetData.CurrentHealth.Value >= 0
                && cureTargetData.Action_actorActionState != ACTOR_ACTION_STATE.Dead)
            {
                List<CombatActorEntity> lst = battleData.atkActorDic[data.targetTeamKey];
                for (int i = 0; i < lst.Count; i++)
                {
                    CombatActorEntity entity = lst[i];
                    System.Action action = delegate()
                    {
                        entity.SetInputTargetInfo(cureTargetData);
                        entity.Action_inputOperateType = INPUT_OPERATE_TYPE.Attack;
                    };
                    if (lst[i].CurrentSkillExecution != null)
                    {
                        lst[i].CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, action);
                    }
                    else
                    {
                        action();
                    }
                }
            }
        }
    }
}
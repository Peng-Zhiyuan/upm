/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    /// <summary>
    /// 集中保护
    /// </summary>
    public sealed class BattleDefFocusOnFiringEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "DefFocusOnFiringEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity safeActorEntity = battleData.allActorDic[data.targetID];
            if (safeActorEntity.IsDead)
            {
                return;
            }
            List<CombatActorEntity> lst = battleData.GetTeamList(safeActorEntity.TeamKey);
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].IsDead
                    || lst[i].CurrentLifeState == ACTOR_LIFE_STATE.LookAt
                    || lst[i].UID == safeActorEntity.UID)
                {
                    continue;
                }
                CombatActorEntity entity = lst[i];
                System.Action action = delegate()
                {
                    entity.ClearTargetInfo();
                    entity.SetInputDefendTarget(safeActorEntity);
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
/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class BattleAtkerFocusOnFiringEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AtkerFocusOnFiringEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity enemyData = battleData.allActorDic[data.targetID];
            if (enemyData.IsCantSelect)
            {
                return;
            }
            List<CombatActorEntity> lst = battleData.GetTeamList(data.targetTeamKey);
            for (int i = 0; i < lst.Count; i++)
            {
                if (BattleControlUtil.IsForbidAttack(lst[i])
                    || BattleControlUtil.IsForbidOperateTarget(lst[i], enemyData))
                {
                    // BattleLog.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                    // BattleLog.LogWarning("Attack " + BattleControlUtil.IsForbidAttack(lst[i]));
                    // BattleLog.LogWarning("Operate " + BattleControlUtil.IsForbidOperateTarget(lst[i], enemyData));
                    continue;
                }
                CombatActorEntity entity = lst[i];
                System.Action action = delegate()
                {
                    entity.SetInputTargetInfo(enemyData);
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
            EventManager.Instance.SendEvent<int>("ExecuteAtkerFocusOnFiringEvent", data.targetTeamKey);
        }
    }
}
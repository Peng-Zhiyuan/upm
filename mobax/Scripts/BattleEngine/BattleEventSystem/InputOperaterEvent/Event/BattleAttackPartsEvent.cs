/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class BattleAttackPartsEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AttackPartsEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity enemyData = battleData.allActorDic[data.targetID];
            List<CombatActorEntity> lst = battleData.atkActorDic[data.targetTeamKey];
            for (int i = 0; i < lst.Count; i++)
            {
                if (BattleControlUtil.IsForbidAttack(lst[i])
                    || BattleControlUtil.IsForbidOperateTarget(lst[i], enemyData))
                {
                    continue;
                }
                CombatActorEntity entity = lst[i];
                System.Action action = delegate()
                {
                    entity.SetInputTargetInfo(enemyData, data.parentNode);
                    entity.Action_inputOperateType = INPUT_OPERATE_TYPE.Attack;
                    entity.LastInputMoveTime = 0;
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
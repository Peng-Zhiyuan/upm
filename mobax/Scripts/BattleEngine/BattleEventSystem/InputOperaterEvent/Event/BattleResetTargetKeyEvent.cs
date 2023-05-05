/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    public sealed class BattleResetTargetKeyEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ResetTargetKeyEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity enemyData = battleData.allActorDic[data.targetID];
            if (enemyData.IsCantSelect)
            {
                return;
            }
            CombatActorEntity entity = battleData.allActorDic[data.originID];
            if (BattleControlUtil.IsForbidAttack(entity)
                || BattleControlUtil.IsForbidOperateTarget(entity, enemyData))
            {
                return;
            }
            System.Action action = delegate()
            {
                entity.SetInputTargetInfo(enemyData);
                entity.Action_inputOperateType = INPUT_OPERATE_TYPE.Attack;
            };
            if (entity.CurrentSkillExecution != null)
            {
                entity.CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, action);
            }
            else
            {
                action();
            }
        }
    }
}
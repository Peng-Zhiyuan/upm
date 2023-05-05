/* Created:Loki Date:2022-09-23*/

namespace BattleEngine.Logic
{
    public sealed class BattleApplySkillEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplySkillEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.targetID))
            {
                return;
            }
            CombatActorEntity targetActor = battleData.allActorDic[data.targetID];
            if (BattleControlUtil.IsForbidAttack(targetActor, SKILL_TYPE.SPSKL)
                || targetActor.IsCantSelect)
            {
                return;
            }
            if (targetActor.CurrentSkillExecution != null)
            {
                targetActor.CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, targetActor.SpellDriveSkill, true);
            }
            else if (battleData.HasEnemy(targetActor.isAtker))
            {
                targetActor.SpellDriveSkill();
            }
            targetActor.LastInputMoveTime = 0;
        }
    }
}
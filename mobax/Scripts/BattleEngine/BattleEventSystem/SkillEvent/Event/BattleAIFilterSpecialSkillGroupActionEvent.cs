namespace BattleEngine.Logic
{
    public sealed class BattleAIFilterSpecialSkillComboActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIFilterSpecialSkillComboActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleSkillComboEventData data = obj as BattleSkillComboEventData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            actorEntity.BreakAllSkill();
            for (int i = 0; i < actorEntity.SkillCombos.Count; i++)
            {
                if (actorEntity.SkillCombos[i].Config.Id != data.skillGroupId)
                {
                    continue;
                }
                if (actorEntity.CurrentSkillComboExecution != null)
                    actorEntity.CurrentSkillComboExecution.EndExecute();
                SkillComboAbilityExecution execution = actorEntity.SkillCombos[i].CreateExecution() as SkillComboAbilityExecution;
                execution.BeginExecute();
                break;
            }
        }
    }
}
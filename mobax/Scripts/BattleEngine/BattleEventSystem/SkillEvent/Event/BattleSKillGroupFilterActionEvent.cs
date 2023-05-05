namespace BattleEngine.Logic
{
    public sealed class BattleAIFilterSKillGroupActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "SKillGroupFilterActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            for (int i = 0; i < actorEntity.SkillCombos.Count; i++)
            {
                if (!actorEntity.SkillCombos[i].IsFilter())
                {
                    continue;
                }
                SkillComboAbilityExecution execution = actorEntity.SkillCombos[i].CreateExecution() as SkillComboAbilityExecution;
                execution.BeginExecute();
                break;
            }
        }
    }
}
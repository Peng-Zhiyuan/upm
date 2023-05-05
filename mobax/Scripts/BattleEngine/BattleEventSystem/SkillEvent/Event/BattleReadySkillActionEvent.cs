namespace BattleEngine.Logic
{
    public sealed class BattleReadySkillActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ReadySkillActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleSkillEventData data = obj as BattleSkillEventData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0)
                return;
            if (actorEntity.SkillSlots.ContainsKey(data.skillId))
            {
                actorEntity.ReadySkill = actorEntity.SkillSlots[data.skillId];
            }
        }
    }
}
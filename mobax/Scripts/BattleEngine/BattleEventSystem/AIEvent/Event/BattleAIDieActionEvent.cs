namespace BattleEngine.Logic
{
    public sealed class BattleAIDieActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIDieActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            actorEntity.SetLifeState(ACTOR_LIFE_STATE.Dead);
            actorEntity.SetActionState(ACTOR_ACTION_STATE.Dead);
        }
    }
}
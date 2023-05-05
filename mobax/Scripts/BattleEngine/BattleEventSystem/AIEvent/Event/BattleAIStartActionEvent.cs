namespace BattleEngine.Logic
{
    public class BattleAIStartActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIStartActionEvent";
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
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Dead);
                actorEntity.SetLifeState(ACTOR_LIFE_STATE.Dead);
                return;
            }
            CombatActorEntity targetEntiy = BattleUtil.GetTargetActorEntity(battleData, actorEntity);
            if (targetEntiy != null
                && targetEntiy.CurrentHealth.Value <= 0)
            {
                actorEntity.ClearTargetInfo();
                return;
            }
        }
    }
}
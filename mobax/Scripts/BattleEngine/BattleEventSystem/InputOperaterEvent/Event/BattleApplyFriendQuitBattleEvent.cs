/* Created:Loki Date:2022-09-28*/

namespace BattleEngine.Logic
{
    public sealed class BattleApplyFriendQuitBattleEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplyFriendQuitBattleEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity friendActor = battleData.GetActorEntity(data.originID);
            if (friendActor == null)
            {
                return;
            }
            friendActor.ClearTargetInfo();
            friendActor.SetLifeState(ACTOR_LIFE_STATE.Assist);
        }
    }
}
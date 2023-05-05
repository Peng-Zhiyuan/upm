namespace BattleEngine.Logic {

    public interface IBattleEvent {
        string GetEventName();
        void Excute(BattleData data, object t);
    }

    public interface IBattleEventData {
        string GetEventName();
    }

}
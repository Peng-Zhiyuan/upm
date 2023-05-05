namespace BattleEngine.Logic
{
    using behaviac;

    //转场
    public class BattleAIChangeStageActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIChangeStageActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            // BattleProcessData data = obj as BattleProcessData;
            // BattleManager.Instance.ChangeRoomStage(data.stageIndexToChange);
        }
    }
}
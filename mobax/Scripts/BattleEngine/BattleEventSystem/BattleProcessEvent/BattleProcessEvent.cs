namespace BattleEngine.Logic
{
    public class BattleProcessData : IBattleEventData
    {
        public string eventName = null;
        public int stageIndexToChange = -1;

        public string GetEventName()
        {
            return this.eventName;
        }

        public void InitAIChangeStageActionEvent(int stageIndex)
        {
            this.eventName = "AIChangeStageActionEvent";
            this.stageIndexToChange = stageIndex;
        }
    }
}
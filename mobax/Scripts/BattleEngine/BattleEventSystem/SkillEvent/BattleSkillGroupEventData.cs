namespace BattleEngine.Logic
{
    public sealed class BattleSkillComboEventData : IBattleEventData
    {
        public string eventName = null;
        public string actorUID;
        public int skillGroupId;

        public string GetEventName()
        {
            return this.eventName;
        }

        public void InitAIFilterSpecialSkillComboActionEvent(string _id, int _skillGroupId)
        {
            this.eventName = "AIFilterSpecialSkillComboActionEvent";
            this.actorUID = _id;
            this.skillGroupId = _skillGroupId;
        }
    }
}
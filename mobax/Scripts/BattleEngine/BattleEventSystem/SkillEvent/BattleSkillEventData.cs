namespace BattleEngine.Logic
{
    public sealed class BattleSkillEventData : IBattleEventData
    {
        public string eventName = null;
        public string actorUID;
        public uint skillId;
        /// <summary>
        /// 属性排序
        /// </summary>
        public AttrType attrType;
        public bool isMax;

        public string GetEventName()
        {
            return this.eventName;
        }

        public void InitSpellSkillActionEvent(string _id, uint _skillId)
        {
            this.eventName = "SpellSkillActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }

        public void InitReadySkillActionEvent(string _id, uint _skillId)
        {
            this.eventName = "ReadySkillActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }

        public void InitSkillFindTargetActionEvent(string _id, uint _skillId)
        {
            this.eventName = "SkillFindTargetActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }

        public void InitSkillFindTargetFromAttributeActionEvent(string _id, uint _skillId, AttrType _attrType, bool _isMax)
        {
            this.eventName = "SkillFindTargetFromAttributeActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
            this.attrType = _attrType;
            this.isMax = _isMax;
        }

        public void InitSkillFindNearestTargetActionEvent(string _id, uint _skillId)
        {
            this.eventName = "SkillFindNearestTargetActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }

        public void InitSkillFindFarestTargetActionEvent(string _id, uint _skillId)
        {
            this.eventName = "SkillFindFarestTargetActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }

        public void InitSkillFindRoleChooseTargetActionEvent(string _id, uint _skillId)
        {
            this.eventName = "SkillFindRoleChooseTargetActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }

        public void InitSkillFindRoleChooseTargetInTargetActionEvent(string _id, uint _skillId)
        {
            this.eventName = "SkillFindRoleChooseTargetInTargetActionEvent";
            this.actorUID = _id;
            this.skillId = _skillId;
        }
    }
}
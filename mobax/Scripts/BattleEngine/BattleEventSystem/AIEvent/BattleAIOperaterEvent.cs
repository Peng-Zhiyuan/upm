namespace BattleEngine.Logic
{
    public class BattleAIOperaterData : IBattleEventData
    {
        public string eventName = null;
        public string actorUID;

        public string GetEventName()
        {
            return this.eventName;
        }

        public void InitAIStartActionEvent(string _id)
        {
            this.eventName = "AIStartActionEvent";
            this.actorUID = _id;
        }

        public void InitAIMoveToPosActionEvent(string _id)
        {
            this.eventName = "AIMoveToPosActionEvent";
            this.actorUID = _id;
        }

        public void InitAIMoveToActorActionEvent(string _id)
        {
            this.eventName = "AIMoveToActorActionEvent";
            this.actorUID = _id;
        }

        public void InitAIKeepSafeDisActionEvent(string _id)
        {
            this.eventName = "AIKeepSafeDisActionEvent";
            this.actorUID = _id;
        }

        public void InitAIFindTargetActionEvent(string _id)
        {
            this.eventName = "AIFindTargetActionEvent";
            this.actorUID = _id;
        }

        public void InitAIDieActionEvent(string _id)
        {
            this.eventName = "AIDieActionEvent";
            this.actorUID = _id;
        }

        public void InitAICureTargetActionEvent(string _id)
        {
            this.eventName = "AICureTargetActionEvent";
            this.actorUID = _id;
        }

        public void InitAIAttackActionEvent(string _id)
        {
            this.eventName = "AIAttackActionEvent";
            this.actorUID = _id;
        }

        public void InitAITeleportToActorActionEvent(string _id)
        {
            this.eventName = "AITeleportToActorActionEvent";
            this.actorUID = _id;
        }

        public void InitAIPureLoveTargetActionEvent(string _id)
        {
            this.eventName = "AIPureLoveTargetActionEvent";
            this.actorUID = _id;
        }

        public void InitAIFilterSKillGroupActionEvent(string _id)
        {
            this.eventName = "AIFilterSKillGroupActionEvent";
            this.actorUID = _id;
        }
    }
}
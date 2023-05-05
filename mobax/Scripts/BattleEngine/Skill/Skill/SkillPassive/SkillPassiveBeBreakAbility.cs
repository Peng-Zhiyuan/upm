namespace BattleEngine.Logic
{
    public sealed class SkillPassiveBeBreakAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_BE_BREAK;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostBreakStatus, OnOwnerBreakStatus);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostBreakStatus, OnOwnerBreakStatus);
        }

        private void OnOwnerBreakStatus(ActionExecution combatAction)
        {
            if (buffTrigger == null
                || combatAction == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            if (isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
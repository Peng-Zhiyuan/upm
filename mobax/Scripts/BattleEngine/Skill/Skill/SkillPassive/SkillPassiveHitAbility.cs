namespace BattleEngine.Logic
{
    public sealed class SkillPassiveHitAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_HIT;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnPostCauseDamage);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnPostCauseDamage);
        }

        private void OnPostCauseDamage(ActionExecution combatAction)
        {
            if (buffTrigger == null
                || combatAction == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            DamageAction damageAction = combatAction as DamageAction;
            if (damageAction == null)
            {
                return;
            }
            if (damageAction.Creator != null
                && damageAction.Creator.UID == SelfActorEntity.UID
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
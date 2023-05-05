namespace BattleEngine.Logic
{
    public sealed class SkillPassiveBeHitBlockAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_BEHIT_BLOCK;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, OnOwnerReciveDamage);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, OnOwnerReciveDamage);
        }

        private void OnOwnerReciveDamage(ActionExecution combatAction)
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
            if (damageAction == null
                || damageAction.Target == null
                || damageAction.Target.UID != SelfActorEntity.UID)
            {
                return;
            }
            if (damageAction.behitData.HasState(HitType.Block)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
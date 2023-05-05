namespace BattleEngine.Logic
{
    //自身攻击有指定buff的敌人时触发
    public sealed class SkillPassiveAttackTargetBuffAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_ATTACK_TARGETBUFF;
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
            if (IsTriggerTargetBuff(damageAction)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }

        private bool IsTriggerTargetBuff(DamageAction action)
        {
            CombatActorEntity targetEntity = action.Target;
            if (targetEntity == null
                || targetEntity.UID == SelfActorEntity.UID)
            {
                return false;
            }
            return targetEntity.HasBuff(buffTrigger.trigRate1);
        }
    }
}
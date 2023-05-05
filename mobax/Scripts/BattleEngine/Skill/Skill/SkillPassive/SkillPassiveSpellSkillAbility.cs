namespace BattleEngine.Logic
{
    public sealed class SkillPassiveSpellSkillAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_SPELL_SKILL_HIT;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity?.ListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnPostCauseDamage);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity?.UnListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnPostCauseDamage);
        }

        private void OnPostCauseDamage(ActionExecution combatAction)
        {
            if (buffTrigger == null
                || SelfActorEntity == null
                || CooldownTimer == null
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
                && damageAction.DamageSkillRow != null
                && damageAction.DamageSkillRow.skillType == buffTrigger.trigRate1
                && isTriggerChance())
            {
                if (buffTrigger.buffTarget == (int)BUFF_EFFECT_TARGET.HIT_TARGET)
                {
                    TriggerAddBuff(damageAction.Target);
                }
                else
                {
                    TriggerAddBuff();
                }
            }
        }
    }
}
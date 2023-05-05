/* Created:Loki Date:2022-10-18*/

namespace BattleEngine.Logic
{
    public sealed class SkillPassiveBuffControlAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_TARGET_BUFF_CONTROL;
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
            if (damageAction == null
                || damageAction.Creator == null
                || damageAction.Creator.UID == SelfActorEntity.UID)
            {
                return;
            }
            if (IsTriggerTargetBuffControlType(damageAction)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }

        private bool IsTriggerTargetBuffControlType(DamageAction action)
        {
            CombatActorEntity targetEntity = action.Target;
            if (targetEntity == null
                || targetEntity.UID == SelfActorEntity.UID)
            {
                return false;
            }
            return targetEntity.HasBuffControlType(buffTrigger.trigRate1);
        }
    }
}
/* Created:Loki Date:2022-10-18*/

namespace BattleEngine.Logic
{
    public sealed class SkillPassiveHitCritAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_HIT_CRIT;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnOwnerReciveDamage);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnOwnerReciveDamage);
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
                || damageAction.Creator == null
                || damageAction.Creator.UID != SelfActorEntity.UID)
            {
                return;
            }
            if (damageAction.behitData.HasState(HitType.Crit)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
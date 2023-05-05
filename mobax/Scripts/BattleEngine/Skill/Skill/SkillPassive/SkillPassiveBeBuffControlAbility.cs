/* Created:Loki Date:2022-10-18*/

namespace BattleEngine.Logic
{
    public sealed class SkillPassiveBeBuffControlAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_SELF_TARGET_BUFF;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, OnPostReceiveDamage);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, OnPostReceiveDamage);
        }

        private void OnPostReceiveDamage(ActionExecution combatAction)
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
            if (SelfActorEntity.HasBuffControlType(buffTrigger.trigRate1)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
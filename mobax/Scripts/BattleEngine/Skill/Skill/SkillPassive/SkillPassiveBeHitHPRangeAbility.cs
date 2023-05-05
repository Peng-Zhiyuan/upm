namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class SkillPassiveBeHitHPRangeAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_BEHIT_HP_RANGE;
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
            int hpPer = Mathf.FloorToInt(SelfActorEntity.CurrentHealth.Percent() * 1000);
            if (hpPer >= buffTrigger.trigRate1
                && hpPer <= buffTrigger.trigRate2
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    public class SkillPassiveAttackMPRangeAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_ATTACK_MP_RANGE;
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
                || damageAction.Creator.UID != SelfActorEntity.UID)
            {
                return;
            }
            if (SelfActorEntity.CurrentMp.Value >= buffTrigger.trigRate1
                && SelfActorEntity.CurrentMp.Value <= buffTrigger.trigRate2
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
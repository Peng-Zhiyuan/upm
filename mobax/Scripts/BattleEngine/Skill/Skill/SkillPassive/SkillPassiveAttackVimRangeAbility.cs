/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    public class SkillPassiveAttackVIMRangeAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_ATTACK_VIM_RANGE;
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
                || damageAction.Target == null
                || damageAction.Target.UID != SelfActorEntity.UID)
            {
                return;
            }
            if (SelfActorEntity.CurrentVim.Value >= buffTrigger.trigRate1
                && SelfActorEntity.CurrentVim.Value <= buffTrigger.trigRate2
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
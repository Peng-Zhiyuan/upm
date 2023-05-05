/* Created:Loki Date:2023-01-31*/

namespace BattleEngine.Logic
{
    public class SkillPassiveAtkVimEnemyAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_ATTACK_ENEMY_VIM;
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
                || damageAction.Target == null
                || damageAction.Creator == null
                || damageAction.Creator.UID != SelfActorEntity.UID)
            {
                return;
            }
            if (SelfActorEntity.CurrentVim.Value > damageAction.Target.CurrentVim.Value
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
/* Created:Loki Date:2023-01-31*/

namespace BattleEngine.Logic
{
    public class SkillPassiveSelfDeadAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_DEAD_SELF;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostDeadStatus, OnPostSelfDead);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostDeadStatus, OnPostSelfDead);
        }

        private void OnPostSelfDead(ActionExecution combatAction)
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
            if (isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
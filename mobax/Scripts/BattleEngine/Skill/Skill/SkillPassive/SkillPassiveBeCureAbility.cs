/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    public class SkillPassiveBeCureAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_BE_CURE;
        }

        protected override void RegisterPassiveEvent()
        {
            SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveCure, OnOwnerReciveCure);
        }

        protected override void UnRgisterPassiveEvent()
        {
            SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveCure, OnOwnerReciveCure);
        }

        private void OnOwnerReciveCure(ActionExecution combatAction)
        {
            if (buffTrigger == null
                || combatAction == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            CureAction damageAction = combatAction as CureAction;
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
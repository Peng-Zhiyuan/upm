/* Created:Loki Date:2023-04-13*/

namespace BattleEngine.Logic
{
    public class SkillPassiveAtkEnemySexAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_TARGET_SEX;
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
            HeroRow targetHeroRow = damageAction.Target.battleItemInfo.GetHeroRow();
            if (targetHeroRow.Gender == buffTrigger.trigRate1
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
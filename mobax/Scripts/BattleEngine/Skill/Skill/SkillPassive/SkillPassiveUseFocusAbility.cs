/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    public class SkillPassiveUseFocusAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_PLAYER_USE_FOCUS;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<int>("ExecuteAtkerFocusOnFiringEvent", CheckAtkerFocusOnFiringEvent);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<int>("ExecuteAtkerFocusOnFiringEvent", CheckAtkerFocusOnFiringEvent);
        }

        /// <summary>
        /// 玩家集火
        /// </summary>
        /// <param name="teamKey"></param>
        private void CheckAtkerFocusOnFiringEvent(int teamKey)
        {
            if (buffTrigger == null
                || SelfActorEntity.TeamKey != teamKey
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
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
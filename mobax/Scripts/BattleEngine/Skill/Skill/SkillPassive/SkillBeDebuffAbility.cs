/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    public class SkillBeDebuffAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_DEBUFF_TRIGGER;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<BuffAbility>("AttachBuffSuccess", AttachBuffSuccess);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<BuffAbility>("AttachBuffSuccess", AttachBuffSuccess);
        }

        /// <summary>
        /// Debuff触发
        /// </summary>
        private void AttachBuffSuccess(BuffAbility buffAbility)
        {
            if (buffTrigger == null
                || buffAbility == null
                || buffAbility.buffRow == null
                || buffAbility.buffRow.Gain != 1
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
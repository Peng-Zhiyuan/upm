namespace BattleEngine.Logic
{
    public sealed class SkillPassiveSSPSkillFinishAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_SSPSKILL_FINISH;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<SkillAbilityExecution>("OnEndSkillPointPoint", OnEndSkillPointPoint);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<SkillAbilityExecution>("OnEndSkillPointPoint", OnEndSkillPointPoint);
        }

        /// <summary>
        /// //释放大招结束后
        /// </summary>
        private void OnEndSkillPointPoint(SkillAbilityExecution combatAction)
        {
            if (buffTrigger == null
                || combatAction == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            if (IsTriggerSSPSkillFinish(combatAction)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }

        /// <summary>
        /// 释放大招结束后
        /// 0.自己 1.我方 2.敌方 3.全战场
        /// </summary>
        /// <returns></returns>
        private bool IsTriggerSSPSkillFinish(SkillAbilityExecution heroTurnOnAction)
        {
            if (buffTrigger == null
                || heroTurnOnAction == null)
            {
                return false;
            }
            if (buffTrigger.trigRate1 == 0
                && heroTurnOnAction.OwnerEntity.UID == SelfActorEntity.UID)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 1
                     && heroTurnOnAction.OwnerEntity.isAtker == SelfActorEntity.isAtker)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 2
                     && heroTurnOnAction.OwnerEntity.isAtker != SelfActorEntity.isAtker)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 3)
            {
                return true;
            }
            return false;
        }
    }
}
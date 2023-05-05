namespace BattleEngine.Logic
{
    public sealed class SkillPassiveEnterFightTimeAbility : SkillPassiveAbility
    {
        private bool isTrigger_PASSIVE_ENTER_FIGHT_TIME = false;

        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_ENTER_FIGHT_TIME;
        }

        public override void Awake(object _initData)
        {
            base.Awake(_initData);
            isTrigger_PASSIVE_ENTER_FIGHT_TIME = false;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<float>("BattleBeginExecute", OnGameTimePoint);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<float>("BattleBeginExecute", OnGameTimePoint);
        }

        /// <summary>
        /// 目前就战斗开始检查下
        /// </summary>
        /// <param name="combatAction"></param>
        private void OnGameTimePoint(float battleTime)
        {
            if (buffTrigger == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            if (!isTrigger_PASSIVE_ENTER_FIGHT_TIME
                && battleTime * 1000 >= buffTrigger.trigRate1
                && isTriggerChance())
            {
                isTrigger_PASSIVE_ENTER_FIGHT_TIME = false;
                TriggerAddBuff();
            }
        }
    }
}
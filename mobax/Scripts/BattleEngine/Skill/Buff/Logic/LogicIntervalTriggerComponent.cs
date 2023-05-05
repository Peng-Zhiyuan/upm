namespace BattleEngine.Logic
{
    /// <summary>
    /// 逻辑间隔触发组件
    /// </summary>
    public class LogicIntervalTriggerComponent : Component
    {
        public override bool Enable { get; set; } = true;
        private GameTimer IntervalTimer { get; set; }
        private LogicEntity selfLogicEntity;
        public LogicEntity SelfLogicEntity
        {
            get
            {
                if (selfLogicEntity == null)
                {
                    selfLogicEntity = GetEntity<LogicEntity>();
                }
                return selfLogicEntity;
            }
        }

        public override void Setup()
        {
            if (SelfLogicEntity.buffEffect != null)
            {
                var interval = SelfLogicEntity.buffEffect.Step * 0.001f;
                IntervalTimer = new GameTimer(interval);
            }
        }

        public void LogicUpdate(float deltaTime)
        {
            if (IntervalTimer != null)
            {
                IntervalTimer.UpdateAsRepeat(deltaTime, SelfLogicEntity.ApplyEffect);
            }
        }

        public void ResetTime(float step)
        {
            if (IntervalTimer != null)
            {
                IntervalTimer.MaxTime = step;
                IntervalTimer.Reset();
            }
        }
    }
}
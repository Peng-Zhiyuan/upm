namespace BattleEngine.Logic
{
    using System;

    public sealed class WhenInTimeNoDamageCondition : ConditionEntity
    {
        private GameTimer NoDamageTimer { get; set; }

        public override void Awake(object initData)
        {
            var time = (float)initData;
            NoDamageTimer = new GameTimer(time);
            OwnActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, WhenReceiveDamage);
        }

        public override void OnDestroy()
        {
            OwnActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, WhenReceiveDamage);
            base.OnDestroy();
        }

        public override void StartListen(Action whenNoDamageInTimeCallback) { }

        private void WhenReceiveDamage(ActionExecution combatAction)
        {
            NoDamageTimer.Reset();
        }
    }
}
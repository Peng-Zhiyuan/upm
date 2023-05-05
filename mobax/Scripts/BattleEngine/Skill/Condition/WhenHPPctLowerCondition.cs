namespace BattleEngine.Logic
{
    using System;

    public sealed class WhenHPPctLowerCondition : ConditionEntity
    {
        private float percent = 0;
        private Action WhenHPPctLowerCallback;

        public override void Awake(object initData)
        {
            this.percent = (float)initData;
            OwnActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, WhenReceiveDamage);
        }

        public override void OnDestroy()
        {
            OwnActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, WhenReceiveDamage);
            base.OnDestroy();
        }

        public override void StartListen(Action whenHPPctLowerCallback)
        {
            WhenHPPctLowerCallback = whenHPPctLowerCallback;
        }

        private void WhenReceiveDamage(ActionExecution combatAction)
        {
            if (OwnActorEntity.CurrentHealth.Percent() <= percent * 0.01f)
            {
                WhenHPPctLowerCallback();
            }
        }
    }
}
namespace BattleEngine.Logic
{
    using System;

    public sealed class WhenHPLowerCondition : ConditionEntity
    {
        private float hpvalue = 0;
        private Action WhenHPLowerCallback;

        public override void Awake(object initData)
        {
            hpvalue = (float)initData;
            OwnActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, WhenReceiveDamage);
        }

        public override void OnDestroy()
        {
            OwnActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, WhenReceiveDamage);
            base.OnDestroy();
        }

        public override void StartListen(Action whenHPLowerCallback)
        {
            WhenHPLowerCallback = whenHPLowerCallback;
        }

        private void WhenReceiveDamage(ActionExecution combatAction)
        {
            if (OwnActorEntity.CurrentHealth.Value <= hpvalue)
            {
                WhenHPLowerCallback();
            }
        }
    }
}
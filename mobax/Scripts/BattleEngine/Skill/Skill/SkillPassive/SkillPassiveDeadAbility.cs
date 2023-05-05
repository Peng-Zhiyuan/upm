namespace BattleEngine.Logic
{
    public sealed class SkillPassiveDeadAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_HAVE_DEAD_EVENT;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<CombatActorEntity>("OnTriggerDeadPoint", OnTriggerDeadPoint);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<CombatActorEntity>("OnTriggerDeadPoint", OnTriggerDeadPoint);
        }

        private void OnTriggerDeadPoint(CombatActorEntity deadActorEntity)
        {
            if (buffTrigger == null
                || deadActorEntity == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            if (IsTriggerDeadEffect(deadActorEntity)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }

        /// <summary>
        /// 场上有角色死亡时触发
        /// 0.自己 1.我方 2.敌人
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private bool IsTriggerDeadEffect(CombatActorEntity deadActorEntity)
        {
            if (buffTrigger == null
                || deadActorEntity == null)
            {
                return false;
            }
            if (buffTrigger.trigRate1 == 0
                && deadActorEntity.UID == SelfActorEntity.UID)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 1
                     && deadActorEntity.isAtker == SelfActorEntity.isAtker)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 2
                     && deadActorEntity.isAtker != SelfActorEntity.isAtker)
            {
                return true;
            }
            return false;
        }
    }
}
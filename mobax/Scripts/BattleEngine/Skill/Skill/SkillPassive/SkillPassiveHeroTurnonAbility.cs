namespace BattleEngine.Logic
{
    public sealed class SkillPassiveHeroTurnonAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_HERO_TURNON;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<CombatActorEntity>("OnHeroTurnOnPoint", OnHeroTurnOnPoint);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<CombatActorEntity>("OnHeroTurnOnPoint", OnHeroTurnOnPoint);
        }

        /// <summary>
        /// 角色从替补位上场 
        /// </summary>
        /// <param name="heroTurnOnActorEntity"></param>
        private void OnHeroTurnOnPoint(CombatActorEntity heroTurnOnActorEntity)
        {
            if (buffTrigger == null
                || heroTurnOnActorEntity == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            if (IsTriggerHeroTurnOn(heroTurnOnActorEntity)
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }

        /// <summary>
        /// 角色从替补位上场
        /// 0.自己 1.我方 2.敌方 3.全战场
        /// </summary>
        /// <returns></returns>
        private bool IsTriggerHeroTurnOn(CombatActorEntity heroTurnOnActorEntity)
        {
            if (buffTrigger == null
                || heroTurnOnActorEntity == null)
            {
                return false;
            }
            if (buffTrigger.trigRate1 == 0
                && heroTurnOnActorEntity.UID == SelfActorEntity.UID)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 1
                     && heroTurnOnActorEntity.isAtker == SelfActorEntity.isAtker)
            {
                return true;
            }
            else if (buffTrigger.trigRate1 == 2
                     && heroTurnOnActorEntity.isAtker != SelfActorEntity.isAtker)
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
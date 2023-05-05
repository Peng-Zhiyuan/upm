namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public sealed class SkillPassiveSexHeroAbility : SkillPassiveAbility
    {
        private bool isTrigger_PASSIVE_ENTER_FIGHT_TIME = false;

        public override void Awake(object _initData)
        {
            base.Awake(_initData);
            isTrigger_PASSIVE_ENTER_FIGHT_TIME = false;
        }

        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_HAVE_SEX_HERO;
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
                && IsTriggerSex()
                && battleTime == 0.0f
                && isTriggerChance())
            {
                isTrigger_PASSIVE_ENTER_FIGHT_TIME = false;
                TriggerAddBuff();
            }
        }

        /// <summary>
        /// 场上，存在特定性别的角色
        /// 0.自己 1.我方 2.敌方 3.全战场
        /// </summary>
        /// <returns></returns>
        private bool IsTriggerSex()
        {
            if (buffTrigger == null
                || SelfActorEntity.IsDead)
            {
                return false;
            }
            List<CombatActorEntity> lst = new List<CombatActorEntity>();
            if (buffTrigger.trigRate1 == 0)
            {
                HeroRow row = StaticData.HeroTable.TryGet(SelfActorEntity.ConfigID);
                if (row == null)
                {
                    return false;
                }
                return row.Gender == buffTrigger.trigRate2;
            }
            if (buffTrigger.trigRate1 == 1
                || buffTrigger.trigRate1 == 3)
            {
                lst = BattleLogicManager.Instance.BattleData.GetTeamList(SelfActorEntity.TeamKey);
                int trigNum = 0;
                for (int i = 0; i < lst.Count; i++)
                {
                    HeroRow row = StaticData.HeroTable.TryGet(lst[i].ConfigID);
                    if (row == null)
                    {
                        continue;
                    }
                    if (row.Gender == buffTrigger.trigRate2)
                    {
                        trigNum += 1;
                    }
                }
                if (trigNum > 0
                    && trigNum >= buffTrigger.trigRate3)
                {
                    return true;
                }
            }
            if (buffTrigger.trigRate1 == 2
                || buffTrigger.trigRate1 == 3)
            {
                lst = BattleLogicManager.Instance.BattleData.GetEnemyLst(SelfActorEntity);
                int trigNum = 0;
                for (int i = 0; i < lst.Count; i++)
                {
                    HeroRow row = StaticData.HeroTable.TryGet(lst[i].ConfigID);
                    if (row == null)
                    {
                        continue;
                    }
                    if (row.Gender == buffTrigger.trigRate2)
                    {
                        trigNum += 1;
                    }
                }
                if (trigNum > 0
                    && trigNum >= buffTrigger.trigRate3)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
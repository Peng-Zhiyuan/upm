/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class SkillPassiveTargetQLVHeroAbility : SkillPassiveAbility
    {
        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_TARGET_QLV;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<int>("BattleWaveExecute", OnWaveExecutePoint);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<int>("BattleWaveExecute", OnWaveExecutePoint);
        }

        private void OnWaveExecutePoint(int waveIndex)
        {
            if (buffTrigger == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished)
            {
                return;
            }
            bool isExist = false;
            List<CombatActorEntity> allLst = BattleLogicManager.Instance.BattleData.allActorLst;
            HeroRow heroRow = null;
            for (int i = 0; i < allLst.Count; i++)
            {
                if (!allLst[i].IsDead)
                {
                    heroRow = allLst[i].battleItemInfo.GetHeroRow();
                    if (heroRow.Qlv == buffTrigger.trigRate1)
                    {
                        isExist = true;
                        break;
                    }
                }
            }
            if (isExist && isTriggerChance())
            {
                TriggerAddBuff();
            }
        }
    }
}
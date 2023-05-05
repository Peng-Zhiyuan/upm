/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    public class SkillPassiveWaveExecuteAbility : SkillPassiveAbility
    {
        private int currentWaveIndex = 0;

        public override BUFF_TRIGGER_TYPE GetPassiveType()
        {
            return BUFF_TRIGGER_TYPE.PASSIVE_WAVE_EXECUTE;
        }

        public override void Awake(object _initData)
        {
            base.Awake(_initData);
            currentWaveIndex = 0;
        }

        protected override void RegisterPassiveEvent()
        {
            EventManager.Instance.AddListener<int>("BattleWaveExecute", OnWaveExecutePoint);
        }

        protected override void UnRgisterPassiveEvent()
        {
            EventManager.Instance.RemoveListener<int>("BattleWaveExecute", OnWaveExecutePoint);
        }

        /// <summary>
        /// 波次执行前检查
        /// </summary>
        /// <param name="waveIndex">波次Index</param>
        private void OnWaveExecutePoint(int waveIndex)
        {
            if (buffTrigger == null
                || SelfActorEntity.IsDead
                || !Enable
                || !CooldownTimer.IsFinished
                || currentWaveIndex == waveIndex)
            {
                currentWaveIndex = waveIndex;
                return;
            }
            if (currentWaveIndex != waveIndex
                && isTriggerChance())
            {
                TriggerAddBuff();
            }
            currentWaveIndex = waveIndex;
        }
    }
}
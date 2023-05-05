/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 击退波次判定胜利
    /// </summary>
    public sealed class BattleWaveResultLogic : BattleResultLogic
    {
        /// <summary>
        /// 战斗结束波次
        /// </summary>
        private int BattleWaveEnd = 1;
        private int currentWaveEnd = 1;

        public override void Init(object param)
        {
            base.Init(param);
            BattleWaveEnd = (int)param;
            currentWaveEnd = 0;
            EventManager.Instance.AddListener<int>("BattleWaveFinish", ChangeBattleWave);
        }

        public override void Dispose()
        {
            base.Dispose();
            EventManager.Instance.RemoveListener<int>("BattleWaveFinish", ChangeBattleWave);
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.WAVE_NUM;
        }

        public override string GetResultConditionParam()
        {
            return BattleWaveEnd.ToString();
        }

        private void ChangeBattleWave(int wave)
        {
            currentWaveEnd = wave;
        }

        public override bool CheckBattleEnd()
        {
            if (currentWaveEnd >= BattleWaveEnd)
            {
                _isBattleWin = true;
                return true;
            }
            return false;
        }
    }
}
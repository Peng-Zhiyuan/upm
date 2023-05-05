/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    /// <summary>
    /// 战斗时间判定胜利
    /// </summary>
    public sealed class BattleTimeResultLogic : BattleResultLogic
    {
        private float _battleTime = 0;

        public override void Init(System.Object param)
        {
            _battleTime = ((int)param * 0.001f);
            _isBattleWin = true;
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.TIMEEND;
        }

        public override string GetResultConditionParam()
        {
            return Mathf.FloorToInt(_battleTime).ToString();
        }

        public override bool CheckBattleEnd()
        {
            if (BattleTimeManager.Instance.CurrentBattleTime >= _battleTime)
            {
                _isBattleWin = false;
                return true;
            }
            return false;
        }
    }
}
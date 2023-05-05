/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    using System;

    /// <summary>
    /// 战斗时间判定胜利
    /// </summary>
    public sealed class BattleDefendTimeResultLogic : BattleResultLogic
    {
        private float _battleTime = 0;

        public override void Init(Object param)
        {
            _battleTime = ((int)param * 0.001f);
            _isBattleWin = false;
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.DEFEND_TIME;
        }

        public override string GetResultConditionParam()
        {
            return Math.Ceiling(_battleTime).ToString();
        }

        public override bool CheckBattleEnd()
        {
            if (BattleTimeManager.Instance.CurrentBattleTime >= _battleTime)
            {
                _isBattleWin = true;
                return true;
            }
            return false;
        }
    }
}
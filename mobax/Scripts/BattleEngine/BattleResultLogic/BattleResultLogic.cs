/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    using System;

    public abstract class BattleResultLogic
    {
        protected bool _isBattleWin = false;

        public virtual void Init(Object param)
        {
            _isBattleWin = false;
        }

        public virtual void Dispose() { }
        public abstract bool CheckBattleEnd();
        public abstract BATTLE_RESULT_TYPE GetResultType();
        public abstract string GetResultConditionParam();

        public bool GetBattleResult()
        {
            return _isBattleWin;
        }
    }
}
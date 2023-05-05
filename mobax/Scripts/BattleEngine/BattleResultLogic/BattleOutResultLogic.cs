/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    public sealed class BattleOutResultData
    {
        public bool bWin = false;
    }

    /// <summary>
    /// 外部强制结束战斗
    /// </summary>
    public sealed class BattleOutResultLogic : BattleResultLogic
    {
        private bool bFinish = false;

        public override void Init(object param)
        {
            base.Init(param);
            EventManager.Instance.AddListener<BattleOutResultData>("SetBattleResult", OutResultHandler);
        }

        public override void Dispose()
        {
            EventManager.Instance.RemoveListener<BattleOutResultData>("SetBattleResult", OutResultHandler);
        }

        private void OutResultHandler(BattleOutResultData data)
        {
            _isBattleWin = data.bWin;
            bFinish = true;
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.OUTCHECKRESULT;
        }

        public override string GetResultConditionParam()
        {
            return "";
        }

        public override bool CheckBattleEnd()
        {
            return bFinish;
        }
    }
}
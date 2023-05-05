/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 击杀数量敌人
    /// </summary>
    public sealed class BattleKillNumResultLogic : BattleResultLogic
    {
        private int killNum = 1;
        private int currentKillNum = 0;

        public override void Init(object param)
        {
            base.Init(param);
            killNum = (int)param;
            currentKillNum = 0;
            EventManager.Instance.AddListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        }

        public override void Dispose()
        {
            EventManager.Instance.RemoveListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.KILL_NUM;
        }

        public override string GetResultConditionParam()
        {
            return killNum.ToString();
        }

        private void CheckKillTargetEvent(CombatActorEntity deadActor)
        {
            if (deadActor.isAtker)
            {
                return;
            }
            currentKillNum += 1;
        }

        public override bool CheckBattleEnd()
        {
            if (currentKillNum >= killNum)
            {
                _isBattleWin = true;
                return true;
            }
            return false;
        }
    }
}
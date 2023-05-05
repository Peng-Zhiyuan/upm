/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    /// <summary>
    /// 队伍死光光
    /// </summary>
    public sealed class BattleTeamStateResultLogic : BattleResultLogic
    {
        private bool AttckAllDie = true;
        private bool isDefWholeDie = true;

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.TEAM_DOOM;
        }

        public override string GetResultConditionParam()
        {
            return "";
        }

        public override bool CheckBattleEnd()
        {
            if (BattleLogicManager.Instance.BattleData == null)
            {
                return false;
            }
            AttckAllDie = true;
            var data = BattleLogicManager.Instance.BattleData.atkActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (!CheckTeamDie(data.Current.Value))
                {
                    AttckAllDie = false;
                    break;
                }
            }
            isDefWholeDie = true;
            data = BattleLogicManager.Instance.BattleData.defActorDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (!CheckTeamDie(data.Current.Value))
                {
                    isDefWholeDie = false;
                    break;
                }
            }
            if (AttckAllDie)
            {
                _isBattleWin = false;
                return true;
            }
            else if (isDefWholeDie)
            {
                _isBattleWin = true;
                return true;
            }
            return false;
        }

        private bool CheckTeamDie(List<CombatActorEntity> _actorList)
        {
            for (int i = 0; i < _actorList.Count; i++)
            {
                if (_actorList[i] == null
                    || _actorList[i].PosIndex == BattleConst.PlayerPosIndex
                    || _actorList[i].PosIndex == BattleConst.FriendPosIndex
                    || _actorList[i].PosIndex == BattleConst.SSPAssistPosIndexStart)
                {
                    continue;
                }
                if (_actorList[i].CurrentHealth.Value > 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
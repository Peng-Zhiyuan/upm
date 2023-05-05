/* Created:Loki Date:2022-09-19*/

using System.Collections.Generic;

namespace BattleEngine.Logic
{
    /// <summary>
    /// 守护目标
    /// </summary>
    public sealed class BattleDefendTargetResultLogic : BattleResultLogic
    {
        private string DefendTargetUID = "";
        private bool isDefendTargetSuccess = true;
        private bool isAtker = true;

        public override void Init(object param)
        {
            base.Init(param);
            DefendTargetUID = param.ToString();
            isDefendTargetSuccess = true;
            _isBattleWin = true;
            EventManager.Instance.AddListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
            CombatActorEntity actorEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(DefendTargetUID);
            isAtker = actorEntity.isAtker;
        }

        public override void Dispose()
        {
            EventManager.Instance.RemoveListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.DEFEND_TARGET;
        }

        public override string GetResultConditionParam()
        {
            CombatActorEntity actorEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(DefendTargetUID);
            if (actorEntity != null)
            {
                HeroRow row = actorEntity.battleItemInfo.GetHeroRow();
                return LocalizationManager.Stuff.GetText(row.Name);
            }
            return "";
        }

        private void CheckKillTargetEvent(CombatActorEntity deadActor)
        {
            if (DefendTargetUID.Equals(deadActor.UID))
            {
                isDefendTargetSuccess = false;
                _isBattleWin = false;
            }
        }

        public override bool CheckBattleEnd()
        {
            if (!isDefendTargetSuccess)
            {
                return true;
            }
            if (CheckTeamDie())
            {
                return true;
            }
            return false;
        }

        private bool CheckTeamDie()
        {
            if (isAtker)
            {
                var actorList = BattleLogicManager.Instance.BattleData.atkActorLst;
                for (int i = 0; i < actorList.Count; i++)
                {
                    if (actorList[i] == null
                        || actorList[i].PosIndex == BattleConst.PlayerPosIndex
                        || actorList[i].PosIndex == BattleConst.FriendPosIndex
                        || actorList[i].PosIndex == BattleConst.SSPAssistPosIndexStart
                        || actorList[i].UID == DefendTargetUID)
                    {
                        continue;
                    }
                    if (actorList[i].CurrentHealth.Value > 0)
                    {
                        return false;
                    }
                }
                _isBattleWin = false;
                return true;
            }
            return false;
        }
    }
}
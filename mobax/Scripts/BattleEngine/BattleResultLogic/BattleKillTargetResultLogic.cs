/* Created:Loki Date:2022-09-19*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 击杀特定敌人判定胜利
    /// </summary>
    public sealed class BattleKillTargetResultLogic : BattleResultLogic
    {
        private string killTargetUID = "";
        private bool isKillTarget = false;

        public override void Init(object param)
        {
            base.Init(param);
            killTargetUID = param.ToString();
            isKillTarget = false;
            EventManager.Instance.AddListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        }

        public override void Dispose()
        {
            EventManager.Instance.RemoveListener<CombatActorEntity>("OnTriggerDeadPoint", CheckKillTargetEvent);
        }

        public override BATTLE_RESULT_TYPE GetResultType()
        {
            return BATTLE_RESULT_TYPE.KILL_TARGET;
        }

        public override string GetResultConditionParam()
        {
            CombatActorEntity actorEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(killTargetUID);
            if (actorEntity != null)
            {
                HeroRow row = actorEntity.battleItemInfo.GetHeroRow();
                return LocalizationManager.Stuff.GetText(row.Name);
            }
            return "";
        }

        private void CheckKillTargetEvent(CombatActorEntity deadActor)
        {
            if (killTargetUID.Equals(deadActor.UID))
            {
                isKillTarget = true;
                _isBattleWin = true;
            }
        }

        public override bool CheckBattleEnd()
        {
            if (isKillTarget)
            {
                return true;
            }
            return false;
        }
    }
}
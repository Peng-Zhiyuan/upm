using UnityEngine;

namespace BattleEngine.Logic
{
    public class BuffForbidCureLogic : BuffEffectLogic
    {
        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            buffAbility.SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PreReceiveCure, OnOwnerPreReciveCure);
            BattleLog.LogWarning("[BUFF] 接受治疗前 ");
        }

        public void OnOwnerPreReciveCure(ActionExecution combatAction)
        {
            var curAction = combatAction as CureAction;
            if (curAction != null)
            {
                curAction.CureValue = 0;
                buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
            }
        }

        public override void ExecuteLogic() { }

        public override void EndLogic()
        {
            buffAbility.SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PreReceiveCure, OnOwnerPreReciveCure);
        }
    }
}
/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 强制承受伤害值
    /// </summary>
    public class BuffForceDamageValueLogic : BuffEffectLogic
    {
        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            buffAbility.SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, OnOwnerPreReciveDamage);
            BattleLog.LogWarning("[BUFF] 强制抵御伤害值 ");
        }

        public void OnOwnerPreReciveDamage(ActionExecution combatAction)
        {
            var damageAction = combatAction as DamageAction;
            damageAction.DamageValue = buffEffect.Param1;
            buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
        }

        public override void ExecuteLogic() { }

        public override void EndLogic()
        {
            buffAbility.SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, OnOwnerPreReciveDamage);
        }
    }
}
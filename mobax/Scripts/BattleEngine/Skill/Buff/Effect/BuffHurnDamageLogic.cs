/* Created:Loki Date:2022-08-31*/

using UnityEngine;

namespace BattleEngine.Logic
{
    /// <summary>
    /// 吸收护盾
    /// </summary>
    public class BuffHurnDamageLogic : BuffEffectLogic
    {
        private int buffHp = -1;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            buffHp = Mathf.FloorToInt((buffAbility.Caster.AttrData.Att_Attack * buffEffect.Param1 * 0.001f + buffEffect.Param2) * (1 + buffAbility.Caster.AttrData.ATT_MANA_ATK_VALUE((MANA_TYPE)buffAbility.buffRow.Element) * 0.001f));
            buffAbility.SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, OnOwnerPreReciveDamage);
            BattleLog.LogWarning("[BUFF] 总抵御伤害 " + buffHp);
        }

        public void OnOwnerPreReciveDamage(ActionExecution combatAction)
        {
            var damageAction = combatAction as DamageAction;
            if (buffHp > 0)
            {
                if (buffHp > damageAction.DamageValue
                    && damageAction.DamageValue > 0)
                {
                    BattleLog.LogWarning("[BUFF] 护盾低伤" + damageAction.DamageValue);
                    buffHp -= damageAction.DamageValue;
                    damageAction.DamageValue = 0;
                }
                else if (buffHp < damageAction.DamageValue)
                {
                    BattleLog.LogWarning("[BUFF] 护盾低伤" + buffHp);
                    damageAction.DamageValue -= buffHp;
                    buffHp = 0;
                }
            }
            buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
            if (buffHp == 0)
            {
                buffAbility.EndAbility();
            }
        }

        public override void ExecuteLogic() { }

        public override void EndLogic()
        {
            buffHp = -1;
            buffAbility.SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, OnOwnerPreReciveDamage);
        }
    }
}
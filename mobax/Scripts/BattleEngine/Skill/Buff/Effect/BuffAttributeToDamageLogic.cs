/* Created:Loki Date:2023-01-30*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    public class BuffAttributeToDamageLogic : BuffEffectLogic
    {
        private DamageAction buffDamage = null;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            int damageBase = 0;
            if (buffDamage != null)
            {
                Entity.Destroy(buffDamage);
                buffDamage = null;
            }
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.MAX_HP_DAMAGE)
            {
                damageBase = Mathf.Max(0, buffAbility.SelfActorEntity.CurrentHealth.MaxValue);
            }
            else if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.CURRENT_HP_DAMAGE)
            {
                damageBase = Mathf.Max(0, buffAbility.SelfActorEntity.CurrentHealth.Value);
            }
            int damageValue = buffAbility.StackNum * Mathf.FloorToInt((damageBase * buffEffect.Param1 * 0.001f + buffEffect.Param2));
            if (damageValue == 0)
            {
                BattleLog.LogWarning("[BUFF] 属性百分比伤害为0");
                return;
            }
            if (buffAbility.SelfActorEntity.battleItemInfo.isBoss)
            {
                damageValue = 0;
            }
            buffDamage = buffAbility.SelfActorEntity.CreateCombatAction<DamageAction>();
            buffDamage.DamageValue = damageValue;
            buffDamage.Creator = buffAbility.Caster;
            buffDamage.Target = buffAbility.SelfActorEntity;
            buffDamage.attackDamage = false;
            buffDamage.behitData = new BehitData();
            buffDamage.behitData.SetState(HitType.BuffDamage);
        }

        public override void ExecuteLogic()
        {
            if (buffDamage != null)
            {
                buffAbility.SelfActorEntity.TriggerActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, buffDamage);
                buffAbility.SelfActorEntity.ReceiveDamage(buffDamage, false);
                buffAbility.SelfActorEntity.battleItemInfo.battlePlayerRecord.ReceiveDamageValue += buffDamage.DamageValue;
                if (buffAbility.Caster != null)
                {
                    buffAbility.Caster.battleItemInfo.battlePlayerRecord.OP_AttackValue += buffDamage.DamageValue;
                }
                buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
                BattleLog.LogWarning("[BUFF] BUFF伤害" + buffAbility.SelfActorEntity.ConfigID + " " + buffDamage.DamageValue);
            }
        }

        public override void EndLogic()
        {
            buffDamage.EndExecute();
            buffDamage = null;
        }
    }
}
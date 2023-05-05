/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 伤害系数
    /// </summary>
    public class BuffDamageValueLogic : BuffEffectLogic
    {
        private CureAction cureAction = null;
        private DamageAction buffDamage = null;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            cureAction = null;
            buffDamage = null;
            int stackNum = _buffAbility.StackNum;
            if (buffEffect.Param3 > 0)
            {
                BuffAbility targetBuffAbility = buffAbility.SelfActorEntity.GetBuff(buffEffect.Param3);
                if (targetBuffAbility != null)
                {
                    stackNum += targetBuffAbility.StackNum;
                }
            }
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.CURE)
            {
                cureAction = buffAbility.SelfActorEntity.CreateCombatAction<CureAction>();
                cureAction.Creator = buffAbility.Caster;
                cureAction.Target = buffAbility.SelfActorEntity;
                cureAction.behitData = new BehitData();
                Formula.CacBufferDamage(cureAction.behitData, buffAbility.Caster, buffAbility.SelfActorEntity, (MANA_TYPE)buffAbility.buffRow.Element, buffEffect);
                cureAction.CureValue = cureAction.behitData.damage * stackNum;
            }
            else
            {
                buffDamage = buffAbility.SelfActorEntity.CreateCombatAction<DamageAction>();
                buffDamage.Creator = buffAbility.Caster;
                buffDamage.Target = buffAbility.SelfActorEntity;
                buffDamage.attackDamage = false;
                buffDamage.behitData = new BehitData();
                Formula.CacBufferDamage(buffDamage.behitData, buffAbility.Caster, buffAbility.SelfActorEntity, (MANA_TYPE)buffAbility.buffRow.Element, buffEffect);
                buffDamage.DamageValue = buffDamage.behitData.damage * stackNum;
            }
        }

        public override void ExecuteLogic()
        {
            if (buffEffect == null)
            {
                return;
            }
            if (cureAction != null)
            {
                buffAbility.SelfActorEntity.TriggerActionPoint(ACTION_POINT_TYPE.PreReceiveCure, cureAction);
                buffAbility.SelfActorEntity.ReceiveCure(cureAction);
                buffAbility.SelfActorEntity.battleItemInfo.battlePlayerRecord.ReceiveCureValue += cureAction.CureValue;
                if (buffAbility.Caster == null)
                {
                    buffAbility.SelfActorEntity.battleItemInfo.battlePlayerRecord.OP_CureValue += cureAction.CureValue;
                }
                else
                {
                    buffAbility.Caster.battleItemInfo.battlePlayerRecord.OP_CureValue += cureAction.CureValue;
                }
                buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
                BattleLog.LogWarning("[BUFF] 回血" + buffAbility.SelfActorEntity.ConfigID + " " + cureAction.CureValue);
            }
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
                BattleLog.LogWarning("[BUFF] 伤害" + buffAbility.SelfActorEntity.ConfigID + " " + buffDamage.DamageValue);
            }
        }

        public override void EndLogic()
        {
            buffDamage?.EndExecute();
            buffDamage = null;
            cureAction?.EndExecute();
            cureAction = null;
        }
    }
}
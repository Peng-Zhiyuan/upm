/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    /// <summary>
    /// 攻击吸血
    /// </summary>
    public class BuffSuckLogic : BuffEffectLogic
    {
        private BuffEffect suckBuff = null;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            suckBuff = null;
            buffAbility.SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnOwnerCauseDamage);
        }

        public void OnOwnerCauseDamage(ActionExecution combatAction)
        {
            if (suckBuff == null)
            {
                return;
            }
            var damageAction = combatAction as DamageAction;
            int suckHP = buffAbility.StackNum * Mathf.FloorToInt((damageAction.DamageValue * buffEffect.Param1 * 0.001f + buffEffect.Param2) * (1 + buffAbility.SelfActorEntity.AttrData.GetValue(AttrType.CURE) * 0.001f));
            if (suckHP > 0)
            {
                CureAction cureAction = buffAbility.SelfActorEntity.CreateCombatAction<CureAction>();
                cureAction.CureValue = suckHP;
                cureAction.Creator = buffAbility.SelfActorEntity;
                cureAction.behitData = new BehitData();
                cureAction.behitData.SetState(HitType.Cure);
                cureAction.behitData.damage = suckHP;
                buffAbility.SelfActorEntity.TriggerActionPoint(ACTION_POINT_TYPE.PreReceiveCure,cureAction);
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
                BattleLog.LogWarning("[BUFF] 吸血" + cureAction.CureValue);
            }
        }

        public override void ExecuteLogic()
        {
            suckBuff = buffEffect;
        }

        public override void EndLogic()
        {
            suckBuff = null;
            buffAbility.SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostCauseDamage, OnOwnerCauseDamage);
        }
    }
}
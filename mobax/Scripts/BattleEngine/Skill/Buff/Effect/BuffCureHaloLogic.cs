/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    /// <summary>
    /// 1.最大生命比例回血
    /// 2.受损生命比例回血
    /// </summary>
    public class BuffCureHaloLogic : BuffEffectLogic
    {
        private CureAction cureAction;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            if (cureAction != null)
            {
                Entity.Destroy(cureAction);
                cureAction = null;
            }
            int cureBase = 0;
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.CURE_HALO)
            {
                cureBase = Mathf.Max(0, buffAbility.SelfActorEntity.CurrentHealth.MaxValue);
            }
            else if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.CURE_DAMAGE_HALO)
            {
                cureBase = Mathf.Max(0, buffAbility.SelfActorEntity.CurrentHealth.MaxValue - buffAbility.SelfActorEntity.CurrentHealth.Value);
            }
            int addHP = buffAbility.StackNum * Mathf.FloorToInt((cureBase * buffEffect.Param1 * 0.001f + buffEffect.Param2) * (1 + buffAbility.SelfActorEntity.AttrData.GetValue(AttrType.CURE) * 0.001f));
            if (addHP == 0)
            {
                BattleLog.LogWarning("[BUFF] 治疗为0");
                return;
            }
            cureAction = buffAbility.SelfActorEntity.CreateCombatAction<CureAction>();
            cureAction.CureValue = addHP;
            cureAction.Creator = buffAbility.SelfActorEntity;
            cureAction.behitData = new BehitData();
            cureAction.behitData.SetState(HitType.Cure);
        }

        public override void ExecuteLogic()
        {
            if (cureAction == null)
            {
                return;
            }
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
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 生命值回血", cureAction.CureValue.ToString()));
        }

        public override void EndLogic() { }
    }
}
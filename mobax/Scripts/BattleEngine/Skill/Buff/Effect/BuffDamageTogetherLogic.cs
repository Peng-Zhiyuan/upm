/* Created:Loki Date:2022-10-18*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    public class BuffDamageTogetherLogic : BuffEffectLogic
    {
        private BuffEffect damageShareBuff = null;
        private CombatActorEntity damageShareActorEntity;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            buffAbility.SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, OnOwnerPreReciveDamage);
        }

        public void OnOwnerPreReciveDamage(ActionExecution combatAction)
        {
            if (damageShareBuff == null
                || damageShareActorEntity == null)
            {
                return;
            }
            if (damageShareActorEntity.IsCantSelect
                || buffAbility.SelfActorEntity.UID == damageShareActorEntity.UID)
            {
                buffAbility.EndAbility();
                return;
            }
            var damageAction = combatAction as DamageAction;
            int shareDamageValue = Mathf.FloorToInt(damageAction.DamageValue * damageShareBuff.Param1 * 0.001f);
            if (shareDamageValue >= damageShareActorEntity.CurrentHealth.Value)
            {
                shareDamageValue = damageShareActorEntity.CurrentHealth.Value - 1;
            }
            DamageAction shareDamageAction = damageShareActorEntity.CreateCombatAction<DamageAction>();
            shareDamageAction.DamageValue = shareDamageValue;
            shareDamageAction.Creator = buffAbility.Caster;
            shareDamageAction.Target = damageShareActorEntity;
            shareDamageAction.attackDamage = false;
            damageShareActorEntity.ReceiveDamage(shareDamageAction, true);
            buffAbility.SelfActorEntity.battleItemInfo.battlePlayerRecord.ReceiveDamageValue += shareDamageValue;
            if (buffAbility.Caster != null)
            {
                buffAbility.Caster.battleItemInfo.battlePlayerRecord.OP_AttackValue += shareDamageValue;
            }
            //剩余伤害
            damageAction.DamageValue = damageAction.DamageValue - shareDamageValue;
            buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 伤害分担:", damageShareActorEntity.ConfigID.ToString(), "  damage : ", shareDamageValue.ToString()));
        }

        public override void ExecuteLogic()
        {
            damageShareBuff = buffEffect;
            damageShareActorEntity = buffAbility.Caster;
        }

        public override void EndLogic()
        {
            buffAbility.SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, OnOwnerPreReciveDamage);
        }
    }
}
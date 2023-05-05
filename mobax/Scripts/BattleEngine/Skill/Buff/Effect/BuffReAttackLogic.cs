/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    /// <summary>
    /// 反弹伤害
    /// </summary>
    public class BuffReAttackLogic : BuffEffectLogic
    {
        private BuffEffect reAttackBuff = null;
        private DamageAction buffDamage = null;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            buffAbility.SelfActorEntity.ListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, OnOwnerTrueReciveDamage);
            reAttackBuff = null;
        }

        public void OnOwnerTrueReciveDamage(ActionExecution combatAction)
        {
            if (reAttackBuff == null)
            {
                return;
            }
            var damageAction = combatAction as DamageAction;
            if (damageAction == null
                || damageAction.DamageValue <= 0
                || damageAction.Creator == null
                || damageAction.Creator.UID == buffAbility.SelfActorEntity.UID)
            {
                return;
            }
            int reAttackValue = buffAbility.StackNum * Mathf.FloorToInt(damageAction.DamageValue * buffEffect.Param1 * 0.001f + buffEffect.Param2);
            DamageAction reAttackDamageAction = damageAction.Creator.CreateCombatAction<DamageAction>();
            reAttackDamageAction.DamageValue = reAttackValue;
            reAttackDamageAction.Creator = buffAbility.SelfActorEntity;
            reAttackDamageAction.Target = damageAction.Creator;
            reAttackDamageAction.attackDamage = false;
            reAttackDamageAction.behitData = new BehitData();
            reAttackDamageAction.behitData.SetState(HitType.AntDamage);
            damageAction.Creator.ReceiveDamage(reAttackDamageAction, true);
            buffAbility.SelfActorEntity.OnBuffTrigger(buffAbility);
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 反弹伤害 ", reAttackValue.ToString()));
        }

        public override void ExecuteLogic()
        {
            reAttackBuff = buffEffect;
        }

        public override void EndLogic()
        {
            reAttackBuff = null;
            buffAbility.SelfActorEntity.UnListenActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, OnOwnerTrueReciveDamage);
        }
    }
}
namespace BattleEngine.Logic
{
    using UnityEngine;

    public class AssignEffectActionAbility : ActionAbility<AssignEffectAction> { }

    public class AssignEffectAction : ActionExecution<AssignEffectActionAbility>
    {
        public AbilityEntity SourceAbility { get; set; }
        public Effect Effect { get; set; }
        //public BuffRow buffRow = null;
        public BuffAbility Buff { get; set; }
        private void PreProcess() { }

        public void ApplyAssignEffect()
        {
            PreProcess();
            if (Effect != null
                && Effect is AddBuffEffect addBuffEffect)
            {
                bool isSuccess = true;
                var buffConfig = addBuffEffect.buffRow;
                if (buffConfig.controlType != (int)ACTION_CONTROL_TYPE.None)
                {
                    isSuccess = BattleControlUtil.RefreshBuffControlState(Target, (ACTION_CONTROL_TYPE)buffConfig.controlType);
                    if (!isSuccess)
                    {
                        return;
                    }
                }
                int addBuffStackNum = 1;
                if (addBuffEffect.buffTrigger != null)
                {
                    addBuffStackNum = Mathf.Max(1, addBuffEffect.buffTrigger.Layer);
                }
                var buff = Target.GetBuff(buffConfig.BuffID);
                if (buff != null)
                {
                    if (buff.buffRow.Level < buffConfig.Level)
                    {
                        buff.EndAbility();
                    }
                    else if (buff.buffRow.Level > buffConfig.Level)
                    {
                        return;
                    }
                    else
                    {
                        buff.StackNum = Mathf.Min(buffConfig.Stack, buff.StackNum + addBuffStackNum);
                        buff.TryActivateAbility();
                        EventManager.Instance.SendEvent<CombatActorEntity>("BattleRefreshBuff", buff.SelfActorEntity);
                        return;
                    }
                }
                Buff = CreateBuffAbility(buffConfig);
                if (Buff != null)
                {
                    Buff.buffRow = (Effect as AddBuffEffect).buffRow;
                    Buff.Caster = Creator;
                    Buff.Level = Buff.buffRow.Level;
                    Buff.StackNum = Mathf.Min(buffConfig.Stack, addBuffStackNum);
                    if (Buff.buffRow.Time > 0)
                    {
                        Buff.AddComponent<BuffLifeTimeComponent>();
                    }
                    Buff.TryActivateAbility();
                    EventManager.Instance.SendEvent<CombatActorEntity>("BattleRefreshBuff", Buff.SelfActorEntity);
                }
                PostProcess();
            }
            ApplyAction();
        }

        private void PostProcess()
        {
            if (Effect is AddBuffEffect addBuffEffect)
            {
                Creator.TriggerActionPoint(ACTION_POINT_TYPE.PostGiveStatus, this);
                Target.TriggerActionPoint(ACTION_POINT_TYPE.PostReceiveStatus, this);
                Destroy(this);
            }
        }

        private BuffAbility CreateBuffAbility(BuffRow buffConfig)
        {
            BuffAbility buff = Target.AttachBuff<BuffAbility>(buffConfig);
            return buff;
        }
    }
}
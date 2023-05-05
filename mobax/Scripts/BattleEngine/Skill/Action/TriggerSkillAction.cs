namespace BattleEngine.Logic
{
    using UnityEngine;

    public class TriggerSkillActionAbility : ActionAbility<TriggerSkillAction> { }

    /// <summary>
    /// 触发技能行动
    /// </summary>
    public class TriggerSkillAction : ActionExecution
    {
        public TriggerSkillEffect TriggerSkillEffect { get; set; }
        public Vector3 InputPoint { get; set; }

        //前置处理
        private void PreProcess() { }

        public void ApplyTrigger()
        {
            PreProcess();
            PostProcess();
        }

        //后置处理
        private void PostProcess()
        {
            EndExecute();
        }
    }
}
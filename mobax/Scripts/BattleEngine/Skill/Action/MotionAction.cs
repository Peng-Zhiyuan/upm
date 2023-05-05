namespace BattleEngine.Logic
{
    public class MotionActionAbility : ActionAbility<MotionAction> { }

    /// <summary>
    /// 动作行动
    /// </summary>
    public class MotionAction : ActionExecution<MotionActionAbility>
    {
        public int MotionType { get; set; }

        //前置处理
        private void PreProcess() { }

        public void ApplyMotion()
        {
            PreProcess();
            PostProcess();
        }

        //后置处理
        private void PostProcess() { }
    }
}
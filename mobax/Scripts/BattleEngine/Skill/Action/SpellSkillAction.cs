namespace BattleEngine.Logic
{
    public class SpellSkillActionAbility : ActionAbility<SpellSkillAction> { }

    /// <summary>
    /// 施放技能行动
    /// </summary>
    public class SpellSkillAction : ActionExecution<SpellSkillActionAbility>
    {
        public SkillAbility SkillAbility { get; set; }
        public SkillAbilityExecution SkillAbilityExecution { get; set; }

        //前置处理
        private void PreProcess() { }

        public void SpellSkill()
        {
            PreProcess();
            if (SkillAbilityExecution == null)
            {
                SkillAbility.ApplyAbilityEffectsTo(Target);
            }
            else
            {
                SkillAbilityExecution.BeginExecute();
            }
            PostProcess();
        }

        //后置处理
        private void PostProcess()
        {
            Destroy(this);
        }
    }
}
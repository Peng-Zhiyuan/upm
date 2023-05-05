namespace BattleEngine.Logic
{
    /// <summary>
    /// 技能组执行体
    /// </summary>
    public class SkillComboAbilityExecution : AbilityExecution
    {
        public SkillComboAbility skillGroupAbility
        {
            get { return AbilityEntity as SkillComboAbility; }
        }

        public override void BeginExecute()
        {
            base.BeginExecute();
            OwnerEntity.CurrentSkillComboExecution = this;
            skillGroupAbility.ComboSkill(0);
        }

        public override void OnUpdate(int frame)
        {
            if (OwnerEntity.CurrentHealth.Value <= 0)
            {
                EndExecute();
                return;
            }
            if (OwnerEntity.CurrentSkillExecution != null)
            {
                return;
            }
            if (OwnerEntity.ReadySkill != null)
            {
                return;
            }
            skillGroupAbility.ComboSkill(frame);
        }

        public bool AbilityFilterCondition()
        {
            return true;
        }
    }
}
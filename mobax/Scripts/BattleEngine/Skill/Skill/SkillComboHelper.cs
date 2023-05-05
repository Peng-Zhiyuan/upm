namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    public class SkillComboHelper
    {
        public static SkillAbility GetAtk(CombatActorEntity owner)
        {
            var skills = owner.SkillSlots.GetEnumerator();
            List<SkillAbility> lst = new List<SkillAbility>();
            while (skills.MoveNext())
            {
                if (skills.Current.Value.SkillBaseConfig.skillType != (int)SKILL_TYPE.ATK)
                {
                    continue;
                }
                lst.Add(skills.Current.Value);
            }
            if (lst.Count <= 0)
            {
                return null;
            }
            if (lst.Count == 1)
            {
                return lst[0];
            }
            int index = BattleLogicManager.Instance.Rand.RandomVaule(0, lst.Count - 1);
            return lst[index];
        }

        public static SkillAbility GetSSP(CombatActorEntity owner)
        {
            var skills = owner.SkillSlots.GetEnumerator();
            List<SkillAbility> lst = new List<SkillAbility>();
            while (skills.MoveNext())
            {
                if (skills.Current.Value.SkillBaseConfig.skillType != (int)SKILL_TYPE.SSP)
                {
                    continue;
                }
                if (!skills.Current.Value.CooldownTimer.IsFinished)
                {
                    continue;
                }
                lst.Add(skills.Current.Value);
            }
            if (lst.Count <= 0)
            {
                return null;
            }
            if (lst.Count == 1)
            {
                return lst[0];
            }
            int index = BattleLogicManager.Instance.Rand.RandomVaule(0, lst.Count - 1);
            return lst[index];
        }

        public static List<SkillComboAbility> GetSkillGroups(CombatActorEntity owner)
        {
            List<SkillComboAbility> lst = new List<SkillComboAbility>();
            UnitComboRow comboRow = null;
            var comboLst = StaticData.UnitComboTable.ElementList;
            for (int i = 0; i < comboLst.Count; i++)
            {
                if (comboLst[i].heroID == owner.battleItemInfo.id)
                {
                    comboRow = comboLst[i];
                    SkillComboAbility ability = owner.AttachAbility<SkillComboAbility>(comboRow);
                    lst.Add(ability);
                }
            }
            return lst;
        }

        public static SkillComboAbility GetFirstSkillGroup(CombatActorEntity owner)
        {
            UnitComboRow comboRow = null;
            var comboLst = StaticData.UnitComboTable.ElementList;
            for (int i = 0; i < comboLst.Count; i++)
            {
                if (comboLst[i].heroID == owner.battleItemInfo.id)
                {
                    comboRow = comboLst[i];
                    SkillComboAbility ability = owner.AttachAbility<SkillComboAbility>(comboRow);
                    return ability;
                }
            }
            return null;
        }
    }
}
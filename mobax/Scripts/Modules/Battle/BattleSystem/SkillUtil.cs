public class SkillUtil
{
    public static SkillRow GetSkillItem(int skillid, int level = 1)
    {
        var array = StaticData.SkillTable.TryGet(skillid);
        if (array == null
            || array.Colls.Count < level)
            return null;
        var skillRow = StaticData.SkillTable.TryGet(skillid).Colls[level - 1];
        skillRow.SkillID = skillid;
        return skillRow;
    }
}
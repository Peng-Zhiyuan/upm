using System;
using System.Linq;
using BattleEngine.Logic;

public static class HeroSkillHelper
{
    public static SkillRow GetSkillConfig(int skillId, int level, out bool bMax)
    {
        var array = StaticData.SkillTable.TryGet(skillId);
        if (null == array)
        {
            bMax = true;
            return null;
        }

        SkillRow cfg = null;
        var rows = array.Colls;
        var lastRow = rows[rows.Count - 1];
        bMax = lastRow.Level == level;
        
        foreach (var row in rows)
        {
            if (row.Level == level)
            {
                cfg = row;
                break;
            }
        }

        return cfg;
    }

    public static SkillRow GetSkillConfig(int skillId, int level = 1)
    {
        // 加个容错
        level = Math.Max(level, 1);
        return GetSkillConfig(skillId, level, out _);
    }

    public static SkillRow GetSkillConfig(HeroInfo heroInfo, int skillId)
    {
        if (!heroInfo.UpableSkills.Contains(skillId))
        {
            return GetDefaultSkill(skillId);
        }
        
        return GetSkillConfig(skillId, heroInfo.GetSkillLevel(skillId));
    }

    public static SkillRow GetDefaultSkill(int skillId)
    {
        var array = StaticData.SkillTable.TryGet(skillId);
        if (null == array) return null;

        return array.Colls.First();
    }

    /** 技能id */
    public static int GetSkillId(int heroId, SKILL_TYPE skillType)
    {
        var heroInfo = HeroManager.Instance.GetHeroInfo(heroId);
        return GetSkillId(heroInfo, skillType);
    }

    /** 技能id */
    public static int GetSkillId(HeroInfo heroInfo, SKILL_TYPE skillType)
    {
        int skillId;
        switch (skillType)
        {
            case SKILL_TYPE.ATK:
                skillId = heroInfo.CommonAtkID;
                break;
            case SKILL_TYPE.SSP:
                skillId = heroInfo.CommonSkillId;
                break;
            case SKILL_TYPE.SPSKL:
                skillId = heroInfo.UltimateId;
                break;
            case SKILL_TYPE.ItemSKL:
                skillId = heroInfo.AssistId;
                break;
            default:
                skillId = 0;
                break;
        }

        return skillId;
    }

    /** 得到技能信息 */
    public static (int, int) GetSkillInfo(HeroInfo heroInfo, SKILL_TYPE skillType)
    {
        var skillId = GetSkillId(heroInfo, skillType);
        return (skillId, heroInfo.GetSkillLevel(skillId));
    }

    public static bool LevelMax(int skillId, int skillLv)
    {
        var array = StaticData.SkillTable.TryGet(skillId);
        return array.Colls.Last().Level == skillLv;
    }
}
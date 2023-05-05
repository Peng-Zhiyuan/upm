namespace BattleEngine.Logic
{
    using UnityEngine;
    using System.Collections.Generic;

    public static class ItemInfoUtil
    {
        public static HeroRow GetHeroRow(this ItemInfo info)
        {
            var dataId = info.id;
            var dataRow = StaticData.HeroTable.TryGet(dataId);
            if (dataRow == null)
                BattleLog.LogWarning("缺少HeroRow数据，ID = " + dataId);
            return dataRow;
        }

        public static int GetLevel(this ItemInfo info)
        {
            if (info != null)
            {
                return info.lv;
            }
            return 0;
        }

        public static List<int> GetHeroSkill(this ItemInfo info)
        {
            List<int> skillList = new List<int>();
            HeroRow row = info.GetHeroRow();
            if (row.autoSkills != null)
            {
                for (int i = 0; i < row.autoSkills.Length; i++)
                {
                    if (row.autoSkills[i] == 0)
                    {
                        continue;
                    }
                    skillList.Add(row.autoSkills[i]);
                }
            }
            if (row.Ultimate > 0)
            {
                skillList.Add(row.Ultimate);
            }
            if (row.assistSkill > 0)
            {
                skillList.Add(row.assistSkill);
            }
            return skillList;
        }

        //（角色基础属性+角色进阶值+角色星耀值+等级成长*等级）*（1+角色进阶百分比+角色星耀百分比）+（（装备基础+精炼值）*（1+精炼百分比）+等级成长*等级+套装值）*（1+套装百分比）+元素导师值*元素导师百分比
        public static float[] HeroAtt(this ItemInfo info, bool isCeil = true)
        {
            var heroRow = info.GetHeroRow();
            float[] attEnd = new float[heroRow.Attrs.Length + 1];
            for (int i = 0; i < heroRow.Attrs.Length; i++)
            {
                attEnd[i + 1] = heroRow.Attrs[i];
            }
            return attEnd;
        }
    }
}
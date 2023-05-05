using System.Collections.Generic;
using System.Linq;

public static class HeroAttrHelper
{
    private static HeroAttr[] _displayAttrs = {HeroAttr.HP, HeroAttr.ATK, HeroAttr.DEF};
    
    private static Dictionary<HeroAttr, HeroAttr> _attrMap;
    
    /** 得到关联的属性（就是指的百分比属性） */
    public static bool GetRelatedAttribute(HeroAttr attr, out HeroAttr relatedAttr)
    {
        if (null == _attrMap)
        {
            _attrMap = new Dictionary<HeroAttr, HeroAttr>();
            var list = StaticData.HeroAttrTable.ElementList;
            foreach (var heroAttrRow in list)
            {
                if (default == heroAttrRow.Ralated)
                {
                    _attrMap[(HeroAttr) heroAttrRow.Ralated] = (HeroAttr) heroAttrRow.Id;
                }
            }
        }

        return _attrMap.TryGetValue(attr, out relatedAttr);
    }

    public static string GetAttrExpression(HeroAttr attr, int val)
    {
        var conf = StaticData.HeroAttrTable.TryGet((int) attr);
        return conf.Ptype == 1 ? $"{val / 10f}%" : $"{val}";
    }

    /// <summary>
    /// 记录变化之前
    /// </summary>
    /// <param name="heroInfo"></param>
    /// <param name="map"></param>
    /// <param name="attrs"></param>
    public static void RecordPrev(HeroInfo heroInfo, Dictionary<HeroAttr, int[]> map, HeroAttr[] attrs = null)
    {
        attrs ??= _displayAttrs;
        
        foreach (var heroAttr in attrs)
        {
            if (!map.TryGetValue(heroAttr, out var arr))
            {
                map[heroAttr] = arr = new int[2];
            }
            arr[0] = heroInfo.GetAttrFinal(heroAttr);
        }
        
        // 再把战力存在default里
        if (!map.TryGetValue(default, out var powerArr))
        {
            map[default] = powerArr = new int[2];
        }
        powerArr[0] = heroInfo.Power;
    }
    public static Dictionary<HeroAttr, int> GetOrRefreshAttrs(HeroCircuitInfo circuitInfo, Dictionary<HeroAttr, int> attrMap = null)
    {
        attrMap ??= new Dictionary<HeroAttr, int>();
        for (var index = 0; index < circuitInfo.Attrs.Count; ++index)
        {
            var attr = circuitInfo.Attrs[index];
            var attrId = (HeroAttr) attr.TryGet("id", 0);
            var val = HeroCircuitHelper.GetAttrVal(circuitInfo, index);
            attrMap.TryGetValue(attrId, out var total);
            attrMap[attrId] = total + val;
        }

        // 如果技能解锁了，那么也要把它被动技能的值也要加进去
        if (circuitInfo.SkillUnlocked)
        {
            var skillRowArr = StaticData.SkillTable.TryGet(circuitInfo.Skill);
            var skillRow = skillRowArr.Colls.First();
            for (var index = 0; index < skillRow.Attrs.Length; index++)
            {
                var val = skillRow.Attrs[index];
                if (val <= 0) continue;
                var attrId = (HeroAttr) (index + 1);
                attrMap.TryGetValue(attrId, out var total);
                attrMap[attrId] = total + val;
            }
        }
        
        return attrMap;
    }

    public static void RecordNew(HeroInfo heroInfo, Dictionary<HeroAttr, int[]> map)
    {
        foreach (var heroAttr in map.Keys)
        {
            if (heroAttr == default)
            {
                map[heroAttr][1] = heroInfo.Power;
            }
            else
            {
                map[heroAttr][1] = heroInfo.GetAttrFinal(heroAttr);
            }
        }
    }
}
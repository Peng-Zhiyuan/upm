using System.Collections.Generic;
using System.Linq;
using CustomLitJson;
using UnityEngine;
using UnityEngine.UI;

public static class HeroCircuitHelper
{
    public static int GetSkillColor(int circuitId)
    {
        var circuitRow = StaticData.PuzzleTable.TryGet(circuitId);
        // 得到形状配置
        if (null == circuitRow)
        {
            Debug.LogError($"【拼图】Circuit表中不存在拼图id={circuitId}");
            return 0;
        }
        
        var circuitSkillRow = StaticData.PuzzlePoolTable.TryGet(circuitRow.Skill);
        var circuitSkillCfg = circuitSkillRow.Colls.First();
        var skillRow = StaticData.SkillTable.TryGet(circuitSkillCfg.Skill);
        return skillRow.Colls.First().colorType;
    }
    
    public static int GetAttrVal(HeroCircuitInfo circuitInfo, int attrIndex)
    {
        var attrInfo = circuitInfo.Attrs[attrIndex];
        var attrId = (HeroAttr) attrInfo.TryGet("id", 0);
        var attrInternalLv = attrInfo.TryGet("qlv", 0);

        return GetAttrVal(circuitInfo.Level, circuitInfo.Qlv, attrId, attrIndex, attrInternalLv);
    }
    
    public static int GetAttrVal(HeroCircuitInfo circuitInfo, int attrIndex, int specifiedLevel)
    {
        var attrInfo = circuitInfo.Attrs[attrIndex];
        var attrId = (HeroAttr) attrInfo.TryGet("id", 0);
        var attrInternalLv = attrInfo.TryGet("qlv", 0);

        return GetAttrVal(specifiedLevel, circuitInfo.Qlv, attrId, attrIndex, attrInternalLv);
    }

    public static int GetNextAttrVal(HeroCircuitInfo circuitInfo, int attrIndex)
    {
        var attrInfo = circuitInfo.Attrs[attrIndex];
        var attrId = (HeroAttr) attrInfo.TryGet("id", 0);
        var attrInternalLv = attrInfo.TryGet("qlv", 0);

        var list = StaticData.PuzzleLevelTable.ElementList;
        var currentIndex = list.IndexOf(circuitInfo.LevelConfig);
        if (currentIndex < list.Count - 1)
        {
            var nextLevelCfg = list[currentIndex + 1];
            return GetAttrVal(nextLevelCfg.Level, nextLevelCfg.Qlv, attrId, attrIndex, attrInternalLv);
        }

        return 0;
    }

    /// <summary>
    /// 根据等级， 获得属性
    /// </summary>
    /// <param name="lv">拼图等级</param>
    /// <param name="qlv">拼图品质</param>
    /// <param name="attr">属性类型</param>
    /// <param name="attrIndex"></param>
    /// <param name="internalLv">属性的内部lv，就是后端给的attr里取出来的qlv(因为名字一样，所以重新给它取了个名)</param>
    /// <returns></returns>
    public static int GetAttrVal(int lv, int qlv, HeroAttr attr, int attrIndex, int internalLv)
    {
        var itemQlv = attrIndex + 2;
        if (itemQlv < qlv)
        {
            // 如果qlv比internalLv高，那么就直接给他满级的属性
            lv = StaticData.BaseTable.TryGet("puzzleLvMax");
        }
        else if (itemQlv > qlv)
        {
            // 如果当前还没到这个等级，那么等级给个1级，而且要置灰
            lv = 1;
        }

        // qlv是从1开始的
        var internalLvIndex = internalLv - 1;
        // id的拼接规则
        var attrId = (int) attr * 100 + lv;
        var attrItem = StaticData.PuzzleAttrTable.TryGet(attrId);

        return attrItem?.Vals[internalLvIndex] ?? 0;
    }

    /// <summary>
    /// 该属性是否已经满级了
    /// </summary>
    /// <param name="qlv"></param>
    /// <param name="attrIndex"></param>
    /// <returns></returns>
    public static bool AttrMax(int qlv, int attrIndex)
    {
        return qlv > attrIndex + 1;
    }

    public static List<HeroCircuitInfo> GetList(HeroInfo heroInfo)
    {
        var list = HeroCircuitManager.All.ConvertAll(HeroCircuitManager.GetCircuitInfo).FindAll(item =>
        {
            if (!item.Bind) return true;

            return item.BindHero == heroInfo.InstanceId;
        });

        return list;
    }

    /** 用作升级或者分解的列表 */
    public static List<HeroCircuitInfo> GetMeltingList(HeroCircuitInfo exception = null)
    {
        var list = HeroCircuitManager.All.ConvertAll(HeroCircuitManager.GetCircuitInfo).FindAll(item =>
        {
            if (exception == item) return false;

            return !item.ItemInfo.IsUsed;
        });
        
        list.Sort((item1, item2) =>
        {
            if (item1.Bind != item2.Bind)
            {
                return item1.Bind ? -1 : 1;
            }

            if (item1.Qlv != item2.Qlv)
            {
                return item1.Qlv - item2.Qlv;
            }

            if (item1.Level != item2.Level)
            {
                return item1.Level - item2.Level;
            }
            
            return item1.ConfId - item2.ConfId;
        });

        return list;
    }

    public static List<HeroCircuitInfo> GetCompoundableList(HeroCircuitInfo referCircuit = null)
    {
        var list = HeroCircuitManager.All.ConvertAll(HeroCircuitManager.GetCircuitInfo).FindAll(item =>
        {
            if (!item.InAdvance) return false;

            if (null != referCircuit)
            {
                if (referCircuit.Bind != item.Bind) return false;
                if (referCircuit.Qlv != item.Qlv) return false;
            }

            return true;
        });
        
        
        list.Sort((item1, item2) =>
        {
            if (item1.Bind != item2.Bind)
            {
                return item1.Bind ? -1 : 1;
            }

            if (item1.Qlv != item2.Qlv)
            {
                return item2.Qlv - item1.Qlv;
            }
            
            return item1.ConfId - item2.ConfId;
        });

        return list;
    }

    /** 当前等级升级上限 */
    public static int GetUpgradeLevelLimit(HeroCircuitInfo circuitInfo)
    {
        var levelTable = StaticData.PuzzleLevelTable;
        var levelId = circuitInfo.LevelId;

        do
        {
            var cfg = levelTable.TryGet(levelId);
            if (default != cfg.Advance || default == cfg.Next)
            {
                return cfg.Level;
            }

            levelId = cfg.Next;
        } while (true);
    }

    public static int GetPower(HeroCircuitInfo circuitInfo)
    {
        var power = 0;
        var attrMap = GetOrRefreshAttrs(circuitInfo);
        foreach (var attrId in attrMap.Keys)
        {
            var heroAttrCfg = StaticData.HeroAttrTable.TryGet((int) attrId);
            power += heroAttrCfg.Power * attrMap[attrId];
        }
        // 属性的战力是放大了10倍的， 所以要除以10
        power = power / 10;

        // 还有技能直接加的战力
        if (circuitInfo.SkillUnlocked)
        {
            var skillCfg = HeroSkillHelper.GetDefaultSkill(circuitInfo.Skill);
            power += skillCfg.Power;
        }

        return power;
    }

    public static int GetAttr(HeroCircuitInfo circuitInfo, HeroAttr attr)
    {
        int result = 0;
        for (var index = 0; index < circuitInfo.Attrs.Count; ++index)
        {
            var attrId = (HeroAttr) circuitInfo.Attrs[index].TryGet("id", 0);
            if (attrId != attr) continue;
            result += GetAttrVal(circuitInfo, index);
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
                if (attrId != attr) continue;
                result += val;
            }
        }

        return result;
    }
    
    public static Dictionary<HeroAttr, int> GetOrRefreshAttrs(HeroCircuitInfo circuitInfo, Dictionary<HeroAttr, int> attrMap = null)
    {
        attrMap ??= new Dictionary<HeroAttr, int>();
        for (var index = 0; index < circuitInfo.Attrs.Count; ++index)
        {
            var attr = circuitInfo.Attrs[index];
            var attrId = (HeroAttr) attr.TryGet("id", 0);
            var val = GetAttrVal(circuitInfo, index);
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

    public static void UpdateAttrsByRemove(HeroCircuitInfo circuitInfo, Dictionary<HeroAttr, int> attrMap)
    {
        // 属性也清掉
        for (var index = 0; index < circuitInfo.Attrs.Count; ++index)
        {
            var attr = circuitInfo.Attrs[index];
            var attrId = (HeroAttr) attr.TryGet("id", 0);
            var val = GetAttrVal(circuitInfo, index);
            _SubtractFromMap(attrMap, attrId, val);
        }
        
        // 如果技能解锁了，那么也要把它被动技能的值也要重新减掉
        if (circuitInfo.SkillUnlocked)
        {
            var skillRowArr = StaticData.SkillTable.TryGet(circuitInfo.Skill);
            var skillRow = skillRowArr.Colls.First();
            for (var index = 0; index < skillRow.Attrs.Length; index++)
            {
                var val = skillRow.Attrs[index];
                if (val <= 0) continue;
                var attrId = (HeroAttr) (index + 1);
                _SubtractFromMap(attrMap, attrId, val);
            }
        }
    }

    public static bool CanTurn(HeroCircuitInfo circuitInfo)
    {
        var shape = circuitInfo.Shape;
        var shapeRow = StaticData.PuzzleShapeTable.TryGet(shape);
        var newShape = shapeRow.Shapenext;
        return shape != newShape;
    }

    private static void _SubtractFromMap(Dictionary<HeroAttr, int> attrMap, HeroAttr attrId, int val)
    {
        if (!attrMap.TryGetValue(attrId, out var prev))
        {
            prev = 0;
        }

        var newVal = prev - val;
        if (newVal <= 0)
        {
            attrMap.Remove(attrId);
        }
        else
        {
            attrMap[attrId] = newVal;
        }
    }
 }
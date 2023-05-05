using System.Collections.Generic;
using System.Linq;
using CustomLitJson;
using UnityEngine;

public class HeroWeaponInfo
{
    private HeroInfo _heroInfo;
    /** 最大等级 */
    private int _maxLevel;
    /** 升级表 */
    private Dictionary<int, WeaponLevelRow> _levelUpMap;
    /** 突破的列表 */
    private List<int> _breakList;

    private WeaponLevelRow _cfg;

    public HeroWeaponInfo(HeroInfo heroInfo)
    {
        _heroInfo = heroInfo;
    }

    public JsonData Info => _heroInfo.ItemInfo.attach.TryGet<JsonData>("weapon", null);
    
    public int Level => Info?.TryGet("lv", 1) ?? 1;
    public int BreakLevel => Info?.TryGet("break", 0) ?? 0;
    // 突破列表
    public List<int> BreakList
    {
        get
        {
            _CheckInit();
            return _breakList;
        }
    }

    // 升级加的属性表
    public Dictionary<int, WeaponLevelRow> LevelUpMap
    {
        get
        {
            _CheckInit();
            return _levelUpMap;
        }
    }
    
    /** 下一突破等级 */
    public int NextBreakLevel
    {
        get
        {
            if (!LevelUpMap.TryGetValue(Level, out var levelRow))
            {
                Debug.LogError($"No row config with level： {Level}");
                return -1;
            }
            var expRow = StaticData.WeaponExpTable.TryGet(levelRow.Level);
            if (expRow.Advance == 1)
            {
                if (BreakLevel != Level)
                {
                    return Level;
                }
            }
            foreach (var lv in _breakList)
            {
                if (lv > Level)
                {
                    return lv;
                }
            }
            return -1;
        }
    }

    public int MaxLevel
    {
        get
        {
            _CheckInit();
            return _maxLevel;
        }
    }

    public bool ReachedMax => Level >= MaxLevel && BreakList.Last() == BreakLevel;

    /** 当前升级可以到达的最大等级 */
    public int LevelUpLimit
    {
        get
        {
            var lv = NextBreakLevel;
            if (lv <= 0)
            {
                lv = MaxLevel;
            }
            return lv;
        }
    }

    /** 当前突破后的可达最大等级 */
    public int NextLimit
    {
        get
        {
            if (NextBreakLevel <= 0) return MaxLevel;

            var index = _breakList.IndexOf(NextBreakLevel);
            if (index >= _breakList.Count - 1)
            {
                return MaxLevel;
            }

            return _breakList[index + 1];
        }
    }

    public int GetAttribute(int level, HeroAttr attr)
    {
        return GetAttribute(level, attr, BreakLevel);
    }

    public int GetAttribute(int level, HeroAttr attr, int breakLv)
    {
        if (!LevelUpMap.TryGetValue(level, out var conf)) return 0;
        
        // 因为VIM是第一个枚举，由其决定attrIndex哪里从几开始
        var attrIndex = attr - HeroAttr.VIM;
        var expRow = StaticData.WeaponExpTable.TryGet(level);
        int advanceAttrValue;
        if (expRow.Advance == 1 && breakLv < level)
        {
            LevelUpMap.TryGetValue(level - 1, out var prevConf);
            advanceAttrValue = null == prevConf ? 0 : prevConf.advanceAttrs.GetItem(attrIndex);
        }
        else
        {
            advanceAttrValue = conf.advanceAttrs.GetItem(attrIndex);
        }
        return conf.levelAttrs.GetItem(attrIndex) + advanceAttrValue;
    }

    public int GetAttribute(HeroAttr attr)
    {
        return GetAttribute(Level, attr);
    }
    
    /** 初始化等级数据 */
    private void _CheckInit()
    {
        if (null == _levelUpMap)
        {
            _levelUpMap = new Dictionary<int, WeaponLevelRow>();
            _breakList = new List<int>();
            StaticData.WeaponLevelTable.ElementList.ForEach(row =>
            {
                if (row.Group == _heroInfo.Conf.weaponGroup)
                {
                    _levelUpMap[row.Level] = row;
                    if (row.Level > _maxLevel)
                    {
                        _maxLevel = row.Level;
                    }

                    // 需要突破的
                    var expRow = StaticData.WeaponExpTable.TryGet(row.Level);
                    if (expRow.Advance == 1)
                    {
                        _breakList.Add(row.Level);
                    }
                }
            });
        }
    }
}
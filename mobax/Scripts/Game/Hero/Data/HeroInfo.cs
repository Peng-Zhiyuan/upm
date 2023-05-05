using System;
using System.Collections.Generic;
using CustomLitJson;
using JetBrains.Annotations;
using UnityEngine;

public class HeroInfo
{
    #region Basic Fields
    /** 服务端数据库数据 */
    private ItemInfo _itemInfo;
    /** 升级表 */
    private Dictionary<int, HeroLevelRow> _levelUpMap;
    /** 突破的列表 */
    private List<int> _breakList;
    /** 最大等级 */
    private int _maxLevel;
    /** 第一条星级数据 */
    private HeroStarRow _firstStarConfig;
    /** 技能们 */
    private int[] _skillIds;
    /** 可升级的技能们 */
    private int[] _upableSkills;
    /** 武器属性 */
    private HeroWeaponInfo _weaponInfo;
    #endregion

    #region Basic Attributes From Table
    /** 配置表数据 */
    public HeroRow Conf { get; }
    /** id */
    public int HeroId => Conf.Id;
    /** 名字 */
    public string Name => Conf.Name;
    /** 性别 */
    public HeroGender Gender => (HeroGender)Conf.Gender;
    /** 职业 */
    public int Job => Conf.Job;
    /** 元素 */
    public int Element => Conf.Element;
    /** 稀有度 */
    public Rarity Rarity => (Rarity)Conf.Qlv;
    /** 稀有度字符串（转小写） */
    public string RarityWord => Rarity.ToString().ToLower();
    /** 稀有度字符串（转大写） */
    public string RarityWordUpper => Rarity.ToString().ToUpper();
    /** 支援技ID */
    public int AssistId => Conf.assistSkill;
    /** 技能ID */
    public int CommonSkillId => Conf.autoSkills.GetItem(1);
    /** 普攻ID */
    public int CommonAtkID => Conf.autoSkills.GetItem(0);
    /** 大招ID */
    public int UltimateId => Conf.Ultimate;
    /** 模型 */
    public string Model => Conf.Model;
    /** 是否显示 */
    public bool Show => Conf.Show != 0;
    /** 技能们 */
    public int[] SkillIds => _skillIds ??= new[] { CommonSkillId, UltimateId, AssistId };
    /** 可升级的技能们 */
    public int[] UpableSkills => _upableSkills ??= new[] { CommonSkillId, UltimateId };
    #endregion

    #region Basic Attributes From Server
    /** 服务器数据 */
    public ItemInfo ItemInfo => _itemInfo ??= Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(HeroId);
    /** 该英雄是否已解锁 */
    public bool Unlocked => ItemInfo != null;
    /** 实例id */
    public string InstanceId => ItemInfo._id;
    /** 等级 */
    public int Level => ItemInfo.attach.TryGet("lv", 1);
    /** 是否喜爱（收藏）（数字的值，0或者1） */
    public int LikeValue => ItemInfo.attach.TryGet("like", 1);
    /** 是否喜爱（收藏） */
    public bool Like => LikeValue != 0;
    /** 服务端给的战力 */
    public int ServerPower => ItemInfo.attach.TryGet("power", 0);
    /** 星级ID */
    public int StarId => ItemInfo.attach.TryGet("star", 0);
    /** Avatar */
    public int Avatar => ItemInfo.attach.TryGet("avatar", 0);
    /** Weapon */
    public HeroWeaponInfo Weapon => _weaponInfo ??= new HeroWeaponInfo(this);
    /** AvatarInfo */
    public (int, int, int, long) AvatarInfo
    {
        get
        {
            var avatar = ItemInfo.attach.TryGet<string>("avatar", null);
            if (!string.IsNullOrEmpty(avatar))
            {
                var info = avatar.Split('_');
                if (info.Length == 4)
                {
                    return (int.Parse(info[0]), int.Parse(info[1]), int.Parse(info[2]), long.Parse(info[3]));
                }
            }
            return (0, 0, 0, 0);
        }
    }
    /** 星级 */
    public HeroStarRow StarConfig
    {
        get
        {
            var starId = StarId;
            return starId == 0 ? null : StaticData.HeroStarTable.TryGet(starId);
        }
    }

    /** 初始星级配置 */
    public HeroStarRow FirstStarConfig
    {
        get
        {
            if (null == _firstStarConfig)
            {
                var table = StaticData.HeroStarTable;
                var list = table.ElementList;
                var length = list.Count;
                for (var i = 0; i < length; ++i)
                {
                    var row = list[i];
                    if (row.Group == Conf.starGroup)
                    {
                        _firstStarConfig = row;
                        break;
                    }
                }
            }
            return _firstStarConfig;
        }
    }

    /** 下一星级 */
    public HeroStarRow NextStarConfig
    {
        get
        {
            var starConfig = StarConfig;
            // 当前没有配置的话，需要遍历取出第一级
            if (null == starConfig)
            {
                return FirstStarConfig;
            }
            if (0 == starConfig.Next)
            {
                return null;
            }
            return StaticData.HeroStarTable.TryGet(starConfig.Next);
        }
    }

    /** 星星数,用作排序用 */
    [UsedImplicitly]
    public int StarNumber
    {
        get
        {
            var starConfig = StarConfig;
            if (null == starConfig) return 0;
            return starConfig.Star * 10 + starConfig.Starlevel;
        }
    }

    /** 星级 */
    public int Star => StarConfig.Star;
    /** 血量 */
    public int Hp => GetAttrFinal(HeroAttr.HP);
    /** 攻击 */
    public int Atk => GetAttrFinal(HeroAttr.ATK);
    /** 防御 */
    public int Def => GetAttrFinal(HeroAttr.DEF);
    /** 拼图解锁阶段 */
    public int CircuitStarStage => StarConfig.puzzleBg;
    /** 拼图解锁阶段 */
    public int CircuitLevelStage => LevelUpMap[Level].puzzleBg;

    /** 突破等级 */
    public int BreakLevel
    {
        get
        {
            var levelId = ItemInfo.attach.TryGet("break", 0);
            if (levelId <= 0) return 0;
            var levelRow = StaticData.HeroLevelTable.TryGet(levelId);
            if (levelRow == null)
                return 0;
            return levelRow.Level;
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
            if (levelRow.Advance == 1)
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

    /** 最大等级 */
    public int MaxLevel
    {
        get
        {
            _CheckInit();
            return _maxLevel;
        }
    }

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

    // 升级加的属性表
    public Dictionary<int, HeroLevelRow> LevelUpMap
    {
        get
        {
            _CheckInit();
            return _levelUpMap;
        }
    }

    // 突破列表
    public List<int> BreakList => _breakList;

    /** 英雄战力 */
    public int Power
    {
        get
        {
            var list = StaticData.HeroAttrTable.ElementList;
            var power = 0;
            var attrPower = 0;
            foreach (var heroAttrRow in list)
            {
                var attr = (HeroAttr) heroAttrRow.Id;
                var val = GetAttrFinal(attr);
                attrPower += val * heroAttrRow.Power;
            }
            // 属性战力放大10倍，所以这里要除掉
            power += attrPower / 10;

            // 技能还有单独配表的战力加成
            foreach (var skillId in SkillIds)
            {
                var skillLv = GetSkillLevel(skillId);
                var cfg = HeroSkillHelper.GetSkillConfig(skillId, skillLv);
                power += cfg.Power;
            }

            // 升星也有技能战力加成
            foreach (var starSkill in StarSkillMap)
            {
                var skillCfg = HeroSkillHelper.GetSkillConfig(starSkill.Key, starSkill.Value);
                if (skillCfg == null)
                {
                    Debug.LogError("Cant find the skill id " + starSkill.Key + " lv" + starSkill.Value);
                    continue;
                }
                power += skillCfg.Power;
            }

            // 拼图也有技能战力加成
            var puzzles = HeroCircuitManager.GetCircuits(HeroId);
            foreach (var puzzleInfo in puzzles)
            {
                if (puzzleInfo.SkillUnlocked)
                {
                    var skillCfg = HeroSkillHelper.GetDefaultSkill(puzzleInfo.Skill);
                    power += skillCfg.Power;
                }
            }
            
            return power;
        }
    }
#endregion

    public HeroInfo(int heroId)
    {
        Conf = StaticData.HeroTable.TryGet(heroId);

        if (null == Conf)
        {
            throw new Exception($"【英雄信息】配表id为{heroId}的英雄无法找到");
        }
    }

    /** 得到属性 */
    public int GetAttribute(HeroAttr attr)
    {
        // 因为VIM是第一个枚举，由其决定attrIndex哪里从几开始
        var attrIndex = attr - HeroAttr.VIM;
        var val = Conf.Attrs.GetItem(attrIndex);

        // 星级提升的数值加成
        var starCfg = StarConfig;
        if (starCfg?.levelAttrs != null)
        {
            if (attrIndex < starCfg.levelAttrs?.Length)
            {
                val += starCfg.levelAttrs[attrIndex];
            }
        }

        // 升级和突破的数值
        var attrs = new[] { HeroAttr.HP, HeroAttr.ATK, HeroAttr.DEF };
        var index = Array.IndexOf(attrs, attr);
        if (index >= 0)
        {
            // 等级提升的数值加成
            val += LevelUpMap[Level].levelAttrs?[index + 1] ?? 0;
            // 突破的话，突破后的数值要单独加上去
            if (BreakLevel == Level)
            {
                val += LevelUpMap[Level].advanceAttrs[index + 1];
            }
        }

        // 加上拼图的属性
        var attrMap = HeroCircuitManager.GetCircuitAttrMap(HeroId);
        if (null != attrMap && attrMap.TryGetValue(attr, out var attrVal))
        {
            val += attrVal;
        }
        
        // 加上武器的属性
        val += Weapon.GetAttribute(attr);
        
        // 升星会获得被动技能
        foreach (var starSkill in StarSkillMap)
        {
            var skillCfg = HeroSkillHelper.GetSkillConfig(starSkill.Key, starSkill.Value);
            if (skillCfg?.Attrs == null)
            {
                Debug.LogError("Cant find the Skill " + starSkill.Key + "  lv" + starSkill.Value);
                continue;
            }
            val += skillCfg.Attrs[attrIndex];
        }
        
        return val;
    }

    /** 得到属性最终值（就是其有对应百分比数值就还需要加上属性百分比） */
    public int GetAttrFinal(HeroAttr attr)
    {
        var val = GetAttribute(attr);
        // 如果有关联千分比， 还要再乘上这个系数
        if (HeroAttrHelper.GetRelatedAttribute(attr, out var relatedAttr))
        {
            var ratio = GetAttribute(relatedAttr);
            val = (int)(val * (1000L + ratio) / 1000);
        }
        return val;
    }

    public int GetSkillLevel(int skillId)
    {
        var skillIndex = Array.IndexOf(SkillIds, skillId);
        if (skillIndex < 0) return 1;
        
        var skillLvs = ItemInfo.attach.TryGet<JsonData>("skillLv", default);
        return (int)skillLvs[skillIndex];
    }

    /** 初始化等级数据 */
    private void _CheckInit()
    {
        if (null == _levelUpMap)
        {
            _levelUpMap = new Dictionary<int, HeroLevelRow>();
            _breakList = new List<int>();
            StaticData.HeroLevelTable.ElementList.ForEach(row =>
            {
                if (row.Group == Conf.lvGroup)
                {
                    _levelUpMap[row.Level] = row;
                    if (row.Level > _maxLevel)
                    {
                        _maxLevel = row.Level;
                    }

                    // 需要突破的
                    if (row.Advance == 1)
                    {
                        _breakList.Add(row.Level);
                    }
                }
            });
        }
    }

#region 计算星级技能
    private int _prevStarId;
    private Dictionary<int, int> _starSkillMap;

    public Dictionary<int, int> StarSkillMap
    {
        get
        {
            _RefreshStarSkills();
            return _starSkillMap;
        }
    }

    private void _RefreshStarSkills()
    {
        if (_prevStarId == StarId) return;
        _starSkillMap ??= new Dictionary<int, int>();
        HeroStarRow starCfg;
        if (_prevStarId == 0)
        {
            starCfg = FirstStarConfig;
        }
        else
        {
            var cfg = StaticData.HeroStarTable.TryGet(_prevStarId);
            starCfg = StaticData.HeroStarTable.TryGet(cfg.Next);
        }
        while (null != starCfg)
        {
            if (0 != starCfg.skillId)
            {
                // 至少要保证有1级
                _starSkillMap[starCfg.skillId] = Math.Max(starCfg.skillLevel, 1);
            }
            if (starCfg.Id == StarId)
            {
                break;
            }
            starCfg = StaticData.HeroStarTable.TryGet(starCfg.Next);
        }
        _prevStarId = StarId;
    }
#endregion

#region 升级的反馈
    private Dictionary<int, Action<int>> _levelUpCallbackMap = new Dictionary<int, Action<int>>();

    public void AddLevelUpCallback(int itemId, Action<int> handler)
    {
        if (!_levelUpCallbackMap.ContainsKey(itemId))
        {
            _levelUpCallbackMap.Add(itemId, handler);
        }
        else
        {
            _levelUpCallbackMap[itemId] -= handler;
            _levelUpCallbackMap[itemId] += handler;
        }
    }

    public void RemoveLevelUpCallback(int itemId, Action<int> handler)
    {
        if (_levelUpCallbackMap.ContainsKey(itemId))
        {
            _levelUpCallbackMap[itemId] -= handler;
        }
    }

    private void InvokeLevelUpCallback(int heroId)
    {
        if (_levelUpCallbackMap.TryGetValue(heroId, out var changeHandler))
        {
            if (null != changeHandler) changeHandler(heroId);
        }
    }
#endregion
}
using System.Collections.Generic;
using System.Threading.Tasks;

public static class HeroReminderProxy
{
    private static List<int> _listeningHeroes;

    public static async void Refresh(EFormationIndex formationIndex = default)
    {
        var defaultFormation = FormationUtil.GetDefaultFormationIndex();
        if (formationIndex == default)
        {
            formationIndex = defaultFormation;
        }
        else if (formationIndex != defaultFormation)
        {
            // 非默认阵型， 是不需要更新红点信息的
            return;
        }
        
        _listeningHeroes ??= new List<int>();
        var newList = _FormationHeroIds(formationIndex);
        
        // 先把删除掉的先清掉红点监听
        for (int i = _listeningHeroes.Count - 1; i >= 0; i--)
        {
            var heroId = _listeningHeroes[i];
            if (!newList.Contains(heroId))
            {
                UnListen(heroId);
                _listeningHeroes.RemoveAt(i);
            }
        }
        
        // 新的加上
        foreach (var heroId in newList)
        {
            if (!_listeningHeroes.Contains(heroId))
            {
                _listeningHeroes.Add(heroId);
                Listen(heroId);
                // 分帧监听
                await Task.Delay(1);
            }
        }
    }

    public static void Listen(int heroId)
    {
        _ListenHero(heroId);
    }

    public static void UnListen(int heroId)
    {
        var heroNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroPrefix}{heroId}");
        Reminder.GetNode(HeroReminderConst.Hero_main).Remove(heroNode);
        heroNode.ClearChildren();
    }

    /// <summary>
    /// 是否在监听列表里的
    /// </summary>
    /// <param name="heroId"></param>
    /// <returns></returns>
    public static bool NeedRemind(int heroId)
    {
        return _listeningHeroes != null && _listeningHeroes.Contains(heroId);
    }
    
    public static void UpdateReminder_StarChanged(HeroInfo heroInfo)
    {
        var heroId = heroInfo.HeroId;
        if (!NeedRemind(heroId)) return;
        
        var starUpNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroStarUpPrefix}{heroId}");
        if (null != heroInfo.NextStarConfig)
        {
            Reminder.ListenItems(starUpNode, heroInfo.StarConfig.soulSubs);
        }
        else
        {
            Reminder.UnListenItems(starUpNode);
        }
    }

    public static void UpdateReminder_LevelChanged(HeroInfo heroInfo)
    {
        var heroId = heroInfo.HeroId;
        if (!NeedRemind(heroId)) return;
        
        var levelUpButtonNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroLevelUpButtonPrefix}{heroId}");
        if (heroInfo.MaxLevel == heroInfo.Level)
        {
            Reminder.UnListenItems(levelUpButtonNode);
        }
        else if (heroInfo.NextBreakLevel == heroInfo.Level)
        {
            // 需要突破
            var requireItems = heroInfo.LevelUpMap[heroInfo.Level].advanceSubs;
            Reminder.ListenItems(levelUpButtonNode, requireItems);
        }
        else
        {
            // 正常升级状态
            var requireItems = heroInfo.LevelUpMap[heroInfo.Level].Subs;
            Reminder.ListenItems(levelUpButtonNode, requireItems);
        }
        
        // 技能开放也受等级限制
        foreach (var skillId in heroInfo.UpableSkills)
        {
            var skillLv = heroInfo.GetSkillLevel(skillId);
            var cfg = HeroSkillHelper.GetSkillConfig(skillId, skillLv);
            var subSkillDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillDemandPrefix}{heroId}_{skillId}");
            subSkillDemandNode.SetValue(heroInfo.Level >= cfg.Herolv);
        }
        
        // 武器开放也受等级限制
        var weaponUpDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponUpDemandPrefix}{heroId}");
        weaponUpDemandNode.SetValue(heroInfo.Level > heroInfo.Weapon.Level);
    }

    public static void UpdateReminder_SkillChanged(HeroInfo heroInfo)
    {
        var heroId = heroInfo.HeroId;
        if (!NeedRemind(heroId)) return;
        
        foreach (var skillId in heroInfo.UpableSkills)
        {
            UpdateReminder_SkillChanged(heroInfo, skillId);
        }
    }
    
    public static void UpdateReminder_SkillChanged(HeroInfo heroInfo, int skillId)
    {
        var heroId = heroInfo.HeroId;
        if (!NeedRemind(heroId)) return;
        
        var skillLv = heroInfo.GetSkillLevel(skillId);
        var cfg = HeroSkillHelper.GetSkillConfig(skillId, skillLv, out var reachMax);
        var subSkillItemRequireNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillItemRequirePrefix}{heroId}_{skillId}");
        if (!reachMax)
        {
            var subSkillDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillDemandPrefix}{heroId}_{skillId}");
            subSkillDemandNode.SetValue(heroInfo.Level >= cfg.Herolv);
            Reminder.ListenItems(subSkillItemRequireNode, cfg.costIds);
        }
        else
        {
            Reminder.UnListenItems(subSkillItemRequireNode);
        }
    }
    
    // 武器等级改变
    public static void UpdateReminder_WeaponLevelChanged(HeroInfo heroInfo)
    {
        var heroId = heroInfo.HeroId;
        if (!NeedRemind(heroId)) return;
        
        var weaponUpButtonNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponLevelUpButtonPrefix}{heroId}");
        var weaponUpRequireNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponUpRequirePrefix}{heroId}");
        var weaponInfo = heroInfo.Weapon;
        if (weaponInfo.ReachedMax)
        {
            Reminder.UnListenItems(weaponUpButtonNode);
        }
        else if (weaponInfo.NextBreakLevel == weaponInfo.Level)
        {
            // 需要突破
            var conf = StaticData.WeaponExpTable.TryGet(weaponInfo.Level);
            var requireItems = conf.advanceSubs;
            Reminder.ListenItems(weaponUpRequireNode, requireItems);
            // 忽略等级限制
            var weaponUpDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponUpDemandPrefix}{heroId}");
            weaponUpDemandNode.SetValue(true);
        }
        else
        {
            // 正常升级状态
            var conf = StaticData.WeaponExpTable.TryGet(weaponInfo.Level);
            var requireItems = conf.Subs;
            Reminder.ListenItems(weaponUpRequireNode, requireItems);
            // 等级限制
            var weaponUpDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponUpDemandPrefix}{heroId}");
            weaponUpDemandNode.SetValue(heroInfo.Level > weaponInfo.Level);
        }
    }

    private static void _ListenHero(int heroId)
    {
        var heroInfo = HeroManager.Instance.GetHeroInfo(heroId);
        var heroNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroPrefix}{heroId}");
        Reminder.GetNode(HeroReminderConst.Hero_main).Add(heroNode);

        // star part
        var starNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroStarPrefix}{heroId}");
        var starUpNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroStarUpPrefix}{heroId}");
        starNode.Add(starUpNode);
        UpdateReminder_StarChanged(heroInfo);
        // puzzle part
        var puzzleNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroPuzzlePrefix}{heroId}");
        // profile part
        var levelUpNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroLevelUpPrefix}{heroId}");
        var skillNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillPrefix}{heroId}");
        var levelUpButtonNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroLevelUpButtonPrefix}{heroId}");
        levelUpNode.Add(levelUpButtonNode);
        // listen all the items for level up
        UpdateReminder_LevelChanged(heroInfo);
        // listen skill state
        skillNode.EnsureCapability(heroInfo.UpableSkills.Length);
        foreach (var skillId in heroInfo.UpableSkills)
        {
            var subSkillNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillPrefix}{heroId}_{skillId}");
            var subSkillButtonNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillButtonPrefix}{heroId}_{skillId}");
            skillNode.Add(subSkillNode);
            subSkillNode.Add(subSkillButtonNode);
            // 技能还要关联2个子节点， 
            var subSkillDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillDemandPrefix}{heroId}_{skillId}");
            var subSkillItemRequireNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillItemRequirePrefix}{heroId}_{skillId}");
            subSkillButtonNode.SetAllDoneMode().Add(subSkillDemandNode, subSkillItemRequireNode);
            UpdateReminder_SkillChanged(heroInfo, skillId);
        }
        // weapon part
        var weaponNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroWeaponPrefix}{heroId}");
        var weaponUpNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponLevelUpPrefix}{heroId}");
        var weaponUpButtonNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponLevelUpButtonPrefix}{heroId}");
        weaponNode.Add(weaponUpNode);
        // 武器也是还要关联2个子节点， 
        var weaponUpDemandNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponUpDemandPrefix}{heroId}");
        var weaponUpRequireNode = Reminder.GetNode($"{HeroReminderConst.Hero_weaponUpRequirePrefix}{heroId}");
        weaponUpButtonNode.SetAllDoneMode().Add(weaponUpDemandNode, weaponUpRequireNode);
        weaponUpNode.Add(weaponUpButtonNode);
        UpdateReminder_WeaponLevelChanged(heroInfo);
        
        // 然后统统加进来
        heroNode.Add(starNode, puzzleNode, levelUpNode, skillNode, weaponNode);
    }

    private static List<int> _FormationHeroIds(EFormationIndex formationIndex)
    {
        var list = new List<int>();
        var formationList = FormationUtil.GetFormationHeroInfos(formationIndex);
        foreach (var formationHeroInfo in formationList)
        {
            if (formationHeroInfo.MainHeroId > 0)
            {
                list.Add(formationHeroInfo.MainHeroId);
                foreach (var subId in formationHeroInfo.SubHeroIdList)
                {
                    if (subId > 0)
                    {
                        list.Add(subId);
                    }
                }
            }
        }

        return list;
    }

    private static void _OnNewHeroAdded(int heroId)
    {
        _ListenHero(heroId);
    }
}
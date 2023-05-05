using System;
using System.Collections.Generic;
using BattleEngine.Logic;
using UnityEngine;

public partial class HeroPage : Page
{
    private List<int> _passiveSkills;
    // 记录当前显示了哪个英雄， 用来判断OnBack后是否需要刷新英雄显示的
    private HeroInfo _displayingHero;
    
    private HeroInfo HeroInfo => HeroDisplayer.Hero;
    
    public override void OnNavigatedTo(PageNavigateInfo info)
    {
        if (info.param is HeroInfo heroInfo)
        {
            // 交给组件处理
            HeroDisplayer.Filter.SetHero(heroInfo);
            _RenderAll();
        }
    }

    public override void OnNavigatedToComplete(PageNavigateInfo navigateInfo)
    {
        base.OnNavigatedToComplete(navigateInfo);
        
        GuideManagerV2.Stuff.Notify("HeroPage.Ready");
    }

    public override void OnBackTo(PageNavigateInfo info)
    {
        if (_displayingHero != HeroInfo)
        {
            _RenderAll();
        }
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "level_up":
                UIEngine.Stuff.ForwardOrBackTo<HeroLevelUpPop>(HeroInfo);
                break;
            case "star_up":
                UIEngine.Stuff.ForwardOrBackTo<HeroStarPop>(HeroInfo);
                break;
            case "display_attrs":
                UIEngine.Stuff.ForwardOrBackTo<HeroAttrsPopLegacy>(HeroInfo);
                break;
            case "display_3d":
                BattleUtil.EnterSkillViewBattle(1500, new SkillViewModeParam() { HeroID = HeroDisplayer.Hero.HeroId });
                break;
            case "display_skin":
                if (!HeroDressData.Instance.HasSkin(HeroDisplayer.Hero.HeroId))
                {
                    ToastManager.ShowLocalize("heroskin_not_available");
                    return;
                }
                UIEngine.Stuff.ForwardOrBackTo<HeroSkinPage>();
                break;
            case "display_weapon":
                UIEngine.Stuff.ForwardOrBackTo<HeroWeaponPage>();
                break;
            case "set_like":
                if (!HeroDressData.Instance.HasSkin(HeroDisplayer.Hero.HeroId))
                {
                    ToastManager.ShowLocalize("heroskin_like_not_available");
                    return;
                }
                _OnSetLike();
                break;
            case "display_puzzle":
                UIEngine.Stuff.ForwardOrBackTo<HeroCircuitPop>(HeroInfo);
                break;
        }
    }

    public void OnTurnNext()
    {
        HeroDisplayer.Filter.TurnTo(1);
        _RenderAll();
    }

    public void OnTurnPrev()
    {
        HeroDisplayer.Filter.TurnTo(-1);
        _RenderAll();
    }

    private void Awake()
    {
        Tab_functions.OnTabChanged = _OnFunctionTabChanged;
        List_passiveSkills.onItemRenderAction = _OnPassiveSkillRender;

        Skill_ulitmate.OnClick = _OnSkillCellClick;
        Skill_common.OnClick = _OnSkillCellClick;
        Skill_attack.OnClick = _OnSkillCellClick;
    }

    private void Start()
    {
        Tab_functions.SetDefault();
    }

    private void OnEnable()
    {
        HeroNotifier.OnChange += _OnChanged;
    }

    private void OnDisable()
    {
        HeroNotifier.OnChange -= _OnChanged;
    }

    private void _RenderAll()
    {
        _RenderBasic();
        _RenderHero();
        _RenderStarView();
        _RenderLevelView();
        _RenderAttributeAndPower();
        _RenderTabContent();
        _RefreshLike();
    }

    private void _RenderBasic()
    {
        // 设置品质
        UiUtil.SetSpriteInBackground(Image_rarity, () => HeroHelper.GetRarityNormalAddress(HeroInfo.Rarity));
        // 设置职业图标
        UiUtil.SetSpriteInBackground(Image_job, () => HeroHelper.GetJobAddress(HeroInfo.Job));
        // 设置元素图标
        UiUtil.SetSpriteInBackground(Image_element, () => HeroHelper.GetResAddress(HeroInfo.HeroId, HeroResType.AttrSmall));
        // 设置名字
        Text_name.SetLocalizer(HeroInfo.Name);
        // 绑定红点组件
        Reminder.Bind($"{HeroReminderConst.Hero_heroLevelUpPrefix}{HeroInfo.HeroId}", Button_levelUp);
        Reminder.Bind($"{HeroReminderConst.Hero_heroStarPrefix}{HeroInfo.HeroId}", Button_starUp);
        Reminder.Bind($"{HeroReminderConst.Hero_heroSkillPrefix}{HeroInfo.HeroId}", Txt_skill);
        Reminder.Bind($"{HeroReminderConst.Hero_heroWeaponPrefix}{HeroInfo.HeroId}", Button_weapon);
        // 记录当前展示的英雄
        _displayingHero = HeroInfo;
    }

    private async void _RenderHero()
    {
        await HeroView.SetInfo(HeroInfo, CameraViewMode.LEFT);
    }

    private void _RenderStarView()
    {
        // 设置星级
        for (var i = 0; i < HeroConst.StarMax; ++i)
        {
            var star = Node_stars.transform.Find($"star{i}");
            star.gameObject.SetActive(i < HeroInfo.Star);
        }
    }

    private void _RenderLevelView()
    {
        // 设置等级
        Text_level.text = $"{HeroInfo.Level:00}<size=32>/{HeroInfo.MaxLevel}</size>";

        string upLabel;
        if (HeroInfo.Level == HeroInfo.MaxLevel)
        {
            upLabel = "M4_level_max";
        }
        else if (HeroInfo.Level == HeroInfo.NextBreakLevel)
        {
            upLabel = "M4_rank_up";
        }
        else
        {
            upLabel = "M4_level_up";
        }
        Label_upButton.SetLocalizer(upLabel);
    }

    private void _RenderAttributeAndPower()
    {
        // 属性们
        Txt_attack.text = "" + HeroInfo.Atk;
        Txt_defence.text = "" + HeroInfo.Def;
        Txt_hp.text = "" + HeroInfo.Hp;
        // 战力
        Txt_power.text = "" + HeroInfo.Power;
    }

    private void _RenderTabContent()
    {
        // 内部会判断是否该组件已经enable
        _RenderActiveSkills();
        _RenderPassiveSkills();
        _RenderCircuits();
    }

    // 主动技能
    private void _RenderActiveSkills()
    {
        if (Tab_functions.CurrentIndex != (int) TabCategory.Skill) return;
        
        Skill_ulitmate.SetInfo(HeroInfo);
        Skill_common.SetInfo(HeroInfo);
        Skill_attack.SetInfo(HeroInfo);
        
        // 红点只要用alias逻辑即可
        var ultimateNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillPrefix}{HeroInfo.HeroId}_{HeroInfo.UltimateId}");
        ultimateNode.Alias($"{HeroReminderConst.Hero_heroSkillAlias}{HeroInfo.UltimateId}").Bind(Skill_ulitmate);
        var commonSkillNode = Reminder.GetNode($"{HeroReminderConst.Hero_heroSkillPrefix}{HeroInfo.HeroId}_{HeroInfo.CommonSkillId}");
        commonSkillNode.Alias($"{HeroReminderConst.Hero_heroSkillAlias}{HeroInfo.CommonSkillId}").Bind(Skill_common);
    }

    // 被动技能
    private void _RenderPassiveSkills()
    {
        if (Tab_functions.CurrentIndex != (int) TabCategory.Skill) return;
        
        int explorerSkillId;
        (_passiveSkills, explorerSkillId) = HeroInfoEx.GetStarSkillList(HeroInfo);
        List_passiveSkills.numItems = (uint) _passiveSkills.Count;
        SkillItem_explore.SetInfo(HeroInfo, explorerSkillId);
    }

    private void _RenderCircuits()
    {
        if (Tab_functions.CurrentIndex != (int) TabCategory.Circuit) return;
        
        Node_functionCircuit.SetInfo(HeroInfo);
        // 派发拼图准备好
        GuideManagerV2.Stuff.Notify("HeroPage.Circuit.Ready");
    }

    private void _RefreshLike()
    {
        var isLike = Database.Stuff.roleDatabase.Me.show == HeroInfo.HeroId;
        Button_like.Selected = isLike;
    }

    private async void _OnSetLike()
    {
        if (Database.Stuff.roleDatabase.Me.show == HeroInfo.HeroId)
        {
            ToastManager.Show("M4_post_already".Localize(HeroInfo.Name.Localize()));
            return;
        }
        
        if (HeroInfo.Conf.Portrait != 1)
        {
            ToastManager.ShowLocalize("M4_post_cantset");
            return;
        }

        await HeroApi.SetDisplayHeroAsync(HeroInfo.HeroId);
        _RefreshLike();
        await Dialog.ConfirmWithNoButton("", "M4_post_set".Localize(HeroInfo.Name.Localize()));
       // var f = UIEngine.Stuff.FindFloating<TopFloating>();
       // f.Refresh();
    }

    private void _OnFunctionTabChanged(int tab)
    {
        Node_functions.SetSelected(tab);
        _RenderTabContent();
    }

    private void _OnPassiveSkillRender(int index, Transform tf)
    {
        var item = tf.GetComponent<HeroPassiveSkillItem>();
        item.SetInfo(HeroInfo, _passiveSkills[index]);
    }

    private void _OnChanged(HeroNotifyEnum notifyEnum)
    {
        switch (notifyEnum)
        {
            case HeroNotifyEnum.Level:
            case HeroNotifyEnum.LevelReset:
                _RenderLevelView();
                _RenderAttributeAndPower();
                break;
            case HeroNotifyEnum.Skill:
            case HeroNotifyEnum.SkillReset:
                _RenderActiveSkills();
                break;
            case HeroNotifyEnum.Star:
                _RenderStarView();
                _RenderPassiveSkills();
                _RenderCircuits();
                _RenderAttributeAndPower();
                break;
            case HeroNotifyEnum.CircuitChange:
                _RenderCircuits();
                _RenderAttributeAndPower();
                break;
            case HeroNotifyEnum.CircuitUpdate:
                _RenderCircuits();
                break;
        }
    }

    private void _OnSkillCellClick(int skillId, int skillLv)
    {
        var skillCfg = HeroSkillHelper.GetSkillConfig(skillId, skillLv);
        if (skillCfg.skillType == (int) SKILL_TYPE.ATK || HeroSkillHelper.LevelMax(skillId, skillLv))
        {
            UIEngine.Stuff.ForwardOrBackTo<HeroSkillPop>(new HeroSkillPopParam {category = SkillCategoryEnum.Active, heroInfo = HeroInfo});
        }
        else
        {
            UIEngine.Stuff.ForwardOrBackTo<HeroSkillUpPopV2>(new HeroSkillUpPopParam
            {
                heroInfo = HeroInfo,
                skillId = skillId,
                skillLv = skillLv,
            });
        }
    }
}

internal enum TabCategory
{
    Circuit = 0,
    Skill
}
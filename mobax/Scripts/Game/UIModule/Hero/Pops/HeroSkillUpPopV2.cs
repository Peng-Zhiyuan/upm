using System;
using UnityEngine.UI;

public partial class HeroSkillUpPopV2 : Page
{
    private HeroInfo _heroInfo;
    private int _skillId;
    private int _skillLv;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroSkillUpPopParam param)
        {
            _heroInfo = param.heroInfo;
            _skillId = param.skillId;
            _skillLv = param.skillLv;
            
            _RefreshView();
            _BindReminders();
        }
    }

    public void OnReset()
    {
        UIEngine.Stuff.ForwardOrBackTo<HeroSkillResetPop>(_heroInfo);
    }

    public async void OnUp()
    {
        if (HeroSkillHelper.LevelMax(_skillId, _skillLv))
        {
            ToastManager.ShowLocalize("M4_max_already");
            return;
        }
        
        var skillCfg = HeroSkillHelper.GetSkillConfig(_skillId, _skillLv);
        if (skillCfg.Herolv > _heroInfo.Level)
        {
            ToastManager.ShowLocalize("M4_hero_level_need", skillCfg.Herolv);
            return;
        }

        if (!ItemUtil.IsEnough(skillCfg.costIds))
        {
            ToastManager.ShowLocalize("M4_not_enough");
            return;
        }

        var newLevel = skillCfg.Level + 1;
        var prevPower = _heroInfo.Power;
        await HeroApi.SkillUpgradeAsync(_heroInfo.HeroId, skillCfg.skillType, newLevel);
        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_upgrade");
        // 消耗上报
        CostReporter.Report(skillCfg.costIds, "skillup");
        _skillLv = newLevel;
        _RefreshView();
        // 标记需要刷新
        HeroNotifier.Invoke(HeroNotifyEnum.Skill);
        // 上报战力变化
        HeroProxy.HandlePowerChange(_heroInfo, prevPower);
        // 更新红点逻辑
        HeroReminderProxy.UpdateReminder_SkillChanged(_heroInfo, _skillId);
    }

    public void OnCancel()
    {
        UIEngine.Stuff.Back();
    }

    private void Awake()
    {
        Cost_up.RequireFormat = "common_format_cost".Localize();
    }
    
    private void OnEnable()
    {
        HeroNotifier.OnChange += _OnChanged;
    }

    private void OnDisable()
    {
        HeroNotifier.OnChange -= _OnChanged;
    }
    
    private void _RefreshView()
    {
        // 原先的技能显示
        Cell_skillCurrent.SetInfo(_heroInfo, _skillId, _skillLv);
        Cell_skillCurrent.ShowBottom();
        // 升级后的技能显示
        var skillCfg = HeroSkillHelper.GetSkillConfig(_skillId, _skillLv);
        var isMax = HeroSkillHelper.LevelMax(_skillId, _skillLv);
        var newLevel = isMax ? _skillLv : _skillLv + 1;
        Cell_skillNext.SetInfo(_heroInfo, _skillId, newLevel);
        Cell_skillNext.ShowBottom();

        if (!isMax)
        {
            Cost_up.SetRequire(skillCfg.costIds);
        }
        Label_upButton.SetLocalizer(isMax ? "M4_level_max" : "M4_level_up");
    }
    
    private void _BindReminders()
    {
        Reminder.Bind($"{HeroReminderConst.Hero_heroSkillPrefix}{_heroInfo.HeroId}_{_skillId}", Button_up);
    }

    private void _OnChanged(HeroNotifyEnum notifyEnum)
    {
        switch (notifyEnum)
        {
            case HeroNotifyEnum.SkillReset:
                UIEngine.Stuff.Back();
                break;
        }
    }
}

public class HeroSkillUpPopParam
{
    public HeroInfo heroInfo;
    public int skillId;
    public int skillLv;
}

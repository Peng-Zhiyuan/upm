using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public partial class HeroLevelUpPop : Page
{
    private HeroInfo _heroInfo;
    private UpMode _upMode;
    private int _levelAvailableMax;
    private int _aimLevel;
    private JLocker _locker;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;

            _RefreshView();
            _BindReminders();
        }
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "up":
                _HandleUp();
                break;
            case "long_press":
                _HandleHoldingInvoke();
                break;
            case "long_press_end":
                _HandleHoldingEnd();
                break;
        }
    }

    public void OnReset()
    {
        if (_heroInfo.Level <= 1)
        {
            ToastManager.ShowLocalize("M4_hero_words_no_need_reset");
            return;
        }

        UIEngine.Stuff.ForwardOrBackTo<HeroLevelResetPop>(_heroInfo);
    }

    private async void _HandleUp(int destLevel = 0)
    {
        if (_heroInfo.Level >= _heroInfo.MaxLevel)
        {
            ToastManager.ShowLocalize("M4_max_already");
            return;
        }

        if (_locker.IsOn)
        {
            ToastManager.ShowLocalize("common_clickCooldown");
            return;
        }

        var prevPower = _heroInfo.Power;
        if (_heroInfo.NextBreakLevel == _heroInfo.Level)
        {
            var requireItems = _heroInfo.LevelUpMap[_heroInfo.Level].advanceSubs;
            if (!ItemUtil.IsEnough(requireItems))
            {
                ToastManager.ShowLocalize("M4_not_enough");
                return;
            }

            _locker.On();
            // 然后打点上报
            await HeroApi.BreakAsync(_heroInfo.HeroId);
            _locker.Off();
            // 播放突破声音
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_upgrade");
        }
        else
        {
            // 至少升到下一级
            if (destLevel <= 0)
            {
                destLevel = _heroInfo.Level + 1;
            }

            // 计算消耗总量
            var costMap = new Dictionary<int, int>();
            for (var lv = _heroInfo.Level; lv < destLevel; ++lv)
            {
                var costs = _heroInfo.LevelUpMap[lv].Subs;
                foreach (var costItem in costs)
                {
                    costMap.TryGetValue(costItem.Id, out var num);
                    costMap[costItem.Id] = num + costItem.Num;
                }
            }

            if (!ItemUtil.IsEnough(costMap))
            {
                ToastManager.ShowLocalize("M4_not_enough");
                return;
            }

            _locker.On();
            // 记录是否是升多级
            var multiLevelup = destLevel - _heroInfo.Level > 1;
            await HeroApi.LevelUpAsync(_heroInfo.HeroId, destLevel);
            _locker.Off();
            if (!multiLevelup)
            {
                // 播放升级声音
                WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_upgrade");
            }
            // 消耗上报
            CostReporter.Report(costMap, "level_up");
        }

        // 上报战力变化
        HeroProxy.HandlePowerChange(_heroInfo, prevPower);
        // 标记需要刷新
        HeroNotifier.Invoke(HeroNotifyEnum.Level);
        // 更新红点逻辑
        HeroReminderProxy.UpdateReminder_LevelChanged(_heroInfo);
        // 因为异步，所以要在界面更新前判断是否还在这个界面
        if (null == this) return;
        _RefreshView();
    }

    private void _HandleHoldingInvoke()
    {
        if (_levelAvailableMax <= _heroInfo.Level)
        {
            Button_up.GetComponent<HoldingButton>().Exit();
            ToastManager.ShowLocalize("M4_not_enough");
            return;
        }

        ++_aimLevel;
        _DisplayLevelAscending(_aimLevel);
        // 播放升级声音
        WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_upgrade");
        if (_aimLevel >= _levelAvailableMax)
        {
            _HandleUp(_aimLevel);
            Button_up.GetComponent<HoldingButton>().Exit();
        }
    }

    private void _HandleHoldingEnd()
    {
        _HandleUp(_aimLevel);
    }

    private void Awake()
    {
        Cost_up.RequireFormat = "common_format_cost".Localize();
        // others
        _locker = new JLocker(1000);
    }

    private void OnEnable()
    {
        HeroNotifier.OnChange += _OnChanged;
    }

    private void OnDisable()
    {
        HeroNotifier.OnChange -= _OnChanged;
    }

    private void _BindReminders()
    {
        Reminder.Bind($"{HeroReminderConst.Hero_heroLevelUpButtonPrefix}{_heroInfo.HeroId}", Button_up);
    }

    private UpMode _GetUpMode(int level)
    {
        UpMode upMode;
        if (level == _heroInfo.MaxLevel)
        {
            upMode = UpMode.Max;
        }
        else if (_heroInfo.NextBreakLevel == level)
        {
            upMode = UpMode.Breaking;
        }
        else
        {
            upMode = UpMode.Normal;
        }

        return upMode;
    }

    private void _RefreshView()
    {
        // 先更新upMode
        _upMode = _GetUpMode(_heroInfo.Level);
        var levelUpMode = _upMode == UpMode.Normal;
        if (levelUpMode) _GetAvailableMaxLevel();
        // 更新原始属性信息
        Txt_hp.text = $"{_heroInfo.GetAttribute(HeroAttr.HP)}";
        Txt_attack.text = $"{_heroInfo.GetAttribute(HeroAttr.ATK)}";
        Txt_defence.text = $"{_heroInfo.GetAttribute(HeroAttr.DEF)}";
        // 增加的属性
        _RenderAddAttr(HeroAttr.HP, Txt_hp_add);
        _RenderAddAttr(HeroAttr.ATK, Txt_attack_add);
        _RenderAddAttr(HeroAttr.DEF, Txt_defence_add);
        // 其他区域
        _DisplayLevelAscending(_heroInfo.Level);
        // 按钮点击组件也要更新
        if (GuideManagerV2.Stuff.IsExecutingForceGuide || GuideManagerV2.Stuff.IsExecutingTriggredGuide)
        {
            Button_up.GetComponent<HoldingButton>().enabled = false;
            Button_up.GetComponent<Button>().enabled = true;
        }
        else
        {
            Button_up.GetComponent<HoldingButton>().enabled = levelUpMode;
            Button_up.GetComponent<Button>().enabled = !levelUpMode;
        }
    }

    private void _RenderAddAttr(HeroAttr attr, Text txt)
    {
        var isMax = UpMode.Max == _upMode;
        txt.gameObject.SetActive(!isMax);
        if (isMax) return;

        var lv = _heroInfo.Level;
        var levelMap = _heroInfo.LevelUpMap;
        var index = (int) attr - 1;
        var valAdded = 0;
        switch (_upMode)
        {
            case UpMode.Normal:
                var current = levelMap[lv].levelAttrs[index];
                var next = levelMap[lv + 1].levelAttrs[index];
                valAdded = next - current;
                // 如果当前等级是突破等级的话，还得额外扣掉
                if (_heroInfo.BreakList.Contains(lv))
                {
                    valAdded -= levelMap[lv].advanceAttrs[index];
                }

                break;
            case UpMode.Breaking:
                valAdded = levelMap[lv].advanceAttrs[index];
                break;
        }

        if (HeroAttrHelper.GetRelatedAttribute(attr, out var relatedAttr))
        {
            var ratio = _heroInfo.GetAttribute(relatedAttr);
            valAdded = (int) (valAdded * (1000L + ratio) / 1000);
        }

        txt.text = $"+{valAdded}";
    }

    private void _GetAvailableMaxLevel()
    {
        var nextLimit = _heroInfo.LevelUpLimit;
        var costMap = new Dictionary<int, int>();
        _levelAvailableMax = nextLimit;
        for (var lv = _heroInfo.Level; lv < nextLimit; ++lv)
        {
            foreach (var costItem in _heroInfo.LevelUpMap[lv].Subs)
            {
                costMap.TryGetValue(costItem.Id, out var num);
                costMap[costItem.Id] = num + costItem.Num;
            }

            if (!ItemUtil.IsEnough(costMap))
            {
                _levelAvailableMax = lv;
                break;
            }
        }

        // aim level也要重置
        _aimLevel = _heroInfo.Level;
    }

    private void _DisplayLevelAscending(int level)
    {
        var upMode = _GetUpMode(level);
        Node_top.Selected = (int) upMode;
        switch (upMode)
        {
            case UpMode.Max:
                Txt_hint.text = "";
                Label_cost.text = "";
                Label_upButton.SetLocalizer("M4_level_max");
                break;
            case UpMode.Breaking:
                Txt_hint.SetLocalizer("M4_hero_hint_break");
                Label_cost.SetLocalizer("M4_hero_words_cost_break");
                Label_upButton.SetLocalizer("M4_rank_up");
                Txt_breakLevel.text = $"<size=60>Lv.</size>{_heroInfo.NextLimit}";
                Cost_up.SetRequire(_heroInfo.LevelUpMap[level].advanceSubs);
                break;
            case UpMode.Normal:
                Txt_hint.SetLocalizer("M4_hero_hint_levelUp");
                Label_cost.SetLocalizer("M4_hero_words_cost_levelup");
                Label_upButton.SetLocalizer("M4_level_up");
                Cost_up.SetRequire(_heroInfo.LevelUpMap[level].Subs);
                Txt_lv1.text = $"<size=28>Lv.</size>{level}";
                Txt_lv2.text = $"<size=28>Lv.</size>{level + 1}";
                break;
        }
    }

    private void _OnChanged(HeroNotifyEnum notifyEnum)
    {
        switch (notifyEnum)
        {
            case HeroNotifyEnum.LevelReset:
                _RefreshView();
                break;
        }
    }
}

internal enum UpMode
{
    Normal = 0,
    Breaking,
    Max
}
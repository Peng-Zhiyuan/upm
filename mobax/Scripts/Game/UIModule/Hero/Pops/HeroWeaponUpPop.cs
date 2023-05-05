using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroWeaponUpPop : Page
{
    private HeroInfo _heroInfo;
    private HeroWeaponInfo _weaponInfo;
    private WeaponUpMode _upMode;
    private int _levelAvailableMax;
    private int _aimLevel;
    private JLocker _locker;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;
            _weaponInfo = heroInfo.Weapon;

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
        if (_weaponInfo.Level <= 1)
        {
            ToastManager.ShowLocalize("M4_hero_words_no_need_reset");
            return;
        }

        // UIEngine.Stuff.Forward<HeroLevelResetPop>(_heroInfo);
    }

    private async void _HandleUp(int destLevel = 0)
    {
        if (_weaponInfo.ReachedMax)
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
        if (_weaponInfo.NextBreakLevel == _weaponInfo.Level)
        {
            var conf = StaticData.WeaponExpTable.TryGet(_weaponInfo.Level);
            var requireItems = conf.advanceSubs;
            if (!ItemUtil.IsEnough(requireItems))
            {
                ToastManager.ShowLocalize("M4_not_enough");
                return;
            }
            _locker.On();
            // 突破
            await HeroWeaponApi.BreakAsync(_heroInfo.HeroId);
            _locker.Off();
            // 播放突破声音
            WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_upgrade");
        }
        else
        {
            if (_weaponInfo.Level >= _heroInfo.Level)
            {
                ToastManager.ShowLocalize("M4_weapon_content_hero_lv_limit");
                return;
            }
            
            // 至少升到下一级
            if (destLevel <= 0)
            {
                destLevel = _weaponInfo.Level + 1;
            }
            // 计算消耗总量
            var costMap = new Dictionary<int, int>();
            for (var lv = _weaponInfo.Level; lv < destLevel; ++lv)
            {
                var conf = StaticData.WeaponExpTable.TryGet(lv);
                var costs = conf.Subs;
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
            var multiLevelup = destLevel - _weaponInfo.Level > 1;
            await HeroWeaponApi.LevelUpAsync(_heroInfo.HeroId,  destLevel);
            _locker.Off();
            if (!multiLevelup)
            {
                // 播放升级声音
                WwiseEventManager.SendEvent(TransformTable.UiControls, "ui_upgrade");
            }
            // 消耗上报
            CostReporter.Report(costMap, "weapon_up");
        }

        _RefreshView();
        // 上报战力变化
        HeroProxy.HandlePowerChange(_heroInfo, prevPower);
        // 标记需要刷新
        HeroNotifier.Invoke(HeroNotifyEnum.WeaponLevel);
        // 更新红点逻辑
        HeroReminderProxy.UpdateReminder_WeaponLevelChanged(_heroInfo);
    }

    private void _HandleHoldingInvoke()
    {
        if (_levelAvailableMax <= _weaponInfo.Level)
        {
            Button_up.GetComponent<HoldingButton>().Exit();
            // 目前就只有这两种情况
            ToastManager.ShowLocalize(_weaponInfo.Level >= _heroInfo.Level
                ? "M4_weapon_content_hero_lv_limit"
                : "M4_not_enough");
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

    private void _BindReminders()
    {
        Reminder.Bind($"{HeroReminderConst.Hero_weaponLevelUpButtonPrefix}{_heroInfo.HeroId}", Button_up);
    }

    private WeaponUpMode _GetUpMode(int level)
    {
        WeaponUpMode upMode;
        if (_weaponInfo.ReachedMax)
        {
            upMode = WeaponUpMode.Max;
        }
        else if (_weaponInfo.NextBreakLevel == level)
        {
            upMode = WeaponUpMode.Breaking;
        }
        else
        {
            upMode = WeaponUpMode.Normal;
        }
        
        return upMode;
    }
    
    private void _RefreshView()
    {
        // 先更新upMode
        _upMode = _GetUpMode(_weaponInfo.Level);
        var levelUpMode = _upMode == WeaponUpMode.Normal;
        var breakMode = _upMode == WeaponUpMode.Breaking;
        Switcher_attrNode.Selected = breakMode;
        if (breakMode)
        {
            var breakLv = _weaponInfo.Level;
            var attrValues = _weaponInfo.LevelUpMap[breakLv].advanceAttrs;
            // 上一级的也需要判断
            _weaponInfo.LevelUpMap.TryGetValue(breakLv - 1, out var prevConf);
            for (var index = 0; index < attrValues.Length; index++)
            {
                var attrVal = attrValues.GetItem(index);
                var prevVal = prevConf == null ? 0 : prevConf.advanceAttrs.GetItem(index);
                if (attrVal != prevVal)
                {
                    var attr = HeroAttr.VIM + index;
                    var conf = StaticData.HeroAttrTable.TryGet((int) attr);
                    var val = _weaponInfo.GetAttribute(attr);
                    Txt_break_attr.SetLocalizer(conf.Desc);
                    Txt_break_current.text = HeroAttrHelper.GetAttrExpression(attr, val);
                    _RenderAttrAddText(attr, Txt_break_add, attrVal - prevVal);
                    break;
                }
            }
        }
        else
        {
            // 更新原始属性信息
            Txt_hp.text = $"{_weaponInfo.GetAttribute(HeroAttr.HP)}";
            Txt_attack.text = $"{_weaponInfo.GetAttribute(HeroAttr.ATK)}";
            Txt_defence.text = $"{_weaponInfo.GetAttribute(HeroAttr.DEF)}";
            // 增加的属性
            _RenderAddAttr(HeroAttr.HP, Txt_hp_add);
            _RenderAddAttr(HeroAttr.ATK, Txt_attack_add);
            _RenderAddAttr(HeroAttr.DEF, Txt_defence_add);
            _GetAvailableMaxLevel();
        }
        // 其他区域
        _DisplayLevelAscending(_weaponInfo.Level);
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
        var isMax = WeaponUpMode.Max == _upMode;
        txt.gameObject.SetActive(!isMax);
        if (isMax) return;

        var lv = _weaponInfo.Level;
        var valAdded = _weaponInfo.GetAttribute(lv + 1, attr) - _weaponInfo.GetAttribute(lv, attr);
        _RenderAttrAddText(attr, txt, valAdded);
    }

    private void _RenderAttrAddText(HeroAttr attr, Text txt, int val)
    {
        if (val == 0)
        {
            txt.text = "";
            return;
        }
        
        var flag = val > 0 ? "+" : "";
        txt.text = $"{flag}{HeroAttrHelper.GetAttrExpression(attr, val)}";
    }

    private void _GetAvailableMaxLevel()
    {
        var costMap = new Dictionary<int, int>();
        var lvLimit = Math.Min(_weaponInfo.LevelUpLimit, _heroInfo.Level);
        _levelAvailableMax = lvLimit;
        for (var lv = _weaponInfo.Level; lv < lvLimit; ++lv)
        {
            var conf = StaticData.WeaponExpTable.TryGet(lv);
            foreach (var costItem in conf.Subs)
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
        _aimLevel = _weaponInfo.Level;
    }

    private void _DisplayLevelAscending(int level)
    {
        var upMode = _GetUpMode(level);
        Node_top.Selected = (int) upMode;
        var conf = StaticData.WeaponExpTable.TryGet(level);
        switch (upMode)
        {
            case WeaponUpMode.Max:
                Txt_hint.text = "";
                Label_cost.text = "";
                Label_upButton.SetLocalizer("M4_level_max");
                break;
            case WeaponUpMode.Breaking:
                Txt_hint.SetLocalizer("M4_hero_hint_break");
                Label_cost.SetLocalizer("M4_hero_words_cost_break");
                Label_upButton.SetLocalizer("M4_rank_up");
                Txt_breakLevel.text = $"<size=60>Lv.</size>{_weaponInfo.NextLimit}";
                Cost_up.SetRequire(conf.advanceSubs);
                break;
            case WeaponUpMode.Normal:
                Txt_hint.SetLocalizer("M4_weapon_content_level_up");
                Label_cost.SetLocalizer("M4_hero_words_cost_levelup");
                Label_upButton.SetLocalizer("M4_level_up");
                Cost_up.SetRequire(conf.Subs);
                Txt_lv1.text = $"<size=28>Lv.</size>{level}";
                Txt_lv2.text = $"<size=28>Lv.</size>{level + 1}";
                break;
        }
    }
}

internal enum WeaponUpMode
{
    Normal = 0,
    Breaking,
    Max
}
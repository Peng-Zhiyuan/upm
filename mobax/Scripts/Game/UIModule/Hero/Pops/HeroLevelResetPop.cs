using System;
using System.Collections.Generic;
using UnityEngine.UI;

public partial class HeroLevelResetPop : Page
{
    private HeroInfo _heroInfo;
    private int _costItem;
    private List<CostItem> _returnItems;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;

            _RefreshData();
            _RefreshView();
        }
    }

    public void OnCancel()
    {
        UIEngine.Stuff.Back();
    }

    public async void OnReset()
    {
        if (!ItemUtil.IsEnough(_costItem))
        {
            ToastManager.ShowLocalize("M4_not_enough");
            return;
        }

        var prevPower = _heroInfo.Power;
        await HeroApi.LevelResetAsync(_heroInfo.HeroId);
        // 上报战力变化
        HeroProxy.HandlePowerChange(_heroInfo, prevPower);
        // 标记需要刷新
        HeroNotifier.Invoke(HeroNotifyEnum.LevelReset);
        // 更新红点逻辑
        HeroReminderProxy.UpdateReminder_LevelChanged(_heroInfo);
        // 先关闭窗口
        await UIEngine.Stuff.RemoveFromStackAsync(GetType().Name);
    }

    private void Awake()
    {
        // Cost_up.RequireFormat = "common_format_cost".Localize();
        _costItem = StaticData.BaseTable.TryGet("HeroLvResetSub");
        Cost_reset.SetRequire(_costItem, 1);
    }

    private void _RefreshData()
    {
    }
    
    private void _RefreshView()
    {
        Cell_heroCurrent.SetInfo(_heroInfo);
        Cell_heroAfterReset.SetInfo(_heroInfo, 1);
        
        // 计算返还的道具数量
        var costDic = new Dictionary<int, int>();
        // 升级的消耗加回
        for (var i = 1; i < _heroInfo.Level; ++i)
        {
            var levelInfo = _heroInfo.LevelUpMap[i];
            foreach (var item in levelInfo.Subs)
            {
                costDic.TryGetValue(item.Id, out var num);
                costDic[item.Id] = num + item.Num;
            }
        }
        // 突破的消耗加回
        foreach (var breakLv in _heroInfo.BreakList)
        {
            if (breakLv > _heroInfo.BreakLevel) break;

            var levelInfo = _heroInfo.LevelUpMap[breakLv];
            foreach (var item in levelInfo.advanceSubs)
            {
                costDic.TryGetValue(item.Id, out var num);
                costDic[item.Id] = num + item.Num;
            }
        }
        // 转成List
        var itemList = new List<VirtualItem>();
        foreach (var kv in costDic)
        {
            itemList.Add(new VirtualItem {id = kv.Key, val = kv.Value});
        }
        ItemList_reset.Set(itemList);
    }

}

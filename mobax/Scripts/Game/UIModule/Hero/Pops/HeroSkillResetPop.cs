using System.Collections.Generic;
using BattleEngine.Logic;

public partial class HeroSkillResetPop : Page
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
        if (!await ItemUtil.IsEnoughAsync(_costItem, 1, true)) return;

        var prevPower = _heroInfo.Power;
        await HeroApi.SkillResetAsync(_heroInfo.HeroId);
        // 上报战力变化
        HeroProxy.HandlePowerChange(_heroInfo, prevPower);
        // 先关闭窗口
        UIEngine.Stuff.Back();
        // 标记需要刷新
        HeroNotifier.Invoke(HeroNotifyEnum.SkillReset);
        // 更新红点逻辑
        HeroReminderProxy.UpdateReminder_SkillChanged(_heroInfo);
    }

    private void Awake()
    {
        // Cost_reset.RequireFormat = "common_format_cost".Localize();
        _costItem = StaticData.BaseTable.TryGet("SkillResetSub");
        Cost_reset.SetRequire(_costItem, 1);
    }

    private void _RefreshData()
    {
    }

    private void _RefreshView()
    {
        Skill_common.SetInfo(_heroInfo);
        Skill_ulitmate.SetInfo(_heroInfo);
        Skill_common_reset.SetInfo(_heroInfo);
        Skill_ulitmate_reset.SetInfo(_heroInfo);

        // 计算返还的道具数量
        var costDic = new Dictionary<int, int>();
        var skillUltimate = HeroSkillHelper.GetSkillId(_heroInfo, SKILL_TYPE.SPSKL);
        var levelUltimate = _heroInfo.GetSkillLevel(skillUltimate);
        var skillCommon = HeroSkillHelper.GetSkillId(_heroInfo, SKILL_TYPE.SSP);
        var levelCommon = _heroInfo.GetSkillLevel(skillCommon);
        // 大招的消耗
        _AddCost(costDic, skillUltimate, levelUltimate);
        // 普通技能的消耗
        _AddCost(costDic, skillCommon, levelCommon);
        // 转成List
        var itemList = new List<VirtualItem>();
        foreach (var kv in costDic)
        {
            itemList.Add(new VirtualItem {id = kv.Key, val = kv.Value});
        }

        ItemList_reset.Set(itemList);
    }

    private void _AddCost(Dictionary<int, int> costDic, int skillId, int skillLv)
    {
        if (skillLv <= 1) return;

        var list = StaticData.SkillTable.TryGet(skillId);
        foreach (var cfg in list.Colls)
        {
            if (cfg.Level >= skillLv) break;

            foreach (var item in cfg.costIds)
            {
                costDic.TryGetValue(item.Id, out var num);
                costDic[item.Id] = num + item.Num;
            }
        }
    }
}
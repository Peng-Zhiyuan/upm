using UnityEngine;

public partial class HeroBookProfile : HeroBookBasePane
{
    protected override void _InternalRefreshHero()
    {
        if (!_heroChanged) return;
        
        var introCfg = StaticData.LibInfoTable.TryGet(_heroInfo.HeroId);
        if (null == introCfg)
        {
            Debug.LogError($"英雄：{_heroInfo.HeroId}未配置(table:LibInfo)");
            return;
        }
        
        Txt_intro.SetLocalizer(introCfg.Info);
        Txt_country.SetLocalizer(introCfg.Place);
        Txt_faction.SetLocalizer(StaticData.LibFactionInfoTable.TryGet(introCfg.Faction).Name);
    }
}
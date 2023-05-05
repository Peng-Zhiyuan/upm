public static class HeroProxy
{
    public static void HandlePowerChange(HeroInfo heroInfo, int prevPower = 0)
    {
        var newPower = heroInfo.Power;
        // 英雄战力更新
        HeroApi.ReportPowerAsync(heroInfo.HeroId, newPower);
        // 编组相关战力计算并更新
        FormationPowerProxy.HeroUpdate(heroInfo);

        // 展示战力变化的floating
        if (prevPower != newPower)
        {
            var floating = UIEngine.Stuff.ShowFloatingImediatly<PowerChangeFloating>();
            floating.SetInfo(prevPower, newPower);
        }
    }
}
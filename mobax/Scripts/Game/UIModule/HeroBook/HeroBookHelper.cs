public static class HeroBookHelper
{
    public static bool CheckUnlock(HeroInfo heroInfo, LibPlateRow cfg, out string lockMessage)
    {
        switch ((UnlockType) cfg.Unlock)
        {
            case UnlockType.T1_Break:
                return CheckBreakUnlock(heroInfo, cfg.Value, out lockMessage);
            case UnlockType.T2_Plot:
                return CheckPlotUnlock(cfg.Value, cfg.plotDesc, out lockMessage);
        }

        lockMessage = null;
        return true;
    }

    public static bool CheckUnlock(HeroInfo heroInfo, LibPlateRow cfg)
    {
        return CheckUnlock(heroInfo, cfg, out _);
    }

    public static bool CheckPlotUnlock(int plotItem, string plotName, out string lockMessage)
    {
        if (ItemUtil.IsEnough(plotItem))
        {
            lockMessage = null;
            return true;
        }

        lockMessage = "M4_herobook_tip_lock_plot".Localize(plotName.Localize());
        return false;
    }
    
    public static bool CheckPlotUnlock(int plotItem, string plotName)
    {
        return CheckPlotUnlock(plotItem, plotName, out _);
    }

    public static bool CheckBreakUnlock(HeroInfo heroInfo, int level, out string lockMessage)
    {
        if (heroInfo.BreakLevel >= level)
        {
            lockMessage = null;
            return true;
        }

        lockMessage = "M4_herobook_tip_lock_break".Localize($"{level}");
        return false;
    }

    public static bool CheckBreakUnlock(HeroInfo heroInfo, int level)
    {
        return CheckBreakUnlock(heroInfo, level, out _);
    }
}
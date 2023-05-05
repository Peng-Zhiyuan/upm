using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HotPreset : HotPresetSettings
{
    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool GuideV2
    {
        get
        {
            return HotLocalSettings.IsGuideV2Enabled;
        }
        set
        {
            HotLocalSettings.IsGuideV2Enabled = value;
        }
    }

    public static string GuideV2_GetDefault()
    {
        return HotLocalSettings.IsGuideV2_Default.ToString();
    }

    public static void GuideV2_DeleteLocalCache()
    {
        HotLocalSettings.IsGuideV2_DeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool IsSkipPlot
    {
        get
        {
            return HotLocalSettings.IsSkipPlot;
        }
        set
        {
            HotLocalSettings.IsSkipPlot = value;
        }
    }

    public static string IsSkipPlot_GetDefault()
    {
        return HotLocalSettings.IsSkipPlot_Default.ToString();
    }

    public static void IsSkipPlot_DeleteLocalCache()
    {
        HotLocalSettings.IsSkipPlot_DeleteLocalCache();
    }



    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool Chat
    {
        get
        {
            return HotLocalSettings.IsChatEnabled;
        }
        set
        {
            HotLocalSettings.IsChatEnabled = value;
        }
    }

    public static string Chat_GetDefault()
    {
        return HotLocalSettings.IsChatEnabled_Default.ToString();
    }

    public static void Chat_DeleteLocalCache()
    {
        HotLocalSettings.IsChatEnabled_DeleteLocalCache();
    }




    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool HeartBeat
    {
        get
        {
            return HotLocalSettings.IsHeartBeat;
        }
        set
        {
            HotLocalSettings.IsHeartBeat = value;
        }
    }

    public static string HeartBeat_GetDefault()
    {
        return HotLocalSettings.IsHeatBeat_Default.ToString();
    }

    public static void HeartBeat_DeleteLocalCache()
    {
        HotLocalSettings.IsHeatBeat_DeleteLocalCache();
    }


    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool SupportVideo
    {
        get
        {
            return HotLocalSettings.IsSuppurtVideo;
        }
        set
        {
            HotLocalSettings.IsSuppurtVideo = value;
        }
    }

    public static string SupportVideo_GetDefault()
    {
        return HotLocalSettings.IsSuppurtVideo_Default.ToString();
    }

    public static void SupportVideo_DeleteLocalCache()
    {
        HotLocalSettings.IsSuppurtVideo_DeleteLocalCache();
    }
}



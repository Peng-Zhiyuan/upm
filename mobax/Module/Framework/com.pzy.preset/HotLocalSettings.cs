using System.Collections;
using System.Collections.Generic;
using CustomLitJson;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 持久化设置
/// </summary>
public class HotLocalSettings
{


    public static string CustomGuestId
    {
        get
        {
            return PlayerPrefs.GetString(nameof(CustomGuestId), "");
        }
        set
        {
            PlayerPrefs.SetString(nameof(CustomGuestId), value);
        }
    }


    public static bool IsGuideV2Enabled
    {
        get
        {
            var defaultBool = IsGuideV2_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsGuideV2Enabled), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsGuideV2Enabled), value ? 1 : 0);
        }
    }
    public static bool IsGuideV2_Default
    {
        get
        {
            var defaultString = VariantManager.GetConfigOfCurrentVariant("guide.default", "true");
            return bool.Parse(defaultString);
        }
    }



    public static void IsGuideV2_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsGuideV2Enabled));
    }

    public static bool IsSkipPlot
    {
        get
        {
            var defaultBool = IsSkipPlot_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsSkipPlot), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsSkipPlot), value ? 1 : 0);
        }
    }

    public static bool IsSkipPlot_Default
    {
        get
        {
            var str = VariantManager.GetConfigOfCurrentVariant("skipPlot.default", "false");
            var ret = bool.Parse(str);
            return ret;
        }
    }

    public static void IsSkipPlot_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsSkipPlot));
    }


    public static bool IsChatEnabled
    {
        get
        {
            var defaultBool = IsChatEnabled_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsChatEnabled), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsChatEnabled), value ? 1 : 0);
        }
    }

    public static bool IsChatEnabled_Default
    {
        get
        {
            return true;
        }
    }

    public static void IsChatEnabled_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsChatEnabled));
    }



    public static bool IsHeartBeat
    {
        get
        {
            var defaultBool = IsHeatBeat_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsHeartBeat), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsHeartBeat), value ? 1 : 0);
        }
    }

    public static bool IsHeatBeat_Default
    {
        get
        {
            return true;
        }
    }

    public static void IsHeatBeat_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsHeartBeat));
    }


    public static bool IsSuppurtVideo
    {
        get
        {
            var defaultBool = IsSuppurtVideo_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsSuppurtVideo), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsSuppurtVideo), value ? 1 : 0);
        }
    }

    public static bool IsSuppurtVideo_Default
    {
        get
        {
            return true;
        }
    }

    public static void IsSuppurtVideo_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsSuppurtVideo));
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GamePreset : PresetSettings
{
    [DashboardGui(DashboardGuiType.Label)]
    public static string Variant
    {
        get
        {
            var env = VariantManager.Variant;
            return env;
        }
    }

    [DashboardGui(DashboardGuiType.Options)]
    public static string Env
    {
        get
        {
            var env = EnvManager.OriginEnv;
            return env;
        }
        set
        {
            EnvManager.OriginEnv = value;
            RecreatePresetPage();
        }
    }

    static void RecreatePresetPage()
    {
        var presetPage = GameObject.FindObjectOfType<PresetPage>();
        if(presetPage != null)
        {
            presetPage.Refresh();
        }
    }


    public static List<string> Env_GetOptionList()
    {
        var envList = EnvManager.EnvList;
        return envList;
    }

    public static string Env_GetDefault()
    {
        var envList = EnvManager.OriginEnvDefault;
        return envList;
    }

    public static void Env_DeleteLocalCache()
    {
        EnvManager.DeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Options)]
    public static string Branch
    {
        get
        {
            var env = EnvManager.Branch;
            return env;
        }
        set
        {
            EnvManager.Branch = value;
        }
    }

    public static List<string> Branch_GetOptionList()
    {
        var ret = EnvManager.BranchListOfFinalEnv;
        return ret;
    }

    public static string Branch_GetDefault()
    {
        var ret = EnvManager.DefaultBranchOfFinalEnv;
        return ret;
    }

    public static void Branch_DeleteLocalCache()
    {
        EnvManager.DeleteCachedBranch();
    }

    [DashboardGui(DashboardGuiType.Options)]
    public static string FPS
    {
        get
        {
            var intFps = DeveloperLocalSettings.FPS;
            return intFps.ToString();
        }
        set
        {
            var intFps = int.Parse(value);
            DeveloperLocalSettings.FPS = intFps;
            Application.targetFrameRate = intFps;
        }
    }

    public static List<string> FPS_GetOptionList()
    {
        return new List<string>() { "30", "60" };
    }

    public static string FPS_GetDefault()
    {
        return DeveloperLocalSettings.FpsDefault.ToString();
    }

    public static void FPS_DeleteLocalCache()
    {
         DeveloperLocalSettings.FpsDeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool Remote
    {
        get
        {
            return DeveloperLocalSettings.ConnectRemote;
        }
        set
        {
            DeveloperLocalSettings.ConnectRemote = value;
        }
    }

    public static string Remote_GetDefault()
    {
        return DeveloperLocalSettings.ConnectRemote_Default.ToString();
    }

    public static void Remote_DeleteLocalCache()
    {
        DeveloperLocalSettings.ConnectRemote_DeleteLocalCache();
    }



    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool ResUpdate
    {
        get
        {
            return DeveloperLocalSettings.IsResUpdateEnabled;
        }
        set
        {
            DeveloperLocalSettings.IsResUpdateEnabled = value;
        }
    }

    public static string ResUpdate_GetDefault()
    {
        return DeveloperLocalSettings.IsResUpdateEnabled_Default.ToString();
    }

    public static void ResUpdate_DeleteLocalCache()
    {
        DeveloperLocalSettings.IsResUpdateEnabled_DeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool UseWwise
    {
        get
        {
            return DeveloperLocalSettings.IsUseWwise;
        }
        set
        {
            DeveloperLocalSettings.IsUseWwise = value;
        }
    }

    public static string UseWwise_GetDefault()
    {
        return DeveloperLocalSettings.IsUseWwise_Default.ToString();
    }

    public static void UseWwise_DeleteLocalCache()
    {
        DeveloperLocalSettings.IsUseWwise_DeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool WwiseUseExternalRes
    {
        get
        {
            return DeveloperLocalSettings.IsWwiseUseExternlRes;
        }
        set
        {
            DeveloperLocalSettings.IsWwiseUseExternlRes = value;
        }
    }

    public static string WwiseUseExternalRes_GetDefault()
    {
        return DeveloperLocalSettings.IsWwiseUseExternlRes_Default.ToString();
    }

    public static void WwiseUseExternalRes_DeleteLocalCache()
    {
        DeveloperLocalSettings.IsWwiseUseExternlRes_DeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Options)]
    public static string Language
    {
        get
        {
            var ret = LanguageManager.Language;
            return ret;
        }
        set
        {
            LanguageManager.Language = value;
        }
    }

    public static List<string> Language_GetOptionList()
    {
        return LanguageManager.LanguageList;
    }

    public static string Language_GetDefault()
    {
        return LanguageManager.Language_Default;
    }

    public static void Language_DeleteLocalCache()
    {
        LanguageManager.DeleteCache();
    }


    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool SimuUpdateInEditor
    {
        get
        {
            return DeveloperLocalSettings.IsSimuUpdateInEditor;
        }
        set
        {
            DeveloperLocalSettings.IsSimuUpdateInEditor = value;
        }
    }

    public static string SimuUpdateInEditor_GetDefault()
    {
        return DeveloperLocalSettings.IsSimuUpdateInEditor_Default.ToString();
    }

    public static void SimuUpdateInEditor_DeleteLocalCache()
    {
        DeveloperLocalSettings.IsSimuUpdateInEditor_DeleteLocalCache();
    }

    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool AheadLogic
    {
        get
        {
            return DeveloperLocalSettings.IsAheadLogic;
        }
        set
        {
            DeveloperLocalSettings.IsAheadLogic = value;
        }
    }

    public static string AheadLogic_GetDefault()
    {
        return DeveloperLocalSettings.IsAheadLogic_Default.ToString();
    }

    public static void AheadLogic_DeleteLocalCache()
    {
        DeveloperLocalSettings.IsAheadLogic_DeleteLocalCache();
    }



    [DashboardGui(DashboardGuiType.Checkbox)]
    public static bool UseOverrideAssembly
    {
        get
        {
            return HotSystemConfig.overrideAssemblyData;
        }
        set
        {
            HotSystemConfig.overrideAssemblyData = value;
        }
    }

    public static string UseOverrideAssembly_GetDefault()
    {
        return HotSystemConfig.overrideAssemblyData.ToString();
    }

}



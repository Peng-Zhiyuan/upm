using System.Collections;
using System.Collections.Generic;
using CustomLitJson;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 持久化设置
/// </summary>
public class DeveloperLocalSettings
{

    /// <summary>
    /// 开启漫画剧情跳过
    /// </summary>
    public static bool IsComicsSkip
    {
        get
        {
            var defaultBool = IsComicsSkip_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsComicsSkip), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsComicsSkip), value ? 1 : 0);
        }
    }

    public static bool IsComicsSkip_Default
    {
        get
        {
            return false;
        }
    }

    public static void IsComics_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsComicsSkip));
    }

    public static bool IsSimuUpdateInEditor
    {
        get
        {
            var defaultBool = IsSimuUpdateInEditor_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsSimuUpdateInEditor), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsSimuUpdateInEditor), value ? 1 : 0);
        }
    }

    public static bool IsSimuUpdateInEditor_Default
    {
        get
        {
            return false;
        }
    }

    public static void IsSimuUpdateInEditor_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsSimuUpdateInEditor));
    }


    /// <summary>
    /// 开发者开关
    /// 在编辑器上默认值为 true
    /// 在真机上默认值为 false
    /// </summary>
    public static bool IsDevelopmentMode
    {
        get
        {
            var defaultValue = 0;
            var str = VariantManager.GetConfigOfCurrentVariant("developer.default", "false");
            if (str == "true")
            {
                defaultValue = 1;
            }

            return PlayerPrefs.GetInt(nameof(IsDevelopmentMode), defaultValue) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsDevelopmentMode), value ? 1 : 0);
        }
    }

    /// <summary>
    /// 在开发者模式下关闭右上角统计状态栏
    /// 默认为打开
    /// </summary>
    public static bool IsStatusOpenInDevMoe
    {
        get 
        { 
            return PlayerPrefs.GetInt(nameof(IsStatusOpenInDevMoe), 1) == 1;
        }
        set 
        { 
            PlayerPrefs.SetInt(nameof(IsStatusOpenInDevMoe), value ? 1 : 0); 
        }
    }

    /// <summary>
    /// FPS
    /// </summary>
    public static int FPS
    {
        get
        {
            //var @default = 30;
            //if(Application.isEditor)
            //{
            //    @default = 60;
            //}
            return PlayerPrefs.GetInt(nameof(FPS), FpsDefault);
        }
        set
        {
            PlayerPrefs.SetInt(nameof(FPS), value);
        }
    }

    public static int FpsDefault
    {
        get
        {
            return 30;
        }
    }

    public static void FpsDeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(FPS));
    }

    /// <summary>
    /// 无资源模式
    /// 不从远端服务器获取任何资源
    /// </summary>
    public static bool ConnectRemote
    {
        get
        {
            var defaultBool = ConnectRemote_Default;
            var defaultValue = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(ConnectRemote), defaultValue) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(ConnectRemote), value ? 1 : 0);
        }
    }

    public static bool ConnectRemote_Default
    {
        get
        {
            var defaultString = VariantManager.GetConfigOfCurrentVariant("remote.default", "true");
            return bool.Parse(defaultString);
        }
    }

    public static void ConnectRemote_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(ConnectRemote));
    }

    public static bool IsResUpdateEnabled
    {
        get
        {
            var defaultBool = IsResUpdateEnabled_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsResUpdateEnabled), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsResUpdateEnabled), value ? 1 : 0);
        }
    }

    public static bool IsResUpdateEnabled_Default
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static void IsResUpdateEnabled_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsResUpdateEnabled));
    }

    public static bool IsUseWwise
    {
        get
        {
            //return true;
            var defaultBool = IsUseWwise_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsUseWwise), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsUseWwise), value ? 1 : 0);
        }
    }

    public static bool IsUseWwise_Default
    {
        get
        {
            return true;
        }
    }

    public static void IsUseWwise_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsUseWwise));
    }

   

    public enum Quality
    {
        Low = 1,
        Mid = 2,
        High = 3,
    }

    public static Quality GraphicQuality
    {
        get
        {
            return PlayerPrefsUtil.GetEnum(nameof(GraphicQuality), Quality.Mid);
        }
        set
        {
            PlayerPrefsUtil.SetEnum(nameof(GraphicQuality), value);
        }
    }

    public static bool QualityDetected
    {
        get
        {
            return PlayerPrefs.GetInt("QualityDetected", 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt("QualityDetected", value? 1: 0);
        }
    }
    


    public static void GraphicQuality_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(GraphicQuality));
    }



    public static bool IsWwiseUseExternlRes
    {
        get
        {
            var defaultBool = IsWwiseUseExternlRes_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsWwiseUseExternlRes), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsWwiseUseExternlRes), value ? 1 : 0);
        }
    }

    public static bool IsWwiseUseExternlRes_Default
    {
        get
        {
            return false;
        }
    }

    public static void IsWwiseUseExternlRes_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsWwiseUseExternlRes));
    }



    public static bool IsAheadLogic
    {
        get
        {
            var defaultBool = IsAheadLogic_Default;
            var defaultInt = defaultBool ? 1 : 0;
            return PlayerPrefs.GetInt(nameof(IsAheadLogic), defaultInt) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(nameof(IsAheadLogic), value ? 1 : 0);
        }
    }

    public static bool IsAheadLogic_Default
    {
        get
        {
            return false;
        }
    }

    public static void IsAheadLogic_DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(nameof(IsAheadLogic));
    }



}
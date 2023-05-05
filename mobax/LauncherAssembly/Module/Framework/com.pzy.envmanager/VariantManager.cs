using System;
using UnityEngine;
using System.Collections.Generic;
using CustomLitJson;

public static class VariantManager
{
    static string _variant;
    public static string Variant
    {
        get
        {  
            if(_variant == null)
            {
                _variant = LauncherBuildInfo.Get("variant");
                if (_variant == "")
                {
                    _variant = GameManifestManager.Get("variant");
                    if(_variant == "")
                    {
                        throw new Exception("[VariantManager] config `variant` not found");
                    }
                }
            }
            return _variant;
        }
    }

    static JsonData _productJsonData;
    public static JsonData VariantJsonData
    {
        get
        {
            if(_productJsonData == null)
            {
                var product = Variant;
                var jdKey = $"variant.{product}";
                JsonData jd;
                if(Application.isPlaying)
                {
                    jd = GameManifestManager.GetObject(jdKey);
                }
                else
                {
                    jd = GameManifestManager.GetObjectInEditor(jdKey);
                }
                if (jd == null)
                {
                    throw new Exception($"[VariantManager] data of variant `{product}` not set in game-manifst");
                }
                _productJsonData = jd;

            }
            return _productJsonData;
        }
    }


    public static string GetConfigOfCurrentVariant(string key, string defaultValue = "")
    {
        var value = VariantJsonData.TryGet<string>(key);
        if(value == null)
        {
            return defaultValue;
        }
        return value;
    }
}
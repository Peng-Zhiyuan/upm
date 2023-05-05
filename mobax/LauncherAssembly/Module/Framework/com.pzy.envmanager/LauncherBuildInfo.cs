using UnityEngine;
using CustomLitJson;
using System;

public static class LauncherBuildInfo
{
    private static JsonData _unityManifest;
    public static JsonData UnityManifest
    {
        get
        {
            if (_unityManifest == null)
            {
                var textAsset = Resources.Load<TextAsset>("launcher-build-info");
                if (textAsset == null)
                {
                    throw new Exception("[BuildInfo] launcher-build-info.json not found");
                }
                var jsonString = textAsset.text;
                Debug.Log("[BuildInfo] launcher-build-info: " + jsonString);
                if (!string.IsNullOrEmpty(jsonString))
                {
                    _unityManifest = JsonMapper.Instance.ToObject(jsonString);
                }
                else
                {
                    _unityManifest = new JsonData();
                }
            }
            return _unityManifest;
        }
    }

    public static string Get(string key, string defaultValue = "")
    {
        if (Application.isEditor)
        {
            return defaultValue;
        }

        var ret = UnityManifest.TryGet<string>(key, null);
        if (ret != null)
        {
            return ret;
        }

        return defaultValue;
    }
}

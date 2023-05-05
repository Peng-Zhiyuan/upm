using UnityEngine;
using CustomLitJson;
using System;
using System.Threading.Tasks;

public static class HotBuildInfo
{
    public static async Task InitAsync()
    {
        if(Application.isEditor)
        {
            return;
        }
        var bucket = BucketManager.Stuff.Main;
        await bucket.AquireIfNeedAsync<TextAsset>("hot-build-info.json");
    }

    private static JsonData _unityManifest;
    public static JsonData UnityManifest
    {
        get
        {
            if (_unityManifest == null)
            {
                var bucket = BucketManager.Stuff.Main;
                var textAsset = bucket.Get<TextAsset>("hot-build-info.json");
                if (textAsset == null)
                {
                    throw new Exception("[HotBuildInfo] hot-build-info.json not found");
                }
                var jsonString = textAsset.text;
                Debug.Log("[HotBuildInfo] hot-biuld-info: " + jsonString);
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

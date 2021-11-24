using UnityEngine;
using System.Collections;
using CustomLitJson;
using UnityEngine.Assertions;
using System.Threading.Tasks;
using System;

public static class GameManifestManager 
{
    private static JsonData remoteManifest;

    private static JsonData _nativeManifest;
    public static JsonData NariveManifest
    {
        get
        {
            if (_nativeManifest == null)
            {
                _nativeManifest = new JsonData();
                string jsonString = null;
                if(NativeBridge.IsSupported)
                {
                    jsonString = NativeBridge.Call("NativeLuncherManifestManager", "GetRawString");
                }
                Debug.Log("[LuncherManifestManager] nativeManifest: " + jsonString);
                if(!string.IsNullOrEmpty(jsonString))
                {
                    _nativeManifest = JsonMapper.Instance.ToObject(jsonString);
                }
                else
                {
                    _nativeManifest = new JsonData();
                }
            }
            return _nativeManifest;
        }
    }

    private static JsonData _unityManifest;
    public static JsonData UnityManifest
    {
        get
        {
            if (_unityManifest == null)
            {
                var textAsset = Resources.Load<TextAsset>("game-manifest");
                if(textAsset == null)
                {
                    throw new Exception("[GameManifestManager] unity game-manifest.json not found");
                }
                var jsonString = textAsset.text;
                Debug.Log("[LuncherManifestManager] unityManifest: " + jsonString);
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

    public static JsonData GetObject(string key)
    {
        Assert.IsTrue(Application.isPlaying, "LuncherManifestManager can't running on editor script, read luncher-manifest.json directly");

        JsonData ret;
        //var ret = remoteManifest.TryGet<JsonData>(key, null);
        //if (ret != null)
        //{
        //    return ret;
        //}

        ret = NariveManifest.TryGet<JsonData>(key, null);
        if (ret != null)
        {
            return ret;
        }
        ret = UnityManifest.TryGet<JsonData>(key, null);
        if (ret != null)
        {
            return ret;
        }

        return null;
    }

    public static string Get(string key, string defaultValue = "")
    {
        if(!Application.isPlaying)
        {
            var ret2 = GetInEditor(key, defaultValue);
            return ret2;
        }

        string ret;

        //var ret = remoteManifest.TryGet<string>(key, null);
        //if(ret != null)
        //{
        //    return ret;
        //}

        ret = NariveManifest.TryGet<string>(key, null);
        if(ret != null)
        {
            return ret;
        }
        ret = UnityManifest.TryGet<string>(key, null);
        if(ret != null)
        {
            return ret;
        }

        return defaultValue;
    }

    public static string GetInEditor(string key, string defaultValue = "")
    {
        var textAsset = Resources.Load<TextAsset>("game-manifest");
        var jsonString = textAsset.text;
        JsonData jd = null;
        if (!string.IsNullOrEmpty(jsonString))
        {
            jd = JsonMapper.Instance.ToObject(jsonString);
        }
        else
        {
            jd = new JsonData();
        }

        var ret = jd.TryGet<string>(key, defaultValue);
        return ret;
    }

    public static string GetInLocal(string key, string defaultValue = "")
    {
        Assert.IsTrue(Application.isPlaying, "LuncherManifestManager can't running on editor script, read luncher-manifest.json directly");

        var ret = NariveManifest.TryGet<string>(key, null);
        if(ret != null)
        {
            return ret;
        }
        ret = UnityManifest.TryGet<string>(key, null);
        if(ret != null)
        {
            return ret;
        }

        return defaultValue;
    }
}

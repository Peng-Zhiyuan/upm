using System;
using UnityEngine;
using System.Collections.Generic;
using CustomLitJson;

public static class EnvManager
{
    public static string Env
    {
        get
        {  
            var entrace = PlayerPrefs.GetString("env", "");
            if(entrace == "" || !IsEnvValid(entrace))
            {
                entrace = LuncherManifestManager.Get("env.default");
            }
            return entrace;
        }
        set
        {
            var valid = IsEnvValid(value);
            if(!valid)
            {
                throw new Exception($"env {value} is invalid, forget to config in luncher-manifest.json?");
            }
            PlayerPrefs.SetString("env", value);
            Debug.Log("[EnvManager] env has been set to: " + value);
            EntraceChnaged?.Invoke(value);
        }
    }


    public static Action<string> EntraceChnaged;

    public static string GetConfigOfCurrentEntrace(string key)
    {
        var env = Env;
        if(string.IsNullOrEmpty(env))
        {
            throw new Exception("env not set, set EnvManager.Env first");
        }


        var fullKey = $"{env}.{key}";
        var value = LuncherManifestManager.Get(fullKey);
        return value;
    }


    public static JsonData GetConfigObjectOfCurrentEntrace(string key)
    {
        var env = Env;
        if (string.IsNullOrEmpty(env))
        {
            throw new Exception("env not set, set EnvManager.Env first");
        }

        var fullKey = $"{env}.{key}";
        var value = LuncherManifestManager.GetObject(fullKey);
        return value;
    }

    private static List<string> _envList;
    public static List<string> EnvList
    {
        get
        {
            if(_envList == null)
            {
                _envList = new List<string>();
                var str = LuncherManifestManager.Get("env.list");
                Debug.Log("env.list: " + str);
                var parts = str.Split(',');
                foreach (var entrace in parts)
                {
                    var trimed = entrace.Trim();
                    _envList.Add(trimed);
                }
            }
            return _envList;
        }
    }

    public static bool IsEnvValid(string env)
    {
        return  EnvList.Contains(env);
    }
}
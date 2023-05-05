using System;
using UnityEngine;
using System.Collections.Generic;
using CustomLitJson;

public static class EnvManager
{
    static string key = "env";
    public static string OriginEnv
    {
        get
        {  
            var env = PlayerPrefs.GetString(key, "");
            if(env == "" || !IsEnvValid(env))
            {
                env = OriginEnvDefault;
            }
            return env;
        }
        set
        {
            var valid = IsEnvValid(value);
            if(!valid)
            {
                throw new Exception($"env {value} is invalid. forget to config in game-manifest.json?");
            }
            PlayerPrefs.SetString(key, value);
            Debug.Log("[EnvManager] env has been set to: " + value);
        }
    }


    public static string overrideEnv;


    public static string FinalEnv
    {
        get
        {
            if(string.IsNullOrEmpty(overrideEnv))
            {
                return OriginEnv;
            }
            else
            {
                return overrideEnv;
            }
        }
    }

    public static string OriginEnvDefault
    {
        get
        {
            var value = VariantManager.GetConfigOfCurrentVariant("env.default");
            return value;
        }
    }

    public static void DeleteLocalCache()
    {
        PlayerPrefs.DeleteKey(key);
    }


    public static JsonData LocalConfig
    {
        get
        {
            var env = FinalEnv;
            var jd = GameManifestManager.GetObject(env);
            return jd;
        }
    }

    public static JsonData RemoteConfig
    {
        get
        {
            var isConnected = Remote.Stuff.isConnected;
            if(!isConnected)
            {
                return null;
            }
            var remoteJd = Remote.Stuff.RawData;
            return remoteJd;
        }
    }

    public static string GetConfigOfFinalEnv(string key, string defaultValue = "")
    {
        var env = FinalEnv;
        if (string.IsNullOrEmpty(env))
        {
            throw new Exception("env not set, set EnvManager.Env first");
        }

        var isConnected = Remote.Stuff.isConnected;
        if(isConnected)
        {
            var remoteJd = RemoteConfig;
            var has = remoteJd.HasKey(key);
            if(has)
            {
                var remoteValue = remoteJd.TryGet(key, "");
                return remoteValue;
            }
        }

        var envObj = LocalConfig;
        var value = envObj.TryGet(key, defaultValue);
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
                var str = GameManifestManager.Get("env.list");
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


    public static void DeleteCachedBranch()
    {
        PlayerPrefs.DeleteKey("branch");
    }

    public static string Branch
    {
        get
        {
            var value = PlayerPrefs.GetString("branch");
            if (string.IsNullOrEmpty(value))
            {
                return DefaultBranchOfFinalEnv;
            }
            var list = BranchListOfFinalEnv;
            if(list.Contains(value))
            {
                return value;
            }
            return DefaultBranchOfFinalEnv;
        }
        set
        {
            PlayerPrefs.SetString("branch", value);
        }
    }

    public static string Channel
    {
        get
        {
            var branch = Branch;
            var env = FinalEnv;
            var envDic = GameManifestManager.GetObject(env);
            var exists = envDic.HasKey("branchToChannel");
            if (!exists)
            {
                throw new Exception("no branchToChannel config in env: " + env);
            }
            var branchToChannelJd = envDic["branchToChannel"];
            var channel = branchToChannelJd[branch];
            var ret = channel.ToString();
            return ret;
        }
    }

    public static string DefaultBranchOfFinalEnv
    {
        get
        {
            var env = FinalEnv;
            var envDic = GameManifestManager.GetObject(env);
            var exists = envDic.HasKey("branch.default");
            if (exists)
            {
                var ret = envDic["branch.default"].ToString();
                return ret;
            }
            var list = BranchListOfFinalEnv;
            if(list.Count == 0)
            {
                throw new Exception("no branchToChannel config in env: " + env);
            }
            var first = list[0].ToString();
            return first;
        }
    }

    static string _lastEnvCache;
    static List<string> _lastBranchListCache;
    public static List<string> BranchListOfFinalEnv
    {
        get
        {
            var env = FinalEnv;
            if(env == _lastEnvCache)
            {
                return _lastBranchListCache;
            }

            var envDic = GameManifestManager.GetObject(env);
            var exists = envDic.HasKey("branchToChannel");
            if(!exists)
            {
                return new List<string>();
            }
            var branchToChannelJd = envDic["branchToChannel"];
            var dic = branchToChannelJd.ToDictionary();
            var keyConnection = dic.Keys;
            var ret = new List<string>();
            foreach(var key in keyConnection)
            {
                ret.Add(key);
            }
            _lastEnvCache = env;
            _lastBranchListCache = ret;
            return ret;
        }
    }
}
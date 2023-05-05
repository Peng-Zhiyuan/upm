using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using CustomLitJson;

public class Remote : LauncherStuffObject<Remote>
{
    [ShowInInspector]
    RemoteGateFile gateFile;

    [Obsolete("兼容线上代码，其值总是 main，待合适的时机删掉")]
    public string subEnv = "main";

    private JsonData _rawData;

    public JsonData RawData
    {
        get
        {
            if (this._rawData == null)
            {
                throw new Exception("conf not inited, call RemoteConfManager#SyncRemoteConfAsync() first");
            }
            return this._rawData;
        }
    }

    [ShowInInspector]
    private RemoteConf _remoteConf;
    public RemoteConf RemoteConf
    {
        get
        {
            if (this._remoteConf == null)
            {
                throw new Exception("conf not inited, call RemoteConfManager#SyncRemoteConfAsync() first");
            }
            return this._remoteConf;
        }
    }

    public static Func<string> getLanguageDelegate;
    public static Func<string> getBaseUrlDelegate;
    public bool isConnected;

    public enum ConnectResult
    {
        Pass,
        VersionNotSupported,
        Redirect
    }

    public async Task<(ConnectResult status, string redirectedToEnv)> ConnectAsync()
    {
        this.gateFile = await RequestGateFileAsync();
        var appVersion = Application.version;
        var command = this.gateFile.GetCommand(appVersion);

        // 没有找到匹配，或者匹配了，但是没有命令
        // 认为此版本不再支持
        if (command == "")
        {
            return (ConnectResult.VersionNotSupported, "");
            //throw new Exception("[GateManager] no subEnv found for version : " + appVersion + " platform: " + Application.platform);
        }
        else if (command.StartsWith("go"))
        {
            var parts = command.Split(' ');
            if (parts.Length < 2)
            {
                throw new Exception("[GateManager] gate commond error");
            }
            var toEnv = parts[1];
            return (ConnectResult.Redirect, toEnv);
        }
        else if (command == "pass")
        {
            await this.SyncRemoteConfAsync();
            this.isConnected = true;
            return (ConnectResult.Pass, "");
        }
        else
        {
            throw new Exception($"[GateManager] unknown command: {command}");
        }
    }

    public async Task SyncRemoteConfAsync()
    {
        var json = await Remote.Stuff.LoadAsync<string>(RemoteLocation.SubEnv, $"RemoteConfig.json", CacheType.None);
        Debug.Log("[Remote] remoteconf: " + json);
        this._rawData = JsonMapper.Instance.ToObject(json);
        this._remoteConf = JsonMapper.Instance.ToObject<RemoteConf>(json);
    }

    public string RootUrl
    {
        get
        {
            if(getBaseUrlDelegate == null)
            {
                throw new Exception("[Remote] getBaseUrlDelegate not set");
            }
            var url = getBaseUrlDelegate?.Invoke();
            return url;
        }
    }

    public string SubEnvUrl
    {
        get
        {
            if(subEnv == null)
            {
                throw new Exception("[Remote] subEnv not decide");
            }
            var root = this.RootUrl;
            var url = $"{root}/{subEnv}";
            return url;
        }
    }

    public string AddressableUrl
    {
        get
        {
            var subEnvUrl = this.SubEnvUrl;
            var ret = $"{subEnvUrl}/Addressable";
            return ret;
        }
    }

    public string CustomResUrl
    {
        get
        {
            var subEnvUrl = this.SubEnvUrl;
            var ret = $"{subEnvUrl}/res";
            return ret;
        }
    }

    string RemoteLocationToUrl(RemoteLocation location)
    {
        if(location == RemoteLocation.Root)
        {
            return this.RootUrl;
        }
        else if(location == RemoteLocation.SubEnv)
        {
            return this.SubEnvUrl;
        }
        else if(location == RemoteLocation.CustomRes)
        {
            return this.CustomResUrl;
        }
        else if(location == RemoteLocation.LocalizedCustomRes)
        {
            var customRes = this.CustomResUrl;
            var language = getLanguageDelegate.Invoke();
            var ret = $"{customRes}/{language}";
            return ret;
        }
        else
        {
            throw new Exception("[Remote] not support location: " + location);
        }
    }

    public string ToStoragePath(RemoteLocation root, string path)
    {
        var baseUrl = RemoteLocationToUrl(root);
        var url = $"{baseUrl}/{path}";
        var storagePath = WebResDownloader.ToStoragePath(url);
        return storagePath;
    }

    public async Task<T> LoadAsync<T>(RemoteLocation root, string path, CacheType cacheType = CacheType.File, bool allowFail = false) where T : class
    {
        var baseUrl = RemoteLocationToUrl(root);
        var url = $"{baseUrl}/{path}";
        var ret = await WebResDownloader.LoadAsync<T>(url, cacheType, allowFail);
        return ret;
    }

    async Task<RemoteGateFile> RequestGateFileAsync()
    {
        var rawString = await this.LoadAsync<string>(RemoteLocation.Root, "gate2.txt", CacheType.None);
        Debug.Log("[GateManager]:" + rawString);
        var gateFile = new RemoteGateFile(rawString);
        return gateFile;
    }
}

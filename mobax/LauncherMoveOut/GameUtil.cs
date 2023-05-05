using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;

public static class GameUtil 
{
    //public static string GetFullVersion()
    //{
    //    var version = GameManifestManager.Get("version", "0.0.0");
    //    var build = GameManifestManager.Get("build", "0");
    //    var remoteConf = EnvRemoteConfManager.Stuff.Conf;
    //    string languageName = Localization.Language;
    //    var staticDataVersion = remoteConf["staticDataVersion"];
    //    string dataName = $"staticDataVersion_{languageName}";
    //    if (remoteConf.HasKey(dataName))
    //    {
    //        staticDataVersion = remoteConf[dataName];
    //    }
    //    return $"v{version}.b{build}.d{staticDataVersion}";
    //}

    public static string Version
    {
        get
        {
            var staticDataVersion = StaticData.MetaTable["version"];
            var staticDataCodeVersion = StaticData.originDataVersion;
            var appVersion = Application.version;
            var launcherBuildVersion = LauncherBuildInfo.Get("build", "0");
            var hotBuildVersion = HotBuildInfo.Get("build", "0");
            var msg = $"Ver {appVersion}.l{launcherBuildVersion}.h{hotBuildVersion}.sc{staticDataCodeVersion}.sd{staticDataVersion}";
            return msg;
        }
    }
    
    public static async Task<LoadingFloating> ShowLoadingFloatingAsync()
    {
        await UIEngine.Stuff.PreLoadFloatingAsync("LoadingFloating");
        var floating = await UIEngine.Stuff.ShowFloatingAsync("LoadingFloating",null, UILayer.TransitionLayer) as LoadingFloating;
        return floating;
    }
    
 
}

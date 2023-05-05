using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherRemoteService : Service
{
    public override void OnCreate()
    {
        Remote.getBaseUrlDelegate = GetBaseUrl;
        WebResDownloader.forceOriginForNonCache = true;
    }


    string GetBaseUrl()
    {
        var baseUrl = EnvManager.GetConfigOfFinalEnv("remoteRes");
        return baseUrl;
    }
}

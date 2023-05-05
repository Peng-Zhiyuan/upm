using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class ConfigApi 
{
    public static async Task<NetPage<ConfigInfo>> FetchAsync(JsonData jd)
    {
        var branchedUrl = "config";
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<ConfigInfo>>(ServerType.Platform, branchedUrl, jd, null, true, DisplayType.None, true);
        return ret;
    }

}

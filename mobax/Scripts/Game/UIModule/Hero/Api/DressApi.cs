using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class DressApi
{
    public static async Task<string> RequestSwitchAvatarAsync(int id, int aid)
    {

        var arg = new JsonData();
        arg["id"] = string.Join(",", id);
        arg["aid"] = string.Join(",", aid);
        var ret = await NetworkManager.Stuff.CallAsync<string>(ServerType.Game, "hero/avatar", arg, null, false);
        return ret;
    }

    public static async Task<string> RequestSwitchAvatarAsync(int id, string aid)
    {

        var arg = new JsonData();
        arg["id"] = string.Join(",", id);
        arg["aid"] = string.Join(",", aid);
        var ret = await NetworkManager.Stuff.CallAsync<string>(ServerType.Game, "hero/avatar", arg, null, false);
        return ret;
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class PlatformApi 
{
    public static async Task<Dictionary<int, GameServerInfo>> RequestGameServerInfoAsync(string access)
    {
        var arg = new JsonData();
        arg["access"] = access;
        var netPage = await NetworkManager.Stuff.CallAsync<NetPage<GameServerInfo>>(ServerType.Platform, $"server", arg);

        var ret = new Dictionary<int, GameServerInfo>();
        foreach(var row in netPage.rows)
        {
            var id = row.sid;
            ret[id] = row;
        }

        return ret;
    }
}

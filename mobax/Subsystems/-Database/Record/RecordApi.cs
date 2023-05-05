using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class RecordApi 
{
    public static async Task<NetPage<RecordInfo>> FetchAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<RecordInfo>>(ServerType.Game, "getter/record", pageArg);
        return ret;
    }
}

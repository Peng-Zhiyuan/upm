using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using CustomLitJson;


public static class NetworkManagerExtension
{
    /// <summary>
    /// 返回 ret 字段下的内容
    /// </summary>
    //public static async Task<JsonData> RequestAssertSuccessAsync(this NetworkManager networkManager, ServerType serverType, string api, Dictionary<string, string> arg, bool isBlock = true)
    //{
    //    var ret = await networkManager.RequestAssertSuccessLagacyAsync<JsonData>(serverType, api, arg, isBlock);
    //    return ret;
    //}

    //public static async Task<NetBaseMsg> RequestAllowLogicFailAsync(this NetworkManager networkManager, ServerType serverType, string api, Dictionary<string, string> arg, bool isBlock = true)
    //{
    //    return await networkManager.RequestAllowLogicFailAsync<JsonData>(serverType, api, arg, isBlock);
    //}
}
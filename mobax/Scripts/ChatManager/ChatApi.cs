using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System.Threading.Tasks;

public static class ChatApi 
{
    public static async Task<NetBaseMsg> RequestTokenAsync()
    {
        var msg = await NetworkManager.Stuff.CallAllowLogicFailAsync<string>(ServerType.Game, "chat/gettoken", null, null, false);
        return msg;
    }

}

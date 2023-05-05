using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public class ItemApi : MonoBehaviour
{
    public static async Task<NetPage<ItemInfo>> FetchAsync(JsonData pageArg)
    {
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<ItemInfo>>(ServerType.Game, "getter/item", pageArg);
        return ret;
    }


}

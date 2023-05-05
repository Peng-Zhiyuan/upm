using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;

public static class DebugApi
{
    public static async Task AddItemAsync(int k, int v)
    {
        var arg = new JsonData();
        arg["k"] = k;
        arg["v"] = v;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "debug/add", arg);
    }

    public static async Task SubItemAsync(int k, int v)
    {
        var arg = new JsonData();
        arg["k"] = k;
        arg["v"] = v;
        await NetworkManager.Stuff.CallAsync(ServerType.Game, "debug/sub", arg);
    }

    public static async Task<string> CreateOrderAsync(int id)
    {
        var arg = new JsonData();
        arg["id"] = id;
        arg["money"] = 1;
        arg["currency"] = "test";
        var orderId = await NetworkManager.Stuff.CallAsync<string>(ServerType.Game, "debug/order/create", arg);
        return orderId;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public static class Operation
{
    public static async Task PurchaseAsync(int itemId)
    {
        var channel = ChannelManager.Channel;
        await channel.PurchaseAsync(itemId);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public static class OrderUtil 
{
    public static async Task WaitForSubmitAsync(string orderId, DisplayType isAutoShowReward)
    {
        //   var tryCount = 1;
        //here:
        //   var orderInfo = await OrderApi.SubmitAsync(orderId, isAutoShowReward);
        //   if (orderInfo.status == OrderStatus.Create)
        //   {
        //       // 服务器还没有收到支付回调
        //       tryCount++;
        //       if(tryCount <= 5)
        //       {
        //           await Task.Delay(5000);
        //           goto here;
        //       }
        //       var e = new GameException(ExceptionFlag.None, "game server not recevie notify", GameErrorCode.OrderSubmitFail);
        //       throw e;
        //   }

        var b = await WaitOrderSubmitedAsync(orderId, 30000);
        if(!b)
        {
            throw new GameException(ExceptionFlag.None, "game server not recevie notify", GameErrorCode.OrderSubmitFail);
        }
    }

    public static async Task<int> TrySubmitAllOrderIfNeedAsync()
    {
        var sum = 0;
        try
        {
            var orderList = Database.Stuff.orderDatabase.GetAll();
            foreach (var info in orderList)
            {
                if (info.status == OrderStatus.Paid)
                {
                    var orderId = info.id;
                    await TrySubmitAsync(orderId, DisplayType.Cache);
                    sum++;
                }
            }
        }
        finally
        {
            UiUtil.CleanAndShowAllCachedReward();
        }
        return sum;
    }

    public static async Task TrySubmitAsync(string orderId, DisplayType displayType)
    {
        var orderInfo = await OrderApi.SubmitAsync(orderId, displayType);
        Database.Stuff.orderDatabase.AddInfo(orderInfo);
    }

    static Task<bool> WaitOrderSubmitedAsync(string orderId, long timeout = 30000)
    {
        var tcs = new TaskCompletionSource<bool>();
        var info = Database.Stuff.orderDatabase.Get(orderId);
        if(info?.status == OrderStatus.Complete)
        {
            tcs.SetResult(true);
            return tcs.Task;
        }
        Action action = null;
        action = () =>
        {
            var info = Database.Stuff.orderDatabase.Get(orderId);
            if (info?.status != OrderStatus.Complete)
            {
                return;
            }
            OrderDatabase.StatusUpdated -= action;
            tcs.TrySetResult(true);
        };
        // tiemout 
        Action cancel = null;
        cancel = async () =>
        {
            await Task.Delay((int)timeout);
            OrderDatabase.StatusUpdated -= action;
            tcs.TrySetResult(false);
        };
        cancel.Invoke();

        OrderDatabase.StatusUpdated += action;
        return tcs.Task;
    }
}

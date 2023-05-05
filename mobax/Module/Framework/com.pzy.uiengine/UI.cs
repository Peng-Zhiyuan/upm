using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public struct PageQueueInfo
{
    public Type PageType;
    public object Params;
}

public static class UI
{
    /// <summary>
    /// 显示页面队列
    /// </summary>
    public static async Task PageQueueAsync(params PageQueueInfo[] pageList)
    {
        foreach (var page in pageList)
        {
            var name = page.PageType.Name;
            await UIEngine.Stuff.ForwardOrBackToAsync(name, page.Params);
            await UIEngine.Stuff.WaitPageBackAsync(name);
        }
    }
}
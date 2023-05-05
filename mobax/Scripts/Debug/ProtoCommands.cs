using System;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;

[ConsoleCommands]
public class ProtoCommands
{
    public static string AddItem_Help = "增加道具命令";
    public static List<string> AddItem_Alias = new List<string> {"增加道具", "add", "additem"};

    public static async void AddItem(string k, string v)
    {
        if (!int.TryParse(k, out _)
            || !int.TryParse(v, out _))
        {
            MainConsoleWind.ShowStringToCmd("Bad Parameters", Color.red);
            return;
        }

        var arg = new JsonData() {["k"] = k, ["v"] = v};
        var (_, rewards) = await NetworkManager.Stuff.CallAndGetCacheAsync<JsonData>(ServerType.Game, "debug/add", arg);
        if (rewards != null)
        {
            var transactionList = UiUtil.JsonDataListToDatabaseTransactionList(rewards);
            var addedItemInfoList = UiUtil.DatabaseTransactionListToDisplayableItemInfoList(transactionList);
            foreach (var itemInfo in addedItemInfoList)
            {
                MainConsoleWind.ShowStringToCmd($"增加道具[{itemInfo.id}]数量: {itemInfo.val}", Color.green);
            }

            UiUtil.ShowReward(addedItemInfoList);
        }
    }

    public static string AddItems_Help = "增加大量道具命令";
    public static List<string> AddItems_Alias = new List<string> {"additems"};

    public static async void AddItems(params string[] p)
    {
        var allCache = new List<JsonData>();
        foreach (var s in p)
        {
            var arr = s.Split(',');
            var arg = new JsonData() {["k"] = arr[0].Trim(), ["v"] = arr[1].Trim()};
            var (_, rewards) =
                await NetworkManager.Stuff.CallAndGetCacheAsync<JsonData>(ServerType.Game, "debug/add", arg);
            allCache.AddRange(rewards);
        }

        var transactionList = UiUtil.JsonDataListToDatabaseTransactionList(allCache);
        var addedItemInfoList = UiUtil.DatabaseTransactionListToDisplayableItemInfoList(transactionList);
        foreach (var itemInfo in addedItemInfoList)
        {
            MainConsoleWind.ShowStringToCmd($"增加道具[{itemInfo.id}]数量: {itemInfo.val}", Color.green);
        }

        UiUtil.ShowReward(addedItemInfoList);
    }

    public static string Send_Help = "直接发送协议，格式类似： send debug/xxx param1:val1 param2:val2";
    public static List<string> Send_Alias = new List<string> {"协议", "send"};

    public static async void Send(string cmd, params string[] p)
    {
        var arg = new JsonData();
        for (int i = 0; i < p.Length; ++i)
        {
            var str = p[i];
            var index = str.IndexOf(":", StringComparison.Ordinal);
            if (index <= 0)
            {
                MainConsoleWind.ShowStringToCmd("Format of parameter should be like key:value", Color.red);
                MainConsoleWind.ShowHelp("Send", true);
                return;
            }

            var k = str.Substring(0, index);
            var v = str.Substring(index + 1);
            arg[k] = v;
        }

        await NetworkManager.Stuff.CallAsync(ServerType.Game, cmd, arg);
        MainConsoleWind.ShowStringToCmd($"协议[cmd={cmd}]成功发送，请查看console", Color.green);
    }
}
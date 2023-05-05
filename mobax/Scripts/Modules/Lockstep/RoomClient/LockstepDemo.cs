using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using System.Net;
using CustomLitJson;
using ProtoBuf;

public static class LockstepDemo
{
    public static async Task CreateAndEnterRoomAsync(string uid, string playerUids, Action<S2CBattleFrame, bool> onExecuteFrame)
    {
        // 登录账号
        //var dic = new Dictionary<string, string>();
        //dic["guid"] = uid;
        //dic["access"] = uid;
        //await HttpUtil.RequestTextAsync("http://10.26.17.138/login", HttpMethod.Post, dic);

        // 创建帧同步房间
        //var dic2 = new Dictionary<string, string>();
        //dic2["uids"] = playerUids;

        //var data = await NetworkManager.Stuff.RequestAssertSuccessAsync(ServerType.Game, "battle/create", dic2);

        //var back = await HttpUtil.RequestTextAsync("http://10.26.17.138/battle/create", HttpMethod.Post, dic2);
        //var jo = JsonMapper.Instance.ToObject(back);
        var response = await BattleApi.CreateRoomAsync(uid);
        var roomId = response.Id;
        var roomAddress = response.Address;
        var parts = roomAddress.Split(':');
        var ip = parts[0];
        var port = int.Parse(parts[1]);

        // 连接帧同步房间 socket
        var address = IPAddress.Parse(ip);
        var endpoint = new IPEndPoint(address, port);
        await LockstepRoomClient.Stuff.JoinAsync(endpoint, uid, roomId, onExecuteFrame, 1);
    }

    public static void SendTestInput()
    {
        var protoMsg = new C2SBattleEvents();
        var battleEvent1 = new battleEvent();
        battleEvent1.Info = "test input";
        protoMsg.Lists.Add(battleEvent1);
        LockstepRoomClient.Stuff.SendInput(protoMsg);
    }

    public static async void SendTestInputForeverInBackground()
    {
        var i = 0;
        while (true)
        {
            var protoMsg = new C2SBattleEvents();
            var battleEvent1 = new battleEvent();
            battleEvent1.Info = $"test input {i}";
            protoMsg.Lists.Add(battleEvent1);
            LockstepRoomClient.Stuff.SendInput(protoMsg);
            await Task.Delay(50);
            i++;
            if (i >= 2000)
            {
                break;
            }
        }
    }
}
/* Created:Loki Date:2022-09-20*/

using System;
using System.Threading.Tasks;
using ProtoBuf;
using UnityEngine;

public class CoBattleRoomClient : StuffObject<CoBattleRoomClient>
{
    public async Task JoinCoBattleRoomNetWorkAsync()
    {
        Debug.LogWarning("Connect CoBattle Room");
        //await NetProtoRpcLayer.Stuff.SocketConnectAsync(OnNetMessageReceived);
        Debug.LogWarning("Connect CoBattle Room Finish");
    }

    public void QuitCoBattleRoomNetWorkAsync()
    {
        NetProtoRpcLayer.Stuff.Disconnect();
    }

    public void OnNetMessageRequest(NetC2sType sendType, IExtensible protoMsg)
    {
        NetProtoRpcLayer.Stuff.Send(sendType, protoMsg);
    }

    /// <summary>
    /// 当从消息传输层获得消息
    /// </summary>
    /// <param name="package"></param>
    void OnNetMessageReceived(BackMessage package)
    {
        var getCode = package.code;
        if (getCode == NetS2cType.S2cBattleFrame) { }
        else if (getCode == NetS2cType.S2cBattleStart) { }
        else if (getCode == NetS2cType.S2cBattleReframe) { }
        else if (getCode == NetS2cType.S2cBattleJoin) { }
        else if (getCode == NetS2cType.S2cBattleChat) { }
    }
}
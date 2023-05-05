using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf;
using UnityEngine;

public static class NetProtoMessageUtil
{
    public static IExtensible DataToProtoMessage(int code, byte[] data)
    {
        var protoMessageType = (NetS2cType)code;
        var steam = new MemoryStream(data);
        if (protoMessageType == NetS2cType.S2cConfirm)
        {
            var protoMsg = Serializer.Deserialize<S2CConfirm>(steam);
            return protoMsg;
        }
        else if (protoMessageType == NetS2cType.S2cBattleJoin)
        {
            var protoMsg = Serializer.Deserialize<S2CBattleJoin>(steam);
            return protoMsg;
        }
        else if (protoMessageType == NetS2cType.S2cBattleStart)
        {
            return null;
        }
        else if (protoMessageType == NetS2cType.S2cBattleFrame)
        {
            var protoMsg = Serializer.Deserialize<S2CBattleFrame>(steam);
            return protoMsg;
        }
        else if (protoMessageType == NetS2cType.S2cBattleDisconnect)
        {
            var protoMsg = Serializer.Deserialize<S2CBattleDisconnect>(steam);
            return protoMsg;
        }
        else if (protoMessageType == NetS2cType.S2cBattlePlayers)
        {
            var protoMsg = Serializer.Deserialize<S2CBattlePlayers>(steam);
            return protoMsg;
        }
        else if (protoMessageType == NetS2cType.S2cBattleReframe)
        {
            var protoMsg = Serializer.Deserialize<S2CBattleReFrame>(steam);
            return protoMsg;
        }
        else if (protoMessageType == NetS2cType.S2cBattleChat)
        {
            var protoMsg = Serializer.Deserialize<S2CBattleChat>(steam);
            return protoMsg;
        }
        Debug.Log($"[NetMssageUtil] unsupport code: {code}  type:{protoMessageType}");
        return null;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CircularBuffer;
using ProtoBuf;
using UnityEngine;

public class NetProtoMessageManager : StuffObject<NetProtoMessageManager>
{
    Action<BackMessage> onNetMessageReceived;

    public static Dictionary<int, bool> ignoredLogCodeToTrueDic = new Dictionary<int, bool>();

    public async Task ConnectAsync(IPEndPoint endpoint, Action<BackMessage> onNetMessageReceived)
    {
        this.onNetMessageReceived = onNetMessageReceived;
        //await SocketManager.Stuff.ConnectAsync(endpoint, OnPackageReceived);
    }

    public void Disconnect()
    {
        SocketManager.Stuff.Disconnect();
    }

    void OnPackageReceived(SocketMessage netMessage)
    {
        var head = netMessage.head;
        var code = head.code;
        string path = netMessage.FullPath;
        Enum.TryParse<NetS2cType>(path, false, out var protoMsgType);
        var body = netMessage.Body;
        var protoMsg = NetProtoMessageUtil.DataToProtoMessage((int)protoMsgType, body);
        if (protoMsg == null)
        {
            return;
        }
        var netMsg = new BackMessage();
        netMsg.code = protoMsgType;
        netMsg.protoMessage = protoMsg;
        if (!IsIgnoreLogCode((int)protoMsgType))
        {
            Debug.Log($"[NetMessageManager] Received {protoMsgType} {netMsg.code}");
        }
        onNetMessageReceived.Invoke(netMsg);
    }

    bool IsIgnoreLogCode(int msgCode)
    {
        var b = ignoredLogCodeToTrueDic.ContainsKey(msgCode);
        return b;
    }

    public void TrySend(SendMessage netMsg)
    {
        //var index = netMsg.index;
        var packageHead = new SocketMessageHead();
        packageHead.magic = 0x78;
        packageHead.code = 0;
        List<byte> bytesLst = new List<byte>();
        byte[] pathBytes = (netMsg.code.ToString()).ToByteArray();
        packageHead.path = (ushort)pathBytes.Length;
        bytesLst.AddRange(pathBytes);
        var protoMessage = netMsg.protoMessage;
        if (protoMessage != null)
        {
            var steam = new MemoryStream();
            Serializer.Serialize(steam, protoMessage);
            byte[] dataBytes = steam.ToArray();
            packageHead.body = (uint)dataBytes.Length;
            bytesLst.AddRange(dataBytes);
        }
        else
        {
            packageHead.body = 0;
        }
        byte[] data = bytesLst.ToArray();
        var package = new SocketMessage();
        package.head = packageHead;
        package.data = data;
        SocketManager.Stuff.TrySend(package);
        var intCode = (ushort)netMsg.code;
        if (!IsIgnoreLogCode(intCode))
        {
            Debug.Log($"[NetMessageManager] Send {intCode} {netMsg.code}");
        }
    }

    public byte[] GetBytesLst(byte[] data, int offset, int length)
    {
        byte[] temp = new byte[length];
        int index = 0;
        for (int i = offset; i < data.Length; i++)
        {
            if (index >= length)
            {
                break;
            }
            temp[index] = data[i];
            index++;
        }
        return temp;
    }
}
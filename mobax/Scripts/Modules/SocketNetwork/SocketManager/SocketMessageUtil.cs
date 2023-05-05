using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CircularBuffer;
using UnityEngine;

public static class SocketMessageUtil
{
    static StringBuilder sb = new StringBuilder();
    public static SocketMessage Create(string nonArgPath, Dictionary<string, string> arg = null, byte[] body = null)
    {
        if(string.IsNullOrEmpty(nonArgPath))
        {
            throw new Exception("[SocketMessageUtil] nonArgPath can't be empty");
        }
        var fullPath = nonArgPath;
        if(arg != null)
        {
            sb.Clear();
            sb.Append(nonArgPath);
            sb.Append('?');
            var first = true;
            foreach (var kv in arg)
            {
                if(first)
                {
                    first = false;
                }
                else
                {
                    sb.Append('&');
                }
                var key = kv.Key;
                var value = kv.Value;
                sb.Append(key);
                sb.Append('=');
                sb.Append(value);
            }
            fullPath = sb.ToString();
        }
        var fullPathData = Encoding.Default.GetBytes(fullPath);
        byte[] data;
        int bodyLength = 0;
        if(body != null)
        {
            data = BitUtil.Combine(fullPathData, body);
            bodyLength = body.Length;
        }
        else
        {
            data = fullPathData;
            bodyLength = 0;
        }
        var head = new SocketMessageHead();
        head.magic = 0x78;
        head.code = 0;
        head.path = (UInt16)fullPathData.Length;
        head.body = (UInt32)bodyLength;
        var msg = new SocketMessage();
        msg.head = head;
        msg.data = data;
        return msg;
    }

    public static byte[] Serilize(SocketMessage package, bool bigHead = true)
    {
        var steam = new MemoryStream();
        var writer = new BinaryWriter(steam);
        writer.Write(package.head.magic);
        writer.Write(BitUtil.GetBytes(package.head.code));
        writer.Write(BitUtil.GetBytes(package.head.path));
        writer.Write(BitUtil.GetBytes(package.head.body));
        writer.Write(package.data);
        var buffer = steam.ToArray();
        return buffer;
    }

    static string BufferToDes(ArraySegment<byte> seg)
    {
        var sb = new StringBuilder();
        var first = true;
        for (int i = seg.Offset; i < seg.Offset + seg.Count; i++)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                sb.Append(", ");
            }
            var b = seg.Array[i];
            sb.Append(b);
        }
        return sb.ToString();
    }



    public static SocketMessageHead Deserilize(byte[] buffer, bool isBigHead = true)
    {
        var magic = buffer[0];
        var code = BitUtil.ToInt16(buffer, 1, isBigHead);
        var path = BitUtil.ToUInt16(buffer, 3, isBigHead);
        var body = BitUtil.ToUInt32(buffer, 5, isBigHead);
        var head = new SocketMessageHead();
        head.magic = magic;
        head.code = code;
        head.path = path;
        head.body = body;
        return head;
    }
}
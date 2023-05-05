using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class EndpointManager : StuffObject<EndpointManager>
{
    public uint ReadUInt32Reversed(BinaryReader reader)
    {
        var data = reader.ReadBytes(4);
        Reverse(data);
        var value = BitConverter.ToUInt32(data, 0);
        return value;
    }


    public void Reverse(byte[] data)
    {
        for(int i = 0; i < data.Length/2; i++)
        {
            var head = i;
            var tail = data.Length - 1 - i;
            var a = data[head];
            var b = data[tail];
            data[head] = b;
            data[tail] = a;
        }
    }



    long ParseBase32NumberString(string base32number)
    {
        char[] base32 = new char[] {
          '0','1','2','3','4','5','6','7',
          '8','9','a','b','c','d','e','f',
          'g','h','i','j','k','l','m','n',
          'o','p','q','r','s','t','u','v'
        };

        long n = 0;

        foreach (char d in base32number.ToLowerInvariant())
        {
            n = n << 5;
            int idx = Array.IndexOf(base32, d);

            if (idx == -1)
                throw new Exception("Provided number contains invalid characters");

            n += idx;
        }

        return n;
    }

    public static IPEndPoint SocketEndpoint
    {
        get
        {
            // 连接消息传输层
            // 游戏服的 url
            var gameUrl = LoginManager.Stuff.session.SelectedGameServerInfo.address;

            // 取 ip
            var splash = gameUrl.LastIndexOf('/');
            if (splash < 0)
            {
                splash = 0;
            }
            else
            {
                splash += 1;
            }
            var portDot = gameUrl.IndexOf(":", splash, StringComparison.Ordinal);
            if (portDot == -1)
            {
                portDot = gameUrl.Length;
            }
            var addressString = gameUrl.Substring(splash, portDot - splash);
            IPAddress address;
            if (Regex.IsMatch(addressString, @"\d+\.\d+\.\d+\.\d+"))
            {
                address = IPAddress.Parse(addressString);
            }
            else
            {
                address = Dns.GetHostEntry(addressString).AddressList.First();
            }
            
            var ret = new IPEndPoint(address, (int)3000 );
            return ret;
        }

    }
}

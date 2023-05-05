using System;
using UnityEngine;
using System.Collections.Generic;

public static class ChannelManager
{
    private static Channel _channel;
    public static Channel Channel
    {
        get
        {
            if(_channel != null)
            {
                return _channel;
            }
            Channel ret = null;
            var clazzName = ClassName;
            Debug.Log($"[ChannelManager] Try select channel : [{clazzName}]");
            var assemblyName = typeof(ChannelManager).Assembly.FullName;
            var handler = Activator.CreateInstance(assemblyName, clazzName);
            var instance = handler.Unwrap();
            ret = instance as Channel;
            _channel = ret;
            Debug.Log($"[ChannelManager] Channel [{clazzName}] selected");
            return ret;
        }
    }

    public static string ClassName
    {
        get
        {
            var shortName = EnvManager.Channel;
            shortName = FirstCharToUpper(shortName);
            var className = shortName + "Channel";
            return className;
        }
    }

    static string FirstCharToUpper(string name)
    {
        var hou = name.Substring(1);
        var qian = name[0];
        var bigChar = Char.ToUpper(qian);
        return bigChar + hou;
    }


 

}
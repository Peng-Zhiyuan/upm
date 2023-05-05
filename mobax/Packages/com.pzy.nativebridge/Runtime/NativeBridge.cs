using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Threading.Tasks;

public static class NativeBridge
{
    public static bool IsSupported
    {
        get
        {
            var platform = Application.platform;
            if(platform == RuntimePlatform.Android)
            {
                return true;
            }
            else if(platform == RuntimePlatform.IPhonePlayer)
            {
                return true;
            }
            return false;
        }
    }

    public async static Task<string> CallAsync(string clazz, string method, string arg = null)
    {
        var platform = Application.platform;
        if(platform == RuntimePlatform.Android)
        {
            var ret = await AndroidBridge.CallAsync(clazz, method, arg);
            return ret;
        }
        else if(platform == RuntimePlatform.IPhonePlayer)
        {
            var ret = await IosBridge.CallAsync(clazz, method, arg);
            return ret;
        }
        else
        {
            throw new Exception($"[NativeBridge] unsupport platform: {platform}");
        }
    }

    public static string Call(string clazz, string method, string arg = null)
    {
        var platform = Application.platform;
        if(platform == RuntimePlatform.Android)
        {
            var ret = AndroidBridge.Call(clazz, method, arg);
            return ret;
        }
        else if(platform == RuntimePlatform.IPhonePlayer)
        {
            var ret = IosBridge.Call(clazz, method, arg);
            return ret;
        }
        else
        {
            throw new Exception($"[NativeBridge] unsupport platform: {platform}");
        }
    }

}
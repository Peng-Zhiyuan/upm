using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class WXService
{
    public async static Task<string> UserLoginAsync()
    {
        if(Application.isEditor)
        {
            throw new Exception("not wx sdk in editor");
        }
        var ret = await NativeBridge.CallAsync("WXProxy", "Login");
        return ret;

    }

    public static bool IsWXAppInstalled
    {
        get
        {
            if (Application.isEditor)
            {
                return true;
            }
            var ret = NativeBridge.Call("WXProxy", "IsWXAppInstalled");
            if(ret == "true")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public async static Task ShareTimeLineURLAsync(string args)
    {
        if (Application.isEditor)
        {
            throw new Exception("not wx sdk in editor");
        }
        await NativeBridge.CallAsync("WXProxy", "ShareTimeLineURL", args);
    }

    public async static Task ShareHaoYouURLAsync(string args)
    {
        if (Application.isEditor)
        {
            throw new Exception("not wx sdk in editor");
        }
        await NativeBridge.CallAsync("WXProxy", "ShareHaoYouURL", args);
    }

    public async static Task ShareTimeLineAsync(string base64)
    {
        if(Application.isEditor)
        {
            throw new Exception("not wx sdk in editor");
        }
        await NativeBridge.CallAsync("WXProxy", "ShareTimeLine", base64);
    }

    public async static Task ShareHaoYouAsync(string base64)
    {
        if(Application.isEditor)
        {
            throw new Exception("not wx sdk in editor");
        }
        await NativeBridge.CallAsync("WXProxy", "ShareHaoYou", base64);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Threading.Tasks;


public class AndroidBridge
{
    private static AndroidJavaClass javaGateProxy;
    private static int callCount;
    private static Dictionary<string, TaskCompletionSource<string>> callIdToTcsDic = new Dictionary<string, TaskCompletionSource<string>>();
    private static Dictionary<string, Action<string>> eventToHandlerDic = new Dictionary<string, Action<string>>();

    static AndroidBridge()
    {
        if(Application.platform != RuntimePlatform.Android)
        {
            return;
        }
        AndroidBridgeReceiver.Stuff.Touch();
        javaGateProxy = new AndroidJavaClass("com.edroity.nativebrige.Gate");
        callCount = 0;
    } 

    public static Task<string> CallAsync(string clazz, string method, string arg = null)
    {
        var tcs = new TaskCompletionSource<string>();
        callCount++;
        string callId = callCount.ToString();
        callIdToTcsDic[callId] = tcs;
        if(Application.platform != RuntimePlatform.Android)
        {
            throw new Exception($"需要安卓环境. {clazz}.{method}() arg: {arg}");
        }
        javaGateProxy.CallStatic("onCall", new object[]{ callId, clazz, method, arg});
        return tcs.Task;
    }

    public static void RegisterEventHandler(string eventName, Action<string> handler)
    {
        eventToHandlerDic[eventName] = handler;
    }

    public static void OnSetResult(string id, string result)
    {
        TaskCompletionSource<string> tcs;
        callIdToTcsDic.TryGetValue(id, out tcs);
        if (tcs != null)
        {
            callIdToTcsDic.Remove(id);
            tcs.SetResult(result);
        }
    }

    public static void OnSetException(string id, string code, string msg)
    {
        TaskCompletionSource<string> tcs;
        callIdToTcsDic.TryGetValue(id, out tcs);
        if (tcs != null)
        {
            callIdToTcsDic.Remove(id);
            var e = new CodedException(code, msg);
            tcs.SetException(e);
        }
    }

    public static void OnNotify(string eventName, string arg)
    {
        Action<string> handler;
        eventToHandlerDic.TryGetValue(eventName, out handler);
        if(handler == null)
        {
            Debug.Log($"[AndroidBridge] event handler of {handler} not found, skip.");
            return; 
        }
        handler.Invoke(arg);
    }


    public static string Call(string clazz, string method, string arg = null)
    {
        return javaGateProxy.CallStatic<string>("onSynCall", clazz, method, arg);
    }
}

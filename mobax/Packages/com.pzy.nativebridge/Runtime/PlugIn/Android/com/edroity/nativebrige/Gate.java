package com.edroity.nativebrige;

import android.util.Log;
import com.unity3d.player.UnityPlayer;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Created by zhiyuan.peng on 2017/8/27.
 */

public class Gate
{

    public static String TAG = "Gate";

    // downsteram call
    public static void onCall(String callId, String clazzName, String msg, String arg)
    {
        Log.i(TAG, "onCall [" + callId + "] clazzName." + msg + " " + arg);
        try
        {
            Class clazz = Class.forName("bridgeClass." + clazzName);
            Method method = clazz.getMethod(msg, String.class, String.class);
            method.invoke(null, callId, arg);
        }
        catch (Exception e)
        {
            Log.e(TAG, "error in onCall, class:" + clazzName + ", msg: " + msg + ", arg: " + arg + ", exception: " + e.getMessage());
            e.printStackTrace();
            String errorMsg = GetNoneNullExceptionMsg(e);
            setException(callId, "NATIVE_EXCEPTION", errorMsg);
        }
    }
    
    static String GetNoneNullExceptionMsg(Throwable e)
    {
        String msg = e.getMessage();
        while (msg == null || msg.isEmpty())
        {
            e = e.getCause();
            if(e != null)
            {
                msg = e.getMessage();
            }
            else
            {
                return "";
            }
        }
        return msg;
    }


    public static void setResult(String callId, String result)
    {
        if(result == null)
        {
            result = "";
        }
        Log.i(TAG, " -> call [" + callId + "] end withs result: " + result);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnSetId", callId);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnSetResult", result);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnInvokeResult", "");
    }

    public static void setException(String callId, String code, String errorMsg)
    {
        if(errorMsg == null)
        {
            errorMsg = "";
        }
        Log.i(TAG, " -> call [" + callId + "] end withs exception code: " + code + ", msg: " + errorMsg);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnSetId", callId);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnSetExceptionCode", code);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnSetExceptionMsg", errorMsg);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnCallReturnInvokeException", "");
    }

    // upstream notify
    public static void notifyEvent(String eventName, String arg)
    {
        Log.i(TAG, " -> notifyEvent: " + eventName + ", arg: " + arg);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnNotifySetEvent", eventName);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnNotifySetArg", arg);
        UnityPlayer.UnitySendMessage("AndroidBridgeReceiver", "OnNotifySend", "");
    }

    // downstream syn call
    public static String onSynCall(String clazzName, String methodName, String arg) throws ClassNotFoundException, NoSuchMethodException, InvocationTargetException, IllegalAccessException {
        Log.i(TAG, "onSynCall " + clazzName + "." + methodName + " " + arg);
        String ret = "";
        Class clazz = Class.forName("bridgeClass." + clazzName);
        Method method = clazz.getMethod(methodName, String.class);
        Object retobj = method.invoke(null, arg);
        ret = (String)retobj;
        return ret;
    }

}

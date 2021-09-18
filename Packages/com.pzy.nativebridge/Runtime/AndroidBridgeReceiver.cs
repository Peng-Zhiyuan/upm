using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AndroidBridgeReceiver : StuffObject<AndroidBridgeReceiver> 
{

    string id;
    string result;
    string exceptionCode;
    string exceptionMsg;

    void OnCallReturnSetId(string id)
    {
        this.id = id;
    }

    void OnCallReturnSetResult(string result)
    {
        this.result = result;
    }

    void OnCallReturnSetExceptionCode(string code)
    {
        this.exceptionCode = code;
    }

    void OnCallReturnSetExceptionMsg(string exceptionMsg)
    {
        this.exceptionMsg = exceptionMsg;
    }

    void OnCallReturnInvokeResult()
    {
        AndroidBridge.OnSetResult(id, result);
    }

    void OnCallReturnInvokeException()
    {
        AndroidBridge.OnSetException(id, exceptionCode, exceptionMsg);
    }

    string eventName;
    string arg;
    void OnNotifySetEvent(string eventName)
    {
        this.eventName = eventName;
    }

    void OnNotifySetArg(string arg)
    {
        this.arg = arg;
    }

    void OnNotifySend()
    {
        AndroidBridge.OnNotify(eventName, arg);
    }
}

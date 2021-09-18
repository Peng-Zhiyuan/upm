//
//  GateC.cpp
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/9/21.
//
//

#include "Gate.h"
#include "Util.h"

extern "C"
{
    void gateOnCallAsync(const char* callId, const char* clazzName, const char* msg, const char* arg)
    {
        NSString* ocCallId = CStringToOCString(callId);
        NSString* ocClassName = CStringToOCString(clazzName);
        NSString* ocMsg = CStringToOCString(msg);
        NSString* ocArg = CStringToOCString(arg);
        //[Gate onCall:ocCallId clazzName:ocClassName msg:ocMsg arg:ocArg];
        id clazz = NSClassFromString(ocClassName);
        NSString* selectorStirng = nil;
        if(clazz != nil)
        {
            // oc class
            selectorStirng = [NSString stringWithFormat:@"%@:arg:", ocMsg];
           
        }
        else
        {
            clazz = swiftClassFromString(ocClassName);
            selectorStirng = [NSString stringWithFormat:@"%@WithCallId:arg:", ocMsg];
        }
        SEL selector = NSSelectorFromString(selectorStirng);
        if([clazz respondsToSelector:selector])
        {
            [clazz performSelector:selector withObject:ocCallId withObject:ocArg];
        }
        else
        {
            //gateCallReturn(ocCallId, @"");
            gateSetException(ocCallId, @"NativeMethodNotFound", @"找不到原生方法");
        }
    }
    
    const char* gateOnCall(const char* clazzName, const char* method, const char* arg)
    {
        NSString* ocClazzName = CStringToOCString(clazzName);
        NSString* ocMethod = CStringToOCString(method);
        NSString* ocArg = CStringToOCString(arg);
        id clazz = NSClassFromString(ocClazzName);

        NSString* selectorStirng = nil;
        if(clazz != nil)
        {
            // oc class
            selectorStirng = [NSString stringWithFormat:@"%@:", ocMethod];
        }
        else
        {
            clazz = swiftClassFromString(ocClazzName);
            selectorStirng = [NSString stringWithFormat:@"%@WithArg:", ocMethod];
        }
        
        SEL selector = NSSelectorFromString(selectorStirng);
        const char* cResult = "";
        if([clazz respondsToSelector:selector])
        {
            id ret = [clazz performSelector:selector withObject:ocArg];
            NSString* ocResult = (NSString*)ret;
            cResult = copyNSStringToCString(ocResult);
        }
        //NSLog(@"[Gate] <- SynCall returns: %s", cResult);
        return strdup(cResult);
    }
}

Class swiftClassFromString(NSString* className)
{
    NSString *appName = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"CFBundleName"];
    NSString *classStringName = [NSString stringWithFormat:@"_TtC%lu%@%lu%@", (unsigned long)appName.length, appName, (unsigned long)className.length, className];
    return NSClassFromString(classStringName);
}


void gateSetResult(NSString* callId, NSString* result)
{
   // NSLog(@"[Gate] <- call [%@] returns %@", callId, result);
    const char* cCallId = copyNSStringToCString(callId);
    const char* cResult = copyNSStringToCString(result);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnSetId", cCallId);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnSetResult", cResult);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnInvokeResult", "");
}

void gateSetException(NSString* callId, NSString* code, NSString* msg)
{
   // NSLog(@"[Gate] <- call [%@] returns %@", callId, result);
    const char* cCallId = copyNSStringToCString(callId);
    const char* cCode = copyNSStringToCString(code);
    const char* cMsg = copyNSStringToCString(msg);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnSetId", cCallId);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnSetExceptionCode", cCode);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnSetExceptionMsg", cMsg);
    UnitySendMessage("IosBridgeReceiver", "OnCallReturnInvokeException", "");
}

void gateNotifyEvent(NSString* eventName, NSString* arg)
{
   // NSLog(@"[Gate] <- call [%@] returns %@", callId, result);
    const char* cEventName = copyNSStringToCString(eventName);
    const char* cArg = copyNSStringToCString(arg);
    UnitySendMessage("IosBridgeReceiver", "OnNotifySetEvent", cEventName);
    UnitySendMessage("IosBridgeReceiver", "OnNotifySetArg", cArg);
    UnitySendMessage("IosBridgeReceiver", "OnNotifySend", "");
}

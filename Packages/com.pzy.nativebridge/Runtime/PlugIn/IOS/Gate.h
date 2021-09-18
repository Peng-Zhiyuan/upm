//
//  GateC.h
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/9/21.
//
//

#ifndef Gate_h
#define Gate_h

#include <stdio.h>
#include "Gate2.h"

extern "C"
{
    void gateOnCallAsync(const char* callId, const char* clazzName, const char* msg, const char* arg);
    
    const char* gateOnCall(const char* clazzName, const char* method, const char* arg);
}

void gateSetResult(NSString* callId, NSString* result);

void gateSetException(NSString* callId, NSString* code, NSString* msg);

void gateNotifyEvent(NSString* eventName, NSString* arg);

Class swiftClassFromString(NSString* className);


#endif /* GateC_h */

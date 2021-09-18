//
//  GateC.cpp
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/9/21.
//
//

#include "Gate2.h"
#include "Util.h"
#include "Gate.h"

@implementation Gate : NSObject

+ (void)SetResultWithCallId:(NSString*) callId result:(NSString*) result
{
    gateSetResult(callId, result);
}

+ (void)SetExceptionWithCallId:(NSString*) callId code:(NSString*) code msg:(NSString*) msg
{
    gateSetException(callId, code, msg);
}


@end

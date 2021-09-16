//
//  GateC.h
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/9/21.
//
//

#ifndef Gate2_h
#define Gate2_h

#include <stdio.h>
#import <Foundation/Foundation.h>


@interface Gate : NSObject

+ (void)SetResultWithCallId:(NSString*) callId result:(NSString*) result;

+ (void)SetExceptionWithCallId:(NSString*) callId code:(NSString*) code msg:(NSString*) msg;

@end

#endif /* Gate2C_h */

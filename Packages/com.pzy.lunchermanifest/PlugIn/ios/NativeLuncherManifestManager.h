//
//  IOSNativeManager_h
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/5/19.
//
//

#ifndef IOSNativeManager_h
#define IOSNativeManager_h

@interface NativeLuncherManifestManager : NSObject
+ (NSString*) GetRawString;
+ (NSString*) tryGet: (NSString* )key default:(NSString* )defaultValue;


@end





#endif /* IOSNativeManager_h */

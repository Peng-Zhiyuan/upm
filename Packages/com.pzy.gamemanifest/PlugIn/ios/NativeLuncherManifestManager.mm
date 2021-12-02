//
//  IOSGameManifestManager.mm
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/5/18.
//
//

#import <Foundation/Foundation.h>
#import <objc/message.h>
#import "Util.h"
#import "NativeLuncherManifestManager.h"


@implementation NativeLuncherManifestManager : NSObject

NSString* manifestString;
NSDictionary* manifest;
bool loaded;

// Bridge method
// Sync call
+ (NSString*)GetRawString:(NSString*)arg
{
    [NativeLuncherManifestManager tryLoad];
    if(manifestString == nil)
    {
        return @"";
    }
    else
    {
        return manifestString;
    }
    
}

+ (void) tryLoad
{
    if(!loaded)
    {
        loaded = true;
        [NativeLuncherManifestManager loadFromFile];
    }
    
}

// 从 conf.json 文件加载数据到变量中
+ (void) loadFromFile
{
    NSError *error;
    NSString *confPath=[NSString stringWithFormat:@"%@%@%@",[[NSBundle mainBundle]resourcePath],@"/",@"luncher-manifest.json"];
    NSString *jsonstring = [NSString stringWithContentsOfFile:confPath  encoding:NSUTF8StringEncoding error:nil];
    NSLog(@"[NativeLuncherManifestManager] manifest: %@",jsonstring);
    manifestString = jsonstring;
    NSData *data=[jsonstring dataUsingEncoding:NSUTF8StringEncoding];
    manifest = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:&error];
}

+ (NSString*) tryGet: (NSString* )key default:(NSString* )defaultValue
{
    [NativeLuncherManifestManager tryLoad];
    id value = [manifest objectForKey:key];
    if(value == nil) return defaultValue;
    else return value;
}


@end

//
//  Util.m
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/5/15.
//
//

#import <Foundation/Foundation.h>



NSString* dictionaryToJsonString(NSDictionary* dict)
{
    
    NSError *error;
    
    NSData *jsonData = [NSJSONSerialization dataWithJSONObject:dict options:NSJSONWritingPrettyPrinted error:&error];
    
    NSString *jsonString;
    
    if (!jsonData) {
        
        NSLog(@"%@",error);
        
    }else{
        
        jsonString = [[NSString alloc]initWithData:jsonData encoding:NSUTF8StringEncoding];
        
    }
    
    NSMutableString *mutStr = [NSMutableString stringWithString:jsonString];
    
    NSRange range = {0,jsonString.length};
    
    //去掉字符串中的空格
    
    [mutStr replaceOccurrencesOfString:@" " withString:@"" options:NSLiteralSearch range:range];
    
    NSRange range2 = {0,mutStr.length};
    
    //去掉字符串中的换行符
    
    [mutStr replaceOccurrencesOfString:@"\n" withString:@"" options:NSLiteralSearch range:range2];
    
    return mutStr;
    
}

NSDictionary* jsonStringToDictionary(NSString* jsonString)
{
    if (jsonString == nil) {
        return nil;
    }
    
    NSData *jsonData = [jsonString dataUsingEncoding:NSUTF8StringEncoding];
    NSError *err;
    NSDictionary *dic = [NSJSONSerialization JSONObjectWithData:jsonData
                                                        options:NSJSONReadingMutableContainers
                                                          error:&err];
    if(err)
    {
        NSLog(@"json解析失败：%@",err);
        return nil;
    }
    return dic;
}

NSString* boolToString(BOOL b)
{
    return b == YES ? @"true" : @"false";
}

NSDictionary* errorToDictionary(NSError* error)
{
    id dic = [[NSMutableDictionary alloc] init];
    id code = [NSString stringWithFormat:@"%d", (int)error.code];
    [dic setValue:code  forKey:@"code"];
    return dic;
}

NSString* CStringToOCString(const char* cString)
{
    if(cString == NULL)
    {
        cString = "";
    }
    return [NSString stringWithUTF8String:cString];
}

NSString* getBundleID()
{
    return [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleIdentifier"];
}

char* copyNSStringToCString(NSString* nsstring)
{
    char* p = (char*)malloc(strlen(nsstring.UTF8String) + 1);
    strcpy(p, nsstring.UTF8String);
    return p;
}

NSString* copyCStringToNSString(const char* cString)
{
    char* p = (char*)malloc(strlen(cString) + 1);
    strcpy(p, cString);
    p[strlen(cString)] = '\0';
    id str = [NSString stringWithUTF8String:p];
    return str;
}

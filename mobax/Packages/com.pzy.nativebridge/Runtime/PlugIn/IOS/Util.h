//
//  Util.h
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2017/5/15.
//
//

#ifndef Util_h
#define Util_h


NSString* dictionaryToJsonString(NSDictionary* dict);
NSDictionary* jsonStringToDictionary(NSString* jsonString);
NSString* boolToString(BOOL b);
NSDictionary* errorToDictionary(NSError* error);
NSString* CStringToOCString(const char* cString);
NSString* getBundleID();
char* copyNSStringToCString(NSString* );
NSString* copyCStringToNSString(const char* cstring);

#endif /* Util_h */

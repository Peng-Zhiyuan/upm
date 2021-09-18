//
//  Hello.swift
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2019/4/10.
//

import Foundation

@objc class Hello: NSObject
{
    
    @objc static func NativeMethod(callId: String, arg: String)
    {
        print(arg)
        //Gate.callReturn(withCallId: callId, result: "result");
        Gate.setResultWithCallId(callId, result: "result");
    }
}

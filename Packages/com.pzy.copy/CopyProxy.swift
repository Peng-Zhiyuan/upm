//
//  Hello.swift
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2019/4/10.
//

import Foundation

@objc class CopyProxy: NSObject
{
    
    @objc static func Copy(arg: String) -> String
    {
        let pasteboard = UIPasteboard.general
        pasteboard.string = arg
        return ""
    }
}

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

    @objc static func IsDistribution(arg: String) -> String
    {
        do
        {
            let bundlePath = Bundle.main.path(forResource: "embedded", ofType: "mobileprovision")
            let bundleData = try Data(contentsOf: URL(fileURLWithPath: bundlePath!))
            let bundleString = String(data: bundleData, encoding: .ascii)!
            if bundleString.contains("<key>ProvisionedDevices</key>") {
                return "false";
            } else {
                return "true";
            }
        }
        catch
        {
            return "true";
        }

    }

}

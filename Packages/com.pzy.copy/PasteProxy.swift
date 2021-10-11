//2021/5/26 NEKOKAMI
import Foundation

@objc class PasteProxy : NSObject
{
    @objc static func Paste(arg: String) -> String
    {
        let pasteboard = UIPasteboard.general
        var ret = pasteboard.string
        return ret!
    }
}
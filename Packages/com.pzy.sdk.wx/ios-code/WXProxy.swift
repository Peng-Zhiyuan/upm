//
//  WXService.swift
//  Unity-iPhone
//
//  Created by zhiyuan.peng on 2019/5/22.
//

import Foundation


@objc class WXProxy: NSObject, WXApiDelegate
{
    public static var _instance: WXProxy? = nil
    @objc public static func getInstance() -> WXProxy?
    {
        if(_instance == nil)
        {
            _instance = WXProxy()
        }
        return _instance
    }
    
    // 微信用户登录，gate返回用户code
    static var _callId: String = ""
    @objc static func Login(callId: String, arg: String)
    {
        
        _callId = callId
        if(!WXApi.isWXAppInstalled())
        {
            //Gate.callReturn(withCallId: WXProxy._callId, result: "");
            Gate.setExceptionWithCallId(WXProxy._callId, code: "WX_NOT_INSTALLED", msg: "没有安装微信");
            return;
        }
        //构造SendAuthReq结构体
        let req = SendAuthReq()
        req.scope = "snsapi_userinfo"
        req.state = "123"
        WXApi.send(req)
        //第三方向微信终端发送一个SendAuthReq消息结构
        // 通过 onResp 异步回调
    }
    
    @objc static func IsWXAppInstalled(arg: String) -> String
    {
        if(!WXApi.isWXAppInstalled())
        {
            return "false"
        }
        else
        {
            return "true"
        }
    }
    
    @objc static func ShareTimeLineURL(callId: String, arg: String)
    {
        _callId = callId
        if(!WXApi.isWXAppInstalled())
        {
            //Gate.callReturn(withCallId: WXProxy._callId, result: "");
            Gate.setExceptionWithCallId(WXProxy._callId, code: "WX_NOT_INSTALLED", msg: "没有安装微信");
            return;
        }
       
        //构造SendAuthReq结构体
        //let arg = "下一个表情王~但求一赞~https://mp.weixin.qq.com/s/9qSx5mLbnc-dAQOSB30Caw?uid=2";
        let args: [Substring] = arg.split(separator:"~");
        let arrayStrings: [String] = args.compactMap { "\($0)" }
        let webpageObject = WXWebpageObject.init();
        webpageObject.webpageUrl = arrayStrings[2];
        let message = WXMediaMessage.init();
        message.title = arrayStrings[0];
        message.description = arrayStrings[1];
        message.setThumbImage(UIImage.init(imageLiteralResourceName: "share.jpg"));
        message.mediaObject = webpageObject;
        let req = SendMessageToWXReq()
        req.bText = false
        req.message = message
        req.scene = Int32(WXSceneTimeline.rawValue)
        WXApi.send(req)
    }
    
    @objc static func ShareTimeLine(callId: String, arg: String)
    {

//        WXMediaMessage *message = [WXMediaMessage message];
//        NSString *filePath = [[NSBundle mainBundle] pathForResource:@"res5"
//            ofType:@"jpg"];
//        message.thumbData = [NSData dataWithContentsOfFile:filePath];
        _callId = callId
        if(!WXApi.isWXAppInstalled())
        {
            //Gate.callReturn(withCallId: WXProxy._callId, result: "");
            Gate.setExceptionWithCallId(WXProxy._callId, code: "WX_NOT_INSTALLED", msg: "没有安装微信");
            return;
        }
        
        let image = Base64ToImage(base64: arg)
        let imageData = image.jpegData(compressionQuality: 0.7)
        let imageObject = WXImageObject.init()
        imageObject.imageData = imageData!
        let message = WXMediaMessage.init()
        message.mediaObject = imageObject
        let req = SendMessageToWXReq()
        req.bText = false
        req.message = message
        req.scene = Int32(WXSceneTimeline.rawValue)
        WXApi.send(req)

        //第三方向微信终端发送一个SendAuthReq消息结构
        // 通过 onResp 异步回调
    }
    
    
    @objc static func ShareHaoYouURL(callId: String, arg: String)
    {

        _callId = callId;
        if(!WXApi.isWXAppInstalled())
        {
            //Gate.callReturn(withCallId: WXProxy._callId, result: "");
            Gate.setExceptionWithCallId(WXProxy._callId, code: "WX_NOT_INSTALLED", msg: "没有安装微信");
            return;
        }
    
        let args: [Substring] = arg.split(separator:"~");
        let arrayStrings: [String] = args.compactMap { "\($0)" }
        let webpageObject = WXWebpageObject.init();
        webpageObject.webpageUrl = arrayStrings[2];
        let message = WXMediaMessage.init();
        message.title = arrayStrings[0];
        message.description =  arrayStrings[1];
        message.setThumbImage(UIImage.init(imageLiteralResourceName: "share.jpg"));
        message.mediaObject = webpageObject;
        let req = SendMessageToWXReq()
        req.bText = false
        req.message = message
        req.scene = Int32(WXSceneSession.rawValue)
        WXApi.send(req)
        
    }
    
    @objc static func ShareHaoYou(callId: String, arg: String)
    {
        
        //        WXMediaMessage *message = [WXMediaMessage message];
        //        NSString *filePath = [[NSBundle mainBundle] pathForResource:@"res5"
        //            ofType:@"jpg"];
        //        message.thumbData = [NSData dataWithContentsOfFile:filePath];
        _callId = callId
        if(!WXApi.isWXAppInstalled())
        {
            //Gate.callReturn(withCallId: WXProxy._callId, result: "");
            Gate.setExceptionWithCallId(WXProxy._callId, code: "WX_NOT_INSTALLED", msg: "没有安装微信");
            return;
        }
        
   
        let image = Base64ToImage(base64: arg)
        let imageData = image.jpegData(compressionQuality: 0.7)
        let imageObject = WXImageObject.init()
        imageObject.imageData = imageData!
        let message = WXMediaMessage.init()
        message.mediaObject = imageObject
        let req = SendMessageToWXReq()
        req.bText = false
        req.message = message
        req.scene = Int32(WXSceneSession.rawValue)
        WXApi.send(req)
        
    }
    
    @objc func onReq(_ req: BaseReq)
    {
        print("onReq")
    }

    @objc func onResp(_ resp: BaseResp)
    {
        print("onResp")
        if(resp is SendAuthResp)
        {
            let r = resp as! SendAuthResp
            let errorCode = r.errCode;
            print(errorCode);
            if(errorCode == 0)
            {
                //Gate.callReturn(withCallId: WXProxy._callId, result: r.code)
                Gate.setResultWithCallId(WXProxy._callId, result: r.code);
            }
            else
            {
                //Gate.callReturn(withCallId: WXProxy._callId, result: "")
                Gate.setExceptionWithCallId(WXProxy._callId, code: r.code, msg: r.errStr);
            }
        }
        else
        {
            //Gate.callReturn(withCallId: WXProxy._callId, result: "")
            Gate.setResultWithCallId(WXProxy._callId, result: "");
        }
    }

    static func Base64ToImage(base64: String) -> UIImage
    {
        let data = Data.init(base64Encoded: base64)
        let image = UIImage.init(data: data!)
        return image!
        
    }
    
//    @objc static func NativeMethod(callId: String, arg: String)
//    {
//        print(arg)
//        gateCallReturn(callId, "123")
//    }
}

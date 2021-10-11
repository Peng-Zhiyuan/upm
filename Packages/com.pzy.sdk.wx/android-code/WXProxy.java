package bridgeClass;

import com.tencent.mm.opensdk.constants.ConstantsAPI;
import android.content.IntentFilter;
import android.content.Intent;
import android.content.BroadcastReceiver;
import android.content.Context;
import com.edroity.nativebrige.Gate;
import com.tencent.mm.opensdk.modelbase.BaseResp;
import com.tencent.mm.opensdk.modelmsg.SendAuth;
import com.tencent.mm.opensdk.modelpay.PayReq;
import com.tencent.mm.opensdk.modelpay.PayResp;
import com.tencent.mm.opensdk.openapi.WXAPIFactory;
import com.tencent.mm.opensdk.openapi.IWXAPI;

import org.json.JSONObject;


public class WXProxy
{
    static Context _context = null;

    public static void init(Context context, String appId)
    {
        _context = context;
        regToWx(appId);
    }

    private static String _callId  = "";

    public static void Login(String callId, String arg)
    {
        _callId = callId;
        SendAuth.Req req = new SendAuth.Req();
        req.scope = "snsapi_userinfo";
        req.state = "wechat_sdk_demo_test";
        api.sendReq(req);
    }


    public static void onResp(BaseResp resp)
    {
        //og.i("onResp");
        if(resp instanceof SendAuth.Resp)
        {
            SendAuth.Resp r = (SendAuth.Resp)resp;
            if(r.errCode == 0)
            {
                Gate.setResult(_callId, r.code);
            }
            else
            {
                Gate.setException(_callId, r.errCode + "", "");
            }
        }
        else if(resp instanceof PayResp)
        {
            int code = resp.errCode;
            Gate.setResult(_callId, code + "");
        }
        else
        {
            Gate.setResult(_callId, "");
        }
    }

    // IWXAPI 是第三方app和微信通信的openApi接口
    static IWXAPI api = null;

    private static void regToWx(final String appId)
    {
        // 通过WXAPIFactory工厂，获取IWXAPI的实例
        api = WXAPIFactory.createWXAPI(_context, appId, true);

        // 将应用的appId注册到微信
        api.registerApp(appId);

        //建议动态监听微信启动广播进行注册到微信
        _context.registerReceiver(new BroadcastReceiver()
        {
            @Override
            public void onReceive(Context context, Intent intent)
            {
                // 将该app注册到微信
                api.registerApp(appId);
            }
        }, new IntentFilter(ConstantsAPI.ACTION_REFRESH_WXAPP));
    }

    public static String IsWXAppInstalled(String arg)
    {
        if(api.isWXAppInstalled())
        {
            return "true";
        }
        else
        {
            return "false";
        }
    }

    /**
     * 返回微信支付返回的 code
     * @param callId
     * @param arg
     */
    public static void Payment(String callId, String arg)
    {
        _callId = callId;
        boolean isWXAppInstalled = api.isWXAppInstalled();
        if (!isWXAppInstalled)
        {
            Gate.setException(callId, "WX_NOT_INSTALLED", "没有安装微信");
            return;
        }
        boolean isPaySupported = (api.getWXAppSupportAPI() >= 570425345);
        if(!isPaySupported)
        {
            Gate.setException(callId, "WX_VERSION_TOO_LOW", "当前微信版本过低，不支持支持");
            return;
        }
        JSONObject jd = JSONObjectUtil.fromString(arg);
        String appid = JSONObjectUtil.getString(jd, "appid");
        String partnerid = JSONObjectUtil.getString(jd, "partnerid");
        String prepayid = JSONObjectUtil.getString(jd, "prepayid");
        String noncestr = JSONObjectUtil.getString(jd, "noncestr");
        String timestamp = JSONObjectUtil.getString(jd, "timestamp");
        String thePackage = JSONObjectUtil.getString(jd, "package");
        String sign = JSONObjectUtil.getString(jd, "sign");
        PayReq req = new PayReq();
        req.appId = appid;
        req.partnerId = partnerid;
        req.prepayId = prepayid;
        req.nonceStr = noncestr;
        req.timeStamp = timestamp;
        req.packageValue = thePackage;
        req.sign = sign;
        req.extData = "app data";
        api.sendReq(req);
    }









}

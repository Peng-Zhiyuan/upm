using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using CustomLitJson;
using System;

public static class IggCdKey
{
    public static async Task SubmitAsync(string theCode)
    {
        if(!IggSdkManager.Stuff.IsIggChannel)
        {
            throw new Exception("仅在 igg 登录时可用");
        }

        BlockManager.Stuff.AddBlock("cdkey");
        try
        {
            // CYA8SM

            // cdkey 参数
            // secret:c163b4b1d9c61e5301f68ha9c9d3da5
            // mp_id：77
            var url = "https://event.k8kg.com/api?cmd=cdkey_exchange";
            var param = new Dictionary<string, string>();
            var iggid = IggSdkManager.Stuff.IggId;
            var gameId = LauncherIggSdkManager.Stuff.GameId;
            var mpId = "77";
            var code = theCode;
            var serverId = LoginManager.Stuff.session.SelectedGameServerInfo.sid.ToString();
            var charId = LoginManager.Stuff.session.selectedRoleData._id;
            var timestamp = Clock.TimestampSec.ToString();
            param["user_id"] = iggid;
            param["game_id"] = gameId;
            param["mp_id"] = mpId;
            param["code"] = code;
            param["server_id"] = serverId;
            param["char_id"] = charId;
            param["timestamp"] = timestamp;
            Sign(param);

            var sb = new StringBuilder();
            foreach (var kv in param)
            {
                var key = kv.Key;
                var value = kv.Value;
                sb.AppendLine($"{key}: " + value);
            }
            Debug.Log(sb.ToString());

            var response = await HttpUtil.RequestTextAsync(url, HttpMethod.Post, param);
            var jd = JsonMapper.Instance.ToObject(response);
            if (jd.HasKey("error"))
            {
                var errorJd = jd["error"];
                var errorCode = errorJd["code"].ToInt();
                var userMessage = errorJd["message"].ToString();
                if (errorCode != 0)
                {
                    Dialog.Confirm("", userMessage);
                    var e = new GameException(ExceptionFlag.Silent, userMessage, "cdkey_" + errorCode);
                    throw e;
                }
                else
                {
                    Dialog.Confirm("", userMessage);
                }
            }
            else
            {
                var errorCode = "cdkey_resposne_format_error";
                var errorMsg = "response not found error object";
                var e = new GameException(ExceptionFlag.None, errorMsg, errorCode);
                throw e;
            }
        }
        finally
        {
            BlockManager.Stuff.RemoveBlock("cdkey");
        }
        
    }

    const string secret = "c163b4b1d9c61e5301f68ha9c9d3da5";
    static void Sign(Dictionary<string, string> param)
    {
        var list = new List<string>();
        foreach (var kv in param)
        {
            var key = kv.Key;
            var value = kv.Value;
            var str = $"{key}={value}";
            list.Add(str);
        }
        list.Sort();
        var final = string.Join("&", list);
        var finalWithSecret = final + secret;
        var sign = SecurityUtil.Md5(finalWithSecret);
        param["sign"] = sign;
    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using CustomLitJson;

public static class HttpUtil
{

    static WWWForm CreateForm(Dictionary<string, string> dic)
    {        
        var result = new WWWForm();
        foreach(var kv in dic)
        {
            var key = kv.Key;
            var value = kv.Value;
            result.AddField(key, value);
        }
        return result;
    }

    public static Task<string> RequestTextAsync(string url, HttpMethod method, Dictionary<string, string> postFormParam)
    { 
        Debug.Log($"[Send] {url} {method}");
        var tcs = new TaskCompletionSource<string>();
        UnityWebRequest request;
        if(method == HttpMethod.Post)
        {

            request = new UnityWebRequest(url, "POST");
            if(postFormParam != null)
            {
                var form = CreateForm(postFormParam);
                var bytes = form.data;
                request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bytes);
            }
        }
        else if(method == HttpMethod.Get)
        {
            request = new UnityWebRequest(url, "Get");
        }
        else
        {
            throw new Exception("unsupport http method: " + method);
        }
        request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        var oper = request.SendWebRequest();
        oper.completed += (a) =>
        {
            var code = request.responseCode;
            if(code != 200)
            {
                var errorMsg = "";
                errorMsg = $"{url} returns http code: " + code;
                var e = new Exception($"{errorMsg}");
                tcs.SetException(e);
                return;
            }
            var response = request.downloadHandler.text;
            Debug.Log($"[Back] {response}");
            tcs.SetResult(response);
            return;

        };
        return tcs.Task;
    }

}
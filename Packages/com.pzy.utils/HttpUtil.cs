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

    static WWWForm Json2WWWForm(string jsonStr){        
        var jsonData = JsonMapper.Instance.ToObject(jsonStr);
        
        IDictionary dict = jsonData as IDictionary;

        var result = new WWWForm();
        foreach(var key in dict.Keys){
            var keyStr = key.ToString();
            Debug.Log($"[HttpUtil] WWWForm insert : {keyStr} -> {jsonData[keyStr].ToString()}");
            result.AddField(keyStr, jsonData[keyStr].ToString());
        }
        return result;
    }

    public static Task<string> RequestTextAsync(string url, HttpMethod method, string content = null, bool useWWWForm = false)
    { 
        Debug.Log($"[Send] {url} {method} {content}");
        var tcs = new TaskCompletionSource<string>();
        UnityWebRequest request;
        if(method == HttpMethod.Post)
        {
            request = new UnityWebRequest(url, "POST");
            if(content != null)
            {
                byte[] bytes = null;
                if(useWWWForm){
                    var www = Json2WWWForm(content);
                    bytes = www.data;
                }
                else{
                    bytes = Encoding.UTF8.GetBytes(content);
                }
                request.uploadHandler = (UploadHandler) new UploadHandlerRaw(bytes);
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

        if(useWWWForm){
            request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        }
        else{
            request.SetRequestHeader("Content-Type", "application/json");
        }

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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.IO;

public static class StreamingUtil
{ 
    public static string GetPathForStreamingAsset(string pathFromStreamingDir)
    {
        var path = $"{Application.streamingAssetsPath}/{pathFromStreamingDir}";
        return path;
    }

    public static Task<T> LoadStreamingAssetsAsync<T>(string pathFromStreamingDir)
    {
        var tcs = new TaskCompletionSource<T>();
        var path = $"{Application.streamingAssetsPath}/{pathFromStreamingDir}";

        if (Application.platform == RuntimePlatform.Android)
        {
            var request = UnityEngine.Networking.UnityWebRequest.Get(path);
            var oper = request.SendWebRequest();
            oper.completed += (req) =>
            {
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    if (typeof(T) == typeof(byte[]))
                    {
                        var data = request.downloadHandler.data;
                        tcs.SetResult((T)(object)data);
                    }
                    else if (typeof(T) == typeof(string))
                    {
                        var data = request.downloadHandler.text;
                        tcs.SetResult((T)(object)data);
                    }
                    else
                    {
                        var e = new Exception("unsupport T : " + typeof(T));
                        tcs.SetException(e);
                    }
                }
                else
                {
                    Debug.LogError("error in load steaming asset: " + pathFromStreamingDir);
                    var error = request.error;
                    var e = new Exception(error);
                    tcs.SetException(e);
                }


            };

        }
        else
        {
            if (typeof(T) == typeof(byte[]))
            {
                var data = File.ReadAllBytes(path);
                tcs.SetResult((T)(object)data);
            }
            else if (typeof(T) == typeof(string))
            {
                var data = File.ReadAllText(path);
                tcs.SetResult((T)(object)data);
            }
            else
            {
                var e = new Exception("unsupport T : " + typeof(T));
                tcs.SetException(e);
            }
        }
        return tcs.Task;


    }
}

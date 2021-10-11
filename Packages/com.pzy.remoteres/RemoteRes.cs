using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using ETModel;

public static class RemoteRes
{
    private static AudioType GetAudioTypeByExtension(string path)
    {
        var extension = Path.GetExtension(path);
        extension = extension.ToLower();
        if(extension == "wav")
        {
            return AudioType.WAV;
        }
        else if(extension == "mp3")
        {
            return AudioType.MPEG;
        }
        else if(extension == "ogg")
        {
            return AudioType.OGGVORBIS;
        }
        else
        {
            return AudioType.UNKNOWN;
        }
    }

    /// <sumfalse>
    /// suport type: byte[], string, Texture2D, Sprite
    /// </summary>
    /// <typeparam name="T"> byte[], string, Texture2D, AudioClip</typeparam>
    public static T Get<T>(string remotePath) where T : class
    {
        var isCached = IsResObjectInMemory(remotePath);
        if(isCached)
        {
            var res = GetResObjectFromMemory(remotePath) as T;
            return res;
        }
        throw new Exception($"[RemoteRes] res: {remotePath} not cached");
    }

    public static async Task LoadListAsync<T>(List<string> remotePathList, CacheType cacheType = CacheType.File, bool forceOrigin = false) where T : class
    {
        var taskList = new List<Task>();
        foreach(var path in remotePathList)
        {
            var task = LoadAsync<T>(path, cacheType, forceOrigin);
            taskList.Add(task);
        }
        await Task.WhenAll(taskList);
    }

    public static async Task<object> LoadAsync(Type type, string remotePath, CacheType cacheType = CacheType.File, bool forceOrigin = false, Action<float> onProgressHandler = null)
    {
        if(cacheType == CacheType.Memory)
        {
            var isCached = IsResObjectInMemory(remotePath);
            if(isCached)
            {
                Debug.Log($"[RemoteRes] load {remotePath} (from Memory)");
                var res = GetResObjectFromMemory(remotePath);
                return res;
            }
            else
            {
                Debug.Log($"[RemoteRes] load {remotePath} (from Network)");
                var bytes = await RequestByOss(remotePath, forceOrigin, onProgressHandler);
                var ret = BytesToType(type, bytes, remotePath);
                SetResObjectInMemory(remotePath, ret);
                return ret;
            }
        }
        else if(cacheType == CacheType.File)
        {
            var isCached = IsResObjectInMemory(remotePath);
            if(isCached)
            {
                Debug.Log($"[RemoteRes] load {remotePath} (from Memory)");
                var res = GetResObjectFromMemory(remotePath);
                return res;
            }
            else
            {
                var inFile = IsResBytesInFile(remotePath);
                if(inFile)
                {
                    Debug.Log($"[RemoteRes] load {remotePath} (from File)");
                    var fileBytes = ReadResBytesFromFile(remotePath);
                    var resObject = BytesToType(type, fileBytes, remotePath);
                    SetResObjectInMemory(remotePath, resObject);
                    return resObject;
                }
                else
                {
                    Debug.Log($"[RemoteRes] load {remotePath} (from Network)");
                    var bytes = await RequestByOss(remotePath, forceOrigin, onProgressHandler);
                    var ret = BytesToType(type, bytes, remotePath);
                    SetResObjectInMemory(remotePath, ret);
                    WriteResBytesToFile(remotePath, bytes);
                    return ret;
                }
            }
        }
        else if(cacheType == CacheType.None)
        {
            Debug.Log($"[RemoteRes] load {remotePath} (from Network)");
            var bytes = await RequestByOss(remotePath, forceOrigin, onProgressHandler);
            var ret = BytesToType(type, bytes, remotePath);
            return ret;
        }
        else
        {
            throw new Exception("shoud not reach here");
        }
    }

    /// <sumfalse>
    /// suport type: byte[], string, Texture2D, Sprite
    /// </summary>
    /// <typeparam name="T"> byte[], string, Texture2D, AudioClip</typeparam>
    public static async Task<T> LoadAsync<T>(string remotePath, CacheType cacheType = CacheType.File, bool forceOrigin = false) where T : class
    {
        var type = typeof(T);
        var ret = await LoadAsync(type, remotePath, cacheType, forceOrigin);
        return ret as T;
    }

    public static string RootDir
    {
        get
        {
            var envName = EnvManager.Env;
            return $"RemoteRes/{envName}";
        }
    }

    public static void DeleteFileCacheOfRemoteDir(string remoteDir, string expectPath)
    {
        var localDirPath = $"{RootDir}/{remoteDir}";
        var expectLocalPath = $"{RootDir}/{expectPath}";
        var list = FileManager.GetFileList(localDirPath);
        foreach(var path in list)
        {
            if(path != expectLocalPath)
            {
                Debug.Log($"[RemoteRes] delete {path}");
                FileManager.DeleteFile(path);
            }
        }
    }

    private static bool IsResObjectInMemory(string remotePath)
    {
        return dic.ContainsKey(remotePath);
    }

    private static void SetResObjectInMemory(string remotePath, object resObject)
    {
        dic[remotePath] = resObject;
    }

    private static object GetResObjectFromMemory(string remotePath)
    {
        return dic[remotePath];
    }

    private static bool IsResBytesInFile(string remotePath)
    {
        var envName = EnvManager.Env;
        var filePath = $"RemoteRes/{envName}/{remotePath}";
        return FileManager.HasFile(filePath);
    }

    private static void WriteResBytesToFile(string remotePath, byte[] bytes)
    {
        var envName = EnvManager.Env;
        var filePath = $"RemoteRes/{envName}/{remotePath}";
        FileManager.WriteBytes(filePath, bytes);
    }

    private static byte[] ReadResBytesFromFile(string remotePath)
    {
        var envName = EnvManager.Env;
        var filePath = $"RemoteRes/{envName}/{remotePath}";
        return FileManager.ReadBytes(filePath);
    }

    private static object BytesToType(Type type, byte[] bytes, String remotePath)
    {
        object ret = null;
        if(type == typeof(string))
        {
            ret = BytesToString(bytes);
        }
        else if(type == typeof(Texture2D))
        {
            ret = BytesToTexture2D(bytes);
        }
        else if(type == typeof(byte[]))
        {
            ret = bytes;
        }
        else if(type == typeof(AudioClip))
        {
            var audioType = GetAudioTypeByExtension(remotePath);
            ret = BytesToAudioClip(bytes, audioType);
        }
        else if(type == typeof(Sprite))
        {
            var texture = BytesToTexture2D(bytes);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero, 100, 0, SpriteMeshType.FullRect);
            return sprite;
        }
        else 
        {
            throw new Exception($"[RemoteRes] Load: unsupported return type: {type.Name}");
        }
        return ret;
    }

    private static string BytesToString(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }

    private static Texture2D BytesToTexture2D(byte[] bytes)
    {
        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes);
        return tex;
    }

    private static AudioClip BytesToAudioClip(byte[] bytes, AudioType audioType)
    {
        if(audioType == AudioType.WAV)
        {
            var wav = new Wav(bytes);
            var audioClip = AudioClip.Create("audioClipName", wav.SampleCount, 1, wav.Frequency, false);
            audioClip.SetData(wav.LeftChannel, 0);
            return audioClip;
        }
        else
        {
            throw new Exception("unsupport audioType: " + audioType);
        }
    }

    /// <summary>
    /// 路径到资源的缓存
    /// </summary>
    private static Dictionary<string, object> dic = new Dictionary<string, object>();

    /// <summary>
    /// 路径到加载进度的字典
    /// </summary>
    private static Dictionary<string, float> progressDic = new Dictionary<string, float>();

    private static void SetProgress(string pathFromResDir, float value)
    {
        progressDic[pathFromResDir] = value;
    }

    public static float GetProgress(string pathFromResDir)
    {
        var ret = DictionaryUtil.TryGet(progressDic, pathFromResDir, 0f);
        return ret;
    }

    static byte[] DecodeHttpData(byte[] data){        
        Debug.Log($"[GetHttpByteData] is encoded by gzip");
        Stream flow = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
        // using(BinaryReader reader = new BinaryReader(flow, Encoding.UTF8)){
        string str = "";//声明空字符串用来接收解压缩后的数据
        using(StreamReader reader = new StreamReader(flow, Encoding.UTF8)){
            // reader.Read(bytes, 0, bytes.Length);
            str = reader.ReadToEnd();
            var bytes = Encoding.UTF8.GetBytes(str);
            Debug.Log($"[GetHttpByteData] decoded length : {data.Length} -> {bytes.Length}");
            return bytes;
        }
        throw new Exception("[GetHttpByteData] Decode fail");
    }

    static byte[] GetHttpByteData(UnityWebRequest request){
        var data = request.downloadHandler.data;
        Debug.Log($"[GetHttpByteData] Original data length : {data.Length}");
        return data;
        var header = request.GetResponseHeaders();
        foreach(var key in header.Keys){
            Debug.LogError($"{key} -> {header[key]}");
        }

        if(header.ContainsKey("Content-Encoding") && header["Content-Encoding"].ToLower().Equals("gzip")){
            return DecodeHttpData(data);
        }
        else{
            Debug.Log($"[GetHttpByteData] not encoded by gzip");
            return data;
        }        
    }

    public static string GetActualUrl(string pathFromResDir, bool forceOrigin){
        var url = "";
        if(forceOrigin)
        {
            // use origin
            var remoteResUrl = EnvUtil.RemoteResOrigin;
            url = $"{remoteResUrl}/{pathFromResDir}";
        }
        else
        {
            // use cdn
            var assetBundleServerUrl = EnvUtil.RemoteResCdn;
            // url = assetBundleServerUrl + "/" + pathFromResDir;
            url = $"{assetBundleServerUrl}/{pathFromResDir}";
        }
        return url;
    }

    public async static Task<byte[]> RequestByOss(string pathFromResDir, bool forceOrigin, Action<float> onProgressHandller)
    {
        var url = GetActualUrl(pathFromResDir, forceOrigin);
        Debug.Log($"[{nameof(RemoteRes)}] LoadAsBytesAsync: {url}");

        var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
        HttpClient clinet = new HttpClient(handler);
        clinet.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        var result = await clinet.GetAsync(url);
        if(!result.IsSuccessStatusCode){            
            var msg = $"[{nameof(RemoteRes)}] Http error in load {url}, code: {result.StatusCode}";
            throw new GameException(ExceptionLevel.Dialog, msg);
        }
        var data = await result.Content.ReadAsByteArrayAsync();
        Debug.Log($"[GetHttpByteData] Http data length : {data.Length}");
        return data;
    }

    public static Task<byte[]> RequestByOssOld(string pathFromResDir, bool forceOrigin, Action<float> onProgressHandller)
    {
        var tcs = new TaskCompletionSource<byte[]>();
        var url = GetActualUrl(pathFromResDir, forceOrigin);

        Debug.Log($"[{nameof(RemoteRes)}] LoadAsBytesAsync: {url}");
        var request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        // request.SetRequestHeader("Accept-Encoding", "gzip");
        Debug.LogError(request.GetRequestHeader("Accept-Encoding"));
        var httpTask = HttpQueueManager.Stuff.Enqueue(request);
        httpTask.Completed += ()=>
        {
            var code = request.responseCode;
            if (request.isHttpError)
            {
                var msg = $"[{nameof(RemoteRes)}] Http error in load {url}, code: {code} error: {request.error}";
                var e = new GameException(ExceptionLevel.Dialog, msg);
                tcs.SetException(e);
            }
            else if (request.isNetworkError)
            {
                var msg = $"[{nameof(RemoteRes)}] network error in load {url}, code: {code} error: {request.error}";
                var e = new GameException(ExceptionLevel.Dialog, msg);
                tcs.SetException(e);
            }
            else
            {
                Debug.Log($"[{nameof(RemoteRes)}] Compleate: {url}");
                var bytes = GetHttpByteData(request);
                tcs.SetResult(bytes);
            }
        };
        httpTask.Updated = onProgressHandller;
        return tcs.Task;
    }

    // private static Task<byte[]> Request(string pathFromResDir, bool forceOrigin)
    // {
    //     var tcs = new TaskCompletionSource<byte[]>();
    //     var timestamp = GetTimeStamp();
    //     var url = $"{RemoteResDir}/{pathFromResDir}";
    //     if(forceOrigin)
    //     {
    //         url += $"?timestamp={timestamp}";
    //     }
    //     Debug.Log($"[{nameof(RemoteRes)}] LoadAsBytesAsync: {url}");
    //     var request = UnityWebRequest.Get(url);
    //     var oper = request.SendWebRequest();
    //     oper.completed += a=>
    //     {
    //         if (request.isHttpError || request.isNetworkError)
    //         {
    //             var e = new Exception($"load remote res {url} error");
    //             tcs.SetException(e);
    //         }
    //         else
    //         {
    //             var bytes = request.downloadHandler.data;
    //             tcs.SetResult(bytes);
    //         }
    //     };
    //     return tcs.Task;
    // }

    public static string GetTimeStamp()
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        DateTime nowTime = DateTime.Now;
        long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
        return unixTime.ToString();
    }

}

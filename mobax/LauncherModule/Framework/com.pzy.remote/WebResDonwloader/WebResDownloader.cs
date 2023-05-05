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

public static class WebResDownloader
{
    /// <summary>
    /// 删除所有缓存在持久化目录下的文件
    /// </summary>
    public static void DeleteFileCache()
    {
        FileManager.DeleteDirectory("WebRes");
    }

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
    /// 尝试从内存或本地文件缓存获得资源
    /// suport type: byte[], string, Texture2D, Sprite, AudioClip
    /// </summary>
    public static T Get<T>(string url) where T : class
    {
        var isCached = IsResObjectInMemory(url);
        if(isCached)
        {
            var res = GetResObjectFromMemory(url) as T;
            return res;
        }
        throw new Exception($"[RemoteRes] res: {url} not cached");
    }

    public static async Task LoadListAsync<T>(List<string> urlList, CacheType cacheType = CacheType.File) where T : class
    {
        var taskList = new List<Task>();
        foreach(var path in urlList)
        {
            var task = LoadAsync<T>(path, cacheType);
            taskList.Add(task);
        }
        await Task.WhenAll(taskList);
    }

    /// <summary>
    /// 对于本地不缓存的资源，是否要强制回源
    /// </summary>
    public static bool forceOriginForNonCache = true;
    public static async Task<object> LoadAsync(Type type, string url, CacheType cacheType = CacheType.File, bool allowFail = false)
    {

        if(cacheType == CacheType.Memory)
        {
            var isCached = IsResObjectInMemory(url);
            if(isCached)
            {
                Debug.Log($"[RemoteRes] load {url} (from Memory)");
                var res = GetResObjectFromMemory(url);
                return res;
            }
            else
            {
                Debug.Log($"[RemoteRes] load {url} (from Network)");
                var bytes = await RequestByOss(url, false, allowFail);
                if(bytes == null)
                {
                    return null;
                }
                var ret = BytesToType(type, bytes, url);
                SetResObjectInMemory(url, ret);
                return ret;
            }
        }
        else if(cacheType == CacheType.File)
        {
            var isCached = IsResObjectInMemory(url);
            if(isCached)
            {
                //Debug.Log($"[RemoteRes] load {url} (from Memory)");
                var res = GetResObjectFromMemory(url);
                return res;
            }
            else
            {
                var inFile = IsResBytesInFile(url);
                if(inFile)
                {
                    Debug.Log($"[RemoteRes] load {url} (from File)");
                    var fileBytes = ReadResBytesFromFile(url);
                    var resObject = BytesToType(type, fileBytes, url);
                    SetResObjectInMemory(url, resObject);
                    return resObject;
                }
                else
                {
                    Debug.Log($"[RemoteRes] load {url} (from Network)");
                    var bytes = await RequestByOss(url, false, allowFail);
                    if(bytes == null)
                    {
                        return null;
                    }
                    // 当路径是个文件夹的时候，可能会 http 访问成功，但是返回的数据长度为 0
                    // 这种情况不能进行缓存，因为路径不正确
                    if(bytes.Length == 0)
                    {
                        return null;
                    }
                    var ret = BytesToType(type, bytes, url);
                    SetResObjectInMemory(url, ret);
                    WriteResBytesToFile(url, bytes);
                    return ret;
                }
            }
        }
        else if (cacheType == CacheType.None)
        {
            Debug.Log($"[RemoteRes] load {url} (from Network)");
            var bytes = await RequestByOss(url, forceOriginForNonCache, allowFail);
            if (bytes == null)
            {
                return null;
            }
            var ret = BytesToType(type, bytes, url);
            return ret;
        }
        else
        {
            throw new Exception("shoud not reach here");
        }
    }

    /// <sumfalse>
    /// suport type: byte[], string, Texture2D, Sprite， AudioClip
    /// </summary>
    public static async Task<T> LoadAsync<T>(string url, CacheType cacheType = CacheType.File,  bool allowFail = false) where T : class
    {
        var type = typeof(T);
        var ret = await LoadAsync(type, url, cacheType, allowFail);
        return ret as T;
    }

    private static bool IsResObjectInMemory(string url)
    {
        return dic.ContainsKey(url);
    }

    private static void SetResObjectInMemory(string url, object resObject)
    {
        dic[url] = resObject;
    }

    private static object GetResObjectFromMemory(string url)
    {
        return dic[url];
    }

    public static string ToStoragePath(string url)
    {
        if(url.StartsWith("http://"))
        {
            url = url.Substring("http://".Length);
        }
        else if(url.StartsWith("https://"))
        {
            url = url.Substring("https://".Length);
        }
        url = url.Replace(':', '_');

        var path = $"WebRes/{url}";
        return path;
    }

    private static bool IsResBytesInFile(string url)
    {
        var storagePath = ToStoragePath(url);
        return FileManager.HasFile(storagePath);
    }

    private static void WriteResBytesToFile(string url, byte[] bytes)
    {
        var storagePath = ToStoragePath(url);
        FileManager.WriteBytes(storagePath, bytes);
    }

    private static byte[] ReadResBytesFromFile(string url)
    {
        var storagePath = ToStoragePath(url);
        return FileManager.ReadBytes(storagePath);
    }

    private static object BytesToType(Type type, byte[] bytes, String url)
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
            var audioType = GetAudioTypeByExtension(url);
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

    public async static Task<byte[]> RequestByOss(string url, bool forceOrigin, bool allowFail)
    {
        if(forceOrigin)
        {
            var ts = GetTimeStamp();
            url = $"{url}?timestamp={ts}";
        }

        Debug.Log($"[{nameof(WebResDownloader)}] LoadAsBytesAsync: {url}");

        var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
        var clinet = new HttpClient(handler);
        clinet.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        var result = await clinet.GetAsync(url);

        

        if (!result.IsSuccessStatusCode)
        {
            var msg = $"[{nameof(WebResDownloader)}] Http error in load {url}, code: {result.StatusCode}";
            //throw new GameException(ExceptionLevel.Dialog, msg);
            if(!allowFail)
            {
                throw new Exception(msg);
            }
            else
            {
                Debug.LogWarning(msg);
                return null;
            }
        }
        var data = await result.Content.ReadAsByteArrayAsync();
        Debug.Log($"[GetHttpByteData] Http data length : {data.Length}");
        return data;
    }

    public static string GetTimeStamp()
    {
        DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
        DateTime nowTime = DateTime.Now;
        long unixTime = (long)System.Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
        return unixTime.ToString();
    }

}

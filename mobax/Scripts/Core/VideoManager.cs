using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;

public class VideoManager : StuffObject<VideoManager>
{
    public async void SyncFromAssetsToDiskAsync()
    {
        var bucket = BucketManager.Stuff.GetBucket("video");
        await bucket.AquireByLabelAsync<TextAsset>("$video_data");
        var dic = bucket.addressToAssetDic;
        // Debug.LogError("SyncFromAssetsToDiskAsync");
        this.DeleteNotNeed(dic);
        var taskList = new List<Task>();
        foreach (var kv in dic)
        {
           
            var address = kv.Key;
            Debug.LogWarning("SyncFromAssetsToDiskAsync:"+ address);
            var asset = kv.Value as TextAsset;
            var b = this.IsFileInDisk(address);
            if(!b)
            {
                var t = WriteFileToDiskAsync(address, asset);
                taskList.Add(t);
            }
        }
        await Task.WhenAll(taskList);
        bucket.ReleaseAll();
    }

    void DeleteNotNeed(Dictionary<string, UnityEngine.Object> addressToAssetDic)
    {
        var virtualPathToAssetDic = new Dictionary<string, UnityEngine.Object>();
        foreach(var kv in addressToAssetDic)
        {
            var address = kv.Key;
            var asset = kv.Value;
            var virtualPath = GetVirtualPathForAddress(address);
            virtualPathToAssetDic[virtualPath] = asset;
        }

        var virtualPathList = FileManager.GetFileList($"video");
        foreach(var virtualPath in virtualPathList)
        {
            if(!virtualPathToAssetDic.ContainsKey(virtualPath))
            {
                FileManager.DeleteFile(virtualPath);
                Debug.Log($"[VideoManager] delete {virtualPath}");
            }
        }
    }

    string GetVirtualPathForAddress(string address)
    {
        var fileName = Path.ChangeExtension(address, ".mp4");
        return $"video/{fileName}";
    }

    bool IsFileInDisk(string address)
    {
        var path = GetVirtualPathForAddress(address);
        return FileManager.HasFile(path);
    }

    Task WriteFileToDiskAsync(string address, TextAsset asset)
    {
        var tcs = new TaskCompletionSource<bool>();
        var data = asset.bytes;
        var path = GetVirtualPathForAddress(address);
        var thread = new Thread(() =>
        {
            try
            {
                FileManager.WriteBytes(path, data);
                tcs.SetResult(true);
            }
            catch(Exception e)
            {
                tcs.SetException(e);
            }
        
        });
        thread.Start();
        Debug.LogWarning($"WriteBytes:{path}");
        Debug.Log($"[VideoManager] {address}({data.Length} byte) write to disk");
        return tcs.Task;
    }

    public string GetRealPathForAddress(string address)
    {
        var virtualPath = GetVirtualPathForAddress(address);
        var basePath = FileManager.PersistentPath;
        Debug.LogWarning($"GetRealPathForAddress:{basePath}/{virtualPath}");
        return $"{basePath}/{virtualPath}";
    }
}

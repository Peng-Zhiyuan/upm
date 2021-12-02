using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor;
using System;
using System.Threading.Tasks;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public static class UpmGitUtil
{
    /// <summary>
    /// 获取内置包的信息
    /// </summary>
    /// <returns></returns>
    public async static Task<List<PackageInfo>> GetEmbededPackageInfoListAsync()
    {
        var request = Client.List();
        await AwateRequest(request);
        var list = request.Result;
        var retList = new List<PackageInfo>();
        foreach(var one in list)
        {
            var name = one.name;
            var type = one.source;
            if(type == PackageSource.Embedded)
            {
                retList.Add(one);
            }
        }
        return retList;
    }

    public async static Task<PackageInfo> GetEmbededPackageInfo(string packageName)
    {
        var request = Client.List();
        await AwateRequest(request);
        var list = request.Result;
        var retList = new List<PackageInfo>();
        foreach (var one in list)
        {
            var name = one.name;
            var type = one.source;
            if (type == PackageSource.Embedded)
            {
                if(name == packageName)
                {
                    return one;
                }
            }
        }
        return null;
    }

    public static Task AwateRequest(Request request)
    {
        var tcs = new TaskCompletionSource<bool>();
        var w = new Waiter(request, () =>
        {
            tcs.SetResult(true);
        });
        return tcs.Task;
    }

    public static string GetGitReleaseTagName(PackageInfo info)
    {
        var name = info.name;
        var version = info.version;
        var tag = $"{name}/v{version}";
        return tag;
    }

    public static async Task<bool> GetIsGitTagExistsAsync(string tagName)
    {
        var result = await Exec.RunGetOutput("git", $"tag -l \"{tagName}\"");
        var output = result.output;
        var trimed = output.Trim();
        if(trimed == tagName)
        {
            return true;
        }
        else
        {
            return false;
        }
        
    }

    public static async Task<bool> CreateTagAsync(string tagName)
    {
        var result = await Exec.RunAsync("git", $"tag \"{tagName}\"");
        var code = result.ExitCode;
        if(code != 0)
        {
            Debug.LogError("git tag not success");
            return false;
        }
        return true;
    }

    public static async Task<bool> GetIsPackageExistsOnOpenUpmAsync(string packageName)
    {
        var result = await ExecQueue.ExecInQueue("powershell", $"openupm search \"{packageName}\"");
        var output = result.output;
        var trimed = output.Trim();
//        Debug.LogError(trimed);
        var firstCha = trimed[0];
        if (firstCha == '┌')
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

class Waiter
{
    Request request;
    Action onComplete;

    public Waiter(Request request, Action onComplete)
    {
        this.request = request;
        this.onComplete = onComplete;
        EditorApplication.update += OnUpdate;
    }

    void OnUpdate()
    {
        if(this.request.IsCompleted)
        {
            EditorApplication.update -= OnUpdate;
            onComplete.Invoke();
        }
    }
}

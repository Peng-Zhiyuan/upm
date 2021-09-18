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

    static Task AwateRequest(Request request)
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

    public static bool IsGitTagExists(string tagName)
    {
        var output = Exec.RunGetOutput("git", $"tag -l \"{tagName}\"");
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

    public static bool CreateTag(string tagName)
    {
        var code = Exec.Run("git", $"tag \"{tagName}\"");
        if(code != 0)
        {
            Debug.LogError("git tag not success");
            return false;
        }
        return true;
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

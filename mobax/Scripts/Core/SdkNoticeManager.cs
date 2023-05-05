using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using CustomLitJson;
using Sirenix.OdinInspector;

public class SdkNoticeManager : StuffObject<SdkNoticeManager>
{
    [ShowInInspector]
    Dictionary<string, SdkNoticeInfo> idToInfoDic = new Dictionary<string, SdkNoticeInfo>();

    public async Task FetchAsync()
    {
        if(!IggSdkManager.Stuff.IsIggChannel)
        {
            return;
        }

        var ssoToken = await RequestSSOAsync();
        var url = LauncherIggSdkManager.Stuff.initResult.appconfBean.url.newsUrl;
        var gameId = LauncherIggSdkManager.Stuff.GameId;
        var finalUrl = string.Format(url, gameId, ssoToken);
        string json;
        try
        {
            json = await HttpUtil.RequestTextAsync(finalUrl, HttpMethod.Get);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            return;
        }
        var msg = JsonMapper.Instance.ToObject<SdkNoticeResponse>(json);
        var infoList = msg.data;
        this.idToInfoDic.Clear();
        this.AddRange(infoList);
        DataChnaged?.Invoke();
    }

    public async void FetchInBackground()
    {
        //await this.FetchAsync();
    }

    public event Action DataChnaged;

    void AddRange(List<SdkNoticeInfo> infoList)
    {
        foreach(var info in infoList)
        {
            this.Add(info);
        }
    }

    public List<SdkNoticeInfo> GetAll()
    {
        var ret = new List<SdkNoticeInfo>();
        foreach(var kv in this.idToInfoDic)
        {
            var id = kv.Key;
            var info = kv.Value;
            var startTime = info.start_time;
            var startDate = ToDateTime(startTime);
            var endTime = info.end_time;
            var endDate = ToDateTime(endTime);
            var valid = ClockHelper.IsBetween(startDate, endDate);
            if(valid)
            {
                ret.Add(info);
            }
        }
        return ret;
    }

    static DateTime? ToDateTime(long magicTs)
    {
        if(magicTs == 0)
        {
            return null;
        }
        var date = Clock.ToDateTime(magicTs);
        return date;
    }

    public SdkNoticeInfo Get(string id)
    {
        var b = idToInfoDic.TryGetValue(id, out var ret);
        if(b)
        {
            return ret;
        }
        return null;
    }

    void Add(SdkNoticeInfo info)
    {
        var id = info.id;
        this.idToInfoDic[id] = info;
    }

    Task<string> RequestSSOAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        LauncherIggSdkManager.Stuff.initResult.session.RequestSSOTokenForWeb("News", (err, token) =>
        {
            if(!err.IsNone())
            {
                var msg = err.GetSuggestion();
                tcs.SetException(new Exception(msg));
            }
            else
            {
                tcs.SetResult(token);
            }
        });
        return tcs.Task;
    }
}

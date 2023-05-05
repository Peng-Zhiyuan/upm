using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;
using System;
using System.Threading.Tasks;
using HybridCLR;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.IO;
using System;
using System.Reflection;
using Ionic.Zlib;
using UnityEngine.Scripting;



public class FullResManager : StuffObject<FullResManager>
{
    public enum Status
    {
        NeedCheck,
        Ready,
        Downloading,
    }

    public Status status;
    Task downloadingTask;
    public DownloadStatus downloadStatus;

    bool IsAheadLogic
    {
        get
        {
            var b = DeveloperLocalSettings.IsAheadLogic;
            return b;
        }
    }

    public async Task OnSoftCheckPointAsync()
    {
        return;
        if(!IsAheadLogic)
        {
            return;
        }

        if (this.status == Status.NeedCheck)
        {
            var result = await AddressableUtil.DownloadBundle("any", ComfirmByUserAsync, OnProgressChanged);
            if (result.status == ResDownloadStatus.NotNeedDownload)
            {
                status = Status.Ready;
            }
            else if (result.status == ResDownloadStatus.NeedDownloadButUserRefused)
            {
                status = Status.NeedCheck;
            }
            else if (result.status == ResDownloadStatus.DownloadingStarted)
            {
                status = Status.Downloading;
                downloadingTask = result.downloadTask;
                this.ShowFloating();
                this.WaitComplete();
            }
        }
        else if (status == Status.Ready)
        {

        }
        else if (status == Status.Downloading)
        {

        }
    }

    async void WaitComplete()
    {
        await downloadingTask;
        status = Status.Ready;
        downloadingTask = null;
        this.RemoveFloating();
    }

//------------------------------------full res loading start-------------------------------------------//
    void DownlaodProgressChanged()
    {
        var status = FullResManager.Stuff.downloadStatus;
        var percent = status.Percent;
        fullResFloating.Value = percent;
    }
    private LoadingFloating fullResFloating;
    async void ShowFullResLoaindgFloating()
    {
        fullResFloating = await UIEngine.Stuff.ShowFloatingAsync<LoadingFloating>(null, UILayer.GlobalDialogLayer);
        FullResManager.Stuff.DownloadProgressChnaged += DownlaodProgressChanged;
    }

    void RemoveFullResLoaindgFloating()
    {
        UIEngine.Stuff.RemoveFloating<LoadingFloating>();
        fullResFloating = null;
        FullResManager.Stuff.DownloadProgressChnaged -= DownlaodProgressChanged;
    }

 //------------------------------------full res loading end-------------------------------------------//
    void ShowFloating()
    {
        UIEngine.Stuff.ShowFloating<FullResDownloadingFloating>(null, UILayer.GlobalDialogLayer);
    }

    void RemoveFloating()
    {
        UIEngine.Stuff.RemoveFloating<FullResDownloadingFloating>();
    }

    public async Task OnHardCheckPointAsync()
    {
        if (!IsAheadLogic)
        {
            return;
        }
        DeveloperLocalSettings.IsAheadLogic = false;

        if (status == Status.NeedCheck)
        {
            var result = await AddressableUtil.DownloadBundle("any", HardComfirmByUserAsync, OnProgressChanged);
            if (result.status == ResDownloadStatus.NotNeedDownload)
            {
                status = Status.Ready;
            }
            else if (result.status == ResDownloadStatus.NeedDownloadButUserRefused)
            {
                Application.Quit();
            }
            else if (result.status == ResDownloadStatus.DownloadingStarted)
            {
                status = Status.Downloading;
                downloadingTask = result.downloadTask;

                this.ShowFullResLoaindgFloating();
                await downloadingTask;
                status = Status.Ready;
                downloadingTask = null;
                this.RemoveFullResLoaindgFloating();
               
            }
        }
        else if(status == Status.Ready)
        {

        }
        else if(status == Status.Downloading)
        {
            var task = downloadingTask;
            await task;
            status = Status.Ready;
            downloadingTask = null;
            this.RemoveFloating();
        }
    }

    public async Task<bool> HardComfirmByUserAsync(float sizeInM, List<DownloadEntry> entryList)
    {
        //var msg = $"需要下载完整资源才能继续游戏 ({sizeInM:0.0}M)";
        var msg = "m1_mustDownload".Localize(sizeInM);
        var result = await Dialog.AskAsync("", msg);
        return result;
    }

    public async Task<bool> ComfirmByUserAsync(float sizeInM, List<DownloadEntry> entryList)
    {
        //var msg = $"在后台下载资源吗？({sizeInM:0.0}M)";
        var msg = "m1_downloadInBackground".Localize(sizeInM);
        var result = await Dialog.AskAsync("", msg);
        return result;
    }

    public Action DownloadProgressChnaged;

    void OnProgressChanged(DownloadStatus status)
    {
        this.downloadStatus = status;
        DownloadProgressChnaged?.Invoke();
    }
}

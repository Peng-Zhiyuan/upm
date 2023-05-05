using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using CustomLitJson;
using System.Text;
using System.Collections.Generic;


using UnityEngine.SceneManagement;


public class LauncherManager : LauncherStuffObject<LauncherManager>
{
    LauncherLoadingPage loadingPage;
    public async void ProcessInBackground(LauncherLoadingPage page)
    {
        loadingPage = page;
        // 显示加载页
        page.UIProgressValue = 0;
        page.UITipText = LauncherLocalizatioManager.Get("loading");
        var connectRemote = DeveloperLocalSettings.ConnectRemote;


        if (EnvManager.Channel == "igg")
        {
            await LauncherIggSdkManager.Stuff.InitAsync();
        }

        here:
        try
        { 
            if (connectRemote)
            {
                Debug.Log("Remote.Stuff.ConnectAsync");
                connect:
                var result = await Remote.Stuff.ConnectAsync();
                if(result.status == Remote.ConnectResult.Redirect)
                {
                    EnvManager.overrideEnv = result.redirectedToEnv;
                    Debug.Log("[env] redirect to: " + result.redirectedToEnv);
                    goto connect;
                }
                else if(result.status == Remote.ConnectResult.VersionNotSupported)
                {
                    var dialog = LauncherUiManager.Stuff.Show<LauncherDialog>();
                    dialog.Set(LauncherLocalizatioManager.Get("open_discord"), true);
                    await dialog.WaitCompleteAsync();
                    Application.OpenURL("https://discord.gg/f5sNh8ex9A");
                    Application.Quit();
                    return;
                }
                else if(result.status == Remote.ConnectResult.Pass)
                {
                    Debug.Log("[env] Remote Connect Success, final env: " + EnvManager.FinalEnv);
                }
                else
                {
                    throw new Exception($"unsuuported connect result: " + result.status);
                }

                // 检查是否开放
                var isOpen = Remote.Stuff.RemoteConf.isOpen;
                if (!isOpen)
                {
                    // 显示维护公告
                    var weihuPage = LauncherUiManager.Stuff.Show<LauncherWeihuPage>();
                    await weihuPage.WaitPassAsync();
                }
            } 

            var isResUpdateEnabled = DeveloperLocalSettings.IsResUpdateEnabled;
            var isHotSystemUpdateEnabled = connectRemote && isResUpdateEnabled;
            Debug.Log("HotSystemManager.InitalizeAsync");
            await HotSystemManager.InitalizeAsync(
                isUpdateAllowed: isHotSystemUpdateEnabled, 
                onComfirmDownload: OnConfirmDownload,
                onProgress: OnProgressUpdate,
                onReportProgress: OnDownloadStatusUpdate);

        }
        catch(Exception e)
        {
            if(!Application.isPlaying)
            {
                return;
            }
            Debug.LogException(e);
            var dialog = LauncherUiManager.Stuff.Show<LauncherDialog>();
            var msg3 = LauncherLocalizatioManager.Get("error_occured");
            dialog.Set($"{msg3}:\n{e.Message}", true);
            await dialog.WaitCompleteAsync();
            goto here;
        }


        page.UITipText = LauncherLocalizatioManager.Get("loading");
        Debug.Log("[LauncherManager] load HotRoot.unity");
        await Addressables.LoadSceneAsync("HotRoot.unity", LoadSceneMode.Additive).Task;

    }

    void OnDownloadStatusUpdate(DownloadStatus downloadStatus)
    {
        var totalM = downloadStatus.TotalBytes / 1024f / 1024f;
        var downloadedM = downloadStatus.DownloadedBytes / 1024f / 1024f;
        var percent = downloadStatus.Percent;

        var msg2 = LauncherLocalizatioManager.Get("downloading");
        var msg = $"{msg2} ({downloadedM:0.00}M/{totalM:0.00}M)";

        this.loadingPage.UIProgressValue = percent;
        this.loadingPage.UITipText = msg;
        if (gamePage != null)
        {
            gamePage.UIDownloadProgress = percent;
            gamePage.UITipText = msg;
        }
    }

    void OnProgressUpdate(float v)
    {
        this.loadingPage.UIProgressValue = v;
    }



    async Task<bool> OnConfirmDownload(float sizeM, List<DownloadEntry> entryList)
    {
        var dialog = LauncherUiManager.Stuff.Show<LauncherDialog>();
        var sb = new StringBuilder();
        var msg2 = LauncherLocalizatioManager.Get("download_content");
        var msg3 = string.Format(msg2, sizeM);
        //sb.AppendLine($"{msg2}({sizeM:0.00}M)");
        //Debug.Log($"{msg2}({sizeM:0.00}M)");
        sb.AppendLine(msg3);
        if (DeveloperLocalSettings.IsDevelopmentMode)
        {
            entryList.Sort((a, b) =>
            {
                if (a.size > b.size)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            });

            var count = entryList.Count;
            sb.AppendLine($"{count} file(s)");
            Debug.Log($"{count} file(s)");
            var lineCount = 0;
            foreach (var item in entryList)
            {
                sb.AppendLine($"{item.name} - {item.SizeInM: 0.00}M");
                Debug.Log($"{item.name} - {item.SizeInM: 0.00}M");
                lineCount++;
                if (lineCount >= 10)
                {
                    sb.Append("...");
                    break;
                }
            }
        }
        var msg = sb.ToString();
        dialog.Set(msg, false, LauncherLocalizatioManager.Get("download_tip"));
        var userAgree = await dialog.WaitCompleteAsync();
        if (!userAgree)
        {
            return false;
        }
        else
        {
            if (sizeM >= 50)
            {
                this.loadingPage.CheckRewards();
                ShowMiniGame();
            }
            this.loadingPage.ResetProgress();
            return true;
        }
    }


    LauncherGamePage gamePage;
    public void ShowMiniGame()
    {
        // LauncherUiManager.Stuff.Show<>
         var page = GameObject.FindObjectOfType<LauncherGamePage>();
         if(page == null)
         {
            gamePage = LauncherUiManager.Stuff.Show<LauncherGamePage>("LauncherGame");
            gamePage.UIDownloadProgress = 0;
            gamePage.UITipText = "";
         }
    }

    async Task<bool> DialogAsync(string key, bool onlyConfirm, string tip_key = null)
    {
        var dialog2 = LauncherUiManager.Stuff.Show<LauncherDialog>();
        var msg = LauncherLocalizatioManager.Get(key);
        var tip = "";
        if (!string.IsNullOrEmpty(tip_key))
        {
            tip = LauncherLocalizatioManager.Get(tip_key);
        }
        dialog2.Set(msg, onlyConfirm, tip);
        var ret = await dialog2.WaitCompleteAsync();
        return ret;
    }
}

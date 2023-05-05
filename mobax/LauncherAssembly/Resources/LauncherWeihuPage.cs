using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;

public partial class LauncherWeihuPage : MonoBehaviour
{
    private void OnEnable()
    {
        this.Refresh();
    }

    void Refresh()
    {
        this.RefreshContent();
    }

    void RefreshContent()
    {
        var remoteConf = Remote.Stuff.RemoteConf;
        this.Text_content.text = remoteConf.closeMsg;
    }

    TaskCompletionSource<bool> tcs;
    public Task WaitPassAsync()
    {
        var tcs = new TaskCompletionSource<bool>();
        this.tcs = tcs;
        return tcs.Task;
    }

    public async void OnButton(string msg)
    {
        if(msg == "refresh")
        {
            await Remote.Stuff.SyncRemoteConfAsync();
            this.Refresh();
            this.CheckPass();
        }
        else if(msg == "discord")
        {
            var url = Remote.Stuff.RemoteConf.discord;
            Application.OpenURL(url);
        }
    }
    
    void CheckPass()
    {
        var remoteConf = Remote.Stuff.RemoteConf;
        var isOpen = remoteConf.isOpen;
        if(isOpen)
        {
            LauncherUiManager.Stuff.Remove<LauncherWeihuPage>();
            this.tcs.SetResult(true);
        }
        else
        {
            this.FreezeRefreshButtonInBackground();
        }
       
    }

    async void FreezeRefreshButtonInBackground()
    {
        this.Button_refresh.gameObject.SetActive(false);
        this.Button_frefreshFrezen.gameObject.SetActive(true);
        var time = 10;
        
        while(time > 0)
        {
            var text = this._button_frefreshFrezen.gameObject.GetComponentInChildren<Text>();
            text.text = $"Refresh({time})";
            await Task.Delay(1000);
            time--;
        }
        this.Button_refresh.gameObject.SetActive(true);
        this.Button_frefreshFrezen.gameObject.SetActive(false);
    }
}

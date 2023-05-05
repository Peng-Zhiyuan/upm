using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using CustomLitJson;
using DG.Tweening;
using System;
public partial class LauncherLoadingPage : MonoBehaviour
{
    public LauncherSimpleItem ItemPrefab;

    private void Awake()
    {
        // 默认不展示
        Node_rewards.SetActive(false);
    }

    private void Start()
    {
        LauncherLocalizatioManager.Stuff.ReloadData();

        this.ResetState();
        this.Refresh();
        this.SetLogo();
        LauncherManager.Stuff.ProcessInBackground(this);
    }

    void Refresh()
    {
        this.RefreshVersion();
    }

    public void RefreshVersion()
    {
        var build = LauncherBuildInfo.Get("build", "0");
        var version = Application.version;
        var str = $"v{version}.l{build}";
        if(DeveloperLocalSettings.IsDevelopmentMode)
        {
            str += " developer";

            var env = EnvManager.FinalEnv;

            str += $".{env}";
        }
        this.Text_version.text = str;
    }
    
    public void CheckRewards()
    {
        var available = LauncherLoadingHelper.BuildRewards(ItemPrefab, List_rewards.transform);
        Node_rewards.SetActive(available);
    }

    private void SetLogo()
    {
        Switcher_logo.Selected = LauncherLocalizatioManager.Stuff.Language != "jp";
    }
    
    private void ResetState()
    {
        this.Progress.DisplayValue = 0f;
    }
    
    public SmoothProgressBar ProgressBar
    {
        get
        {
            return this.Progress;
        }
    }

    private void Update()
    {
        this.Progress_text.text = this.ProgressBar.DisplayValue.ToString("P0");
        this.Refresh();
    }

    public float UIProgressValue
    {
        get
        {
            return this.ProgressBar.targetValue;
        }
        set
        {
            this.ProgressBar.targetValue = value;
        }
    }

    public void ResetProgress()
    {
        this.ProgressBar.ResetProgress();
    }

    public string UITipText
    {
        set
        {
            this.Tip_loading.text = value;
        }
        get
        {
            return this.Tip_loading.text;
        }
    }

    public Task WaitProcessBarFullAsync()
    {
        return this.ProgressBar.WaitDisplayFullAsync();
    }

    public static bool holdon;
    public async void OnButton(string msg)
    {
        if(msg == "cs")
        {
            if (Application.isEditor)
            {
                Debug.LogError("[LauncherLoadingPage] 在编辑器上不支持客服");
            }

            if (EnvManager.Channel == "igg")
            {
                GPC.Helper.Jingwei.Script.TSH.TSHHelper.SharedInstance().ShowPanel();
            }
            else
            {
                Debug.LogError("[LauncherLoadingPage] 仅在 igg 登录时支持");
            }
        }

        else if(msg == "dev")
        {
            holdon = true;
            try
            {
                if (!DeveloperLocalSettings.IsDevelopmentMode)
                {
                    var input = LauncherUiManager.Stuff.Show<LauncherInputDialog>();
                    var result = await input.WaitCompleteAsync();
                    if (result.text == "dev123")
                    {
                        DeveloperLocalSettings.IsDevelopmentMode = true;
                        await Comfirm("!");
                    }
                }
                else
                {
                    DeveloperLocalSettings.IsDevelopmentMode = false;
                    await Comfirm("!");
                }
            }
            finally
            {
                holdon = false;
            }
        }
        else if (msg == "play_game")
        {
            LauncherManager.Stuff.ShowMiniGame();
        }
    }

    public bool IsForDistribution
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                var release = GameManifestManager.Get("release", "false");
                if (release == "true")
                {
                    return true;
                }
                return false;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var ret = Copy.IsDistribution();
                return ret;
            }
            return false;
        }
    }
    public static async Task Comfirm(string msg)
    {
        var dialog = LauncherUiManager.Stuff.Show<LauncherDialog>();
        dialog.Set(msg, true);
        await dialog.WaitCompleteAsync();
    }
}

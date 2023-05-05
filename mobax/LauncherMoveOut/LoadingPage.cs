using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CustomLitJson;
using DG.Tweening;
using UnityEngine.UI;

public partial class LoadingPage : Page
{
    public float refreshTipTm = 5f;

    private Transform animRoot;
    private CanvasGroup canvas;
    public Image bg;

    private void RefreshTip()
    {
        int index = UnityEngine.Random.Range(0, StaticData.TipsTable.ElementList.Count);
        var tipTitle = LocalizationManager.Stuff.GetText(StaticData.TipsTable.ElementList[index].Des);
        this.Tip_loading.text = tipTitle;
        TimerMgr.Instance.ScheduleTimerDelay(refreshTipTm, RefreshTip, false, "TIP_RANDOM");
    }

    private void OnEnable()
    {
       RefreshTip();
    }

    private void OnDisable()
    {
        TimerMgr.Instance.Remove("TIP_RANDOM");
    }

    public override async Task OnNavigatedToPreperAsync(PageNavigateInfo info)
    {
        if(info.terminalOperation == NavigateOperation.Forward)
        {
            int index = UnityEngine.Random.Range(0, 5) + 1;
            string addressName = $"loading_{index.ToString("00")}.png";
            bg.sprite = await this.PageBucket.GetOrAquireSpriteAsync(addressName, false);
        }
    }

    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        this.Refresh();
    }

    void Refresh()
    {
        var version = GameUtil.Version;
        this.Label_version.text = version;
        this.CustomerService.text = LauncherLocalizatioManager.Get("common_service");

    }

    public override void OnPush()
    {
        this.ResetState();
    }

    private void ResetState()
    {
        this.Progress.DisplayValue = 0f;
        this.Progress_text.text = "0%";
        this.canvas.DOKill();
    }

    public SmoothProgressBar ProgressBar
    {
        get { return this.Progress; }
    }

    private void Update()
    {
        this.Progress_text.text = $"{(int) (this.ProgressBar.DisplayValue * 100)}%";
    }

    public float Value
    {
        get { return this.ProgressBar.targetValue; }
        set { this.ProgressBar.targetValue = value; }
    }

    public Task WaitProcessBarFullAsync()
    {
        return this.ProgressBar.WaitDisplayFullAsync();
    }

    public void ShowMask()
    {
        this.Comp_Mask.SetActive(true);
    }

    public override void OnButton(string msg)
    {
        if(msg == "cs")
        {
            IggSdkManager.Stuff.ShowTshUi();
        }
    }
}
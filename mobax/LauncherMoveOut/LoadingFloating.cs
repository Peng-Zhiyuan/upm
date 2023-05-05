using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

public partial class LoadingFloating : Floating
{
    private Transform animRoot;
    private CanvasGroup canvas;

    public override void OnHide()
    {
        base.OnHide();
        this.Value = 0;
        TimerMgr.Instance.Remove("TIP_RANDOM");
    }

    private void RefreshTip()
    {
        int index = UnityEngine.Random.Range(0, StaticData.TipsTable.ElementList.Count);
        var tipTitle = LocalizationManager.Stuff.GetText(StaticData.TipsTable.ElementList[index].Des);
        this.Tip_loading.text = tipTitle;
        TimerMgr.Instance.ScheduleTimerDelay(3, RefreshTip, false, "TIP_RANDOM");
    }

    public override async Task OnShowPreperAsync()
    {
        RefreshTip();

        int i = UnityEngine.Random.Range(0, 5) + 1;
        string addressName = $"loading_{i.ToString("00")}.png";
        var bucket = BucketManager.Stuff.GetBucket(UIEngine.LatestNavigatePageName);
        Bg.sprite = await bucket.GetOrAquireSpriteAsync(addressName, false);
    }

    private void ResetState()
    {
        this.Progress.DisplayValue = 0f;
        this.canvas.DOKill();
        this.Progress_text.text = "0%";
    }

    public SmoothProgressBar ProgressBar
    {
        get { return this.Progress; }
    }

    private void Update()
    {
        this.Progress_text.text = $"{(int)(this.ProgressBar.DisplayValue * 100)}%";
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
}
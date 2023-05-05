using Plot.Runtime;

public partial class HeroBookDrawItem : ClickableItem
{
    public LibPlateRow Cfg { get; set; }
    
    public void SetInfo(HeroInfo heroInfo, LibPlateRow cfg)
    {
        Cfg = cfg;

        Txt_name.text = cfg.Title.Localize();
        UiUtil.SetSpriteInBackground(Img_thumb, () => $"{cfg.Icon}.png");

        var locked = !HeroBookHelper.CheckUnlock(heroInfo, cfg);
        Node_lock.SetActive(locked);
    }
}

internal enum UnlockType
{
    T1_Break = 1,
    T2_Plot
}
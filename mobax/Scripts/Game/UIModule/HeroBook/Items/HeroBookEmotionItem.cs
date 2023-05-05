public partial class HeroBookEmotionItem : ClickableItem
{
    public LibEmoRow Cfg { get; set; }
    
    public void SetInfo(LibEmoRow cfg)
    {
        Cfg = cfg;
        
        Txt_name.text = cfg.Title.Localize();
        UiUtil.SetSpriteInBackground(Img_thumb, () => $"{cfg.Icon}.png");
    }
}
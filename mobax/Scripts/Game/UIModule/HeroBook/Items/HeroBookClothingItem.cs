public partial class HeroBookClothingItem : ClickableItem
{
    public LibClothRow Cfg { get; set; }
    
    public bool Locked { get; private set; }
    
    public void SetInfo(LibClothRow cfg)
    {
        Cfg = cfg;

        Txt_name.text = cfg.Title.Localize();
        UiUtil.SetSpriteInBackground(Img_thumb, () => $"{cfg.Icon}.png",
            _ => Img_thumb.SetNativeSize());

        Locked = false;
        Node_lock.SetActive(Locked);
    }
}
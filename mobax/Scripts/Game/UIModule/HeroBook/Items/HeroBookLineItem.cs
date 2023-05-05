using UnityEngine;

public partial class HeroBookLineItem : ClickableItem
{
    private int AnimTalking { get; } = Animator.StringToHash("Talking");
    private int AnimMute { get; } = Animator.StringToHash("Mute");
    
    public LibDialogRow Cfg { get; set; }
    
    public void SetInfo(HeroInfo heroInfo, LibDialogRow cfg)
    {
        Cfg = cfg;

        Txt_name.text = cfg.Title.Localize();
        Flag_talking.Selected = false;
        var locked = !HeroBookHelper.CheckBreakUnlock(heroInfo, cfg.Unlock);
        Node_lock.SetActive(locked);
    }

    public void SetTalking(bool talking)
    {
        Flag_talking.Selected = talking;
        if (talking)
        {
            Anim_talking.Play(AnimTalking);
        }
    }
    
    public void StopTalking()
    {
        Anim_talking.Play(AnimMute);
    }
}
using System;
using UnityEngine;

public partial class HeroBookPlotItem : MonoBehaviour
{
    public Action<LibStoryRow> OnClick;
    
    public LibStoryRow Cfg { get; private set; }

    public void SetInfo(LibStoryRow cfg)
    {
        Cfg = cfg;
        Txt_plotName.text = cfg.Title.Localize();

        var locked = !HeroBookHelper.CheckPlotUnlock(cfg.Unlock, cfg.Title);
        Node_lock.SetActive(locked);
    }
    
    public void OnClickHandler()
    {
        OnClick?.Invoke(Cfg);
    }
}
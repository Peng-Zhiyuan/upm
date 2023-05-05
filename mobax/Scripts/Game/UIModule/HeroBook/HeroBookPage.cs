using System.Collections.Generic;
using SpineRegulate;
using UnityEngine;

public partial class HeroBookPage : Page
{
    public List<Transform> heroDisplays;
    
    private HeroInfo _heroInfo;
    private int _spineId;
    
    public override void OnNavigatedTo(PageNavigateInfo navigateInfo)
    {
        if (navigateInfo.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;
            Tabs.SetSelect(HeroBookEnum.Clothing);
        }
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "back":
                UIEngine.Stuff.Back();
                break;
        }
    }

    private void Awake()
    {
        Tabs.OnSelect = _OnTabSelected;
        Tabs.OnBeforeSelect = _OnBeforeTabSelected;
    }

    private bool _OnBeforeTabSelected(string tab)
    {
        switch (tab)
        {
            case HeroBookEnum.Draw:
                var cfgArr = StaticData.LibPlateTable.TryGet(_heroInfo.HeroId);
                var list = cfgArr?.Colls;
                if (list == null || list.Count <= 0)
                {
                    ToastManager.ShowLocalize("M4_hero_noDraw");
                    return false;
                }
                var unlock = false;
                string msg = null;
                foreach (var cfg in list)
                {
                    if (HeroBookHelper.CheckUnlock(_heroInfo, cfg, out var errorMessage))
                    {
                        unlock = true;
                        break;
                    }

                    msg ??= errorMessage;
                }

                if (!unlock)
                {
                    ToastManager.Show(msg);
                    return false;
                }
                
                break;
        }

        return true;
    }

    private void _OnTabSelected(string tab)
    {
        // 展示spine
        switch (tab)
        {
            case HeroBookEnum.Lines:
            case HeroBookEnum.Scenario:
            case HeroBookEnum.Profile:
            case HeroBookEnum.Emotion:
                _ShowDisplay(Spine_common);
                // 设置spine
                var templateType = HeroBookEnum.Emotion == tab
                    ? ESpineTemplateType.FeatureCamera
                    : ESpineTemplateType.Model;
                if (_spineId != _heroInfo.HeroId)
                {
                    Spine_common.Set(_heroInfo.HeroId, templateType);
                    _spineId = _heroInfo.HeroId;
                }
                else
                {
                    _EaseShowSpine(templateType);
                }
                break;
            case HeroBookEnum.Draw:
                _HideAllDisplays();
                break;
            case HeroBookEnum.Clothing:
                _ShowDisplay(Img_clothing);
                break;
        }
        var pane = Views.transform.Find($"HeroBook{StringUtil2.CapitalizeFirstLetter(tab)}");
        var page = pane.GetComponent<HeroBookBasePane>();
        page.SetHero(_heroInfo);
        Views.SetSelected(pane);
        Txt_heroName.text = _heroInfo.Name.Localize();
    }

    private void _EaseShowSpine(ESpineTemplateType templateType)
    {
        var prevType = Spine_common.TemplateType;
        if (default != prevType && templateType != prevType)
        {
            Spine_common.ChangeTypeAndPlayAnimation(templateType, .5f);
        }
    }

    private void _ShowDisplay(Component display)
    {
        var displayTf = display == null ? null : display.transform;
        foreach (var heroDisplay in heroDisplays)
        {
            heroDisplay.SetActive(heroDisplay == displayTf);
        }
    }

    private void _HideAllDisplays()
    {
        _ShowDisplay(null);
    }
}

public static class HeroBookEnum
{
    public const string Clothing = "clothing";
    public const string Draw = "draw";
    public const string Lines = "lines";
    public const string Scenario = "scenario";
    public const string Profile = "profile";
    public const string Emotion = "emotion";
}
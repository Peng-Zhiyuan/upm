using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public partial class HeroBookScenario : HeroBookBasePane
{
    private List<LibStoryRow> _whole;
    private List<LibStoryRow> _list;
    
    protected override void _InternalRefreshHero()
    {
        if (!_heroChanged) return;
        
        _whole = StaticData.LibStoryTable.TryGet(_heroInfo.HeroId).Colls;
    }

    private void Awake()
    {
        List_plots.onItemRenderAction = _OnPlotItemRender;
        Tabs_all.OnSelect = _OnTabSelected;
        Tabs_all.OnSelectComp = _OnTabItemSelected;
    }

    private void Start()
    {
        Tabs_all.SetSelect((int) PlotType.Individual);
    }

    private void _OnTabSelected(int label)
    {
        _list = _whole.FindAll(item => item.Type == label);
        List_plots.numItems = (uint) _list.Count;
    }

    private void _OnTabItemSelected(MutexComponent comp)
    {
        if (!Img_switcher.gameObject.activeSelf)
        {
            Img_switcher.SetActive(true);
            Img_switcher.SetAnchoredPositionX(comp.rectTransform().anchoredPosition.x);
        }
        else
        {
            Img_switcher.transform.DOMoveX(comp.transform.position.x, .2f);
        }
    }

    private void _OnPlotItemRender(int index, Transform tf)
    {
        var plotItem = tf.GetComponent<HeroBookPlotItem>();
        plotItem.SetInfo(_list[index]);
        plotItem.OnClick = _OnPlotClick;
    }

    private async void _OnPlotClick(LibStoryRow cfg)
    {
        if (!HeroBookHelper.CheckPlotUnlock(cfg.Unlock, cfg.Title, out var lockMessage))
        {
            ToastManager.Show(lockMessage);
            return;
        }
        
        await PlotPipelineManager.Stuff.PlayPlotAsync(cfg.Storyid, true);
    }
}

internal enum PlotType
{
    Individual = 1,
    Memory
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class HeroCircuitPop : Page
{
    private HeroInfo _heroInfo;

    public override void OnForwardTo(PageNavigateInfo info)
    {
        if (info.param is HeroInfo heroInfo)
        {
            _heroInfo = heroInfo;

            Tab_functions.SetDefault();
        }
    }

    /// <summary>
    /// 提供给引导用
    /// </summary>
    /// <returns></returns>
    public Transform GetCircuitCell()
    {
        var comp = Switcher.list[(int) CircuitFunctionType.T0_Equip];
        var pane = comp.GetComponent<CircuitEquipPane>();
        return pane.GetListCell();
    }

    private void Awake()
    {
        // Tab_functions.Disables = new[] {(int) CircuitFunctionType.T1_Enhance};
        Tab_functions.OnLockedTabClicked = _OnLockedTabClicked;
        Tab_functions.OnTabChanged = _OnTabChanged;
    }

    private void _OnTabChanged(int index)
    {
        Switcher.Selected = index;

        var comp = Switcher.list[index];
        comp.GetComponent<CircuitBasePane>().SetInfo(_heroInfo);
    }

    private void _OnLockedTabClicked(int index)
    {
        ToastManager.ShowLocalize("common_lockTip");
    }
}

internal enum CircuitFunctionType
{
    T0_Equip = 0,
    T1_Enhance,
    T2_Resolve,
}
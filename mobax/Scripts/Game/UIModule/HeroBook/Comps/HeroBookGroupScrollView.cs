using System;
using System.Collections.Generic;
using FancyScrollView;
using UnityEngine;

public class HeroBookGroupScrollView : FancyScrollView<HeroBookGroupData, HeroBookGroupContext>
{
    [SerializeField] Scroller scroller = default;
    [SerializeField] GameObject cellPrefab = default;
    
    public Action<int> OnSelected;

    protected override GameObject CellPrefab => cellPrefab;

    protected override void Initialize()
    {
        base.Initialize();

        Context.OnCellClicked = SelectCell;

        scroller.OnValueChanged(UpdatePosition);
        scroller.OnSelectionChanged(UpdateSelection);
    }

    void UpdateSelection(int index)
    {
        if (Context.SelectedIndex == index)
        {
            return;
        }

        Context.SelectedIndex = index;
        Refresh();
        
        // 执行回调
        if (null != OnSelected) OnSelected(index);
    }

    public void UpdateData(IList<HeroBookGroupData> items)
    {
        UpdateContents(items);
        scroller.SetTotalCount(items.Count);
    }

    public void SelectCell(int index)
    {
        if (index < 0 || index >= ItemsSource.Count || index == Context.SelectedIndex)
        {
            return;
        }

        UpdateSelection(index);
        scroller.ScrollTo(index, 0.35f, EasingCore.Ease.OutCubic);
    }
}
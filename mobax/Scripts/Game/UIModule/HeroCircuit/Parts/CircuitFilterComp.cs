using System;
using UnityEngine;
using UnityEngine.UI;

public class CircuitFilterComp : MonoBehaviour
{
    public BaseFilterPane pane;
    public Text sortTxt;
    public Image orderImage;

    private CircuitFilter _filter;
    
    public void OnButton(string msg)
    {
        switch (msg)
        {
            case "click_filter":
                _ClickFilter();
                break;
            case "click_order":
                _ClickOrder();
                break;
        }
    }

    public void BindFilter(CircuitFilter filter, bool filterUnequipped = true)
    {
        _filter = filter;
        pane.Orders = CircuitFilter.GeneralSorts;
        pane.Filters = CircuitFilter.GeneralFilters;
        filter.FilterUnequipped = filterUnequipped;
        pane.AddExtraFilter(new ExtraFilterItem
        {
            Description = "M4_circuit_words_not_equipped",
            FilterAction = _OnEquipButtonClicked,
        }).Selected = filter.FilterUnequipped;
        sortTxt.SetLocalizer(filter.CurrentSort.Description);
        _RefreshOrderView();
    }

    private void Awake()
    {
        pane.onSortChanged = _OnSortChanged;
        pane.onFilterChanged = _OnFilterChanged;
    }

    private void _ClickFilter()
    {
        pane.Show();
    }

    private void _ClickOrder()
    {
        _filter.SwitchOrder();
        _RefreshOrderView();
    }

    private void _OnSortChanged()
    {
        sortTxt.SetLocalizer(pane.SelectedSort.Description);
        _filter.SetSort(pane.SelectedSort);
    }
    
    private void _OnFilterChanged()
    {
        _filter.SetFilter(pane.SelectedFilter);
    }
    
    private void _OnEquipButtonClicked(ClickBehaviour btn)
    {
        var mutex = btn.GetComponent<MutexComponent>();
        var selected = !mutex.Selected;
        mutex.Selected = selected;
        _filter.SetFilterUnequipped(selected);
    }

    private void _RefreshOrderView()
    {
        orderImage.SetLocalScale(new Vector3(1, _filter.CurrentOrder == SortOrderEnum.Descend ? -1 : 1, 1));
    }
}
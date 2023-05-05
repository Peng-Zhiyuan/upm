using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class CircuitResolvePane : CircuitBasePane
{
    private List<HeroCircuitInfo> _pickedList;
    private Dictionary<int, int> _itemMap;
    private CircuitFilter _filter;

    public override void SetInfo(HeroInfo _)
    {
        _RefreshView();
    }

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "resolve":
                _DoResolve();
                break;
            case "cancel":
                UIEngine.Stuff.Back();
                break;
        }
    }

    private void Awake()
    {
        List_circuit.onItemRenderAction = _OnCircuitRender;
        // 过滤逻辑
        _filter = new CircuitFilter();
        Node_filter.BindFilter(_filter);
        _filter.OnListChanged = _OnListChanged;
    }

    private void _RefreshView()
    {
        _ResetPickList();
        _ResetList();
        _RefreshReturnItems();
    }

    private void _ResetList()
    {
        var list = HeroCircuitManager.All.ConvertAll(HeroCircuitManager.GetCircuitInfo);
        _filter.ResetList(list);
        _filter.Refresh();
        _RefreshListRelated();
    }

    private void _RefreshListRelated()
    {
        List_circuit.numItems = (uint) _filter.List.Count;
    }

    private void _ResetPickList()
    {
        if (null == _pickedList)
        {
            _pickedList = new List<HeroCircuitInfo>();
        }
        else
        {
            _pickedList.Clear();
        }

        if (null == _itemMap)
        {
            _itemMap = new Dictionary<int, int>();
        }
        else
        {
            _itemMap.Clear();
        }
    }
    
    private void _OnCircuitRender(int index, Transform tf)
    {
        var cell = tf.GetComponent<CircuitPickableListCell>();
        var circuitInfo = _filter.List[index];
        cell.SetInfo(circuitInfo);
        cell.Selected = _pickedList.Contains(circuitInfo);
        cell.onSelect = _OnCellSelect;
    }

    private void _OnCellSelect(CircuitPickableListCell cell)
    {
        var circuitInfo = cell.CircuitInfo;
        if (circuitInfo.ItemInfo.IsUsed)
        {
            ToastManager.ShowLocalize("M4_circuit_toast_resolve_notavailable");
            return;
        }

        var itemInfo = circuitInfo.LevelConfig.Resolve;
        var itemId = itemInfo.Id;
        var selected = cell.Selected = !cell.Selected;
        _itemMap.TryGetValue(itemId, out var total);
        if (selected)
        {
            _pickedList.Add(circuitInfo);
            _itemMap[itemId] = total + itemInfo.Num;
        }
        else
        {
            _pickedList.Remove(circuitInfo);
            var left = _itemMap[itemId] = total - itemInfo.Num;
            if (left <= 0)
            {
                _itemMap.Remove(itemId);
            }
        }

        _RefreshReturnItems();
    }

    private void _RefreshReturnItems()
    {
        var available = _itemMap.Count > 0;
        if (available)
        {
            var itemList = new List<VirtualItem>();
            foreach (var kv in _itemMap)
            {
                itemList.Add(new VirtualItem {id = kv.Key, val = kv.Value});
            }
            ItemList_resolve.Set(itemList);
        }
        Node_resolveInfo.Selected = available;
        Button_confirm.Selected = available;
    }

    private void _OnListChanged()
    {
        // _ResetPickList();
        // _RefreshReturnItems();
        _RefreshListRelated();
    }

    private async void _DoResolve()
    {
        if (_itemMap.Count <= 0)
        {
            ToastManager.ShowLocalize("m4_desc_puzzle_no_picked");
            return;
        }

        if (!await Dialog.AskAsync("", "M4_dismantle_commit".Localize())) return;
        await HeroCircuitApi.Resolve(_pickedList.ConvertAll(item => item.InstanceId));
        _RefreshView();
    }
}
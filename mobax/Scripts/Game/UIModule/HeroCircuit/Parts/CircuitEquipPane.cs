using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public partial class CircuitEquipPane : CircuitBasePane
{
    private const int PageSize = 10;
    private Pager<HeroCircuitInfo> _pager;
    private HeroCircuitInfo _pickedCircuitInfo;
    private HeroInfo _heroInfo;
    private Dictionary<HeroCircuitInfo, CircuitPickableListCell> _cellMap;
    private bool _rotateFlag;
    private List<HeroAttr> _attrsList;
    private CircuitFilter _filter;

    public override void OnButton(string msg)
    {
        switch (msg)
        {
            case "cancel_pick":
                _SetNewPick(null);
                break;
        }
    }

    public override void SetInfo(HeroInfo heroInfo)
    {
        _heroInfo = heroInfo;
        Board_circuit.SetInfo(heroInfo);
        _ResetView();
        _ResetList();
        _RefreshAttrs();
        _OnEditingModeChange(false);
    }

    public Transform GetListCell()
    {
        return _cellMap[_pager.List.First()].transform;
    }
    
    private void Awake()
    {
        // 整个界面的这个小块都是由此提供的
        _pager = new Pager<HeroCircuitInfo>(PageSize);
        View_pager.SetPager(_pager);
        View_pager.OnTurnPage = _RefreshCircuitPageView;
        List_circuit.onItemRenderAction = _OnCircuitCellRender;
        Board_circuit.onCellOn = _OnCircuitOn;
        Board_circuit.onCellOff = _OnCircuitOff;
        Board_circuit.onCellPicked = _OnCircuitPicked;
        Board_circuit.onCircuitRotate = _OnCircuitRotate;
        Board_circuit.onEditing = _OnEditingModeChange;
        Board_circuit.onCircuitChange = _OnCircuitChange;
        Board_circuit.onCircuitUpdate = _OnCircuitUpdate;
        // 过滤逻辑
        _filter = new CircuitFilter();
        Node_filter.BindFilter(_filter);
        _filter.OnListChanged = _OnListChanged;
        
        CircuitDragHelper.OnListItemDown = _OnCellDown;
        CircuitDragHelper.OnListItemClick = _OnCellClick;
        List_attrs_all.onItemRenderAction = _OnCircuitAttrRender;
    }

    private void _ResetView()
    {
        _SetNewPick(null);
    }

    private void _ResetList()
    {
        _filter.ResetList(HeroCircuitHelper.GetList(_heroInfo));
        _filter.Refresh();
        _RefreshListRelated();
    }

    private void _RefreshListRelated()
    {
        _pager.RefreshList(_filter.List);
        _RefreshCircuitPageView();
        View_pager.RefreshPagerView();
    }

    private void _OnListChanged()
    {
        _RefreshListRelated();
    }

    private void _OnCircuitCellRender(int index, Transform tf)
    {
        var cell = tf.GetComponent<CircuitPickableListCell>();
        var circuitInfo = _pager.GetItem(index);
        cell.SetInfo(circuitInfo);
        cell.Selected = cell.CircuitInfo == _pickedCircuitInfo;
        _cellMap[circuitInfo] = cell;
        _RefreshCellDragger(cell);
    }
    
    private void _RefreshCircuitPageView()
    {
        _RefreshCircuitPageList();
    }
    
    private void _RefreshCircuitPageList()
    {
        if (null == _cellMap)
        {
            _cellMap ??= new Dictionary<HeroCircuitInfo, CircuitPickableListCell>();
        }
        else
        {
            _cellMap.Clear();
        }
        
        List_circuit.numItems = _pager.DisplayNum;
    }

    private void _OnCellDown(CircuitPickableListCell cell)
    {
        var newPickInfo = cell.CircuitInfo;
        var newPicked = cell.CircuitInfo != _pickedCircuitInfo;
        _rotateFlag = !newPicked;

        if (newPicked)
        {
            _SetNewPick(newPickInfo);
        }
    }

    private void _OnCellClick(CircuitPickableListCell cell)
    {
        if (!_rotateFlag) return;
        
        var circuitInfo = cell.CircuitInfo;
        if (circuitInfo.ItemInfo.IsUsed)
        {
            // 目前不处理
        }
        else if (circuitInfo.Turn())
        {
            cell.RefreshShape();
            Node_circuitInformation.RefreshShape();
        }
    }

    private void _OnCircuitOn(HeroCircuitInfo circuitInfo)
    {
        _RefreshUsedStatus(circuitInfo);
    }
    
    private void _OnCircuitOff(HeroCircuitInfo circuitInfo)
    {
        _RefreshUsedStatus(circuitInfo);
    }

    private void _OnCircuitPicked(HeroCircuitInfo circuitInfo)
    {
        _SetNewPick(circuitInfo);
    }
    
    private void _OnCircuitRotate(HeroCircuitInfo circuitInfo)
    {
        if (null != _pickedCircuitInfo && _cellMap.TryGetValue(_pickedCircuitInfo, out var cell))
        {
            if (null != cell) cell.RefreshShape();
        }
        
        Node_circuitInformation.RefreshShape();
    }

    private void _OnEditingModeChange(bool editing)
    {
        Image_gray.SetActive(editing);
    }

    private void _OnCircuitChange()
    {
        _RefreshAttrs();
        // 有属性改变的，都要取消选中
        _SetNewPick(null);
        // 通知外面拼图变化
        HeroNotifier.Invoke(HeroNotifyEnum.CircuitChange);
    }

    private void _OnCircuitUpdate()
    {
        HeroNotifier.Invoke(HeroNotifyEnum.CircuitUpdate);
    }

    private void _RefreshUsedStatus(HeroCircuitInfo circuitInfo)
    {
        // 先刷新列表
        if (_filter.FilterUnequipped)
        {
            _filter.Refresh();
        }
        
        if (_cellMap.TryGetValue(circuitInfo, out var cell))
        {
            cell.RefreshUsed();
            _RefreshCellDragger(cell);
        }
    }

    private void _RefreshCellDragger(CircuitPickableListCell cell)
    {
        CircuitDragHelper.Activate(cell, !cell.CircuitInfo.ItemInfo.IsUsed);
    }
    
    private void _RefreshAttrs()
    {
        var attrsMap = HeroCircuitManager.GetCircuitAttrMap(_heroInfo.HeroId);
        _attrsList = attrsMap?.Keys.ToList();
        List_attrs_all.numItems = (uint) (_attrsList?.Count ?? 0);
    }

    private void _OnCircuitAttrRender(int index, Transform tf)
    {
        var heroAttrItem = tf.GetComponent<HeroAttrCommonCell>();
        var attr = _attrsList[index];
        var attrsMap = HeroCircuitManager.GetCircuitAttrMap(_heroInfo.HeroId);
        heroAttrItem.SetCommonInfo(attr, attrsMap[attr]);
    }

    private void _SetNewPick(HeroCircuitInfo circuitInfo)
    {
        if (null != _pickedCircuitInfo && _cellMap.TryGetValue(_pickedCircuitInfo, out var prevCell))
        {
            prevCell.Selected = false;
        }
        
        if (null != circuitInfo && _cellMap.TryGetValue(circuitInfo, out var newCell))
        {
            newCell.Selected = true;
        }
        // 设置盘子的选中
        Board_circuit.SetSelect(circuitInfo);
        // 展示信息
        var circuitPicked = null != circuitInfo;
        Switcher_info.Selected = circuitPicked;
        if (null != circuitInfo)
        {
            Node_circuitInformation.SetInfo(circuitInfo);
        }

        _pickedCircuitInfo = circuitInfo;
    }
}
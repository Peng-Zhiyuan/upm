using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 拼图面板
/// </summary>
public class CircuitBoard : MonoBehaviour
{
    public RectTransform container;
    public Transform spotsNode;
    public Transform circuitNode;
    public Transform buttonsNode;
    public RectTransform circuitAnchor;
    public CircuitCellCore circuitPrefab;
    public CircuitCellSpot spotPrefab;
    public CircuitNaFlags circuitNas;
    public CircuitPickComp pickComp;
    public MutexComponent confirmButton;
    public Action<HeroCircuitInfo> onCellOn;
    public Action<HeroCircuitInfo> onCellOff;
    public Action<HeroCircuitInfo> onCellPicked;
    public Action<HeroCircuitInfo> onCircuitRotate;
    public Action<bool> onEditing;
    public Action onCircuitChange; // 有改变（引起了属性改变）
    public Action onCircuitUpdate; // 只是移动或者转动，没有引起属性变化
    
    private int _type;
    private int _starStage;
    private int _lvStage;
    private HeroInfo _heroInfo;
    private BehaviourPool<CircuitCellSpot> _spotPool;
    private BehaviourPool<CircuitCellCore> _circuitPool;
    private CircuitCellCore _editingCircuit;
    private CircuitCellCore _originCircuit;
    private CircuitCellCore _previewCircuit;
    private HeroCircuitInfo _pickedCircuitInfo;
    private AdjustCircuit _adjustingInfo;
    private Pos _movingPos = Pos.Default;
    private List<HeroCircuitInfo> _overlayList; // 重叠列表
    private bool _flagNa;
    private bool _rotatable;

    public void OnButton(string msg)
    {
        switch (msg)
        {
            case "settled":
                _OnSettled();
                break;
            case "cancel":
                _OnCancel();
                break;
        }
    }
    
    public void SetInfo(HeroInfo heroInfo)
    {
        _heroInfo = heroInfo;

        _Reset();
        _SetSpots();
        _PutCircuits();
        _ResetEditingView();
    }

    public void SetSelect(HeroCircuitInfo circuitInfo)
    {
        var matchedCell = null == circuitInfo ? null : _circuitPool?.List?.Find(cell => cell.CircuitInfo == circuitInfo);
        _SetSelect(matchedCell);
    }

    private void Awake()
    {
        _spotPool = new BehaviourPool<CircuitCellSpot>();
        _spotPool.SetParent(spotsNode);
        _spotPool.SetPrefab(spotPrefab);
        _circuitPool = new BehaviourPool<CircuitCellCore>();
        _circuitPool.SetParent(circuitNode);
        _circuitPool.SetPrefab(circuitPrefab);
        CircuitDragHelper.OnCircuitDown = _OnCircuitPicked;
        CircuitDragHelper.OnCircuitClick = _OnCircuitClick;
        CircuitDragHelper.OnMoveStart = _OnDragStart;
        CircuitDragHelper.OnMoving = _OnDragging;
        CircuitDragHelper.OnOff = _OnDragOff;
        CircuitDragHelper.SetCircuitProvider(_CreateCircuit); 

        _overlayList = new List<HeroCircuitInfo>();
        // spotsNode的宽高也是能直接确定的
        container.sizeDelta = new Vector2(
            CircuitCellExt.MapSizeH * CircuitCellExt.UnitLength,
            CircuitCellExt.MapSizeV * CircuitCellExt.UnitLength);
        CircuitEffectHelper.EffectNode = circuitNas.transform;
    }

    private async void OnDestroy()
    {
        if (null != CircuitDragHelper.CurrentDragger)
        {
            var rt = CircuitDragHelper.CurrentDragger.DraggingRt;
            var circuit = rt.GetComponent<CircuitCellCore>();
            ResetCircuit(circuit);
            // 销毁拖动
            CircuitDragHelper.DestroyDragger();
        }
        else if (null != _editingCircuit)
        {
            ResetCircuit(_editingCircuit);
        }
        // 如果还有没有提交完成的内容，也是需要提交掉的
        await CircuitTransactionHelper.Submit();

        void ResetCircuit(CircuitCellCore circuit)
        {
            if (circuit.CircuitInfo.ItemInfo.IsUsed)
            {
                // 然后因为之前TakeAway了， 这里得数据放回去
                HeroCircuitManager.PutOn(_heroInfo.HeroId, circuit.CircuitInfo);
            }
        }
    }

    /// <summary>
    /// 一系列的重置
    /// </summary>
    private void _Reset()
    {
        SetSelect(_pickedCircuitInfo);
    }

    /// <summary>
    /// 布置背景（可放区域和锁住区域）
    /// </summary>
    private void _SetSpots()
    {
        var type = _heroInfo.Job;
        var starStage = _heroInfo.CircuitStarStage;
        var lvStage = _heroInfo.CircuitLevelStage;
        if (type == _type && _starStage == starStage && _lvStage == lvStage) return;
        
        var halfH = CircuitCellExt.MapSizeH / 2;
        var halfV = CircuitCellExt.MapSizeV / 2;
        var arr = StaticData.PuzzleBgTable.TryGet(type).Colls;
        _spotPool.MarkClear();
        foreach (var row in arr)
        {
            var unlocked = false;
            switch ((CircuitUnlockType) row.Type)
            {
                case CircuitUnlockType.T0_Basic:
                // 如果星级小于当前值， 就是开放了
                case CircuitUnlockType.T1_Star when row.Stage <= starStage:
                // 如果等级小于当前值， 就是开放了
                case CircuitUnlockType.T2_Level when row.Stage <= lvStage:
                    unlocked = true;
                    break;
            }

            foreach (var rowNode in row.Dots)
            {
                var x = rowNode.X + halfH;
                var y = rowNode.Y + halfV;
                var spotCell = _spotPool.Get();
                spotCell.SetLocked(!unlocked);
                spotCell.rectTransform().anchoredPosition =
                    new Vector2((x + .5f) * CircuitCellExt.UnitLength, (y + .5f) * CircuitCellExt.UnitLength);
            }
        }
        _spotPool.RecycleLeft();

        _type = type;
        _starStage = starStage;
        _lvStage = lvStage;
    }

    /// <summary>
    /// 放置拼图
    /// </summary>
    private void _PutCircuits()
    {
        // 新的放上去
        var circuitInfos = HeroCircuitManager.GetCircuits(_heroInfo.HeroId);
        _circuitPool.MarkClear();
        foreach (var circuitInfo in circuitInfos)
        {
            // 开始放置实体
            var circuit = _circuitPool.Get();
            // 拖拽支持
            CircuitDragHelper.Register(circuit);
            // 显示拼图块
            circuit.Render(circuitInfo);
            // 放置到正确的位置
            circuit.SetAnchoredPosition(CircuitCellExt.GetPos(circuitInfo));
        }
        _circuitPool.RecycleLeft();
    }

    private void _OnCircuitPicked(CircuitCellCore circuit)
    {
        var circuitInfo = circuit == null ? null : circuit.CircuitInfo;
        var sameCircuit = _rotatable = circuitInfo == _pickedCircuitInfo;
        if (sameCircuit) return;
        
        onCellPicked?.Invoke(circuitInfo);
    }
    
    private void _OnCircuitClick(CircuitCellCore circuit)
    {
        if (_rotatable)
        {
            _DoRotate(circuit);
        }
    }

    private void _OnDragStart(CircuitCellCore circuit)
    {
        var circuitInfo = circuit.CircuitInfo;
        if (CircuitDragHelper.RawDrag)
        {
            // 清掉身上的不能摆放flag
            circuitNas.Clear();
        }
        
        var shape = _editingCircuit == null ? circuitInfo.Shape : _adjustingInfo.shape;
        _RenderMovingPreview(circuitInfo, shape);
        
        // 如果已经装在英雄身上了，那是先从数据层上拿掉的
        if (circuitInfo.ItemInfo.IsUsed)
        {
            HeroCircuitManager.TakeAway(circuitInfo);

            // 如果还没有进入编辑模式， 在该位置留下残影
            if (null == _editingCircuit)
            {
                if (null == _originCircuit)
                {
                    _originCircuit = Instantiate(circuitPrefab, circuitNode);
                    _originCircuit.name = "Circuit_origin";
                    var dragBehaviour = _originCircuit.GetComponent<DragBehaviour>();
                    Destroy(dragBehaviour);
                }

                _originCircuit.Render(circuitInfo);
                _originCircuit.SetAlpha(.2f);
                _originCircuit.SetAnchoredPosition(CircuitCellExt.GetPos(circuitInfo));
                _originCircuit.SetActive(true);
            }
        }
    }

    private void _OnDragging(CircuitCellCore circuit)
    {
        var circuitInfo = circuit.CircuitInfo;
        // 计算位置
        var circuitRt = circuit.GetComponent<RectTransform>();
        circuitAnchor.position = circuitRt.position;
        var coordinate = circuitAnchor.anchoredPosition;
        var shape = _editingCircuit == null ? circuitInfo.Shape : _adjustingInfo.shape;
        var pos = CircuitCellExt.GetCoordinate(shape, coordinate);
        
        // 同一个位置就不继续处理
        if (pos == _movingPos) return;
        _movingPos = pos;
        
        // 出界了就不显示了
        if (HeroCircuitManager.CheckOutside(shape, pos.X, pos.Y))
        {
            _previewCircuit.SetActive(false);
            circuitNas.Clear();
        }
        else
        {
            _previewCircuit.SetActive(true);
            // 放置位置
            _previewCircuit.SetAnchoredPosition(CircuitCellExt.GetPos(shape, pos.X, pos.Y));
            _InternalCheckNa(shape, pos.X, pos.Y);
        }
    }
    
    private async void _OnDragOff(CircuitCellCore circuit)
    {
        var circuitInfo = circuit.CircuitInfo;
        var circuitRt = circuit.rectTransform();
        var coordinate = circuitRt.anchoredPosition;
        var shape = _editingCircuit == null ? circuitInfo.Shape : _adjustingInfo.shape;
        var pos = CircuitCellExt.GetCoordinate(shape, coordinate);
        _ResetPreviewCircuit();
        
        // 如果是出去了， 那就拿掉
        if (HeroCircuitManager.CheckOutside(shape, pos.X, pos.Y))
        {
            if (CircuitDragHelper.RawDrag)
            {
                // 如果原来在盘子上取掉了， 那就要播放音效
                WwiseEventManager.SendEvent(TransformTable.Custom, "ui_equipoff");
            }
            // 回收掉
            _circuitPool.Recycle(circuit);
            // 重置场景
            _ResetEditingView();
            // 取消选中
            pickComp.SetActive(false);
            
            if (circuitInfo.ItemInfo.IsUsed)
            {
                // 更新数据
                var success = await CircuitTransactionHelper.Off(circuitInfo);
                if (!success)
                {
                    // 失败了就放回原位
                    HeroCircuitManager.PutOn(_heroInfo.HeroId, circuitInfo);
                    circuit.SetAnchoredPosition(CircuitCellExt.GetPos(circuitInfo));
                    return;
                }
                // 通知拿掉了
                onCellOff?.Invoke(circuitInfo);
                // 通知属性改变
                onCircuitChange?.Invoke();
            }

            return;
        }

        if (null == _editingCircuit)
        {
            // 如果非编辑模式，且位置没变，就一切复原
            if (CircuitDragHelper.RawDrag && pos == circuitInfo.Coordinate)
            {
                HeroCircuitManager.PutOn(_heroInfo.HeroId, circuitInfo);
                circuit.SetAnchoredPosition(CircuitCellExt.GetPos(circuitInfo));
            }
            else
            {
                _EnterEditing(circuit, pos.X, pos.Y);

                // 如果是外面拖进来的，那么要注册拖拽
                if (!CircuitDragHelper.RawDrag)
                {
                    CircuitDragHelper.Register(circuit);
                }
            }
        }
        else
        {
            _AdjustCircuit(pos.X, pos.Y);
            _CheckNaPlace();
        }
        
        // 如果编辑状态就直接选中显示
        if (null != _editingCircuit)
        {
            _SetSelect(circuit, _adjustingInfo);
            // 播放特效
            // _PlayEffect(circuit, _adjustingInfo);
        }
    }
    
    private void _ResetPreviewCircuit()
    {
        if (null != _previewCircuit)
        {
            _previewCircuit.SetActive(false);
        }
        _movingPos = Pos.Default;
    }
    
    private void _ResetEditingView()
    {
        var prevCircuit = _editingCircuit;
        
        _editingCircuit = null;
        circuitNas.Clear();
        buttonsNode.SetActive(false);
        var circuits = _circuitPool.List;
        if (null != circuits)
        {
            foreach (var circuit in circuits)
            {
                if (circuit == prevCircuit) continue;
            
                CircuitDragHelper.Register(circuit);
                circuit.ResetColor();
            }
        }
        
        // 然后各种影子也要干掉
        if (null != _originCircuit)
        {
            _originCircuit.SetActive(false);
        }
        if (null != _previewCircuit)
        {
            _previewCircuit.SetActive(false);
        }
        
        // 回调通知出去
        onEditing?.Invoke(false);
    }
    
    // 只是先把形状准备好，并不会显示
    private void _RenderMovingPreview(HeroCircuitInfo circuitInfo, int? shape = null)
    {
        if (null == _previewCircuit)
        {
            _previewCircuit = Instantiate(circuitPrefab, circuitNode);
            _previewCircuit.name = "Block_preview";
            _previewCircuit.SetActive(false);
            var dragBehaviour = _previewCircuit.GetComponent<DragBehaviour>();
            Destroy(dragBehaviour);
            _previewCircuit.SetColor((int) BlockColorEnum.Preview);
        }
        
        // 让其放在最上方
        _previewCircuit.transform.SetAsLastSibling();
        _previewCircuit.Render(circuitInfo, shape);
        _previewCircuit.SetAlpha(.2f);
    }

    private void _SetSelect(CircuitCellCore circuit = null, AdjustCircuit adjust = default)
    {
        pickComp.SetActive(null != circuit);

        if (null != circuit)
        {
            // default时候，shape是为0的， 但是如果有赋值的话， 这个值不会为0
            if (adjust.shape == 0)
            {
                pickComp.Render(circuit.CircuitInfo);
            }
            else
            {
                pickComp.Render(adjust.shape, adjust.x, adjust.y);
            }

            _pickedCircuitInfo = circuit.CircuitInfo;
        }
        else
        {
            _pickedCircuitInfo = null;
        }
    }

    private void _ClearSelect()
    {
        _SetSelect();
    }

    private void _PlayEffect(CircuitCellCore circuit, AdjustCircuit adjust = default)
    {
        if (null == circuit) return;

        var shape = adjust.shape == 0 ? circuit.CircuitInfo.Shape : adjust.shape;
        CircuitEffectHelper.PlaySettle(shape, circuit.transform.position);
    }
    
    private void _DoRotate(CircuitCellCore circuit)
    {
        if (null == _editingCircuit)
        {
            HeroCircuitManager.TakeAway(circuit.CircuitInfo);
            _EnterEditing(circuit);
        }
        var p = _adjustingInfo;
        var shapeRow = StaticData.PuzzleShapeTable.TryGet(p.shape);
        var newShape = _adjustingInfo.shape = shapeRow.Shapenext;
        circuit.SetShape(newShape);
        circuit.SetAnchoredPosition(CircuitCellExt.GetPos(newShape, p.x, p.y));
        pickComp.SetShape(newShape, p.x, p.y);
        _CheckNaPlace();
    }
    
    private void _EnterEditing(CircuitCellCore circuit)
    {
        var circuitInfo = circuit.CircuitInfo;
        var coordinate = circuitInfo.Coordinate;
        _EnterEditing(circuit, coordinate.X, coordinate.Y, circuitInfo.Shape);
    }

    private void _EnterEditing(CircuitCellCore circuit, int x, int y)
    {
        var shape = circuit.CircuitInfo.Shape;
        _EnterEditing(circuit, x, y, shape);
    }

    private void _EnterEditing(CircuitCellCore circuit, int x, int y, int shape)
    {
        _editingCircuit = circuit;
        circuit.SetActive(true);
        buttonsNode.SetActive(true);
        
        _adjustingInfo = new AdjustCircuit { shape = shape };
        _AdjustCircuit(x, y);
        // 其他积木都先变成不可操作状态
        var circuits = _circuitPool.List;
        foreach (var tmpCircuit in circuits)
        {
            if (circuit == tmpCircuit) continue;
            
            var dragBehaviour = tmpCircuit.GetComponent<DragBehaviour>();
            if (null != dragBehaviour)
            {
                dragBehaviour.Clickable = false;
                dragBehaviour.Dragable = false;
            }

            tmpCircuit.SetGray();
        }
        // 检查不合法区域
        _CheckNaPlace();
        // 回调通知出去
        onEditing?.Invoke(true);
    }
    
    private void _AdjustCircuit(int x, int y)
    {
        var circuit = _editingCircuit;
        _adjustingInfo.x = x;
        _adjustingInfo.y = y;
        // 这块拼图直接放好
        circuit.SetAnchoredPosition(CircuitCellExt.GetPos(_adjustingInfo.shape, x, y));
        // 播放音效
        WwiseEventManager.SendEvent(TransformTable.Custom, "ui_equipon");
    }
    
    // 检查不合法位置
    private void _CheckNaPlace()
    {
        var p = _adjustingInfo;
        _flagNa = _InternalCheckNa(p.shape, p.x, p.y, true);
        confirmButton.Selected = !_flagNa;
    }
    
    private bool _InternalCheckNa(int shape, int posX, int posY, bool recordOverlay = false)
    {
        if (recordOverlay) _overlayList.Clear();
        // 先清掉原来的
        circuitNas.Clear();
        // 重新计算新的非法位置
        var occupied = HeroCircuitManager.GetOccupied(_heroInfo.HeroId);
        var na = false; // means "not available"
        var shapeRow = StaticData.PuzzleShapeTable.TryGet(shape);
        foreach (var node in shapeRow.Dots)
        {
            var x = posX + node.X;
            var y = posY + node.Y;
            
            if (!HeroCircuitManager.CoordinateAvailable(_heroInfo.Job, _heroInfo.CircuitStarStage, _heroInfo.CircuitLevelStage, x, y))
            {
                // 先检查该坐标点是否开放了
                circuitNas.SetNa(x, y);
                na = true;
            }
            else if (occupied.Contains($"{x}_{y}"))
            {
                // 再检查该坐标点是否重叠
                circuitNas.SetOverlay(x, y);

                // 加到overlay列表中去
                var circuitInfo = HeroCircuitManager.FindCircuit(_heroInfo.HeroId, x, y);
                if (recordOverlay && !_overlayList.Contains(circuitInfo))
                {
                    _overlayList.Add(circuitInfo);
                }
            }
        }
        
        return na;
    }

    private async void _OnSettled()
    {
        if (_flagNa)
        {
            ToastManager.ShowLocalize("M4_circuit_words_place_notavailable");
            return;
        }

        var hasOverlay = _overlayList.Count > 0;
        if (hasOverlay)
        {
            if (!await Dialog.AskAsync("", "M4_circuit_confirm_overlay".Localize())) return;

            var circuits = _circuitPool.List;
            foreach (var overlayCircuitInfo in _overlayList)
            {
                var overlayCircuit = circuits.Find(cell => cell.CircuitInfo == overlayCircuitInfo);
                // 可以拖动
                CircuitDragHelper.Register(overlayCircuit);
                // 回收掉
                _circuitPool.Recycle(overlayCircuit);
                // 数据也拿掉
                HeroCircuitManager.TakeAway(overlayCircuitInfo);
                // 提交数据流
                await CircuitTransactionHelper.Off(overlayCircuitInfo, false);
                // 这个也拿掉了
                onCellOff?.Invoke(overlayCircuitInfo);
            }
            _overlayList.Clear();
        }
        
        var circuit = _editingCircuit;
        var circuitInfo = circuit.CircuitInfo;
        var newAdded = !circuitInfo.ItemInfo.IsUsed;
        var modify = _adjustingInfo;
        var shapeChanged = modify.shape != circuitInfo.Shape;
        var success = await CircuitTransactionHelper.PutOn(circuitInfo, modify.x, modify.y, modify.shape, _heroInfo.InstanceId);
        if (!success)
        {
            ToastManager.Show("Unknown error occurs");
            // 取消掉操作
            _ResetCircuit(circuit);
            return;
        }
        
        _ResetEditingView();
        // 数据放上去
        HeroCircuitManager.PutOn(_heroInfo.HeroId, circuitInfo);
        // 播放效果
        _PlayEffect(circuit);
        if (newAdded)
        {
            onCellOn?.Invoke(circuitInfo);
        }

        if (newAdded || hasOverlay)
        {
            onCircuitChange?.Invoke();
        }
        else
        {
            onCircuitUpdate?.Invoke();
        }

        if (shapeChanged)
        {
            onCircuitRotate?.Invoke(circuitInfo);
        }
    }
    
    private void _OnCancel()
    {
        _ResetCircuit(_editingCircuit);
    }

    private void _ResetCircuit(CircuitCellCore circuit)
    {
        var circuitInfo = circuit.CircuitInfo;
        _ResetEditingView();
        
        if (circuitInfo.ItemInfo.IsUsed)
        {
            // 然后因为之前TakeAway了， 这里得数据放回去
            HeroCircuitManager.PutOn(_heroInfo.HeroId, circuitInfo);
            // 然后初始影子也要干掉
            if (null != _originCircuit) _originCircuit.SetActive(false);
            // 然后把拼图放回原来的位置
            var p = _adjustingInfo;
            var coordinate = circuitInfo.Coordinate;
            if (circuitInfo.Shape == p.shape && coordinate.X == p.x && coordinate.Y == p.y)
                return;
            
            if (circuitInfo.Shape != p.shape)
            {
                circuit.SetShape(circuitInfo.Shape);
            }
            circuit.SetAnchoredPosition(CircuitCellExt.GetPos(circuitInfo));
            // 设置选中
            _SetSelect(circuit);
        }
        else
        {
            _circuitPool.Recycle(circuit);
            // 否则就干掉选中
            _ClearSelect();
        }
    }

    private CircuitCellCore _CreateCircuit()
    {
        return _circuitPool.Get();
    }
}

internal struct AdjustCircuit
{
    public int x;
    public int y;
    public int shape;
}

internal enum BlockColorEnum
{
    Preview = 5,
}
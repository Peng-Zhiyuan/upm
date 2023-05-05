using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class CircuitDragHelper
{
    public delegate CircuitCellCore CellProvide();
    
    public static Action<CircuitPickableListCell> OnListItemDown;
    public static Action<CircuitCellCore> OnCircuitDown;
    public static Action<CircuitPickableListCell> OnListItemClick;
    public static Action<CircuitCellCore> OnCircuitClick;
    public static Action<CircuitCellCore> OnMoveStart;
    public static Action<CircuitCellCore> OnMoving;
    public static Action<CircuitCellCore> OnOff;

    private static CellProvide _provider;

    // 如果是直接拖的CircuitCellCore， 此为真
    public static bool RawDrag { get; private set; }
    public static DragBehaviour CurrentDragger { get; private set; }

    public static void SetCircuitProvider(CellProvide provider)
    {
        _provider = provider;
    }

    public static void UnRegister(MonoBehaviour circuit)
    {
        var dragger = circuit.GetComponent<DragBehaviour>();
        if (null != dragger)
        {
            Object.Destroy(dragger);
        }
    }
    
    public static void Register(MonoBehaviour circuit)
    {
        // 放好的block也可以改变位置
        var dragger = circuit.GetComponent<DragBehaviour>();
        if (null == dragger)
        {
            dragger = circuit.gameObject.AddComponent<DragBehaviour>();
            dragger.onDown = _OnTouch;
            dragger.onClick = _OnClick;
            dragger.onDragStart = _OnTouchMove;
            dragger.onDragging = _OnTouchMoving;
            dragger.onDragEnd = _OnTouchOff;
        }
        else
        {
            dragger.Clickable = true;
            dragger.Dragable = true;
        }
    }

    public static void Activate(MonoBehaviour circuit, bool draggable)
    {
        Register(circuit);
        
        // 设置是否可拖动
        var dragger = circuit.GetComponent<DragBehaviour>();
        dragger.Dragable = draggable;
    }

    public static void DestroyDragger()
    {
        if (CurrentDragger == null) return;
        
        Object.Destroy(CurrentDragger.DraggingRt.gameObject);
        Object.Destroy(CurrentDragger);
    }

    private static void _OnClick(DragBehaviour dragger)
    {
        var listCell = dragger.GetComponent<CircuitPickableListCell>();

        if (null != listCell)
        {
            OnListItemClick?.Invoke(listCell);
        }
        else
        {
            OnCircuitClick?.Invoke(dragger.GetComponent<CircuitCellCore>());
        }
    }

    private static void _OnTouch(DragBehaviour dragger)
    {
        var listCell = dragger.GetComponent<CircuitPickableListCell>();

        if (null != listCell)
        {
            OnListItemDown?.Invoke(listCell);
        }
        else
        {
            OnCircuitDown?.Invoke(dragger.GetComponent<CircuitCellCore>());
        }
    }
    
    private static void _OnTouchMove(DragBehaviour dragger)
    {
        var listCell = dragger.GetComponent<CircuitPickableListCell>();
        RawDrag = listCell == null;
        CircuitCellCore circuit;
        if (!RawDrag)
        {
            circuit = _provider();
            dragger.DraggingRt = circuit.rectTransform();
            circuit.Render(listCell.CircuitInfo);
        }
        else
        {
            circuit = dragger.GetComponent<CircuitCellCore>();
        }

        CurrentDragger = dragger;
        OnMoveStart?.Invoke(circuit);
    }
    
    private static void _OnTouchMoving(DragBehaviour dragger)
    {
        var circuit = dragger.DraggingRt.GetComponent<CircuitCellCore>();
        OnMoving?.Invoke(circuit);
    }
    
    private static void _OnTouchOff(DragBehaviour dragger)
    {
        CurrentDragger = null;
        
        var circuit = dragger.DraggingRt.GetComponent<CircuitCellCore>();
        OnOff?.Invoke(circuit);
    }
}
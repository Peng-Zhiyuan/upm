using UnityEngine;
using System.Collections;
using System;

public class UniversalEventRaycastTarget : MonoBehaviour
{
    public Action<Touch> TouchDown;
    public Action<Touch> TouchUp;

    public Action<Touch> DragStart; 
    public Action<Touch> Drag;
    public Action<Touch> DragEnd;

    public Action<Touch, UniversalEventRaycastTarget> Drop;
    public Action<Touch, UniversalEventRaycastTarget> Hover;

    public void OnTouchDown(Touch touch)
    {
        this.TouchDown?.Invoke(touch);
    }

    public void OnTouchUp(Touch touch)
    {
        this.TouchUp?.Invoke(touch);
    }

    public void OnDrag(Touch touch)
    {
        this.Drag?.Invoke(touch);
    }

    public void OnDragStart(Touch touch)
    {
        this.DragStart?.Invoke(touch);
    }

    public void OnDragEnd(Touch touch)
    {
        this.DragEnd?.Invoke(touch);
    }

    public void OnHover(Touch touch, UniversalEventRaycastTarget hoverTarget)
    {
        this.Hover?.Invoke(touch, hoverTarget);
    }

    public void OnDrop(Touch touch, UniversalEventRaycastTarget dropTarget)
    {
        this.Drop?.Invoke(touch, dropTarget);
    }
}

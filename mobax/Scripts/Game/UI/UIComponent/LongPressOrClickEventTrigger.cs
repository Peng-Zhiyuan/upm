//using LuaInterface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
public class LongPressOrClickEventTrigger : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler,IDragHandler
{
    public float durationThreshold = 1.0f;
    public UnityEvent onLongPress = new UnityEvent();
    public UnityEvent onClickDown = new UnityEvent();
    public OnUpHandler onClickUp = new OnUpHandler();
    public UnityEvent onClick = new UnityEvent();
    private bool isPointerDown = false;
    private bool longPressTriggered = false;
    private float timePressStarted;

    public UnityEvent LongPress
    {
        get { return onLongPress; }
        set { onLongPress = value; }
    }

    public UnityEvent ClickDown
    {
        get { return onClickDown; }
        set { onClickDown = value; }
    }

    public class OnUpHandler : UnityEvent<GameObject> { }
    public OnUpHandler ClickUp
    {
        get { return onClickUp; }
        set { onClickUp = value; }
    }
    private void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;

        onClickUp.Invoke(eventData.pointerCurrentRaycast.gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerDown = false;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!longPressTriggered)
        {
            onClick.Invoke();
        }
    }
    public void AddClickDown(Action<GameObject> cb)
    {
        if (cb == null) return;
        onClickDown.RemoveAllListeners();
        onClickDown.AddListener(
            () =>
            {
                cb?.Invoke(this.gameObject);
            }
        );
    }

    public void AddClickUp(Action<GameObject, GameObject> cb)
    {
        if (cb == null) return;
        onClickUp.RemoveAllListeners();
        onClickUp.AddListener(
            (GameObject go) =>
            {
                cb?.Invoke(this.gameObject, go);
            }
        );
    }

    public void AddLongClickDown(Action<GameObject> cb)
    {
        if (cb == null) return;
        LongPress.RemoveAllListeners();
        LongPress.AddListener(
            delegate ()
            {
                cb?.Invoke(this.gameObject);
            }
        );
    }
    
    public class OnDragEventHandler : UnityEvent<Vector2, GameObject> { }
    public OnDragEventHandler onDragEvent = new OnDragEventHandler();
    public OnDragEventHandler DragEvent
    {
        get { return onDragEvent; }
        set { onDragEvent = value; }
    }
    public void OnDrag(PointerEventData eventData)
    {
        //if (!actived) return;
        //Vector3 globalMousePos = eventData.pressEventCamera.ScreenToViewportPoint(eventData.position);
        //onDrag.Invoke(globalMousePos);
        onDragEvent.Invoke(eventData.position, eventData.pointerCurrentRaycast.gameObject);
    }


}
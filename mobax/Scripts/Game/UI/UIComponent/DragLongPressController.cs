using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
//using LuaInterface;

using System;

public class DragLongPressController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public float durationThreshold = 1.0f;
    public UnityEvent onLongPress = new UnityEvent();
    private bool longPressTriggered = false;
    private float timePressStarted;
    private bool isPointerDown = false;
    
    public class OnDownHandler : UnityEvent<Vector2> { }
    public OnDownHandler onDownClick = new OnDownHandler();
    public OnDownHandler DownClick
    {
        get { return onDownClick; }
        set { onDownClick = value; }
    }
    public class OnUpHandler : UnityEvent<GameObject, Vector2> { }
    private OnUpHandler onUpClick = new OnUpHandler();
    public OnUpHandler UpClick
    {
        get{ return onUpClick; }
        set{ onUpClick = value;}
    }

    private float lastTime = 0;
    public class OnDoubleHandler : UnityEvent { }
    private OnDoubleHandler onDoubleClick = new OnDoubleHandler();
    public OnDoubleHandler DoubleClick
    {
        get{ return onDoubleClick; }
        set{ onDoubleClick = value;}
    }
    
    public class OnDragHandler : UnityEvent<Vector2> { }
    public OnDragHandler onDrag = new OnDragHandler();
    public OnDragHandler Drag
    {
        get { return onDrag; }
        set { onDrag = value; }
    }

    public class OnDragEventHandler : UnityEvent<Vector2, GameObject> { }
    public OnDragEventHandler onDragEvent = new OnDragEventHandler();
    public OnDragEventHandler DragEvent
    {
        get { return onDragEvent; }
        set { onDragEvent = value; }
    }

    public class OnBeginDragHandler : UnityEvent<Vector2> { }
    public OnBeginDragHandler onBeginDragEvent = new OnBeginDragHandler();
    public OnBeginDragHandler BeginDragEvent
    {
        get { return onBeginDragEvent; }
        set { onBeginDragEvent = value; }
    }

public class OnEndDragHandler : UnityEvent<Vector2> { }
    public OnEndDragHandler onEndDragEvent = new OnEndDragHandler();
    public OnEndDragHandler EndDragEvent
    {
        get { return onEndDragEvent; }
        set { onEndDragEvent = value; }
    }

    public class OnEnlargeHandler : UnityEvent<float> { }
    private OnEnlargeHandler onEnlargeEvent = new OnEnlargeHandler();
    public OnEnlargeHandler EnlargeEvent
    {
        get { return onEnlargeEvent; }
        set { onEnlargeEvent = value; }
    }


    
    public bool actived = true;
    
    void Start()
    {
        Input.multiTouchEnabled = true;//开启多点触碰
    }

    void Update()
    {
        if (isPointerDown && !longPressTriggered)
        {
            if (Time.time - timePressStarted > durationThreshold)
            {
                longPressTriggered = true;
                onLongPress.Invoke();
                
                //Debug.LogError("长安了");
            }
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (!actived) return;
        //Vector3 globalMousePos = eventData.pressEventCamera.ScreenToViewportPoint(eventData.position);
        //onDrag.Invoke(globalMousePos);
		onDragEvent.Invoke(eventData.position, eventData.pointerCurrentRaycast.gameObject);
        //Debug.LogError("拖动");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!actived) return;
        //Debug.LogError("开始拖动");
        onBeginDragEvent.Invoke(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDragEvent.Invoke(eventData.position);
        //Debug.Log("---------------------OnEndDrag pos = " + eventData.position);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!actived) return;
        if (Time.realtimeSinceStartup - lastTime < 0.3f)
        {
            onDoubleClick.Invoke();
            return;
        }

        lastTime = Time.realtimeSinceStartup;
        
        onDownClick.Invoke(eventData.position);
        
        timePressStarted = Time.time;
        isPointerDown = true;
        longPressTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        
        if (!actived) return;
        onUpClick.Invoke(eventData.pointerCurrentRaycast.gameObject, eventData.position);

    }

    public void AddDoubleClick(Action<GameObject> cb)
    {
        if (cb == null) return;
        
        DoubleClick.RemoveAllListeners();
        DoubleClick.AddListener(
            () =>
            {
                cb?.Invoke(this.gameObject);
            }
        );
    }
    
    public void AddClickDown(Action<GameObject, Vector2> cb)
    {
        if (cb == null) return;
        
        DownClick.RemoveAllListeners();
        DownClick.AddListener(
            (Vector2 pos) =>
            {
                cb?.Invoke(this.gameObject, pos);
            }
        );
    }
    public void AddClickUp(Action<GameObject, GameObject, Vector2> cb)
    {
        if (cb == null) return;
        UpClick.RemoveAllListeners();
        UpClick.AddListener(
            (GameObject go, Vector2 pos) =>
            {
                cb?.Invoke(this.gameObject, go, pos);
            }
        );
    }
    public void AddDragEvent(Action<GameObject, Vector2, GameObject> cb)
    {
        if (cb == null) return;
        DragEvent.RemoveAllListeners();
        DragEvent.AddListener(
            (Vector2 pos, GameObject go) =>
            {
                cb?.Invoke(this.gameObject, pos, go);
            }
        );
    }

    public void AddBeginDragEvent(Action<GameObject, Vector2> cb)
    {
        if (cb == null) return;
        BeginDragEvent.RemoveAllListeners();
        BeginDragEvent.AddListener(
            (Vector2 pos) =>
            {
                cb?.Invoke(this.gameObject, pos);
            }
        );
    }
    
    public UnityEvent LongPress
    {
        get { return onLongPress; }
        set { onLongPress = value; }
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
}

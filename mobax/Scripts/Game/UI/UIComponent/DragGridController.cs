using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
//using LuaInterface;

using System;
public class DragGridController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public class OnDownHandler : UnityEvent<Vector2> { }
    public OnDownHandler onDownClick = new OnDownHandler();
    public OnDownHandler DownClick
    {
        get { return onDownClick; }
        set { onDownClick = value; }
    }
    public class OnUpHandler : UnityEvent<GameObject> { }
    private OnUpHandler onUpClick = new OnUpHandler();
    public OnUpHandler UpClick
    {
        get{ return onUpClick; }
        set{ onUpClick = value;}
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

    //private void Awake()
    //{            
    //}

    void Start()
    {
        Input.multiTouchEnabled = true;//开启多点触碰
    }

    //记录两个手指的旧位置
    private Vector2 oposition1 = new Vector2();
    private Vector2 oposition2 = new Vector2();

    Vector2 m_screenPos = new Vector2(); //记录手指触碰的位置

    private float isforward = 0;

    //用于判断是否放大
    bool isEnlarge(Vector2 oP1, Vector2 oP2, Vector2 nP1, Vector2 nP2)
    {
        //函数传入上一次触摸两点的位置与本次触摸两点的位置计算出用户的手势
        float leng1 = Mathf.Sqrt((oP1.x - oP2.x) * (oP1.x - oP2.x) + (oP1.y - oP2.y) * (oP1.y - oP2.y));
        float leng2 = Mathf.Sqrt((nP1.x - nP2.x) * (nP1.x - nP2.x) + (nP1.y - nP2.y) * (nP1.y - nP2.y));
        if (leng1 < leng2)
        {
            //放大手势
            return true;
        }
        else
        {
            //缩小手势
            return false;
        }
    }
    //这个变量用来记录单指双指的变换
    private bool m_IsSingleFinger;

    //记录上一次手机触摸位置判断用户是在左放大还是缩小手势
    private Vector2 oldPosition1;
    private Vector2 oldPosition2;


    void Update()
    {
#if UNITY_STANDALONE_WIN
        var val = Input.GetAxis("Mouse ScrollWheel");
        if(val != 0)
        {
            onEnlargeEvent.Invoke(val);
            return;
        }
#endif

        //if (Input.touchCount < 2)
        //    return;

        //if (Input.touchCount > 1)//多点触碰
        //{
        //    //记录两个手指的位置
        //    Vector2 nposition1 = new Vector2();
        //    Vector2 nposition2 = new Vector2();

        //    //记录手指的每帧移动距离
        //    Vector2 deltaDis1 = new Vector2();
        //    Vector2 deltaDis2 = new Vector2();

        //    for (int i = 0; i < 2; i++)
        //    {
        //        Touch touch = Input.touches[i];
        //        if (touch.phase == TouchPhase.Ended)
        //            break;
        //        if (touch.phase == TouchPhase.Moved) //手指在移动
        //        {

        //            if (i == 0)
        //            {
        //                nposition1 = touch.position;
        //                deltaDis1 = touch.deltaPosition;
        //            }
        //            else
        //            {
        //                nposition2 = touch.position;
        //                deltaDis2 = touch.deltaPosition;

        //                if (isEnlarge(oposition1, oposition2, nposition1, nposition2)) 
        //                    isforward = 1;
        //                else
        //                    isforward = -1;
        //            }
        //            //记录旧的触摸位置
        //            oposition1 = nposition1;
        //            oposition2 = nposition2;
        //        }
        //        onEnlargeEvent.Invoke(isforward);
        //    }
        //}

        //判断触摸数量为单点触摸
        if (Input.touchCount == 1)
        {
            m_IsSingleFinger = true;

        }
        else if (Input.touchCount > 1)
        {
            //当从单指触摸进入多指触摸的时候,记录一下触摸的位置
            //保证计算缩放都是从两指手指触碰开始的
            if (m_IsSingleFinger)
            {
                oldPosition1 = Input.GetTouch(0).position;
                oldPosition2 = Input.GetTouch(1).position;
            }
            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                ScaleCamera();
            }
            m_IsSingleFinger = false;
        }
    }

    /// <summary>
    /// 触摸缩放摄像头
    /// </summary>
    private void ScaleCamera()
    {
        //计算出当前两点触摸点的位置
        var tempPosition1 = Input.GetTouch(0).position;
        var tempPosition2 = Input.GetTouch(1).position;


        float currentTouchDistance = Vector3.Distance(tempPosition1, tempPosition2);
        float lastTouchDistance = Vector3.Distance(oldPosition1, oldPosition2);


        onEnlargeEvent.Invoke(currentTouchDistance - lastTouchDistance);

        //备份上一次触摸点的位置，用于对比
        oldPosition1 = tempPosition1;
        oldPosition2 = tempPosition2;
    }
 
    public void OnDrag(PointerEventData eventData)
    {
        if (!actived) return;
        //Vector3 globalMousePos = eventData.pressEventCamera.ScreenToViewportPoint(eventData.position);
        //onDrag.Invoke(globalMousePos);
		onDragEvent.Invoke(eventData.position, eventData.pointerCurrentRaycast.gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!actived) return;
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
        onDownClick.Invoke(eventData.position);

        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!actived) return;
        onUpClick.Invoke(eventData.pointerCurrentRaycast.gameObject);
    }

    public void AddClickDown(Action<GameObject, Vector2> cb)
    {
        if (cb == null) return;
        DownClick.RemoveAllListeners();
        DownClick.AddListener(
            delegate (Vector2 pos)
            {
                //luafunc.Call(this.gameObject, pos);
                cb?.Invoke(this.gameObject, pos);
                //SoundManager.Instance.PlayButtonClick();
            }
        );
    }
    public void AddClickUp(Action<GameObject, GameObject> cb)
    {
        if (cb == null) return;
        UpClick.RemoveAllListeners();
        UpClick.AddListener(
            delegate (GameObject go)
            {
                //luafunc.Call(this.gameObject, go);
                cb?.Invoke(this.gameObject, go);
            }
        );
    }

    public Action<GameObject, Vector2, GameObject> cb;
    public void AddDragEvent(Action<GameObject, Vector2, GameObject> cb)
    {
        //if (luafunc == null) return;
        DragEvent.RemoveAllListeners();
        DragEvent.AddListener(
            delegate (Vector2 pos, GameObject go)
            {
                //luafunc.Call(this.gameObject, pos, go);
                cb?.Invoke(this.gameObject, pos, go);
            }
        );
    }

    public void AddBeginDragEvent(Action<GameObject, Vector2> cb)
    {
        if (cb == null) return;
        BeginDragEvent.RemoveAllListeners();
        BeginDragEvent.AddListener(
            delegate (Vector2 pos)
            {
                //luafunc.Call(this.gameObject, pos);
                cb?.Invoke(this.gameObject, pos);
            }
        );
    }

    public void AddEnlargeEvent(Action<float> cb)
    {
        if (cb == null) return;
        EnlargeEvent.RemoveAllListeners();
        EnlargeEvent.AddListener(
            delegate (float forward)
            {
                //luafunc.Call(forward);
                cb?.Invoke(forward);
            }
        );
    }
}

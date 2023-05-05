using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.EventSystems;
using System;
public class PressComponent : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IUpdatable
{
    private Vector3 pos;                            //控件初始位置
    private Vector2 mousePos;                       //鼠标初始位置（画布空间）
    private Vector3 mouseWorldPos;                  //鼠标初始位置（世界空间）
    private RectTransform canvasRec;
    private float maxClickTick = 0.2f;
    //控件所在画布
    //private Vector3 startPos = Vector3.zero;
    private float startTime = 0;
    //public bool hasClickEvent = false;
    private bool pressing = false;
    private bool pointerDown = false;
    public bool pressable = true;
    public bool clickable = true;
    public Action<Vector3> onClick = null;
    public Action<Vector3> onPressStart = null;
    public Action<Vector3> onPressUp = null;
    public Action<Vector3> onClickDown = null;
    public Action<Vector3> onClickUp = null;
    //private void Start()
    //{
    //    canvasRec = this.GetComponentInParent<Canvas>().transform as RectTransform;
    //}



    //开始拖拽
    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.LogError("OnPointerDown");
        if (canvasRec == null)
        {
            canvasRec = this.GetComponentInParent<Canvas>().transform as RectTransform;
        }
        //控件所在画布空间的初始位置
        Camera camera = eventData.pressEventCamera;
        //canvasRec.worldToLocalMatrix(this.GetComponent<RectTransform>().position);

        ////将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out mousePos);
        var screenPoint = RectTransformUtility.WorldToScreenPoint(camera,this.GetComponent<RectTransform>().position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, screenPoint, camera, out mousePos);
        pos = mousePos;
        //startPos = mousePos;
        startTime = Time.time;
        pressing = false;
        this.pointerDown = true;
        if (onClickDown != null) this.onClickDown(pos);
        UpdateManager.Stuff.Add(this);
    }

    private void PressStart()
    {
        //pos = this.GetComponent<RectTransform>().anchoredPosition;
        //Camera camera = eventData.pressEventCamera;
        ////将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out mousePos);
        if (onPressStart != null) this.onPressStart(pos);//EventManager.Instance.SendEvent<Vector3>("OnPressStart", pos);
    }

    // private void Drag(PointerEventData eventData)
    // {
    //     Vector2 newVec = new Vector2();
    //     Camera camera = eventData.pressEventCamera;
    //     RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out newVec);
    //     //鼠标移动在画布空间的位置增量
    //     Vector3 offset = new Vector3(newVec.x - mousePos.x, newVec.y - mousePos.y, 0);

    //     //原始位置增加位置增量即为现在位置
    //     Vector3 taretPos = pos + offset;
    //     (this.transform as RectTransform).anchoredPosition = taretPos;
    //     EventManager.Instance.SendEvent<Vector3>("OnDrag", taretPos);
    // }

    private void PressUp(PointerEventData eventData)
    {
       // Debug.LogError("PressUp");
        Vector2 newVec = new Vector2();
        Camera camera = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out newVec);
        //鼠标移动在画布空间的位置增量
        Vector3 offset = new Vector3(newVec.x - mousePos.x, newVec.y - mousePos.y, 0);

        //原始位置增加位置增量即为现在位置
        Vector3 taretPos = pos + offset;
      
        if (onPressUp != null) this.onPressUp(pos);
     
        //EventManager.Instance.SendEvent<Vector3>("OnPressUp", taretPos);
    }

    private void ClickEvent(PointerEventData eventData)
    {
        //if (!clickable) return;
        Vector2 newVec = new Vector2();
        Camera camera = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out newVec);
        //鼠标移动在画布空间的位置增量
        Vector3 offset = new Vector3(newVec.x - mousePos.x, newVec.y - mousePos.y, 0);

        //原始位置增加位置增量即为现在位置
        Vector3 taretPos = pos + offset;
       // Debug.LogError("pos:"+ pos.ToDetailString());
        if (onClick != null) this.onClick(pos);
        //EventManager.Instance.SendEvent<Vector3>("OnPressClick", taretPos);
    }

    //拖拽过程中
    public void OnUpdate()
    {
        if (!pressable || !pointerDown) return;
        if (!pressing)
        {
            //Camera camera = eventData.pressEventCamera;
            ////将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out mousePos);
            var deltaTime = Time.time - startTime;
            if (deltaTime > 0.3f * Time.timeScale)// || Vector3.Distance(startPos, mousePos) > 5)
            {
                PressStart();
                pressing = true;
            }
        }
    }

    private void OnEnable()
    {
       
        UpdateManager.Stuff.Remove(this);
    }

    private void OnDisable()
    {
        UpdateManager.Stuff.Remove(this);
    }

    //结束拖拽（此处没做任何处理，可自行拓展）
    public void OnPointerUp(PointerEventData eventData)
    {
        UpdateManager.Stuff.Remove(this);
        this.pointerDown = false;
        if (onClickUp != null) this.onClickUp(pos);
        if (pressing)
        {
            pressing = false;
            PressUp(eventData);
        }
        else
        {
            float tick = Time.time - startTime;
            if (tick < maxClickTick && clickable)
            {
                //Debug.LogError("Click");
                ClickEvent(eventData);
            }
        }
    }    
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.EventSystems;
using System;
public class DragComponent : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    private Vector3 pos;                            //控件初始位置
    private Vector2 mousePos;                       //鼠标初始位置（画布空间）
    private Vector3 mouseWorldPos;                  //鼠标初始位置（世界空间）
    private RectTransform canvasRec;                //控件所在画布
    private Vector3 startPos = Vector3.zero;
    private float startTime = 0;
    //public bool hasClickEvent = false;
    private bool dragging = false;
    public bool dragable = true;
    public bool clickable = true;
    //private void Start()
    //{
    //    var canvas = this.GetComponentInParent<Canvas>();
    //    if (canvas != null)
    //    {
    //        canvasRec = canvas.transform as RectTransform;
    //    }
        
    //}

    //开始拖拽
    public void OnPointerDown(PointerEventData eventData)
    {
        if (canvasRec == null)
        {
            canvasRec = this.GetComponentInParent<Canvas>().transform as RectTransform;
        }
       
        //控件所在画布空间的初始位置
        Camera camera = eventData.pressEventCamera;
        //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out mousePos);
        pos = this.GetComponent<RectTransform>().anchoredPosition;
        startPos = mousePos;
        startTime = Time.time;
        dragging = false;

    }

    private void BeginDrag(PointerEventData eventData)
    {
        pos = this.GetComponent<RectTransform>().anchoredPosition;
        Camera camera = eventData.pressEventCamera;
        //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out mousePos);
        EventManager.Instance.SendEvent<Vector3>("OnBeginDrag", pos);
    }

    private void Drag(PointerEventData eventData)
    {
        Vector2 newVec = new Vector2();
        Camera camera = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out newVec);
        //鼠标移动在画布空间的位置增量
        Vector3 offset = new Vector3(newVec.x - mousePos.x, newVec.y - mousePos.y, 0);

        //原始位置增加位置增量即为现在位置
        Vector3 taretPos = pos + offset;
        (this.transform as RectTransform).anchoredPosition = taretPos;
        EventManager.Instance.SendEvent<Vector3>("OnDrag", taretPos);
    }

    private void EndDrag(PointerEventData eventData)
    {
        Vector2 newVec = new Vector2();
        Camera camera = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out newVec);
        //鼠标移动在画布空间的位置增量
        Vector3 offset = new Vector3(newVec.x - mousePos.x, newVec.y - mousePos.y, 0);

        //原始位置增加位置增量即为现在位置
        Vector3 taretPos = pos + offset;
        EventManager.Instance.SendEvent<Vector3>("OnEndDrag", taretPos);
    }

    private void ClickEvent(PointerEventData eventData)
    {
        if (!clickable) return;
        Vector2 newVec = new Vector2();
        Camera camera = eventData.pressEventCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out newVec);
        //鼠标移动在画布空间的位置增量
        Vector3 offset = new Vector3(newVec.x - mousePos.x, newVec.y - mousePos.y, 0);

        //原始位置增加位置增量即为现在位置
        Vector3 taretPos = pos + offset;
        EventManager.Instance.SendEvent<Vector3>("OnDragClick", taretPos);
    }


    //拖拽过程中
    public void OnDrag(PointerEventData eventData)
    {
        if (!dragable) return;
        if (dragging)
        {
            Drag(eventData);
        }
        else
        {
            Camera camera = eventData.pressEventCamera;
            //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRec, eventData.position, camera, out mousePos);
            var deltaTime = Time.time - startTime;
            if (deltaTime > 0.5f || Vector3.Distance(startPos, mousePos) > 5)
            {
                //Debug.LogError("BeginDrag");
                BeginDrag(eventData);
                dragging = true;
            }
        }

    }
    //结束拖拽（此处没做任何处理，可自行拓展）
    public  void OnPointerUp(PointerEventData eventData)
    {
        if (dragging)
        {
            dragging = false;
            EndDrag(eventData);
        }
        else
        {
            ClickEvent(eventData);
        }
    }    
}
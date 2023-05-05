using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ClickBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<ClickBehaviour> onDown; // 触摸（或者MouseDown）的时候响应
        public Action<ClickBehaviour> onClick;

        private Vector2 _pos; //控件初始位置
        private Vector2 _startPos; // 鼠标初始位置
        private float _startTime;
        private RectTransform _canvasRt; //控件所在画布
        private bool _dragging;
        private PointerEventData _lastPointerEventData;

        private RectTransform CanvasRt
        {
            get
            {
                if (null == _canvasRt)
                {
                    _canvasRt = GetComponentInParent<Canvas>().transform as RectTransform;
                }

                return _canvasRt;
            }
        }
        
        //开始拖拽
        public void OnPointerDown(PointerEventData eventData)
        {
            //控件所在画布空间的初始位置
            Camera cam = eventData.pressEventCamera;
            //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam,
                out var pos);
            _startPos = pos;
            _startTime = Time.time;
            _dragging = false;
            _lastPointerEventData = eventData;
            // down的回调
            onDown?.Invoke(this);
        }

        //结束拖拽（此处没做任何处理，可自行拓展）
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_dragging)
            {
                _lastPointerEventData = eventData;
                onClick?.Invoke(this);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                Camera cam = eventData.pressEventCamera;
                //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
                RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam,
                    out var pos);
                var deltaTime = Time.time - _startTime;
                if (deltaTime > 0.5f || Vector3.Distance(_startPos, pos) > 5)
                {
                    _dragging = true;
                }
            }
        }
        
        
        /** 是否点到目标物 */
        public Transform HitTestSubNodes(Transform tf)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_lastPointerEventData, raycastResults);

            foreach (var raycastResult in raycastResults)
            {
                var node = raycastResult.gameObject.transform;
                while (node.parent != null)
                {
                    if (node.parent == tf)
                    {
                        return node;
                    }

                    node = node.parent;
                }
            }

            return null;
        }
    }
}
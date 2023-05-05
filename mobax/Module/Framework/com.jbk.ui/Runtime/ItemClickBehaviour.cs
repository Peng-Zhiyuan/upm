using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemClickBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Action<ClickableItem> OnItemClick;

        private Vector2 _pos; //控件初始位置
        private Vector2 _startPos; // 鼠标初始位置
        private float _startTime;
        private RectTransform _canvasRt; //控件所在画布
        private bool _dragging;
        private ClickableItem _pointerDownItem;

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

        // 开始拖拽
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
            
            var item = _GetHitItem(eventData);
            if (null != item)
            {
                item.OnPress(true);
                _pointerDownItem = item;
            }
        }

        // 结束拖拽
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_dragging)
            {
                if (null != OnItemClick)
                {
                    var item = _GetHitItem(eventData);
                    if (null != item)
                    {
                        OnItemClick(item);
                        item.OnPress(false);
                    }
                }
            }
            
            // 选中的item还原
            if (null != _pointerDownItem)
            {
                _pointerDownItem.OnPress(false);
                _pointerDownItem = null;
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
        private ClickableItem _GetHitItem(PointerEventData eventData)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach (var raycastResult in raycastResults)
            {
                var node = raycastResult.gameObject.transform;

                do
                {
                    var comp = node.GetComponent<ClickableItem>();
                    if (null != comp)
                    {
                        return comp;
                    }

                    node = node.parent;
                } while (node != null);
            }

            return null;
        }
    }
}
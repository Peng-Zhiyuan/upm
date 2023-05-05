using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class DragBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        public bool Dragable = true;
        public bool Clickable = true;

        public Action<DragBehaviour> onDown; // 触摸（或者MouseDown）的时候响应
        public Action<DragBehaviour> onClick;
        public Action<DragBehaviour> onDragStart;
        public Action<DragBehaviour> onDragging;
        public Action<DragBehaviour> onDragEnd;

        private Vector2 _pos; //控件初始位置
        private Vector2 _startPos; // 鼠标初始位置
        private float _startTime;
        private bool _dragging;
        private RectTransform _canvasRt; //控件所在画布
        private Transform _dragParent;

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

        private RectTransform _draggingRt;

        public RectTransform DraggingRt
        {
            set => _draggingRt = value;
            get => _draggingRt != null ? _draggingRt : transform as RectTransform;
        }

        private PointerEventData _lastPointerEventData;

        public PointerEventData LastPointerEventData
        {
            get => _lastPointerEventData;
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
            if (Clickable)
            {
                onDown?.Invoke(this);
            }
        }

        private void BeginDrag(PointerEventData eventData)
        {
            _dragging = true;

            if (!Dragable) return;
            _lastPointerEventData = eventData;
            // 先触发回调（这里面可能会重新修改DraggingRt的指向，所以要在最上面）
            onDragStart?.Invoke(this);
            // 先将其原始的父节点记录
            _dragParent = DraggingRt.parent;
            // 开始拖动的时候都将其的父容器设为最高容器
            DraggingRt.SetParent(CanvasRt);
            // 如果是不同组件的话，还需要赋值位置
            if (DraggingRt != transform)
            {
                DraggingRt.position = transform.position;
            }

            // 记录起始位置
            _pos = DraggingRt.anchoredPosition;
            // 然后还要把位置调整正确
            Camera cam = eventData.pressEventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam, out var newVec);
            Vector2 offset = new Vector2(newVec.x - _startPos.x, newVec.y - _startPos.y);
            Vector2 targetPos = _pos + offset;
            DraggingRt.anchoredPosition = targetPos;
        }

        private void Drag(PointerEventData eventData)
        {
            if (!Dragable) return;

            _lastPointerEventData = eventData;
            Camera cam = eventData.pressEventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam, out var newVec);
            //鼠标移动在画布空间的位置增量
            Vector2 offset = new Vector2(newVec.x - _startPos.x, newVec.y - _startPos.y);

            //原始位置增加位置增量即为现在位置
            Vector2 targetPos = _pos + offset;
            DraggingRt.anchoredPosition = targetPos;
            onDragging?.Invoke(this);
        }

        private void EndDrag(PointerEventData eventData)
        {
            if (!Dragable) return;

            _lastPointerEventData = eventData;
            Camera cam = eventData.pressEventCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam, out var newVec);
            //鼠标移动在画布空间的位置增量
            Vector2 offset = new Vector2(newVec.x - _startPos.x, newVec.y - _startPos.y);

            // 原始位置增加位置增量即为现在位置
            Vector2 targetPos = _pos + offset;
            DraggingRt.anchoredPosition = targetPos;
            // 然后还原它原来的父节点
            if (null != _dragParent)
            {
                DraggingRt.SetParent(_dragParent);
            }

            onDragEnd?.Invoke(this);
        }

        private void ClickEvent(PointerEventData eventData)
        {
            if (!Clickable) return;
            _lastPointerEventData = eventData;
            onClick?.Invoke(this);
        }

        // 拖拽过程中
        public void OnDrag(PointerEventData eventData)
        {
            if (_dragging)
            {
                Drag(eventData);
            }
            else
            {
                Camera cam = eventData.pressEventCamera;
                //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
                RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam, out var pos);
                var deltaTime = Time.time - _startTime;
                if (deltaTime > 0.5f || Vector3.Distance(_startPos, pos) > 5)
                {
                    BeginDrag(eventData);
                }
            }
        }

        // 结束拖拽
        public void OnPointerUp(PointerEventData eventData)
        {
            if (_dragging)
            {
                _dragging = false;
                EndDrag(eventData);
            }
            else
            {
                ClickEvent(eventData);
            }
        }

        public List<GameObject> CheckPointerOverGameObject(PointerEventData eventData)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);
            List<GameObject> lst = new List<GameObject>();
            if (raycastResults.Count > 0)
            {
                for (int i = 0; i < raycastResults.Count; i++)
                {
                    lst.Add(raycastResults[i].gameObject);
                }
            }

            return lst;
        }

        public bool HitTest(GameObject go)
        {
            return HitTest(go.transform);
        }

        /** 是否点到目标物 */
        public bool HitTest(Transform tf)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_lastPointerEventData, raycastResults);

            foreach (var raycastResult in raycastResults)
            {
                var node = raycastResult.gameObject.transform;
                while (node != null)
                {
                    if (node == tf)
                    {
                        return true;
                    }

                    node = node.parent;
                }
            }

            return false;
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


        /** 是否点到目标物 */
        public Transform HitTestTargets(params Transform[] targets)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_lastPointerEventData, raycastResults);

            foreach (var raycastResult in raycastResults)
            {
                var currentTf = raycastResult.gameObject.transform;
                if (Array.IndexOf(targets, currentTf) >= 0)
                    return currentTf;
            }

            return null;
        }

        /** 是否点到目标物 */
        public (int index, Transform target) GetTargetByHitTestTargets(bool needScale = false,
            params Transform[] targets)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                var rt = target.GetComponent<RectTransform>();
                var sizeDelta = this._draggingRt.sizeDelta * (needScale ? 0.6f : 1);

                var dragRtWidth = sizeDelta.x;
                var dragRtHeight = sizeDelta.y;

                var delta = rt.sizeDelta;
                var rtWidth = delta.x;
                var rtHeight = delta.y;

                var localPosition = this._draggingRt.localPosition;
                var position = rt.localPosition;

                var inside = localPosition.x + dragRtWidth <= position.x + rtWidth &&
                             localPosition.x - dragRtWidth >= position.x - rtWidth &&
                             localPosition.y + dragRtHeight <= position.y + rtHeight &&
                             localPosition.y - dragRtHeight >= position.y - rtHeight;
                if (inside)
                {
                    return (i, target);
                }
            }

            return (-1, null);

            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(_lastPointerEventData, raycastResults);


            foreach (var raycastResult in raycastResults)
            {
                var currentTf = raycastResult.gameObject.transform;
                if (Array.IndexOf(targets, currentTf) >= 0)
                    return (Array.IndexOf(targets, currentTf), currentTf);
            }
        }
    }
}
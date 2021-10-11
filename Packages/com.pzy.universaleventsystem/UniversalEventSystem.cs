using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalEventSystem : StuffObject<UniversalEventSystem>
{
    private UniversalEventRaycastTarget pickedTarget;

    private Camera _uiCamera;
    public Camera UiCamera
    {
        get
        {
            if(_uiCamera == null)
            {
                _uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
            }
            return _uiCamera;
        }

    }

    private Camera _mapCamera;
    public Camera MapCamera
    {
        get
        {
            if (_mapCamera == null)
            {
                _mapCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            }
            return _mapCamera;
        }
    }


    private UniversalEventRaycastTarget GetRaycastTarget(Touch? touch, Camera camera, string layerName, Color raycastColor)
    {
        if (touch != null)
        {
            var position = touch.Value.position;
            var ray = camera.ScreenPointToRay(position);
            var layerMast = LayerMask.GetMask(layerName);
            //var isHit = Physics.Raycast(ray, out hit, 10000, layerMast);
            var hitList = Physics.RaycastAll(ray, 20000, layerMast);

            Debug.DrawRay(ray.origin, ray.direction * 20000, raycastColor);

            foreach (var hit in hitList)
            {
                var targetCollider = hit.collider;
                var raycastTarget = targetCollider.GetComponent<UniversalEventRaycastTarget>();
                if (raycastTarget != null)
                {
                    return raycastTarget;
                }
            }

        }
        return null;
    }


    public UniversalEventRaycastTarget rayHitedTarget;
    public Vector2 lastDragPosition;
    public bool isDragStartCalled;
    public Touch? touch;
    public void UpdateEvent()
    {
        var touch = InputUtil.TryGetTouchOrSimulateTouch();
        this.touch = touch;
        var hitedTarget = GetRaycastTarget(touch, this.UiCamera, "UI", Color.blue);
        if(hitedTarget == null)
        {
            hitedTarget = GetRaycastTarget(touch, this.MapCamera, "Map", Color.red);
        }
        this.rayHitedTarget = hitedTarget;

        if (touch == null)
        {
            return;
        }

        if (pickedTarget == null)
        {
            if (hitedTarget != null)
            {
                var isTouchDown = touch.Value.phase == TouchPhase.Began;
                if (isTouchDown)
                {
                    pickedTarget = hitedTarget;
                    pickedTarget.OnTouchDown(touch.Value);
                    Debug.Log($"[UIEngine] OnTouchDown -> {pickedTarget.name}");
                    lastDragPosition = touch.Value.position;
                    isDragStartCalled = false;
                }
            }
        }
        else if (pickedTarget != null)
        {
            var isTouchUp = touch.Value.phase == TouchPhase.Ended;
            var isTouchMove = touch.Value.phase == TouchPhase.Moved;
            if (isTouchUp)
            {
                pickedTarget.OnTouchUp(touch.Value);
                Debug.Log($"[UIEngine] OnTouchUp -> {pickedTarget.name}");

                if(isDragStartCalled)
                {
                    pickedTarget.OnDragEnd(touch.Value);
                }

                if (hitedTarget != null)
                {
                    if (hitedTarget != pickedTarget)
                    {
                        pickedTarget.OnDrop(touch.Value, hitedTarget);
                        Debug.Log($"[UIEngine] OnDrop -> {pickedTarget.name}");
                    }
                }
                pickedTarget = null;
            }
            else if (isTouchMove)
            {
                var nowPosition = touch.Value.position;
                if(nowPosition != lastDragPosition)
                {
                    lastDragPosition = nowPosition;
                    if(!isDragStartCalled)
                    {
                        Debug.Log($"[UIEngine] OnDragStart -> {pickedTarget.name}");
                        isDragStartCalled = true;
                        pickedTarget.OnDragStart(touch.Value);
                    }

                    pickedTarget.OnDrag(touch.Value);

                    if(rayHitedTarget != null)
                    {
                        pickedTarget.OnHover(touch.Value, rayHitedTarget);
                    }
                }
            }
        }
    }


    private bool _enable;
    public bool Enable
    {
        get
        {
            return _enable;
        }
        set
        {
            _enable = value;
        }
    }

    void Update()
    {
        if(_enable)
        {
            this.UpdateEvent();
        }
    }
}

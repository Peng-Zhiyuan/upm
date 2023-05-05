using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using Sirenix.OdinInspector;

/// <summary>
/// 长按按钮
/// </summary>
public class HoldingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [LabelText("首次触发延迟(ms)")]
    public int delay = 500; //延迟时间(ms);
    [LabelText("触发间隔(ms)")]
    public int repeatInterval = 100; //回调触发间隔时间;

    public UnityEvent onHoldingInvoke = new UnityEvent();
    public UnityEvent onHoldingEnd = new UnityEvent();
    public UnityEvent onClick = new UnityEvent();

    private bool _isDown = false;
    private float _lastInvokeTime;
    private float _countdown;
    private RectTransform _canvasRt; //控件所在画布
    private Vector2 _startPos; // 鼠标初始位置
    
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

    // Use this for initialization
    void Start()
    {
        _countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isDown)
        {
            if ((_countdown -= Time.deltaTime * 1000) > 0)
            {
                return;
            }
            
            if ((Time.time - _lastInvokeTime) * 1000 >= repeatInterval)
            {
                //触发点击;
                onHoldingInvoke.Invoke();
                _lastInvokeTime = Time.time;
            }
        }
    }

    public void Exit()
    {
        _isDown = false;
        _lastInvokeTime = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isDown = true;
        _countdown = delay;
        _lastInvokeTime = 0;
        
        // 记录这个点， 记录是有滑动操作
        Camera cam = eventData.pressEventCamera;
        //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam,
            out var pos);
        _startPos = pos;
    }

    public void OnPointerUp(PointerEventData _)
    {
        if (_isDown)
        {
            if (_countdown > 0)
            {
                // 如果没有触发长按，就当做点击
                onClick?.Invoke();
            }
            else
            {
                onHoldingEnd?.Invoke();
            }
        }
        
        Exit();
    }

    // 当开始拖动后，就不计算长按了
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDown) return;
        
        Camera cam = eventData.pressEventCamera;
        //将屏幕空间鼠标位置eventData.position转换为鼠标在画布空间的鼠标位置
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRt, eventData.position, cam,
            out var pos);
        if (Vector3.Distance(_startPos, pos) > 5)
        {
            // 移动数值过大就当做是滑动，那么就结束拖动了
            if (_countdown <= 0)
            {
                onHoldingEnd?.Invoke();
            }
            Exit();
        }
    }
}
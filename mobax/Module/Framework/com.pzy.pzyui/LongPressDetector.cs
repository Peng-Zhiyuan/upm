using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// 提供被长按事件
/// </summary>
public class LongPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public float delay = 0f;//延迟时间;

    public bool repeat = false;
    public float repeatInterval = 0.1f; //回调触发间隔时间;


    public UnityEvent onLongPress = new UnityEvent();
    public UnityEvent onLongPressStop = new UnityEvent();

    private bool isPointDown = false;
    private bool invokedAfterLastPointDown = false;
    private float lastInvokeTime;

    private float countdown = 0f;

    // Use this for initialization
    void Start()
    {
        countdown = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPointDown)
        {
            if(repeat || !invokedAfterLastPointDown)
            {

                if ((countdown -= Time.deltaTime) > 0f)
                {
                    return;
                }

                if(!invokedAfterLastPointDown)
                {
                    //触发点击;
                    onLongPress.Invoke();
                    invokedAfterLastPointDown = true;
                    lastInvokeTime = Time.time;
                    return;
                }

                if (Time.time - lastInvokeTime > repeatInterval)
                {
                    //触发点击;
                    onLongPress.Invoke();
                    invokedAfterLastPointDown = true;
                    lastInvokeTime = Time.time;
                }
            }
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointDown = true;
        invokedAfterLastPointDown = false;
        countdown = delay;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointDown = false;
        invokedAfterLastPointDown = false;
        countdown = delay;
    }

    // 当开始拖动后，就不计算长按了
    public void OnDrag(PointerEventData eventData)
    {
        if (invokedAfterLastPointDown)
        {
            onLongPressStop?.Invoke();
        }
        isPointDown = false;
    }
}
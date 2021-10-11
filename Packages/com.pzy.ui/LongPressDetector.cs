using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class LongPressDetector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public float minInterval = 0.1f; //回调触发间隔时间;

    public float delay = 0f;//延迟时间;

    public UnityEvent onPressDown = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    public UnityEvent onPressUp = new UnityEvent();

    private bool isPointDown = false;
    private float lastInvokeTime;

    private float m_Delay = 0f;

    // Use this for initialization
    void Start()
    {
        m_Delay = delay;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPointDown)
        {
            if ((m_Delay -= Time.deltaTime) > 0f)
            {
                return;
            }

            if (Time.time - lastInvokeTime > minInterval)
            {
                //触发点击;
                onLongPress.Invoke();
                lastInvokeTime = Time.time;
            }
        }

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointDown = true;
        m_Delay = delay;
        this.onPressDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointDown = false;
        m_Delay = delay;
        this.onPressUp?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointDown = false;
        m_Delay = delay;
    }
}
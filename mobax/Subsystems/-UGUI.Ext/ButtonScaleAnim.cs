using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScaleAnim : MonoBehaviour, IUpdatable, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler,
    IPointerEnterHandler
{
    public bool buttonMode = true;

    public Vector3 toScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float duration = 0.3f;
    public Transform target;
    private bool isPress = false;
    private Vector3 originScale = Vector3.one;
    private float time = 0;
    private Button btn;

    void Awake()
    {
        if (target == null)
        {
            target = this.transform;
        }

        originScale = target.localScale;
        // 修改在在原来的缩放上调整缩放百分比
        toScale = new Vector3(originScale.x * toScale.x, originScale.y * toScale.z, originScale.y * toScale.z);
        duration = Mathf.Max(0.1f, duration);
        btn = this.GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_isInteractive()) return;

        OnPress(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isInteractive()) return;

        if (isPress)
        {
            OnPress(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isInteractive()) return;

        if (isPress)
        {
            OnPress(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log("进入");
    }

    void OnPress(bool isPressed) //OnPress长按事件
    {
        if (!_isInteractive()) return;

        if (isPress != isPressed)
        {
            isPress = isPressed;
            UpdateManager.Stuff.Add(this);
            this.time = 0;
        }
    }

    public void OnUpdate()
    {
        if (target == null)
        {
            UpdateManager.Stuff.Remove(this);
            return;
        }

        time += Time.deltaTime;
        if (isPress)
        {
            if (time < duration)
            {
                var s = Vector3.Lerp(originScale, toScale, time / duration);
                this.target.localScale = s;
            }
            else
            {
                this.target.localScale = toScale;
                UpdateManager.Stuff.Remove(this);
            }
        }
        else
        {
            if (time < duration)
            {
                var s = Vector3.Lerp(toScale, originScale, time / duration);
                this.target.localScale = s;
            }
            else
            {
                this.target.localScale = originScale;
                UpdateManager.Stuff.Remove(this);
            }
        }
    }

    private bool _isInteractive()
    {
        if (buttonMode)
        {
            // return null != btn && btn.interactable;
            return null != btn;
        }

        return true;
    }

    void OnEnable()
    {
        target.localScale = originScale;
    }

    // void OnDestroy()
    // {
    //     UpdateManager.Instance?.Remove (this);
    // }

    void OnDisable()
    {
        //  UpdateManager.Instance?.Remove (this);
        isPress = false;
        target.localScale = originScale;
        this.time = 0;
    }
}
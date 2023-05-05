using UnityEngine;
using System;
[Serializable]
public class TriggerTarget : GizmosMonoBehaviour
{
    public virtual void OnReset()
    {
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
    }
    public virtual void OnOpen()
    {
        this.gameObject.SetActive(true);
    }
    public virtual void OnClose()
    {
        this.gameObject.SetActive(false);
    }
    public virtual void OnSwitch()
    {
        if (this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            this.gameObject.SetActive(true);
        }
    }
}
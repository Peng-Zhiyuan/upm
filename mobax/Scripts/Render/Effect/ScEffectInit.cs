using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效初始化器
/// </summary>
public class ScEffectInit : MonoBehaviour
{
    public virtual void SetIntArgs(int arg,int index=0)
    {
        
    }
    public virtual void SetFloatArgs(float arg,int index=0)
    {
        
    }
    public virtual void SetVector3Args(Vector3 arg,int index=0)
    {
        
    }
    public virtual void SetColorArgs(Color arg,int index=0)
    {
        
    }

    public virtual void Play()
    {
        root.gameObject.SetActive(true);
    }

    public GameObject root;
}

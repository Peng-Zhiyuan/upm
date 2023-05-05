using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyEffect : BaseEffect
{
    private Transform m_BodyPoint = null;
    private Vector3 m_Offset = Vector3.zero;

    public BodyEffect(int param_ID, int param_configID, Transform param_Self)
        : base(param_ID, param_configID, param_Self)
    {

    }

    public override void Play()
    {
        base.Play();
        if (m_BodyPoint != null)
        {
            CachedTransform.SetParent(m_BodyPoint);
        }

        CachedTransform.localPosition = m_Offset;
        //CachedTransform.localRotation = Quaternion.identity;
        CachedTransform.localEulerAngles = Rotation;
        CachedTransform.localScale = Vector3.one;
    }

    public Transform BodyPoint
    {
        get { return m_BodyPoint; }
        set { m_BodyPoint = value; }
    }

    public Vector3 Offset
    {
        get { return m_Offset; }
        set { m_Offset = value; }
    }
    
    public Vector3 Rotation
    {
        get;
        set;
    } = Vector3.zero;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MapEffect : BaseEffect
{
    private Vector3 m_Position = Vector3.zero;
    private Vector3 m_Direction = Vector3.forward;

    public MapEffect(int param_ID, int param_configID, Transform param_Self)
        : base(param_ID, param_configID, param_Self)
    {
        
    }

    public override void Play()
    {
        base.Play();

        CachedTransform.position = m_Position;

		if (m_Direction != Vector3.zero)
			CachedTransform.rotation = Quaternion.LookRotation(m_Direction, Vector3.up);
    }

    public Vector3 Position
    {
        get { return m_Position; }
        set { m_Position = value; }
    }

    public Vector3 Direction
    {
        get { return m_Direction; }
        set { m_Direction = value; }
    }
}

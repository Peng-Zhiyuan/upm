using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyEffect : BaseEffect
{
    private Vector3 m_Start;
    private Vector3 m_Direction;
    private float m_Speed = 50f;
    private Transform m_Target = null;
    private bool m_IsTrace = false;

    //private EffectFadeCtrl _effectFade;
    private float _fadeTime = 0.2f;

    public FlyEffect(int param_ID, int param_configID, Transform param_Self)
        : base(param_ID, param_configID, param_Self)
    {
    }

    public override void InitEffectGameObject()
    {
    }

    public override void Play()
    {
        base.Play();

        CachedTransform.position = m_Start;
        CachedTransform.rotation = Quaternion.LookRotation(m_Direction, Vector3.up);

        m_IsTrace = m_Target != null ? true : false;
    }

    public override void Update (float param_deltaTime)
    {
        base.Update(param_deltaTime);
        if (m_Target != null)
        {
            TraceTarget();
            if (CachedTransform.position == m_Target.position)
            {
                Completed = true;
            }
        }
        else
        {
            if (m_IsTrace)
            {
                Completed = true;
                return;
            }
            if (m_TimeSincePlay >= TotalTime - _fadeTime) {
            }
            MoveForward();
        }
    }

    private void TraceTarget()
    {
        CachedTransform.LookAt(m_Target.position);
        CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, m_Target.position, m_Speed * Time.deltaTime);
    }

    private void MoveForward()
    {
        CachedTransform.Translate(Vector3.forward * m_Speed * Time.deltaTime);
    }

    public Vector3 Start
    {
        get { return m_Start; }
        set { m_Start = value; }
    }

    public Vector3 Direction
    {
        get { return m_Direction; }
        set { m_Direction = value; }
    }

    public Transform Target
    {
        get { return m_Target; }
        set { m_Target = value; }
    }

    public float Speed
    {
        get { return m_Speed; }
        set { m_Speed = value; }
    }

    public override float TotalTime
    {
        get { return base.TotalTime; }
        set { base.TotalTime = value + _fadeTime; }
    }
}

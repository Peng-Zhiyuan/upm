using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveEffect : BaseEffect
{
    private Vector3 m_Start;
    private Vector3 m_Direction;
    private float m_Speed = 50f;
    private Transform m_Target = null;

    private Vector3 _speed;
    //private EffectFadeCtrl _effectFade;
    private float _fadeTime = 0.2f;

    private float CurHeightParam = 3f;

    public bool SecondFly = false;

    public CurveEffect(int param_ID, int param_configID, Transform param_Self, bool param_SecondFly)
        : base(param_ID, param_configID, param_Self)
    {
        SecondFly = param_SecondFly;
    }

    public override void InitEffectGameObject()
    {
    }

    public override void Play()
    {
        base.Play();

        float distance = Vector3.Distance(m_Start, m_Target.position);
        float temp = distance - 1;
        g = Mathf.Clamp(temp, 0, 10f) * CurHeightParam;

        CachedTransform.position = m_Start;
        CachedTransform.rotation = Quaternion.LookRotation(m_Direction, Vector3.up);
    }

    public override void Update (float param_deltaTime)
    {
        base.Update(param_deltaTime);

        if (!Completed) {
            if (m_Target != null)
            {
                Vector3 temp_vec = m_Target.position - m_Start;
                _speed = temp_vec / TotalTime;
            }

            Vector3 vec = _speed * m_TimeSincePlay;
            Vector3 tmp_dst = m_Start + vec;
            tmp_dst = DestHandler(tmp_dst);

            CachedTransform.LookAt(tmp_dst);
            CachedTransform.position = tmp_dst;

            if (SecondFly && TotalTime - m_TimeSincePlay < 0.05f)
            {
                SecondFly = false;
                m_Start = tmp_dst;
                m_TimeSincePlay = 0;
                TotalTime = 0.3f;
            }
        }
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

    public float HeightParam
    {
        get { return CurHeightParam; }
        set { CurHeightParam = value; }
    }

    private float g = 5f;
    private Vector3 DestHandler(Vector3 tmp_dst) {
        float curTime = m_TimeSincePlay / TotalTime;
        
        tmp_dst.y += 0.5f * g * curTime - 0.5f * g * curTime * curTime;
        return tmp_dst;
    }
}

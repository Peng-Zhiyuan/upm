using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLineEffect : BaseEffect
{
    private float LiftTime = 2f;
    private float MaxPos = 0;
    private float MinPos = 0;

    private float DurTime
    {
        get
        {
            if (LiftTime <= 0)
                return 2;

            return LiftTime;
        }
    }

    private float GetY(float x)
    {
        float innner = (2 * (float)Mathf.PI) / DurTime;
        float ya = (float)Mathf.Sin(innner * x);
        float maall = (MaxPos - MinPos) * 0.5f;
        float middle = maall + MinPos;
        return (middle + maall * ya); 
    }

    private Vector3 m_Start;
    private Vector3 m_Direction;
    private float m_Speed = 50f;
    private Transform m_Target = null;

    private Vector3 _speed;
    //private EffectFadeCtrl _effectFade;
    private float _fadeTime = 0.2f;

    private float CurHeightParam = 3f;

    private float AngleSpeed = 0f;

    public SLineEffect(int param_ID, int param_configID, Transform param_Self)
        : base(param_ID, param_configID, param_Self)
    {
    }

    public override void InitEffectGameObject()
    {
    }

    private int stage = 0;
    private float wSpeed = 12f;
    private float radius = 1.2f;
    private Vector3 target1;
    private Vector3 target2;
    private Vector3 target3;
    private float intervalTime = 0.75f;
    private float maxdis = 20f;
    public override void Play()
    {
        base.Play();
        
        


        wSpeed = UnityEngine.Random.Range(8f, 12.0f);
        radius = UnityEngine.Random.Range(1.2f, 1.6f);
        float distance = Vector3.Distance(m_Start, m_Target.position);
        float temp = distance - 1;
        g = Mathf.Clamp(temp, 0, 10f) * 2;
        AngleParam = UnityEngine.Random.Range(0.1f, 0.6f);
        CachedTransform.position = m_Start;
        CachedTransform.rotation = Quaternion.LookRotation(m_Direction, Vector3.up);

        float offsetrandom = 10f;
        float offset = 3f;
        if (m_Target != null)
        {
            Vector3 temp_vec = m_Target.position - m_Start;
            _speed = temp_vec / (TotalTime - 0.2f);
            float speedk = 0.3f;
            target1 = DestHandler(m_Start + _speed * intervalTime * speedk, intervalTime) + 
                      new Vector3(UnityEngine.Random.Range(offset, -offset), UnityEngine.Random.Range(offset, -offset), UnityEngine.Random.Range(offset, -offset));
            
            target2 = DestHandler(m_Start + _speed * 2 * intervalTime * speedk, 2 * intervalTime) + 
                              new Vector3(UnityEngine.Random.Range(offset, -offset), UnityEngine.Random.Range(offset, -offset), UnityEngine.Random.Range(offset, -offset));
            
            target3 = DestHandler(m_Start + _speed *3 * intervalTime * speedk, 3 * intervalTime) + 
                              new Vector3(UnityEngine.Random.Range(offset, -offset), UnityEngine.Random.Range(offset, -offset), UnityEngine.Random.Range(offset, -offset));

            target1 = DestHandler(m_Start + _speed * intervalTime * speedk, intervalTime) +
                      Vector3.right * UnityEngine.Random.Range(-offsetrandom, offsetrandom) + Vector3.up *
                      UnityEngine.Random.Range(-offsetrandom, offsetrandom);
            
            target2 = DestHandler(m_Start + _speed * 2 * intervalTime * speedk, 2 * intervalTime) +
                      Vector3.right * UnityEngine.Random.Range(-offsetrandom, offsetrandom) + Vector3.up * UnityEngine.Random.Range(-offsetrandom, offsetrandom);

        }
    }

   
    public override void Update (float param_deltaTime)
    {
        base.Update(param_deltaTime);

        if (!Completed) {
            //Vector3 vec = m_Target.position - m_Start;
            //Vector3 mid = m_TimeSincePlay * Speed * vec.normalized + Start;
            
            /*if (Vector3.Distance(m_Target.position, mid) < 1)
            {
                CachedTransform.LookAt(m_Target.position);
                CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, m_Target.position, (m_Speed + 1) * Time.deltaTime);
            }
            else
            {
                //Vector3 desPos = mid + Vector3.right * Mathf.Sin(m_TimeSincePlay * Speed * AngleParam) * HeightParam/* + Vector3.up * Mathf.Sin(m_TimeSincePlay * Speed * AngleParam) * UnityEngine.Random.Range(-0.2f, 0.2f)#1#;
                Vector3 desPos = mid + new Vector3(Mathf.Cos(m_TimeSincePlay * 10f) * 1f, Mathf.Sin(m_TimeSincePlay * 10) * 1f, 0);
                                 //Vector3 desPos = CachedTransform.position + vec.normalized * 5 * Time.deltaTime;
                CachedTransform.LookAt(desPos);
                CachedTransform.position = desPos;
            }*/
            
            if (!Completed) {
                if (m_Target != null)
                {
                    Vector3 temp_vec = m_Target.position - m_Start;
                    _speed = temp_vec / TotalTime;
                }

                Vector3 vec = _speed * m_TimeSincePlay;
                Vector3 tmp_dst = m_Start + vec;
                tmp_dst = DestHandler(tmp_dst);
                
                
                if (m_TimeSincePlay < intervalTime * 0.3f)
                {
                    //Vector3 target = DestHandler(m_Start + _speed * 0.2f, 0.2f);
                    CachedTransform.LookAt(target1);
                    CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, target1, 0.2f * maxdis * Time.deltaTime);
                }
                else if (m_TimeSincePlay < 2 * intervalTime * 0.3f)
                {
                    //Vector3 target = DestHandler(m_Start + _speed * 0.2f, 0.2f);
                    CachedTransform.LookAt(target2);
                    CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, target2, 0.2f * maxdis * Time.deltaTime);
                }
                /*else if (m_TimeSincePlay < 3 * intervalTime)
                {
                    //Vector3 target = DestHandler(m_Start + _speed * 0.2f, 0.2f);
                    CachedTransform.LookAt(target3);
                    CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, target3, maxdis * Time.deltaTime);
                }*/
                else
                {
                    CachedTransform.LookAt(tmp_dst);
                    CachedTransform.position = Vector3.MoveTowards(CachedTransform.position, tmp_dst, maxdis * Time.deltaTime);
                }

                //CachedTransform.LookAt(tmp_dst);
                //CachedTransform.position = tmp_dst;
            }
            
            
            
            if (Vector3.Distance(CachedTransform.position, m_Target.position) < 0.01f)
            {
                Completed = true;
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
    
    public float AngleParam
    {
        get { return AngleSpeed; }
        set { AngleSpeed = value; }
    }

    private float g = 5f;
    private Vector3 DestHandler(Vector3 tmp_dst) {
        float curTime = m_TimeSincePlay / TotalTime;
        
        tmp_dst.y += 0.5f * g * curTime - 0.5f * g * curTime * curTime;
        return tmp_dst;
    }
    
    private Vector3 DestHandler(Vector3 tmp_dst, float curTime) {
        tmp_dst.y += 0.5f * g * curTime - 0.5f * g * curTime * curTime;
        return tmp_dst;
    }
}

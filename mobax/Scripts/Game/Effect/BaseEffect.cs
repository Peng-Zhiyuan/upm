using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BaseEffect
{
    private readonly int m_ID = 0;
    private readonly int _configID = 0;
    private Transform m_Transform = null;
    protected float m_TimeSincePlay = 0f;
    protected float m_TotalTime = 0f;
    protected float _MaxTotalTime = 50000;
    private bool m_Completed = false;

    private GameObject m_gameobj = null; //cache

    public BaseEffect(int param_ID, int param_configID, Transform param_Transform)
    {
        param_Transform.name = "Effect_" + param_Transform.name;
        param_Transform.SetParent(SceneObjectManager.Instance.GetEffectRootTransform);

        m_ID = param_ID;
        _configID = param_configID;
        m_Transform = param_Transform;
        Completed = false;        
    }

    public virtual void ResetParent() {
        CachedTransform.SetParent(SceneObjectManager.Instance.GetEffectRootTransform);
    }

    public virtual void Play()
    {
        m_TimeSincePlay = 0f;
    }

	public virtual void Update (float param_deltaTime)
    {
        if (m_Transform == null)
        {
            Completed = true;
            return;
        }

        m_TimeSincePlay += param_deltaTime;
        if (m_TotalTime >= 0 && m_TimeSincePlay >= m_TotalTime)
        {
            Completed = true;
            return;
        }
	}

    public virtual void Unitialize()
    {
        if (m_Transform != null)
        {
            GameObject.Destroy(m_Transform.gameObject);
            m_Transform = null;
        }

        if(m_gameobj != null)
        {
            GameObject.Destroy(m_gameobj);
            m_gameobj = null;
        }

        Destroy();
    }

    public int ID
    {
        get { return m_ID; }
    }

    public int ConfigID
    {
        get { return _configID; }
    }

    public bool Completed
    {
        get { return m_Completed; }
        protected set { m_Completed = value; }
    }

    public virtual float PlayTime
    {
        get { return m_TimeSincePlay; }
    }

    public virtual float TotalTime
    {
        get { return m_TotalTime; }
        set {
            //LogMgr.Log("特效ID：" + _configID + "        持续时间： " + value);
            m_TotalTime = Mathf.Clamp(value, -1f, _MaxTotalTime);
        }
    }

    public Transform CachedTransform
    {
        get { return m_Transform; }
    }

    public void Destroy()
    {
        //base.Destroy();
        //GameObject.Destroy(m_Transform.gameObject);

    }

    public GameObject EffectGameObject
    {
        get { return m_gameobj; }
    }

    public void SetEffectGameObject(GameObject tmp_obj) {
        m_gameobj = tmp_obj;
        InitEffectGameObject();
    }

    public virtual void InitEffectGameObject() {

    }

    public void SetEffectScale(float value)
    {
        if (m_gameobj != null)
            m_gameobj.transform.localScale = Vector3.one * value;
    }
}

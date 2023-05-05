using UnityEngine;
using System.Collections.Generic;
using System;

public enum SceneObjectType
{
    None = 0
    , Player
    , Monster
    , Leader
    , LocalPlayer
    , Item
    , Target
    , //目标点
    NPC
    ,
}

// 场景对象基类
public abstract class SceneObject
{
    public SceneObjectEvent objectEvent = new SceneObjectEvent();

    // cached gameobject and components
    private GameObject m_GameObject = null;
    private Transform m_Transform = null;

#region 构造函数
    public SceneObject(string param_ID, int configID = 0)
    {
        m_Id = param_ID;
        m_ConfigId = configID;
        Initialize();
    }
#endregion

#region 初始化和销毁
    //所有初始化的入口
    public virtual void Initialize()
    {
        // create real object instance
        CreateGameObject();

        // add other components and load resources
        LoadInstance();
    }

    protected void CreateGameObject()
    {
        m_GameObject = new GameObject(m_Name);
        m_Transform = m_GameObject.transform;
        /*if (sceneObjectType == SceneObjectType.Player || sceneObjectType == SceneObjectType.NPC)
        {
            var go = GameObject.Find("Env_BoxPlane004");
            m_Transform.SetParent(go.transform);
        }
        else
        {
            m_Transform.SetParent(SceneObjectManager.Instance.GetRootTransform);
        }*/
        m_Transform.SetParent(SceneObjectManager.Instance.GetRootTransform);
        m_Transform.localPosition = Vector3.zero;
        m_Transform.localRotation = Quaternion.identity;
        m_Transform.localScale = Vector3.one;
    }

    public void LoadInstance()
    {
        InitializeGameObject();
    }

    // gameobject initialization
    protected abstract void InitializeGameObject();
    protected abstract void DestroyObject();

    // update per frame
    public abstract void Update(float param_deltaTime);

    public virtual void LateUpdate(float param_deltaTime)
    {
        //_so_select.LateUpdate(param_deltaTime);
    }

    //销毁总入口
    public virtual void Uninitialize()
    {
        DestroyObject();

        // destroy real object instance
        DestroyGameObject();
    }

    protected void DestroyGameObject()
    {
        m_Transform = null;
        GameObject.Destroy(m_GameObject);
        m_GameObject = null;
    }
#endregion

#region 设置相关的方法
    public virtual void SetPosition(Vector3 param_Position)
    {
        m_Transform.position = param_Position;
    }

    public virtual void SetGroundPosition(Vector3 param_Position)
    {
        m_Transform.position = RaycastUtil.GetGroundPosition(param_Position);
    }

    public Vector3 RegionPosition { get; set; }

    public virtual Vector3 GetPosition()
    {
        if (m_Transform == null)
            return Vector3.zero;
        return m_Transform.position;
    }

    public void SetDirection(Vector3 param_Direction)
    {
        if (param_Direction != Vector3.zero)
        {
            m_Transform.forward = param_Direction;
        }
    }

    public Vector3 GetDirection()
    {
        if (m_Transform == null)
        {
            return Vector3.zero;
        }
        return m_Transform.forward.normalized;
    }

    public void SetRotation(Quaternion param_Rotation)
    {
        m_Transform.rotation = param_Rotation;
    }

    public Quaternion GetRotation()
    {
        return m_Transform.localRotation;
    }

    public void SetScale(Vector3 scale)
    {
        m_Transform.localScale = scale;
    }

    public Vector3 GetScale()
    {
        return m_Transform.localScale;
    }

    public Vector3 GetEulerAngles()
    {
        return m_Transform.localEulerAngles;
    }

    public void SetActive(bool value)
    {
        if (m_active != value)
        {
            m_active = value;
            gameObject.SetActive(m_active);
            if (m_active)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }
    }

    public virtual float GetHeight()
    {
        return 0;
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }
#endregion

#region 各种属性
    public GameObject gameObject
    {
        get { return m_GameObject; }
    }

    public Transform transform
    {
        get { return m_Transform; }
    }

    public float Radius { get; set; }

    public Transform GetRootTransform
    {
        get { return null; /* SceneObjectManager.Instance.GetRootTransform; */ }
    }

    public string Tag
    {
        get { return m_GameObject.tag; }
        set { m_GameObject.tag = value; }
    }

    public int Layer
    {
        get { return m_GameObject.layer; }
        set { m_GameObject.layer = value; }
    }

    private string m_Id = "";
    public string ID
    {
        set { m_Id = value; }
        get { return m_Id; }
    }
    //configID

    private int m_ConfigId = 0;
    public int ConfigID
    {
        set { m_ConfigId = value; }
        get { return m_ConfigId; }
    }

    private string m_Name = string.Empty;
    public string Name
    {
        get { return m_Name; }
        set { m_Name = value; }
    }

    private bool m_active = true;
    public bool IsActive
    {
        get { return m_active; }
        set { m_active = value; }
    }

    public virtual SceneObjectType sceneObjectType
    {
        get { return SceneObjectType.None; }
    }

    //public float LastUnusedTime { get; set; } = 0;

    public virtual bool IsVisible
    {
        get { return true; }
    }
#endregion

#region 选中
    public bool Selected { get; set; }

    public void OnSelected()
    {
        Selected = true;
        objectEvent.Broadcast(GameEvent.SceneObjectSelected, true);
    }

    public void OnUnSelected()
    {
        Selected = false;
        objectEvent.Broadcast(GameEvent.SceneObjectSelected, false);
    }
#endregion
}
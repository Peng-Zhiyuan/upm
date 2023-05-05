using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// core engine 版的 monobehavior
public class CoreComponent : ICoreEngineSystemObject
{
    public int instanceId;
    public CoreObject coreObject;

    public CoreEngine coreEngine
    {
        get
        {
            return this.coreObject.coreEngine;
        }
    }


    public override string ToString()
    {
        var name = this.Name;
        var type = this.GetType();
        var typeName = type.Name;
        return $"{name}({typeName})";
    }

    public static implicit operator bool(CoreComponent comp)
    {
        var ret = comp != null;
        return ret;
    }

    public static bool operator ==(CoreComponent a, CoreComponent b)
    {
        var refEqual = ReferenceEquals(a, b);
        return refEqual;
    }

    public static bool operator !=(CoreComponent a, CoreComponent b)
    {
        var equal = a == b;
        var notEqual = !equal;
        return notEqual;
    }

    public override bool Equals(object other)
    {
        var refEqual = ReferenceEquals(this, other);
        return refEqual;
    }


    public int GetInstanceID()
    {
        return this.instanceId;
    }

    public override int GetHashCode()
    {
        return this.instanceId;
    }

    public CoreTransform Transform
    {
        get
        {
            return this.coreObject.Transform;
        }
    }

    public static bool inAddCompnent;
    public CoreComponent()
    {
        if (!inAddCompnent)
        {
            Debug.Log("[CoreComponent] one CoreComponent instance not created by CoreObject.AddComponent");
        }
    }

    public string Name
    {
        get
        {
            return this.coreObject.Name;
        }
        set
        {
            this.coreObject.Name = value;
        }
    }

    public bool _isEnabled;

    public bool IsEnabled
    {
        get
        {
            return _isEnabled;
        }
        set
        {
            if (_isEnabled == value)
            {
                return;
            }
            this._isEnabled = value;

            if (this.coreObject.IsActived)
            {
                if (value)
                {
                    this.OnCoreEnable();
                }
                else
                {
                    this.OnCoreDisable();
                }
            }
        }
    }


    /// <summary>
    /// 当组件被添加时
    /// </summary>
    public virtual void OnCoreAwake()
    {

    }

    public bool isStartCalled;
    public virtual void OnCoreStart()
    {

    }

    public virtual void OnCoreUpdate()
    {

    }

    public virtual void OnCoreLateUpdate()
    {

    }

    public virtual void OnCoreEnable()
    {

    }

    public virtual void OnCoreDisable()
    {

    }

    public virtual void OnCoreDestory()
    {

    }

    public IEnumerator StartCoroutine(string methodName)
    {
        throw new Exception("[CoreComponent] Coroutine not support");
    }

    public IEnumerable StartCoroutine(IEnumerator routineEnumerator)
    {
        throw new Exception("[CoreComponent] Coroutine not support");
    }

    public void StopAllCoroutines()
    {
        throw new Exception("[CoreComponent] Coroutine not support");
    }

    public virtual void OnCoreDrawGizmos()
    {

    }

    public virtual void OnCoreDrawGizmosSelected()
    {

    }

    public virtual void OnGUI()
    {

    }
    public T GetComponent<T>() where T : class
    {
        var ret = this.coreObject.GetComponent<T>();
        return ret;
    }

    public T[] GetComponents<T>() where T : class
    {
        var ret = this.coreObject.GetComponents<T>();
        return ret;
    }


    public bool TryGetComponent<T>(out T component) where T : class
    {
        var ret = this.coreObject.TryGetComponent<T>(out component);
        return ret;
    }

    public bool isActiveAndEnabled
    {
        get
        {
            return this.IsEnabled && this.coreObject.IsActived;
        }
    }

    //public void FindObjectsOfType(Type type)
    //{

    //}
    public static void DestroyImmediate(CoreComponent component)
    {
        throw new Exception("[CoreComponent] not implement yet");
    }

    /// <summary>
    /// 搜索一个核心组件
    /// 1. 该组件所在的核心对象名称与类型 T 同名
    /// 2. 该组件仅存在一个实例
    /// 3. 搜索结果会被缓存（不包括没有找到的情况）
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T FindStuff<T>() where T : CoreComponent
    {
        var stuff = this.coreEngine.FindStuff<T>();
        return stuff;
    }
}

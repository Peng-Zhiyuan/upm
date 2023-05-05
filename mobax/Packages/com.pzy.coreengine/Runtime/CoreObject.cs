using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;


// core engine 版的 gameobject
public class CoreObject : ICoreEngineSystemObject
{
    public CoreEngine coreEngine;

    public object debbugerGameObject;

    /// <summary>
    /// 标记在调用销毁所有 CoreObject 时，不对此对象进行销毁
    /// </summary>
    public bool dontDestoryFlag;

    public int instanceId;
    public List<CoreComponent> componentList = new List<CoreComponent>();
  

    public bool isMarkedDestoryed;

    public Action<CoreComponent> OnComponentAdded;

    public override int GetHashCode()
    {
        return this.instanceId;
    }


    private CoreTransform _transform;
    public CoreTransform Transform
    {
        get
        {
            if(_transform == null)
            {
                _transform = this.GetComponent<CoreTransform>();
            }
            return _transform;
        }
    }

    public CoreObject()
    {
        
    }
    /*internal CoreObject()
    {

    }*/

    public T[] GetComponents<T>() where T : class
    {
        var type = typeof(T);
        var ret = GetComponents(type);
        var array = new T[ret.Length];
        for(int i = 0; i < ret.Length; i++)
        {
            var comp = ret[i];
            array[i] = comp as T;
        }
        return array;
    }

 
    public CoreComponent[] GetComponents(Type type)
    {
        var retList = new List<CoreComponent>();
        var list = this.componentList;
        foreach(var comp in list)
        {
            var compType = comp.GetType();
            if(type.IsAssignableFrom(compType))
            {
                retList.Add(comp);
            }
        }
        var array = retList.ToArray();
        return array;
    }

    public Dictionary<Type, CoreComponent> typeToComponentDic = new Dictionary<Type, CoreComponent>();
    public CoreComponent GetComponent(Type type)
    {
        CoreComponent ret;
        typeToComponentDic.TryGetValue(type, out ret);
        if(ret != null)
        {
            return ret;
        }

        var list = this.componentList;
        foreach(var comp in list)
        {
            var compType = comp.GetType();
            if(type.IsAssignableFrom(compType))
            {
                typeToComponentDic[type] = comp;
                return comp;
            }
        }
        return null;
    }

    // 没有约束为 CoreComponent
    // 因为 T 可以是接口类型
    public T GetComponent<T>() where T : class
    {
        var type = typeof(T);
        var ret = GetComponent(type);
        return ret as T;
    }


    public CoreComponent AddComponent(Type type, Action<CoreComponent> onPreAwake = null)
    {
        CoreComponent.inAddCompnent = true;
        var comp = Activator.CreateInstance(type) as CoreComponent;
        CoreComponent.inAddCompnent = false;

        this.componentList.Add(comp);
        //this.typeToComponentDic[t] = comp;

        comp.instanceId = CoreInstanceGenerator.GenerateId();

        // 设置 core object
        comp.coreObject = this;

        onPreAwake?.Invoke(comp);
        comp.OnCoreAwake();

        comp.IsEnabled = true;

        this.OnComponentAdded?.Invoke(comp);

        return comp;
    }

    public T AddComponent<T>(Action<T> onPreAwake = null) where T : CoreComponent
    {
        var t = typeof(T);

        Action<CoreComponent> cb = null;
        if(onPreAwake != null)
        {
            cb = (comp) => onPreAwake.Invoke(comp as T);
        }

        var ret = this.AddComponent(t, cb);
        return ret as T;
    }

    public Action<string> OnNameChnaged;
    private string _name;
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            if(this._name == value)
            {
                return;
            }
            this._name = value;
            OnNameChnaged?.Invoke(value);
        }
    }

    public Action<bool> OnActivedChnaged;
    public bool _isActived;

    public bool IsActived
    {
        get
        {
            return this._isActived;
        }
        set
        {
            if(this._isActived == value)
            {
                return;
            }
            this._isActived = value;
            this.OnActivedChnaged?.Invoke(value);

            var list = this.componentList;
            if (value)
            {
                foreach (var comp in list)
                {
                    if (comp.IsEnabled)
                    {
                        comp.OnCoreEnable();
                    }
                }
            }
            else
            {
                foreach (var comp in list)
                {
                    if (comp.IsEnabled)
                    {
                        comp.OnCoreDisable();
                    }
                }
            }


        }
    }

    public Action OnDestoryed;

    public bool TryGetComponent<T>(out T component) where T : class
    {
        var type = typeof(T);
        CoreComponent comp;
        var ret = TryGetComponent(type, out comp);
        component = comp as T;
        return ret;
    }

    public bool TryGetComponent(Type type, out CoreComponent component)
    {
        component = this.GetComponent(type);
        if (component != null)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 寻找所有指定类型的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] FindObjectsOfType<T>() where T : CoreComponent
    {
        var ret = CoreEngine.lastestInstance.FindObjectsOfType<T>();
        return ret;
    }

    /// <summary>
    /// 寻找所有指定类型的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FindObjectOfType<T>() where T : CoreComponent
    {
        var ret = CoreEngine.lastestInstance.FindObjectOfType<T>();
        return ret;
    }

    public static void DestroyImmediate(CoreObject co)
    {
        CoreEngine.Destory(co);
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





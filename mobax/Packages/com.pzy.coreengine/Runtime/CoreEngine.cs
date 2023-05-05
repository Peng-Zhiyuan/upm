using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class CoreEngine
{
    public static CoreEngine lastestInstance;
    public object owner;

    public CoreEngine(object owner)
    {
        this.owner = owner;
        lastestInstance = this;
    }

    Dictionary<int, CoreObject> intanceIdToCoreObjectDic = new Dictionary<int, CoreObject>();

    public CoreObject Create(string name = null)
    {
        if(!Application.isPlaying)
        {
            throw new Exception("[CoreEngine] not support edit mode");
        }

        if(name == null)
        {
            name = "New CoreObject";
        }

        var co = new CoreObject();
        co.coreEngine = this;

        co.instanceId = CoreInstanceGenerator.GenerateId();
        var id = co.instanceId;

        intanceIdToCoreObjectDic[id] = co;

        co.Name = name;

        co.AddComponent<CoreTransform>();

        // 设置 enable
        co.IsActived = true;


        // 创建调试 gameObject
        // 在非 Unity 模式下去除此语句
#if UNITY_EDITOR
        CoreEngineUtil.CreateDebugerGameObject(co);
#endif

        return co;
    }

    public T CreateAsStuff<T>(Action<T> onPreAwake = null) where T : CoreComponent
    {
        var type = typeof(T);
        var name = type.Name;
        var co = this.Create(name);
        var comp = co.AddComponent(onPreAwake);
        return comp;
    }

    public CoreComponent CreateAsStuff(Type type, Action<CoreComponent> onPreAwake = null)
    {
        var name = type.Name;
        var co = this.Create(name);
        var comp = co.AddComponent(type, onPreAwake);
        return comp;
    }

    /// <summary>
    /// 销毁所有核心对象，但排除被标记为'不被摧毁'的部分
    /// </summary>
    public void DestoryAllObject()
    {
        var coList = GetCoreObjectList();
        foreach(var co in coList)
        {
            if(!co.dontDestoryFlag)
            {
                Destory(co);
            }
        }
    }

    List<int> idList = new List<int>();
    /// <summary>
    /// 外部更新驱动
    /// </summary>
    public void CoreUpdate()
    {
        var idCollection = intanceIdToCoreObjectDic.Keys;
        idList.Clear();
        foreach (var id in idCollection)
        {
            idList.Add(id);
        }

        foreach (var id in idList)
        {
            var co = intanceIdToCoreObjectDic[id];

            var isMarkedDestoryed = co.isMarkedDestoryed;
            if (isMarkedDestoryed)
            {
                intanceIdToCoreObjectDic.Remove(id);
                continue;
            }

            var list = co.componentList;
            foreach (var comp in list)
            {
                // 如果没有通知过 start，则通知 start
                var isStartCaleed = comp.isStartCalled;
                if (!isStartCaleed)
                {
                    comp.isStartCalled = true;
                    comp.OnCoreStart();
                }

                // 通知 update
                comp.OnCoreUpdate();
            }

            foreach (var comp in list)
            {

                // 通知 update
                comp.OnCoreLateUpdate();
            }

        }
    }

    Dictionary<Type, CoreComponent> findStuffCache = new Dictionary<Type, CoreComponent>();

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
        var type = typeof(T);

        CoreComponent cachedComp;
        var find = this.findStuffCache.TryGetValue(type, out cachedComp);
        if(find)
        {
            return cachedComp as T;
        }

        var name = type.Name;

        // 搜索名称与 T 同名的核心对象
        var co = Find(name);

        T stuff = null;
        if(co != null)
        {
            // 在其上获得类型为 T 的核心组件
            stuff = co.GetComponent<T>();
        }


        // 缓存结果
        if(stuff != null)
        {
            findStuffCache[type] = stuff;
        }

        return stuff;
    }

    public CoreObject Find(string name)
    {
        foreach (var kv in intanceIdToCoreObjectDic)
        {
            var obj = kv.Value;
            if (obj.Name == name)
            {
                return obj;
            }
        }
        return null;
    }

    public static void Destory(CoreObject co)
    {
        // 真机上可能出空引用错误，但是为什么？
        try
        {
            if (co == null)
            {
                return;
            }

            // 先设置为不活动
            co.IsActived = false;

            // OnDestory 通知
            CoreEngineUtil.InvokCoreDestroy(co);

            // 从字典中移除
            //var id = co.intanceId;
            //intanceIdToCoreObjectDic.Remove(id);


            // 先标记摧毁，之后再真的移除
            co.isMarkedDestoryed = true;

            co?.OnDestoryed();
        }
        catch(Exception e)
        {
            Debug.Log("[CoreObjectManager] exception: " + e.Message);
        }
    }

    /// <summary>
    /// 寻找所有指定类型的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T[] FindObjectsOfType<T>() where T :CoreComponent
    {
        var ret = new List<T>();
        foreach(var kv in intanceIdToCoreObjectDic)
        {
            var co = kv.Value;
            if(co.isMarkedDestoryed)
            {
                continue;
            }
            var compList = co.GetComponents<T>();
            ret.AddRange(compList);
        }
        return ret.ToArray();
    }

    /// <summary>
    /// 寻找所有指定类型的组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T FindObjectOfType<T>() where T : CoreComponent
    {
        foreach (var kv in intanceIdToCoreObjectDic)
        {
            var co = kv.Value;
            if (co.isMarkedDestoryed)
            {
                continue;
            }
            var comp = co.GetComponent<T>();
            if(comp != null)
            {
                return comp;
            }
        }
        return null;
    }

    /// <summary>
    /// 获得所有 CoreObject 的列表
    /// </summary>
    /// <returns></returns>
    public CoreObject[] GetCoreObjectList()
    {
        var ret = new List<CoreObject>();
        foreach (var kv in intanceIdToCoreObjectDic)
        {
            var co = kv.Value;
            if(co.isMarkedDestoryed)
            {
                continue;
            }
            ret.Add(co);
        }
        return ret.ToArray();
    }
}



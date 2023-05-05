using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PoolManager 
{
    readonly static Dictionary<Type, Pool> typeToPoolDic = new Dictionary<Type, Pool>();

    static Pool GetOrCreatePool(Type type)
    {
        Pool pool = null;
        if (typeToPoolDic.ContainsKey(type))
        {
            pool = typeToPoolDic[type];
        }
        else
        {
            pool = new Pool();
            typeToPoolDic[type] = pool;
        }
        return pool;
    }

    static public T Take<T>() where T : class, new()
    {
        var type = typeof(T);
        T obj = null;
        var pool = GetOrCreatePool(type);
        if (pool != null)
        {
            obj = pool.Get() as T;
        }

        if (obj == null)
        {
            obj = new T();
        }
        return obj;
    }

    static public void Put(object obj)
    {
        var type = obj.GetType();
        var pool = GetOrCreatePool(type);
        if (pool != null)
        {
            pool.Release(obj);
        }
    }

}

public class Pool
{
    private readonly Stack stack = new Stack();

    public object Get()
    {
        if (stack.Count == 0)
            return null;

        var element = stack.Pop();
        return element;
    }

    public void Release(object element)
    {
        if (stack.Count > 0 && ReferenceEquals(stack.Peek(), element))
            Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
        stack.Push(element);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

public class Property<T>
{
    object obj;
    PropertyInfo propertyInfo;

    public Property(object obj, string propertyName)
    {
        this.obj = obj;
        var type = obj.GetType();
        var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var proerptyType = propertyInfo.PropertyType;
        if(proerptyType != typeof(T))
        {
            throw new Exception($"[Property] biding error. proeprty {type}.{propertyName} type is not {typeof(T)}");
        }
        this.propertyInfo = propertyInfo;
    }

    public Property(Type type, string propertyName)
    {
        var propertyInfo = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        this.propertyInfo = propertyInfo;
        var proerptyType = propertyInfo.PropertyType;
        if (proerptyType != typeof(T))
        {
            throw new Exception($"[Property] biding error. proeprty {type}.{propertyName} type is not {typeof(T)}");
        }
    }

    public T Value
    {
        get
        {
            var ret = (T)this.propertyInfo.GetValue(this.obj);
            return ret;
        }
        set
        {
            this.propertyInfo.SetValue(this.obj, value);
        }
    }
}

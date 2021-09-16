using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;

public static class ReflectionUtil
{
    public static T? TryGetPropertyValue<T>(object obj, string propertyName) where T : struct
    {
        var type = obj.GetType();
        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo != null)
        {
            var value = propertyInfo.GetValue(obj);
            var ret = (T)value;
            return ret;
        }
        return null;
    }

    public static bool HasProperty<T>(object obj, string propertyName) 
    {
        var type = obj.GetType();
        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo != null)
        {
            // 类型匹配
            var valueType = propertyInfo.PropertyType;
            var expectedType = typeof(T);
            if(valueType == expectedType)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public static T CallMethod<T>(object obj, string methodName, object[] paramList) 
    {
        var type = obj.GetType();
        var methodInfo = type.GetMethod(methodName);
        var ret = methodInfo.Invoke(obj, paramList);
        var t = (T)ret;
        return t;
    }

    public static List<Type> GetSubClasses<T>(Assembly assembly)
    {
        var subTypeQuery = from t in assembly.GetTypes()
                           where (typeof(T).IsAssignableFrom(t) && typeof(T) != t)
                           select t;
        return subTypeQuery.ToList();
    }

}

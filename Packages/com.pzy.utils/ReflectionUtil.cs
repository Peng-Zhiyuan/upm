using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;

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

    public static List<Type> GetSubClassesInAllAssemblies<T>()
    {
        var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
        var ret = new List<Type>();
        foreach(var assembly in assemblyList)
        {
            var list = GetSubClasses<T>(assembly);
            ret.AddRange(list);
        }
        return ret;
    }
    
    public static Func<object, object[], object> GetExecuteDelegate(MethodInfo methodInfo)
    {
        // parameters to execute
        ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "instance");
        ParameterExpression parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

        // build parameter list
        List<Expression> parameterExpressions = new List<Expression>();
        ParameterInfo[] paramInfos = methodInfo.GetParameters();
        for (int i = 0; i < paramInfos.Length; i++)
        {
            // (Ti)parameters[i]
            BinaryExpression valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
            UnaryExpression valueCast = Expression.Convert(valueObj, paramInfos[i].ParameterType);

            parameterExpressions.Add(valueCast);
        }

        // non-instance for static method, or ((TInstance)instance)
        Expression instanceCast = methodInfo.IsStatic ? null :
            Expression.Convert(instanceParameter, methodInfo.ReflectedType);

        // static invoke or ((TInstance)instance).Method
        MethodCallExpression methodCall = Expression.Call(instanceCast, methodInfo, parameterExpressions);
        
        // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
        if (methodCall.Type == typeof(void))
        {
            Expression<Action<object, object[]>> lambda = Expression.Lambda<Action<object, object[]>>(
                methodCall, instanceParameter, parametersParameter);

            Action<object, object[]> execute = lambda.Compile();
            return (instance, parameters) =>
            {
                execute(instance, parameters);
                return null;
            };
        }
        else
        {
            UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(object));
            Expression<Func<object, object[], object>> lambda = Expression.Lambda<Func<object, object[], object>>(
                castMethodCall, instanceParameter, parametersParameter);

            return lambda.Compile();
        }
    }

}

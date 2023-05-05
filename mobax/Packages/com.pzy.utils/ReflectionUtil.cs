using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Linq.Expressions;

public static class ReflectionUtil
{
    public static (T, bool) TryGetPropertyValue<T>(object obj, string propertyName)
    {
        var type = obj.GetType();
        var propertyInfo = type.GetProperty(propertyName);
        if (propertyInfo != null)
        {
            var value = propertyInfo.GetValue(obj);
            var ret = (T)value;
            return (ret, true);
        }
        return (default(T), false);
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

    public static T GetField<T>(object obj, string fieldName)
    {
        var type = obj.GetType();
        var fieldInfo = type.GetField(fieldName);
        var ret = fieldInfo.GetValue(obj);
        var t = (T)ret;
        return t;
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

    public static List<Type> GetAttributedClasses<TAttribute>(Assembly assembly) where TAttribute : Attribute
    {
        var subTypeQuery = from t in assembly.GetTypes()
                           where HasCustomAttribute<TAttribute>(t)
                           select t;
        return subTypeQuery.ToList();
    }

    public static bool HasCustomAttribute<TAttribute>(Type type) where TAttribute : Attribute
    {
        var t = type.GetCustomAttribute<TAttribute>();
        if(t != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 在所有程序集中寻找指定类型的子类
    /// * 不包括自己
    /// </summary>
    public static List<Type> GetSubClassesInAllAssemblies<T>(string excludeAssembleName = null)
    {
        var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
        var ret = new List<Type>();
        foreach(var assembly in assemblyList)
        {
            if (excludeAssembleName != null)
            {
                var name = assembly.GetName().Name;
                if(name == excludeAssembleName)
                {
                    continue;
                }
            }
           
            var list = GetSubClasses<T>(assembly);
            ret.AddRange(list);
        }
        return ret;
    }

    /// <summary>
    /// 在所有程序集中寻找包含指定 Attribute 的类型
    /// </summary>
    public static List<Type> GetAttributedClassesInAllAssemblies<T>() where T :Attribute
    {
        var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
        var ret = new List<Type>();
        foreach (var assembly in assemblyList)
        {
            var list = GetAttributedClasses<T>(assembly);
            ret.AddRange(list);
        }
        return ret;
    }

    public static Assembly GetAssembly(string name)
    {
        var assemblyList = AppDomain.CurrentDomain.GetAssemblies();
        foreach(var assembly in assemblyList)
        {
            if(assembly.GetName().Name == name)
            {
                return assembly;
            }
        }
        return null;
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

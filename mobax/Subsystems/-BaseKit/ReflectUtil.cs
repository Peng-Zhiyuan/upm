using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class ReflectUtil : Single<ReflectUtil>
{
    /// 
    /// 获取类中的属性值
    /// 
    public string GetModelString(string fieldName, object obj)
    {
        try
        {
            Type Ts = obj.GetType();
            object o = Ts.GetProperty(fieldName).GetValue(obj, null);
            string Value = Convert.ToString(o);
            if (string.IsNullOrEmpty(Value)) return null;
            return Value;
        }
        catch
        {
            return null;
        }
    }

    public int GetModelInt(string fieldName, object obj)
    {
        try
        {
            Type Ts = obj.GetType();
            object o = Ts.GetProperty(fieldName).GetValue(obj, null);
            int ? Value = Convert.ToInt32(o);
            if (Value != null) return (int)Value;
            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// 
    /// 获取类中的属性值
    /// 
    public object GetModelObject(string fieldName, object obj)
    {
        try
        {
            Type Ts = obj.GetType();
            object o = Ts.GetProperty(fieldName).GetValue(obj, null);
            return o;
        }
        catch
        {
            return null;
        }
    }

    public T GetModelValue<T>(string fieldName, object obj) where T : class
    {
        try
        {
            Type Ts = obj.GetType();
            object o = Ts.GetProperty(fieldName).GetValue(obj, null);
            T Value = o as T;
            return Value;
        }
        catch
        {
            return null;
        }
    }

    /// 
    /// 设置类中的属性值
    /// 
    public bool SetModelValue(string fieldName, object Value, object obj)
    {
        try
        {
            Type Ts = obj.GetType();
            object v = Convert.ChangeType(Value, Ts.GetProperty(fieldName).PropertyType);
            Ts.GetProperty(fieldName).SetValue(obj, v, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public object InvokeGetMethod(string methodName, object obj)
    {
        Type Ts = obj.GetType();
        BindingFlags flag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase;
        PropertyInfo property = Ts.GetProperty(methodName, flag);
        MethodInfo method = property.GetGetMethod();
        object o = method.Invoke(obj, null);
        return o;
    }

    public object InvokeMethod(string MethodName, object root, object[] parameters)
    {
        Type Ts = root.GetType();
        MethodInfo method = Ts.GetMethod(MethodName);
        BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
        object result = method.Invoke(root, flag, Type.DefaultBinder, parameters, null);
        return result;
    }

    /// <summary>
    /// 获取指定接口下的子类
    /// </summary>
    /// <param name="type">接口类型</param>
    /// <param name="nspace">接口所在命名空间</param>
    /// <returns></returns>
    public static List<Type> FromAssemblyGetInterfaceTypes(Type type, string nspace = "")
    {
        List<Type> lst = new List<Type>();
        IEnumerable<Type> typeLst = null;
        if (!string.IsNullOrEmpty(nspace))
        {
            //typeLst = from t in Assembly.GetExecutingAssembly().GetTypes() where t.Namespace == nspace select t;
            typeLst = from t in typeof(ReflectUtil).Assembly.GetTypes() where t.Namespace == nspace select t;
        }
        else
        {
            //typeLst = Assembly.GetExecutingAssembly().GetTypes();
            typeLst = typeof(ReflectUtil).Assembly.GetTypes();
        }
        typeLst.ToList().ForEach(t =>
                        {
                            if (!t.IsInterface)
                            {
                                Type[] ins = t.GetInterfaces();
                                for (int j = 0; j < ins.Length; j++)
                                {
                                    if (ins[j] == null
                                        || !type.IsAssignableFrom(ins[j]))
                                        continue;
                                    lst.Add(t);
                                }
                            }
                        }
        );
        return lst;
    }

    /// <summary>
    /// 获取指定基类下的子类
    /// </summary>
    /// <param name="nspace"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    public static List<Type> FromAssemblyGetTypes(Type baseType, string nspace = "")
    {
        List<Type> lst = new List<Type>();
        IEnumerable<Type> typeLst = null;
        if (!string.IsNullOrEmpty(nspace))
        {
            //typeLst = from t in Assembly.GetExecutingAssembly().GetTypes() where t.Namespace == nspace select t;
            typeLst = from t in typeof(ReflectUtil).Assembly.GetTypes() where t.Namespace == nspace select t;
        }
        else
        {
            //typeLst = Assembly.GetExecutingAssembly().GetTypes();
            typeLst = typeof(ReflectUtil).Assembly.GetTypes();
        }
        typeLst.ToList().ForEach(t =>
                        {
                            if (t.IsSubclassOf(baseType)
                                && t.Name != baseType.Name)
                            {
                                lst.Add(t);
                            }
                        }
        );
        return lst;
    }
}
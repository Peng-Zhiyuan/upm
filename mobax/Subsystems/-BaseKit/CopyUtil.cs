using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Reflection;
public class CopyUtil : Single<CopyUtil>
{
    public  T DeepCopyByBinary<T>(T obj)
    {
    　　object retval;
    　　using (MemoryStream ms = new MemoryStream())
    　　{
    　　　　BinaryFormatter bf = new BinaryFormatter();
    　　　　bf.Serialize(ms, obj);
    　　　　ms.Seek(0, SeekOrigin.Begin);
    　　　　retval = bf.Deserialize(ms);
    　　　　ms.Close();
    　　}
    　　return (T)retval;
    }
    public  T DeepCopyByReflection<T>(T obj)
    {
    　　if (obj is string || obj.GetType().IsValueType)
    　　return obj;

    　　object retval = Activator.CreateInstance(obj.GetType());
    　　FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance);
    　　foreach(var field in fields)
    　　{
    　　　　try
    　　　　{
    　　　　　　field.SetValue(retval, DeepCopyByReflection(field.GetValue(obj)));
    　　　　}
    　　　　catch { }
    　　}

    　　return (T)retval;
    }
}

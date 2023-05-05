using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
public class SerializeExtend : MonoBehaviour
{
    public sealed class Vector3SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Vector3 v3 = (Vector3)obj;
            v3.x = info.GetSingle("x");
            v3.y = info.GetSingle("y");
            v3.z = info.GetSingle("z");
            obj = v3;
            return obj;
        }
    }

    public sealed class Vector4SerializationSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector4 v4 = (Vector4)obj;
            info.AddValue("x", v4.x);
            info.AddValue("y", v4.y);
            info.AddValue("z", v4.z);
            info.AddValue("w", v4.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Vector4 v4 = (Vector3)obj;
            v4.x = info.GetSingle("x");
            v4.y = info.GetSingle("y");
            v4.z = info.GetSingle("z");
            v4.w = info.GetSingle("w");
            obj = v4;
            return obj;
        }
    }

    class QuaternionSerializationSurrogate : ISerializationSurrogate
    {

        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {

            Quaternion q = (Quaternion)obj;
            info.AddValue("x", q.x);
            info.AddValue("y", q.y);
            info.AddValue("z", q.z);
            info.AddValue("w", q.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {

            Quaternion q = (Quaternion)obj;
            q.x = info.GetSingle("x");
            q.y = info.GetSingle("y");
            q.z = info.GetSingle("z");
            q.w = info.GetSingle("w");
            obj = q;
            return obj;
        }
    }

    /*
    class Person : ISerializable
    {
        public int age;
        public string name;
        public string sex;
        public Person() { }
        public Person(SerializationInfo info, StreamingContext context)
        {
            age = info.GetInt32("age");
            name = info.GetString("name");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("age", age);
            info.AddValue("name", name);
        }
    }
    */

}

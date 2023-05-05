using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolveResult
{
    public ResultValueType type;
    public int intValue;
    public bool boolValue;
    public string stringValue;

    public object AnyValue
    {
        get
        {
            if(type == ResultValueType.Bool)
            {
                return this.boolValue;
            }
            else if(type == ResultValueType.String)
            {
                return this.stringValue;
            }
            else if(type == ResultValueType.Int)
            {
                return this.intValue;
            }
            return null;
        }
    }

    public T ToNativeValue<T>(T defaultValue)
    {
        var t = typeof(T);
        if(t == typeof(int))
        {
            if(this.type == ResultValueType.Int)
            {
                return (T)(object)this.intValue;
            }
        }
        else if(t == typeof(bool))
        {
            if(this.type == ResultValueType.Bool)
            {
                return (T)(object)this.boolValue;
            }
        }
        else if(t == typeof(string))
        {
            if(this.type == ResultValueType.String)
            {
                return (T)(object)this.stringValue;
            }
        }
        return defaultValue;
    }

    public override string ToString()
    {
        if(this.type == ResultValueType.Int)
        {
            return this.intValue.ToString();
        }
        else if(this.type == ResultValueType.Bool)
        {
            return this.boolValue.ToString();
        }
        else if(this.type == ResultValueType.String)
        {
            return $"\"{this.stringValue}\"";
        }
        return base.ToString();
    }

    public static ResolveResult Create(bool b)
    {
        var r = new ResolveResult();
        r.type = ResultValueType.Bool;
        r.boolValue = b;
        return r;
    }
}

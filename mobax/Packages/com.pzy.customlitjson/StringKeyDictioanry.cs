using System;
using System.Collections.Generic;
using System.Collections;

public class StringKeyDictionary<TValue> : Dictionary<string, TValue>
{
    bool ignoreCase;

    public StringKeyDictionary(bool ignoreCase)
    {
        this.ignoreCase = ignoreCase;
    }

    public new void Add(string key, TValue value)
    {
        if (this.ignoreCase)
        {
            key = key.ToLower();
        }
        base.Add(key, value);
    }

    public new bool ContainsKey(string key)
    {
        if (this.ignoreCase)
        {
            key = key.ToLower();
        }
        var ret = base.ContainsKey(key);
        return ret;
    }

    public new TValue this[string key]
    {
        get
        {
            if (this.ignoreCase)
            {
                key = key.ToLower();
            }
            var ret = base[key];
            return ret;
        }
    }



}
using System.Collections.Generic;
using System;

public static class DictionaryUtil
{
    public static TV TryGet<TK, TV>(this Dictionary<TK, TV> dic, TK key, TV _default)
    {
        TV ret;
        var b = dic.TryGetValue(key, out ret);
        if(b)
        {
            return ret;
        }
        else
        {
            return _default;
        }
    }

    public static TValue GetOrCreate<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TValue> createDelegate)
    {
        var b = dic.TryGetValue(key, out var value);
        if(b)
        {
            return value;
        }
        var newValue = createDelegate.Invoke();
        dic[key] = newValue;
        return newValue;
    }

    public static List<TElement> GetOrCreateList<TK, TElement>(this Dictionary<TK, List<TElement>> dic, TK key)
    {
        List<TElement> list;
        dic.TryGetValue(key, out list);
        if(list == null)
        {
            list = new List<TElement>();
            dic[key] = list;
        }
        return list;
    }

    public static Dictionary<TInnerKey, TElement> GetOrCreateDic<TK, TInnerKey, TElement>(this Dictionary<TK, Dictionary<TInnerKey, TElement>> dic, TK key)
    {
        Dictionary<TInnerKey, TElement> innerDic;
        dic.TryGetValue(key, out innerDic);
        if (innerDic == null)
        {
            innerDic = new Dictionary<TInnerKey, TElement>();
            dic[key] = innerDic;
        }
        return innerDic;
    }

    public static List<TV> GetValueList<TK, TV>(this Dictionary<TK, TV> dic)
    {
        var ret = new List<TV>();
        foreach(var kv in dic)
        {
            var item = kv.Value;
            ret.Add(item);
        }
        return ret;
    }

    public static Dictionary<TK, bool> GetCommon<TK, TV>(Dictionary<TK, TV> a, Dictionary<TK, TV> b)
    {
        var ret = new Dictionary<TK, bool>();
        foreach(var kv in a)
        {
            var key = kv.Key;
            if(b.ContainsKey(key))
            {
                ret[key] = true;
            }
        }
        return ret;
    }
}
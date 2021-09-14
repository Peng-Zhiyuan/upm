using System.Collections.Generic;

public static class DictionaryUtil
{
    public static TV TryGet<TK, TV>(Dictionary<TK, TV> dic, TK key, TV _default)
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

    public static List<T> Keys2List<T, T1>(this Dictionary<T, T1>.KeyCollection keys)
    {
        List<T> resKeys = new List<T>();
        foreach (T key in keys)
        {
            resKeys.Add(key);
        }
        return resKeys;
    }

    public static List<TElement> GetOrCreateList<TK, TElement>(Dictionary<TK, List<TElement>> dic, TK key)
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

    public static List<TV> GetValueList<TK, TV>(Dictionary<TK, TV> dic)
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
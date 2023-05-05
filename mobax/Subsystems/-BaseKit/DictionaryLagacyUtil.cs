using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DictionaryLagacyUtil {
    public static List<TVal> ToArray<TKey, TVal>(this Dictionary<TKey, TVal> self) {
        List<TVal> list = new List<TVal>();
        foreach (TVal val in self.Values) {
            list.Add(val);
        }

        return list;
    }

    public static TVal TryGetValue<TKey, TVal>(this Dictionary<TKey, TVal> self, TKey key, TVal defaultValue = default(TVal))
    {
        if (self.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }
}
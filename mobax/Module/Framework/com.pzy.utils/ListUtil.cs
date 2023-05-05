using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListUtil
{

    public static List<TTo> ConvertElementType<TTo>(this IList list) 
    {
        var ret = new List<TTo>();
        foreach(var one in list)
        {
            var e = (TTo)(object)one;
            ret.Add(e);
        }
        return ret;
    }

    public static T TryGet<T>(List<T> list, int index, T @default = default)
    {
        if(list == null)
        {
            return @default;
        }
        if(index < 0 || index >= list.Count)
        {
            return @default;
        }
        return list[index];
    }

    public static List<TOut> ChangeElementType<TIn, TOut>(List<TIn> list)
    {
        var result = new List<TOut>();
        foreach (var item in list)
        {
            result.Add((TOut)(object)item);
        }
        return result;
    }

    public static List<object> ToObjectList<T>(List<T> list)
    {
        var ret = ChangeElementType<T, object>(list);
        return ret;
    }

    /// <summary>
    /// 获取子列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="startIndex">开始的下标</param>
    /// <param name="length">截取个数，null 表示直到结尾</param>
    /// <returns></returns>
    public static List<T> Sublist<T>(List<T> list, int startIndex, int? length)
    {
        var ret = new List<T>();
        var count = 0;
        for(int i = startIndex; i < list.Count; i++)
        {
            if (length != null)
            {
                if (count >= length)
                {
                    break;
                }
            }
            var one = list[i];
            ret.Add(one);
            count++;
        }
        return ret;
    }



    public static void RemoveCommonElements<T>(List<T> list1, List<T> list2)
    {
        if(list1 == null || list2 == null)
        {
            return;
        }
        for (int i = list1.Count - 1; i >= 0; i--)
        {
            if (list2.Contains(list1[i]))
            {
                list2.Remove(list1[i]);
                list1.RemoveAt(i);
            }
        }
    }
}

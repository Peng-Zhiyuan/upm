using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayUtil 
{
    public static bool Contains<T>(T[] array, T element)
    {
        for (int i = 0; i < array.Length; i++)
        {
            var one = array[i];
            if(one.Equals(element))
            {
                return true;
            }
        }
        return false;
    }

    public static T TryGet<T>(T[] list, int index, T @default = default)
    {
        if (list == null)
        {
            return @default;
        }
        if (index < 0 || index >= list.Length)
        {
            return @default;
        }
        return list[index];
    }
}

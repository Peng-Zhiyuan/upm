using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class VectorExtend
{
    public static string ToDetailString(this Vector3 v, string separate = ", ")
    {
        return v.x + separate + v.y + separate + v.z;
    }

}
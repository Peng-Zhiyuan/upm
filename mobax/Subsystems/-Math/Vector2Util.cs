using UnityEngine;
using System;

public static class Vector2Util {
    /// <summary>
    /// 用两个点指定向量
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    public static Vector2 FromTo(Vector2 startPoint, Vector2 endPoint) {
        var x = endPoint.x - startPoint.x;
        var y = endPoint.y - startPoint.y;
        var ret = new Vector2(x, y);
        return ret;
    }

    public static Vector2 AngleAndModulusToVector2(float angleInDeg, float modulus) {
        var angleInRad = angleInDeg * Mathf.Deg2Rad;
        var x = Mathf.Cos(angleInRad) * modulus;
        var y = Mathf.Sin(angleInRad) * modulus;
        var ret = new Vector2(x, y);
        return ret;
    }

    public static Vector2 CreateOfPower(float a) {
        var ret = new Vector2(a, a);
        return ret;
    }
}
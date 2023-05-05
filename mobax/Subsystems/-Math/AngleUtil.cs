using UnityEngine;
using System;

public static class AngleUtil
{
    /// <summary>
    /// 返回两个点移动向量的角度(deg)
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <returns></returns>
    public static float FromTo(Vector2 startPoint, Vector2 endPoint)
    {
        var vector = Vector2Util.FromTo(startPoint, endPoint);
        var r = vector.magnitude;
        if(r == 0)
        {
            return 0;
        }
        var sinThlta = vector.y / r;
        var angleRad = Mathf.Asin(sinThlta);
        var angleDeg = angleRad * Mathf.Rad2Deg;
        if(vector.x < 0)
        {
            angleDeg = 180 - angleDeg;
        }
        if(angleDeg > 180)
        {
            angleDeg -= 360;
        }
        return angleDeg;
        // return angleDeg;
        // var vector = Vector2Util.FromTo(startPoint, endPoint);
        // if(vector.x == 0)
        // {
        //     if(vector.y > 0)
        //     {
        //         return 90;
        //     }
        //     else if(vector.y < 0)
        //     {
        //         return -90;
        //     }
        //     return 0;
        // }
        // var tanThelta = vector.y / vector.x;
        // var angleRad = Mathf.Atan(tanThelta);
        // var angleDeg = angleRad * Mathf.Rad2Deg;
        // if(vector.x < 0)
        // {
        //     angleDeg += 180;
        // }
        // if(angleDeg > 180)
        // {
        //     angleDeg -= 360;
        // }
        // return angleDeg;
    }

}
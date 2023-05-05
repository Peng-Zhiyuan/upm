using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RectUtil : Single<RectUtil>
{
    private float GetCross(Vector3 p1, Vector3 p2, Vector3 p)
    {
         return (p2.x - p1.x) * (p.z - p1.z) - (p.x - p1.x) * (p2.z - p1.z);
    }
    //判断点p是否在p1p2p3p4的正方形内
    public bool IsPointInMatrix(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 p)
    {
        var isPointIn = GetCross(p1, p2, p) * GetCross(p3, p4, p) >= 0 && GetCross(p2, p3, p) * GetCross(p4, p1, p) >= 0;
        return isPointIn;
    }
}

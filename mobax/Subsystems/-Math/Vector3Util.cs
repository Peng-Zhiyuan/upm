using System.Collections.Generic;
using UnityEngine;

public static class Vector3Util
{
    public static Vector3 StringToVec3(string param)
    {
        if (string.IsNullOrEmpty(param))
        {
            return Vector3.zero;
        }
        string[] datas = param.Split(',');
        if (datas.Length != 3)
        {
            return Vector3.zero;
        }
        Vector3 tempVec3 = Vector3.zero;
        tempVec3.x = float.Parse(datas[0]);
        tempVec3.y = float.Parse(datas[1]);
        tempVec3.z = float.Parse(datas[2]);
        return tempVec3;
    }

    /// <summary>
    /// 计算AB与CD两条线段的交点.
    /// </summary>
    /// <param name="a">A点</param>
    /// <param name="b">B点</param>
    /// <param name="c">C点</param>
    /// <param name="d">D点</param>
    /// <param name="intersectPos">AB与CD的交点</param>
    /// <returns>是否相交 true:相交 false:未相交</returns>
    public static bool TryGetIntersectPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 intersectPos)
    {
        intersectPos = Vector3.zero;
        Vector3 ab = b - a;
        Vector3 ca = a - c;
        Vector3 cd = d - c;
        Vector3 v1 = Vector3.Cross(ca, cd);
        if (Mathf.Abs(Vector3.Dot(v1, ab)) > 1e-6)
        {
            // 不共面
            return false;
        }
        if (Vector3.Cross(ab, cd).sqrMagnitude <= 1e-6)
        {
            // 平行
            return false;
        }
        Vector3 ad = d - a;
        Vector3 cb = b - c;
        // 快速排斥
        if (Mathf.Min(a.x, b.x) > Mathf.Max(c.x, d.x)
            || Mathf.Max(a.x, b.x) < Mathf.Min(c.x, d.x)
            || Mathf.Min(a.y, b.y) > Mathf.Max(c.y, d.y)
            || Mathf.Max(a.y, b.y) < Mathf.Min(c.y, d.y)
            || Mathf.Min(a.z, b.z) > Mathf.Max(c.z, d.z)
            || Mathf.Max(a.z, b.z) < Mathf.Min(c.z, d.z))
            return false;

        // 跨立试验
        if (Vector3.Dot(Vector3.Cross(-ca, ab), Vector3.Cross(ab, ad)) > 0
            && Vector3.Dot(Vector3.Cross(ca, cd), Vector3.Cross(cd, cb)) > 0)
        {
            Vector3 v2 = Vector3.Cross(cd, ab);
            float ratio = Vector3.Dot(v1, v2) / v2.sqrMagnitude;
            intersectPos = a + ab * ratio;
            return true;
        }
        return false;
    }

    public static bool TryGetIntersectPointRect(Vector3 a, Vector3 b, Rect rect, out Vector3 intersectPos)
    {
        Vector3 tempIntersectPos = Vector3.zero;
        Vector3 bLeft = new Vector3(rect.xMin, 0, rect.yMin);
        Vector3 TLeft = new Vector3(rect.xMin, 0, rect.yMax);
        Vector3 bRight = new Vector3(rect.xMax, 0, rect.yMin);
        Vector3 TRight = new Vector3(rect.xMax, 0, rect.yMax);
        if (TryGetIntersectPoint(a, b, bLeft, TLeft, out tempIntersectPos))
        {
            intersectPos = tempIntersectPos;
            return true;
        }
        if (TryGetIntersectPoint(a, b, TLeft, TRight, out tempIntersectPos))
        {
            intersectPos = tempIntersectPos;
            return true;
        }
        if (TryGetIntersectPoint(a, b, TRight, bRight, out tempIntersectPos))
        {
            intersectPos = tempIntersectPos;
            return true;
        }
        if (TryGetIntersectPoint(a, b, bRight, bLeft, out tempIntersectPos))
        {
            intersectPos = tempIntersectPos;
            return true;
        }
        intersectPos = Vector3.zero;
        return false;
    }

    /// <summary>  
    /// 获取中心点坐标  (Y为0)
    /// </summary>  
    /// <param name="p"></param>  
    /// <returns></returns> 
    public static Vector3 GetCenterPoint(List<Vector3> _points)
    {
        int total = _points.Count;
        float lat = 0, lon = 0;
        foreach (Vector3 p in _points)
        {
            lat += p.x;
            lon += p.z;
        }
        lat /= total;
        lon /= total;
        Vector3 centerPoint = new Vector3(lat, 0, lon);
        return centerPoint;
    }

    /// <summary>
    /// 获取两点之间距离一定百分比的一个点
    /// </summary>
    /// <param name="start">起始点</param>
    /// <param name="end">结束点</param>
    /// <param name="distance">起始点到目标点距离百分比</param>
    /// <returns></returns>
    public static Vector3 GetBetweenPoint(Vector3 start, Vector3 end, float percent = 0.5f)
    {
        Vector3 normal = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        return normal * (distance * percent) + start;
    }
}
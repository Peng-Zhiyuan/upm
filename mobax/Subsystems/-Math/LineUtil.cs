using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineUtil : Single<LineUtil>
{

    public float ClosestSqrDistance(Vector3 a, Vector3 b, Vector3 p)
    {
        var m = b - a;
        var t = Vector3.Dot(p - a, m) / Vector3.Dot(m, m);
        var c = a + t * m;
        var exceed = Vector3.Dot(c - a, b - c) < 0;
        if (exceed)
        {
            var d1 = Vector3.SqrMagnitude(p - a);
            var d2 = Vector3.SqrMagnitude(p - b);
            c = d1 < d2 ? a : b;
        }
        var distance = Vector3.SqrMagnitude(p - c);
        return distance;

    }

    public bool InSameLine(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 v1 = a - b;
        Vector3 v2 = p - a;
        Debug.LogError("Vector3.Angle(v1, v2) :" + Vector3.Angle(v1, v2));
        return Vector3.Angle(v1, v2) == 0;// Mathf.Abs(Vector3.Dot(v1, v2)) == 1;
    }
    public Vector3 ClosestPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        var m = b - a;
        var t = Vector3.Dot(p - a, m) / Vector3.Dot(m, m);
        var c = a + t * m;
        var exceed = Vector3.Dot(c - a, b - c) < 0;
        if (exceed)
        {
            var d1 = Vector3.SqrMagnitude(p - a);
            var d2 = Vector3.SqrMagnitude(p - b);
            return d1 < d2 ? a : b;
        }
        return c;

    }
}

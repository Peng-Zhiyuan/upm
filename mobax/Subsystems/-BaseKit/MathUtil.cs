using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public static class MathUtil
{
    public static float PointDistancePower2(Vector2 p1, Vector2 p2)
    {
        var a = p1.x - p2.x;
        var b = p1.y - p2.y;
        return a * a + b * b;
    }

    public static float VectorsToDegress(Vector3 toVector, Vector3 fromVector)
    {
        float angle = Vector3.Angle(fromVector, toVector); //求出两向量之间的夹角
        Vector3 normal = Vector3.Cross(fromVector, toVector); //叉乘求出法线向量
        angle *= Mathf.Sign(-normal.z);
        return angle;
    }

    public static float Angle(Vector3 fromVector, Vector3 toVector, Vector3 axis)
    {
        float angle = Vector3.SignedAngle(fromVector, toVector, axis); //求出两向量之间的夹角
        if (angle < 0)
        {
            angle += 360;
        }
        return angle;
    }

    public static bool IsRectIntersect(Rect a, Rect b)
    {
        if (a.min.x > b.max.x)
        {
            return false;
        }
        if (a.max.x < b.min.x)
        {
            return false;
        }
        if (a.min.y > b.max.y)
        {
            return false;
        }
        if (a.max.y < b.min.y)
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 设置一维向量的模
    /// </summary>
    /// <param name="origin">原始向量</param>
    /// <param name="mode">要设置的模，必须为正数</param>
    /// <returns></returns>
    public static float SetVectorMode(float origin, float mode)
    {
        if (origin > 0)
        {
            return mode;
        }
        else if (origin < 0)
        {
            return -mode;
        }
        return 0;
    }

    public static bool IsRange(int val)
    {
        int num = UnityEngine.Random.Range(0, 1000);
        return num < val;
    }

    public static Vector3 RandomShperePos(float innerRadius, float toOutderadd, Vector3 centerPos)
    {
        Vector3 p = Random.insideUnitSphere * toOutderadd;
        Vector3 pos = p.normalized * (innerRadius + p.magnitude);
        return pos + centerPos;
    }

    //获取二阶贝塞尔曲线路径数组
    private static Vector3[] Bezier2Path(Vector3 startPos, Vector3 controlPos, Vector3 endPos, int count)
    {
        Vector3[] path = new Vector3[(int)count];
        for (int i = 1; i <= count; i++)
        {
            float t = i * 1f / count;
            path[i - 1] = (1 - t) * (1 - t) * startPos + 2 * t * (1 - t) * controlPos + t * t * endPos;
        }
        return path;
    }

    public static Vector3[] DOBezier(GameObject go, Vector3 startPos, Vector3 endPos, int count, float time, int interval, float offset, float outAdd, Action callback)
    {
        Vector3[] pathvec = new Vector3[(count - 1) * interval];
        Vector3 start = startPos;
        Vector3 end = endPos;
        var dir = (endPos - startPos).normalized;
        var edir = GetVerticalDir(dir);
        var dis = Vector3.Distance(endPos, startPos);
        for (int i = 0; i < count - 1; i++)
        {
            if (i == count - 2)
            {
                end = endPos;
            }
            else
            {
                end = dir * (i + 1) / count * dis + startPos;
                end = RandomShperePos(offset, outAdd, end);
            }
            var controlPoint = dir * (i + UnityEngine.Random.Range(0.2f, 0.8f)) / count * dis + startPos;
            var temp_dir = edir * UnityEngine.Random.Range(0.2f, 1.0f) * offset;
            temp_dir = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * temp_dir;
            controlPoint = temp_dir + controlPoint;
            Vector3[] temp = Bezier2Path(start, controlPoint, end, interval);
            for (int j = 0; j < temp.Length; j++)
            {
                pathvec[i * interval + j] = temp[j];
            }
            start = end;
        }
        go.transform.DOPath(pathvec, time).SetEase(Ease.InOutSine).OnComplete(() => { callback.Invoke(); });
        return pathvec;
    }

    public static Vector3 GetVerticalDir(Vector3 _dir)
    {
        //（_dir.x,_dir.z）与（？，1）垂直，则_dir.x * ？ + _dir.z * 1 = 0
        if (_dir.z == 0)
        {
            return new Vector3(0, 0, -1);
        }
        else
        {
            return new Vector3(-_dir.z / _dir.x, 0, 1).normalized;
        }
    }

    public static string BigNumShortStr(int num)
    {
        if (num > 1000000)
        {
            return (num / 1000000) + "M";
        }
        else if (num > 1000)
        {
            return (num / 1000) + "K";
        }
        else
        {
            return num.ToString();
        }
    }

    public static bool IsInRectRange(Rect rect, Vector2 pos)
    {
        if (pos.x < rect.xMin)
        {
            return false;
        }
        if (pos.x > rect.xMax)
        {
            return false;
        }
        if (pos.y < rect.yMin)
        {
            return false;
        }
        if (pos.y > rect.yMax)
        {
            return false;
        }
        return true;
    }

    public static float Distance(Vector3 Start, Vector3 End)
    {
        float dx = End.x - Start.x;
        float dy = End.y - Start.y;
        float dz = End.z - Start.z;
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public static float DoubleDistanceNoY(Vector3 Start, Vector3 End)
    {
        float dx = End.x - Start.x;
        float dz = End.z - Start.z;
        return dx * dx + dz * dz;
    }

    public static float DoubleDistance(Vector3 Start, Vector3 End)
    {
        return (Start - End).sqrMagnitude;
    }

    public static int ErrorStringToInt = 0;
    public static float ErrorStringToFloat = 0.0f;

    public static int StringToInt(string str)
    {
        int strInt = 0;
        if (string.IsNullOrEmpty(str))
        {
            Debug.LogError("string is null");
            return ErrorStringToInt;
        }
        if (!int.TryParse(str, out strInt))
        {
            Debug.LogError("string cant to int " + str);
            return ErrorStringToInt;
        }
        return strInt;
    }

    public static float StringToFloat(string str)
    {
        float strFloat = 0.0f;
        if (string.IsNullOrEmpty(str))
        {
            Debug.LogError("string is null");
            return ErrorStringToFloat;
        }
        if (!float.TryParse(str, out strFloat))
        {
            Debug.LogError("string cant to float " + str);
            return ErrorStringToFloat;
        }
        return strFloat;
    }

    public static List<int> ParseStringToInt(string param)
    {
        List<int> temp = new List<int>();
        if (string.IsNullOrEmpty(param))
            return temp;
        string[] datas = param.Split(',');
        for (int i = 0; i < datas.Length; i++)
        {
            if (string.IsNullOrEmpty(datas[i]))
                continue;
            temp.Add(int.Parse(datas[i]));
        }
        return temp;
    }

    //获取值在-1到1之间，根据需求上下求整，用于战斗数值
    public static void ValueAmendToFloat(ref float value)
    {
        if (Mathf.Abs(value) < 1.0f
            && value != 0.0f)
        {
            if (value < 0.0f)
                value = -1.0f;
            else
                value = 1.0f;
        }
    }

    //值比对
    public static bool ValueIsSame(float value1, float value2)
    {
        float offset = value1 - value2;
        if (Mathf.Abs(offset) < 0.1f)
            return true;
        return false;
    }

    public static Vector3 FindCenterPos(List<Vector3> targetList)
    {
        Vector3 centerPos = new Vector3();
        for (int i = 0; i < targetList.Count; i++)
        {
            centerPos += targetList[i];
        }
        centerPos /= targetList.Count;
        return centerPos;
    }
}
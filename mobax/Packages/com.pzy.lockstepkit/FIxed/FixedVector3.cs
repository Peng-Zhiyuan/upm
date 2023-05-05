using UnityEngine;
using System;

[Serializable]
public struct FixedVector3
{
    public Fixed x;
    public Fixed y;
    public Fixed z;

    public FixedVector3(Fixed x, Fixed y, Fixed z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static implicit operator Vector3(FixedVector3 fixedV3)
    {
        var v3 = fixedV3.ToVector3();
        return v3;
    }

    public static implicit operator FixedVector3(Vector3 v3)
    {
        var fixedV3 = FromVector3(v3);
        return fixedV3;
    }

    public Vector3 ToVector3()
    {
        var ret = new Vector3(x, y, z);
        return ret;
    }

    public static FixedVector3 FromVector3(Vector3 v)
    {
        var ret = new FixedVector3(v.x, v.y, v.z);
        return ret;
    }

    public static FixedVector3 operator +(FixedVector3 a, FixedVector3 b)
    {
        var ret = new FixedVector3();
        ret.x = a.x + b.x;
        ret.y = a.y + b.y;
        ret.z = a.z + b.z;
        return ret;
    }

    public static FixedVector3 operator -(FixedVector3 a, FixedVector3 b)
    {
        var ret = new FixedVector3();
        ret.x = a.x - b.x;
        ret.y = a.y - b.y;
        ret.z = a.z - b.z;
        return ret;
    }

    public static FixedVector3 operator *(FixedVector3 a, Fixed b)
    {
        var ret = new FixedVector3();
        ret.x = a.x * b;
        ret.y = a.y * b;
        ret.z = a.z * b;
        return ret;
    }

    public static FixedVector3 operator /(FixedVector3 a, Fixed b)
    {
        var ret = new FixedVector3();
        ret.x = a.x / b;
        ret.y = a.y / b;
        ret.z = a.z / b;
        return ret;
    }

    public FixedVector3 normalized
    {
        get { throw new System.Exception("[FixedVector3] normalize property not complete yet"); }
    }

    public Fixed sqrMagnitude
    {
        get
        {
            return this.x * this.x + this.y * this.y + this.z * this.z;
            //throw new System.Exception("[FixedVector3] sqrMagnitude property not complete yet");
        }
    }

    [Obsolete("sqrt root not support in fixed")]
    public Fixed magnitude
    {
        get { throw new System.Exception("[FixedVector3] magnitude property not complete yet"); }
    }

    public static FixedVector3 zero
    {
        get { return new FixedVector3(0, 0, 0); }
    }

    public static FixedVector3 forward
    {
        get { return new FixedVector3(0, 0, 1); }
    }
    public static FixedVector3 right
    {
        get { return new FixedVector3(1, 0, 0); }
    }

    public static FixedVector3 up
    {
        get { return new FixedVector3(0, 1, 0); }
    }

    public static FixedVector3 back
    {
        get { return new FixedVector3(0, 0, -1); }
    }
    public static FixedVector3 left
    {
        get { return new FixedVector3(-1, 0, 0); }
    }

    public static FixedVector3 down
    {
        get { return new FixedVector3(0, -1, 0); }
    }

    public override string ToString()
    {
        return $"FixedVector3 ({this.x}, {this.y}, {this.z})";
    }
}
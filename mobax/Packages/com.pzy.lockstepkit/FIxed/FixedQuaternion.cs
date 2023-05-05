using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FixedQuaternion
{
    Fixed x;
    Fixed y;
    Fixed z;
    Fixed w;


    public FixedQuaternion(Fixed x, Fixed y, Fixed z, Fixed w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public static implicit operator Quaternion(FixedQuaternion fixedV3)
    {
        var v3 = fixedV3.ToQuaternion();
        return v3;
    }

    public static implicit operator FixedQuaternion(Quaternion v3)
    {
        var fixedV3 = FromQuaternion(v3);
        return fixedV3;
    }

    public Quaternion ToQuaternion()
    {
        var ret = new Quaternion(x, y, z, w);
        return ret;
    }

    public static FixedQuaternion FromQuaternion(Quaternion v)
    {
        var ret = new FixedQuaternion(v.x, v.y, v.z, v.w);
        return ret;
    }

    public override string ToString()
    {
        return $"FixedQuaternion ({this.x}, {this.y}, {this.z}, {this.w})";
    }
}

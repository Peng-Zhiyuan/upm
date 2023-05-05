using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CoreTransform : CoreComponent
{
    public FixedVector3 position;
    public FixedQuaternion rotation;
    public FixedVector3 scale;


    [Obsolete("core engine 并不支持 transform 树，localPosition 就是 position")]
    public FixedVector3 localPosition
    {
        get
        {
            return position;
        }
        set
        {
            this.position = value;
        }
    }

    [Obsolete("core engine 并不支持 transform 树，localScale 就是 scale")]
    public FixedVector3 localScale
    {
        get
        {
            return scale;
        }
        set
        {
            this.scale = value;
        }
    }

    [Obsolete("core engine 并不支持 transform 树")]
    public FixedVector3 TransformPoint(FixedVector3 p)
    {
        return p;
    }

    public Matrix4x4 localToWorldMatrix
    {
        get
        {
            return Matrix4x4.identity;
        }
    }

    public Matrix4x4 worldToLocalMatrix
    {
        get
        {
            return Matrix4x4.identity;
        }
    }
    public FixedVector3 InverseTransformPoint(FixedVector3 point)
    {
        throw new Exception("[CoreTransform] not implement yet");
    }

    public FixedVector3 forward
    {
        get
        {
            throw new Exception("[CoreTransform] forward not implement yet");
        }
    }

    public FixedVector3 up
    {
        get
        {
            throw new Exception("[CoreTransform] forward not implement yet");
        }
    }

    public FixedVector3 Translate(FixedVector3 v)
    {
        throw new Exception("[CoreTransform] Translate not implement yet");
    }
}

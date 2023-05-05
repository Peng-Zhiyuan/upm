using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TransformLagacyUtil
{
    public static void SetLocalScale(this Transform tf, float scale)
    {
        tf.localScale = Vector3.one * scale;
    }

    public static RectTransform rectTransform(this Transform tf)
    {
        return tf.GetComponent<RectTransform>();
    }
    public static void CustomSetParent(this Transform tf, Transform parent, Vector3 position = default, Vector3? localScale = null)
    {
        tf.SetParent(parent);
        tf.localScale = localScale != null?(Vector3)localScale:Vector3.one;
        tf.localPosition = position;
        tf.rotation =  Quaternion.identity;
        //tf.rotation = Quaternion.Euler(Vector3.zero);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpaceUtil 
{
    // 世界矩形到本地矩形
    public static Rect InverseTransformRect(Transform localObj, Rect worldRect)
    {
        var worldMin = worldRect.min;
        var worldMax = worldRect.max;

        var localMin = localObj.InverseTransformPoint(worldMin);
        var localMax = localObj.InverseTransformPoint(worldMax);

        var localWidth = localMax.x - localMin.x;
        var localHeight = localMax.y - localMin.y;

        var localRect = new Rect(localMin.x, localMin.y, localWidth, localHeight);
        return localRect;
    }

    /// <summary>
    /// 本地矩形到世界
    /// </summary>
    public static Rect TransformRect(Transform localObj, Rect localRect)
    {
        var localMin = localRect.min;
        var localMax = localRect.max;

        var worldMin = localObj.TransformPoint(localMin);
        var worldMax = localObj.TransformPoint(localMax);

        var worldWidth = worldMax.x - worldMin.x;
        var worldHeight = worldMax.y - worldMin.y;

        var worldRect = new Rect(worldMin.x, worldMin.y, worldWidth, worldHeight);
        return worldRect;
    }

    /// <summary>
    /// 本地高度到世界高度
    /// </summary>
    public static float TransformHeight(Transform localObj, float localHeight)
    {
        var localV = new Vector3(0, localHeight, 0);
        var worldV = localObj.TransformVector(localV);
        var worldHeight = worldV.y;
        return worldHeight;
    }


    /// <summary>
    /// 世界高度到本地高度
    /// </summary>
    public static float InverseTransformHeight(Transform localObj, float worldHight)
    {
        var worldV = new Vector3(0, worldHight, 0);
        var localV = localObj.InverseTransformVector(worldV);
        var localHeight = localV.y;
        return localHeight;
    }
}

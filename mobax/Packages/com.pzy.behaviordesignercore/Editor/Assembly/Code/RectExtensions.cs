namespace BehaviorDesigner.Editor
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine;


    public static class RectExtensions
    {
 
        public static Rect ScaleSizeBy(this Rect rect, float scale)
        {
            return ScaleSizeBy(rect, scale, rect.center);
        }


        public static unsafe Rect ScaleSizeBy(this Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect rect2 = rect;

            // 将缩放中心点移动到原点
            rect2.x = rect2.x - pivotPoint.x;
            rect2.y = rect2.y - pivotPoint.y;

            // 缩放矩形
            rect2.xMin = rect2.xMin * scale;
            rect2.xMax = rect2.xMax * scale;
            rect2.yMin = rect2.yMin * scale;
            rect2.yMax = rect2.yMax * scale;

            // 还原中心点
            rect2.x = rect2.x + pivotPoint.x;
            rect2.y = rect2.y + pivotPoint.y;
            return rect2;
        }


        public static Vector2 TopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }
    }
}


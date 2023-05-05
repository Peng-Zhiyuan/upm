namespace BehaviorDesigner.Editor
{
    using System;
    using UnityEngine;

    public class EditorZoomArea
    {
        private static Matrix4x4 _prevGuiMatrix;
        private static Rect groupRect;

        static EditorZoomArea()
        {
            Rect rect = new Rect();
            groupRect = rect;
        }

        public static unsafe Rect Begin(Rect screenCoordsArea, float zoomScale)
        {
            GUI.EndGroup();
            Rect rect = RectExtensions.ScaleSizeBy(screenCoordsArea, 1f / zoomScale, RectExtensions.TopLeft(screenCoordsArea));
            rect.y = rect.y + 21f;
            GUI.BeginGroup(rect);
            _prevGuiMatrix = GUI.matrix;
            Matrix4x4 matrixx = Matrix4x4.TRS(RectExtensions.TopLeft(rect), Quaternion.identity, Vector3.one);
            Vector3 vector = Vector3.one;
            vector.x = vector.y = zoomScale;
            GUI.matrix = ((matrixx * Matrix4x4.Scale(vector)) * matrixx.inverse) * GUI.matrix;
            return rect;
        }

        public static void End()
        {
            GUI.matrix = (_prevGuiMatrix);
            GUI.EndGroup();
            groupRect.y = (21f);
            groupRect.width = ((float)Screen.width);
            groupRect.height = ((float)Screen.height);
            GUI.BeginGroup(groupRect);
        }
    }
}


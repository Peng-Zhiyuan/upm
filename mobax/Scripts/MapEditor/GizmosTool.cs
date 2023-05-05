using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosTool 
{
    public static void DrawCircle(Vector3 center, float Radius,  Vector3 Offset, Color c, int VertexCount = 50)
    {
       
        float deltaTheta = (2f * Mathf.PI) / VertexCount;
        float theta = 0f;
        Vector3? oldPos = null;
        for (int i = 0; i < VertexCount + 1; i++)
        {
            Vector3 pos = new Vector3(Radius * Mathf.Cos(theta), 0f, Radius * Mathf.Sin(theta));
            if (oldPos != null)
            {
                DrawLine((Vector3)oldPos, center + pos + Offset, c);
                //Gizmos.DrawLine((Vector3)oldPos, center + pos + Offset);
                //Gizmos.DrawLine((Vector3)oldPos, center + pos + Offset);
            }
            Gizmos.color = c;
            oldPos = center + pos + Offset;

            theta += deltaTheta;
        }
    }

    public static void DrawLine(Vector3 v1, Vector3 v2, Color c)
    {
        Gizmos.color = c;
        //Debug.DrawLine(v1 + Vector3.up * 0.5f, v2 + Vector3.up * 0.5f, c, 0);
        Gizmos.DrawLine(v1, v2);
    }


    public static float VectorsToDegress(Vector3 toVector)
    {
        float angle = Vector3.Angle(Vector3.right, toVector); //求出两向量之间的夹角
    
        Vector3 normal = Vector3.Cross(Vector3.right, toVector);//叉乘求出法线向量

        // angle *= Mathf.Sign (Vector2.Dot(normal,Vector3.up)); 
        angle *= Mathf.Sign(-normal.y);
        //Debug.LogError("normal.y:" + normal.y);
        //Debug.LogError("angle:" + angle);
        return angle;
    }

    public static float VectorsTo360Degress(Vector3 toVector)
    {
        float angle = Vector3.Angle(Vector3.right, toVector); //求出两向量之间的夹角

        Vector3 normal = Vector3.Cross(Vector3.right, toVector);//叉乘求出法线向量
        if (Mathf.Sign(-normal.y) < 0)
        {
            //Debug.LogError("-angle:" + angle);
            angle = 360 - angle;
           // Debug.LogError("angle:" + angle);
        }
        

        // angle *= Mathf.Sign (Vector2.Dot(normal,Vector3.up)); 
        //angle *= Mathf.Sign(-normal.z);

        return angle;
    }

    public static void DrawRect(Vector3[] points, Color c)
    {

        DrawLine(points[0], points[1], c);
        DrawLine(points[1], points[2], c);
        DrawLine(points[2], points[3], c);
        DrawLine(points[0], points[3], c);

    }
    

    public static void DrawFlabellate(Vector3 center, float Radius, Vector3 Offset, Color c, int angle, Vector3 dir, int VertexCount = 50)
    {
        //Gizmos.color = c;
        //DrawLine(center, center + dir*100);

        float deltaTheta = (2f * Mathf.PI) / VertexCount;


        Vector3 oldPos = center;
        float centerAngle = VectorsTo360Degress(dir);
        float start = (centerAngle - angle / 2) * Mathf.PI / 180;

        float end = (centerAngle + angle / 2) * Mathf.PI / 180;
       // Debug.Log("start:"+ start + "  end:"+ end);
        float theta = start;
        while (theta <= end)
        {
            Vector3 pos = new Vector3(Radius * Mathf.Cos(theta), 0f, Radius * Mathf.Sin(theta));
           // Gizmos.color = c;
            DrawLine((Vector3)oldPos, center + pos + Offset, c);
            Gizmos.color = Color.red;
            oldPos = center + pos + Offset;
            theta += deltaTheta;
        }

        //for (int i = 0; i < VertexCount + 1; i++)
        //{
        //    Vector3 pos = new Vector3(Radius * Mathf.Cos(theta), 0f, Radius * Mathf.Sin(theta));
        //    Gizmos.color = c;
        //    Gizmos.DrawLine((Vector3)oldPos, center + pos + Offset);
        //    Gizmos.color = Color.red;
        //    oldPos = center + pos + Offset;
        //    theta += deltaTheta;
        //}

        DrawLine((Vector3)oldPos, center, c);
    }

    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {

#if UNITY_EDITOR
        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        if (view == null)
            return;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0) {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
#endif
    }
#if UNITY_EDITOR
    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {

        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
    
    }
#endif
}

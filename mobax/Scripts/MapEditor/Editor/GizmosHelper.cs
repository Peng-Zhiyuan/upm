using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class GizmosHelper 
{
    public static void SwitchAllGizmos(bool value)
    {
        var annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
        var classId = annotation.GetField("classID");
        var scriptClass = annotation.GetField("scriptClass");

        var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
        var getAnnotations = annotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var setGizmoEnabled = annotationUtility.GetMethod("SetGizmoEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var setIconEnabled = annotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        var annotations = (Array)getAnnotations.Invoke(null, null);
        foreach (var a in annotations) {
            var classIdValue = (int)classId.GetValue(a);
            var scriptClassValue = (string)scriptClass.GetValue(a);

            setGizmoEnabled.Invoke(null, new object[] { classIdValue, scriptClassValue, value ? 1 : 0, false });
            setIconEnabled.Invoke(null, new object[] { classIdValue, scriptClassValue, value ? 1 : 0 });
        }
    }

    public static void SwitchGizmo<T>(bool value)
    {
        var typeName = typeof(T).Name;
        SwitchGizmo(typeName, value);
    }
    public static void SwitchGizmo(string typeName, bool value)
    {
        var annotation = Type.GetType("UnityEditor.Annotation, UnityEditor");
        var classId = annotation.GetField("classID");
        var scriptClass = annotation.GetField("scriptClass");

        var annotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
        var getAnnotations = annotationUtility.GetMethod("GetAnnotations", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var setGizmoEnabled = annotationUtility.GetMethod("SetGizmoEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        var setIconEnabled = annotationUtility.GetMethod("SetIconEnabled", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

        var annotations = (Array)getAnnotations.Invoke(null, null);
        foreach (var a in annotations) {
            var scriptClassValue = (string)scriptClass.GetValue(a);
            if (scriptClassValue.Equals(typeName) == false) continue;
            var classIdValue = (int)classId.GetValue(a);

            setGizmoEnabled.Invoke(null, new object[] { classIdValue, scriptClassValue, value ? 1 : 0, false });
            setIconEnabled.Invoke(null, new object[] { classIdValue, scriptClassValue, value ? 1 : 0 });
            break;
        }
    }

    public static void ArrowForHandle(in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Arrow(TargetType.Handle, pos, direction, color, arrowHeadLength, arrowHeadAngle);
    }

    private static void Arrow(TargetType targetType, in Vector3 pos, in Vector3 direction, in Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        var right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadLength;
        var left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back * arrowHeadLength;
        var up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back * arrowHeadLength;
        var end = pos + direction;
        Color colorPrew;

        switch (targetType) {
            case TargetType.Gizmo:
                colorPrew = Gizmos.color;
                Gizmos.color = color;
                Gizmos.DrawRay(pos, direction);
                Gizmos.DrawRay(end, right);
                Gizmos.DrawRay(end, left);
                Gizmos.DrawRay(end, up);
                Gizmos.DrawRay(end, down);
                Gizmos.color = colorPrew;
                break;

            case TargetType.Debug:
                Debug.DrawRay(end, right, color);
                Debug.DrawRay(end, left, color);
                Debug.DrawRay(end, up, color);
                Debug.DrawRay(end, down, color);
                break;

            case TargetType.Handle:
                colorPrew = Handles.color;
                Handles.color = color;
                Handles.DrawLine(pos, end);
                Handles.DrawLine(end, end + right);
                Handles.DrawLine(end, end + left);
                Handles.DrawLine(end, end + up);
                Handles.DrawLine(end, end + down);
                Handles.color = colorPrew;
                break;
        }
    }

    private enum TargetType
    {
        Gizmo, Debug, Handle
    }
}

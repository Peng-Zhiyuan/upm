#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugCamera))]
public class DebugCameraEditor : Editor
{
    private CameraState curState = CameraState.Disabled;
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        DebugCamera data = (DebugCamera)target;
        
        data.CurCameraState = (CameraState)EditorGUILayout.EnumPopup("当前状态", data.CurCameraState);
        data.CameraState = (CameraState)EditorGUILayout.EnumPopup("调整状态", data.CameraState);
        
        
        
        //myTest.number = EditorGUILayout.FloatField("number", myTest.number);
        //myTest.staticFlagMask = (StaticEditorFlags)EditorGUILayout.EnumMaskField("static Flags", myTest.staticFlagMask);
        data.VFollowTarget =  EditorGUILayout.ToggleLeft("水平角度是否跟随角色",  data.VFollowTarget);
        data.HAngle = EditorGUILayout.FloatField("相机水平角度", data.HAngle);
        data.VAngle = EditorGUILayout.FloatField("相机垂直角度", data.VAngle);
        data.Distance = EditorGUILayout.FloatField("相机距离", data.Distance);
        data.SideOffsetY = EditorGUILayout.FloatField("相机高度", data.SideOffsetY);
        data.SideOffsetX = EditorGUILayout.FloatField("相机水平偏移", data.SideOffsetX);
        data.FOV = EditorGUILayout.FloatField("相机FOV", data.FOV);
        data.LookY = EditorGUILayout.FloatField("角色高度", data.LookY);
        data.MoveSpeed = EditorGUILayout.FloatField("漫游速度", data.MoveSpeed);
        
        data.Curve = EditorGUILayout.CurveField("镜头平移曲线", data.Curve, Color.green, new Rect(0, 1, 1, 2),GUILayout.Height(60), GUILayout.MinWidth(100));

        if (data.CameraState != curState)
        {
            data.ReLoad();
            curState = data.CameraState;
        }
        else if (GUI.changed)
        {
            data.UpdateData();
        }
    }


}
#endif
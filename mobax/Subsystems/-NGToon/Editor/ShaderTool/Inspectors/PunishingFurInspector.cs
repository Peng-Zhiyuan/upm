using UnityEngine;
using UnityEditor;

public class PunishingFurInspector : ShaderGUI
{
    private string localX = "0";
    private string localY = "0";
    private string localZ = "0";
    private string globalX = "0";
    private string globalY = "0";
    private string globalZ = "0";

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material targetMat = materialEditor.target as Material;
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Force Global Eula Angle", GUILayout.Width(138));
        GUILayout.Label("X", GUILayout.Width(12));
        globalX = GUILayout.TextField(globalX);
        GUILayout.Label("Y", GUILayout.Width(12));
        globalY = GUILayout.TextField(globalY);
        GUILayout.Label("Z", GUILayout.Width(12));
        globalZ = GUILayout.TextField(globalZ);
        if (EditorGUI.EndChangeCheck())
        {
            SetForceGlobal(targetMat, globalX, globalY, globalY);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        GUILayout.Label("Force Local Eula Angle", GUILayout.Width(138));
        GUILayout.Label("X", GUILayout.Width(12));
        localX = GUILayout.TextField(localX);
        GUILayout.Label("Y", GUILayout.Width(12));
        localY = GUILayout.TextField(localY);
        GUILayout.Label("Z", GUILayout.Width(12));
        localZ = GUILayout.TextField(localZ);
        if (EditorGUI.EndChangeCheck())
        {
            SetForceLocal(targetMat, localX, localY, localZ);
        }
        EditorGUILayout.EndHorizontal();
        base.OnGUI(materialEditor, properties);
    }
    private Matrix4x4 GetRotateMatrix(Vector3 eularAngle)
    {
        Quaternion q = Quaternion.identity;
        q.eulerAngles = eularAngle;
        return Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
    }

    private void SetForceLocal(Material mat,string sx,string sy,string sz)
    {
        float x, y, z = 0;
        float.TryParse(sx,out x);
        float.TryParse(sy, out y);
        float.TryParse(sz, out z);
        Matrix4x4 rotateMatrix = GetRotateMatrix(new Vector3(x,y,z));
        Vector3 direction = rotateMatrix.MultiplyVector(new Vector3(0, 0, -1));
        mat.SetVector("_ForceLocal", new Vector4(direction.x, direction.y, direction.z, 0));
    }

    private void SetForceGlobal(Material mat, string sx, string sy, string sz)
    {
        float x, y, z = 0;
        float.TryParse(sx, out x);
        float.TryParse(sy, out y);
        float.TryParse(sz, out z);
        Matrix4x4 rotateMatrix = GetRotateMatrix(new Vector3(x, y, z));
        Vector3 direction = rotateMatrix.MultiplyVector(new Vector3(0, 0, -1));
        mat.SetVector("_ForceGlobal", new Vector4(direction.x, direction.y, direction.z, 0));
    }
}
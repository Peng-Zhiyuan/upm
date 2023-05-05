#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
public class MatObseletePropertyWindow : EditorWindow
{
    public static string assetFolderPath = "Assets/Arts/Env";
    [MenuItem("Tools/MatObseleteProperty")]
    public static void ShowWindow()
    {
        EditorWindow thisWindow = EditorWindow.GetWindow(typeof(MatObseletePropertyWindow));
        thisWindow.titleContent = new GUIContent("MatObseleteProperty");
        thisWindow.position = new Rect(Screen.width / 2, Screen.height / 2, 600, 1000);
    }
    public static void RemoveRedundantMaterialShaderKeywords(Material material)
    {
        List<string> materialKeywordsLst = new List<string>(material.shaderKeywords);
        List<string> shaderKeywordsLst = new List<string>();
        var getKeywordsMethod =
            typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
        string[] keywords = (string[])getKeywordsMethod.Invoke(null, new object[] { material.shader });
        shaderKeywordsLst.AddRange(keywords);

        getKeywordsMethod =
            typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
        keywords = (string[])getKeywordsMethod.Invoke(null, new object[] { material.shader });
        shaderKeywordsLst.AddRange(keywords);

        List<string> notExistKeywords = new List<string>();
        foreach (var each in materialKeywordsLst)
        {
            if (!shaderKeywordsLst.Contains(each))
            {
                notExistKeywords.Add(each);
            }
        }

        foreach (var each in notExistKeywords)
        {
            materialKeywordsLst.Remove(each);
        }

        material.shaderKeywords = materialKeywordsLst.ToArray();
    }

    public static void ClearMatObseleteProperty()
    {
        Debug.Log(Application.dataPath);
        string projectPath = Application.dataPath.Replace("Assets", "");
        assetFolderPath = assetFolderPath.Replace(projectPath, "");
        var matList = AssetDatabase.FindAssets("t:Material", new[] { assetFolderPath });

        foreach (var i in matList)
        {
            // EditorUtility.DisplayProgressBar("���������Ƴ�ͳ���ļ�", "����д��ͳ���ļ���...", ix/shaderList.Length);
            var path = AssetDatabase.GUIDToAssetPath(i);
            Material mat = AssetDatabase.LoadAssetAtPath(path, typeof(Material)) as Material;
            Debug.Log(mat.name);
            SerializedObject so = new SerializedObject(mat);
            SerializedProperty m_SavedProperties = so.FindProperty("m_SavedProperties");
            RemoveElement(mat, "m_TexEnvs", m_SavedProperties);
            RemoveElement(mat, "m_Floats", m_SavedProperties);
            RemoveElement(mat, "m_Colors", m_SavedProperties);
            RemoveRedundantMaterialShaderKeywords(mat);
            so.ApplyModifiedProperties();
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    private static void RemoveElement(Material mat, string spName, SerializedProperty saveProperty)
    {
        SerializedProperty property = saveProperty.FindPropertyRelative(spName);
        for (int i = property.arraySize - 1; i >= 0; i--)
        {
            var prop = property.GetArrayElementAtIndex(i);
            string propertyName = prop.displayName;
            if (!mat.HasProperty(propertyName))
            {
                property.DeleteArrayElementAtIndex(i);
                Debug.Log("Delete:" + propertyName);
            }
        }
    }
    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("path");
        EditorGUILayout.TextField(assetFolderPath);
        if (GUILayout.Button("select"))
        {
            assetFolderPath = EditorUtility.OpenFolderPanel("select path", assetFolderPath, "");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("clear unused property and keyword") && assetFolderPath != null)
        {
            ClearMatObseleteProperty();
        }
    }
}

#endif
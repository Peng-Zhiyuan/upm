using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

public class ResourceFinderWindow : EditorWindow
{
    private string searchPath = "Assets/Resources";
    private List<Texture2D> targetTextures = new List<Texture2D>();
    private Vector2 scrollPosition;

    [MenuItem("Tools/Resource Finder")]
    public static void ShowWindow()
    {
        GetWindow<ResourceFinderWindow>("Resource Finder");
    }

    void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        searchPath = EditorGUILayout.TextField("Search Path", searchPath);

        EditorGUILayout.BeginHorizontal();
        int toDelete = -1;
        for (int i = 0; i < targetTextures.Count; i++)
        {
            EditorGUILayout.BeginVertical();
            targetTextures[i] = (Texture2D)EditorGUILayout.ObjectField(targetTextures[i], typeof(Texture2D), false);
            if (GUILayout.Button("Remove"))
            {
                toDelete = i;
            }
            EditorGUILayout.EndVertical();
        }
        if (toDelete != -1)
        {
            targetTextures.RemoveAt(toDelete);
        }
        if (GUILayout.Button("Add Texture", GUILayout.Width(100)))
        {
            targetTextures.Add(null);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Find References"))
        {
            FindReferences();
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.EndScrollView();
    }

    void FindReferences()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab t:Material", new string[] { searchPath });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object[] dependencies = EditorUtility.CollectDependencies(new[] { AssetDatabase.LoadMainAssetAtPath(assetPath) });

            bool found = false;
            foreach (Object dependency in dependencies)
            {
                if (dependency is Texture2D texture && targetTextures.Contains(texture))
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                Debug.Log($"Found referenced asset: {assetPath}", AssetDatabase.LoadAssetAtPath<Object>(assetPath));
            }
        }
    }
}
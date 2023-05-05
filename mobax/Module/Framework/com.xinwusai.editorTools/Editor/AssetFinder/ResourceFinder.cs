using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class ResourceFinder : EditorWindow
{
    private static string _searchPath = "Assets/"; // 要搜索的文件夹路径
    private List<string> _selectedImages = new List<string>(); // 存放要查找的图片资源路径

    private Vector2 _scrollPosition; // 窗口滚动条位置
    private List<Object> _foundObjects = new List<Object>(); // 找到的资源列表

    [MenuItem("Tools/Resource Finder")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ResourceFinder), false, "Resource Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Search Path:");
        _searchPath = EditorGUILayout.TextField(_searchPath);

        GUILayout.Label("Selected Images:");
        for (int i = 0; i < _selectedImages.Count; i++)
        {
            GUILayout.BeginHorizontal();
            _selectedImages[i] = EditorGUILayout.TextField(_selectedImages[i]);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                _selectedImages.RemoveAt(i);
            }

            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+"))
        {
            _selectedImages.Add("");
        }

        if (GUILayout.Button("Find Resources"))
        {
            FindResources();
        }

        GUILayout.Space(20);

        // 显示找到的资源
        GUILayout.Label("Found Resources:");
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
        foreach (var obj in _foundObjects)
        {
            var path = AssetDatabase.GetAssetPath(obj);

            EditorGUILayout.ObjectField(obj, typeof(Object), false);

            if (GUILayout.Button("Locate"))
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        GUILayout.EndScrollView();
    }

    private void FindResources()
    {
        _foundObjects.Clear();

        string[] guids = AssetDatabase.FindAssets("t:prefab t:material", new[] {_searchPath});
        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarningFormat("No resources found in folder '{0}'", _searchPath);
            return;
        }

        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (obj == null)
            {
                Debug.LogErrorFormat("Failed to load asset at path '{0}'", assetPath);
                continue;
            }

            var dependencies = AssetDatabase.GetDependencies(assetPath, true);
            if (dependencies == null || dependencies.Length == 0)
            {
                Debug.LogWarningFormat("No dependencies found for asset at path '{0}'", assetPath);
                continue;
            }

            var imageDependencies = dependencies.Where(x => _selectedImages.Contains(x)).ToList();
            if (imageDependencies.Count == 0)
            {
                continue;
            }

            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go == null)
            {
                Debug.LogErrorFormat("Failed to load prefab at path '{0}'", assetPath);
                continue;
            }

            var images = go.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            if (images == null || images.Length == 0)
            {
                continue;
            }

            foreach (var image in images)
            {
                if (imageDependencies.Contains(AssetDatabase.GetAssetPath(image.mainTexture)))
                {
                    _foundObjects.Add(obj);
                    break;
                }
            }
        }
    }
}
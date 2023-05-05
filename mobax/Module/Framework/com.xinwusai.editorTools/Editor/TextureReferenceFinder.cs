using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine.UI;

public class TextureReferenceFinder : MonoBehaviour
{
    private static string searchPath = "Assets/res"; // 使用您自己的文件夹路径替换

    [MenuItem("Assets/Find Texture References")]
    private static void FindReferences()
    {
        Texture2D selectedTexture = Selection.activeObject as Texture2D;

        if (selectedTexture == null)
        {
            Debug.LogError("请选择一个纹理。");
            return;
        }

        TextureReferenceResultWindow.ShowWindow(selectedTexture, searchPath);
    }
}

public class TextureReferenceResultWindow : EditorWindow
{
    private Texture2D selectedTexture;
    private Vector2 scrollPosition;
    private List<GameObject> referencedPrefabs;
    private GameObject selectedPrefab;
    private int selectedIndex = -1;

    public static void ShowWindow(Texture2D texture, string searchPath)
    {
        TextureReferenceResultWindow window = GetWindow<TextureReferenceResultWindow>("纹理引用结果");
        window.selectedTexture = texture;
        window.FindReferencedPrefabs(searchPath);
        window.minSize = new Vector2(400, 600); // Set the minimum size of the window
    }

    // private void OnGUI()
    // {
    //     EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height),
    //         new Color(0.25f, 0.25f, 0.25f)); // Set background color
    //
    //     EditorGUILayout.Space();
    //
    //     // Change the title style
    //     GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
    //     titleStyle.fontSize = 24;
    //     titleStyle.normal.textColor = new Color32(46, 139, 87, 255); // Dark green color (#2E8B57)
    //
    //     EditorGUILayout.LabelField("纹理引用结果", titleStyle, GUILayout.Height(40));
    //     EditorGUILayout.Space();
    //
    //     if (selectedTexture != null)
    //     {
    //         EditorGUILayout.BeginHorizontal("box");
    //
    //         GUILayout.FlexibleSpace();
    //         GUILayout.Label(new GUIContent(selectedTexture));
    //         GUILayout.FlexibleSpace();
    //
    //         EditorGUILayout.EndHorizontal();
    //
    //         EditorGUILayout.Space();
    //
    //         EditorGUILayout.BeginHorizontal("box");
    //
    //         GUILayout.FlexibleSpace();
    //         EditorGUILayout.LabelField($"纹理名称: {selectedTexture.name}", EditorStyles.boldLabel);
    //         GUILayout.FlexibleSpace();
    //
    //         EditorGUILayout.EndHorizontal();
    //
    //         EditorGUILayout.Space();
    //     }
    //
    //     scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width),
    //         GUILayout.Height(position.height - 200));
    //
    //     EditorGUILayout.BeginVertical("box");
    //
    //     for (int i = 0; i < referencedPrefabs.Count; i++)
    //     {
    //         GameObject prefab = referencedPrefabs[i];
    //         EditorGUILayout.BeginHorizontal();
    //
    //         EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
    //
    //         GUILayout.FlexibleSpace();
    //
    //         // Highlight the selected button
    //         GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    //         if (i == selectedIndex)
    //         {
    //             buttonStyle.normal.background = Texture2D.whiteTexture;
    //             buttonStyle.normal.textColor = new Color(0.1f, 0.5f, 0.1f);
    //         }
    //
    //         if (GUILayout.Button("定位预制体", buttonStyle, GUILayout.MaxWidth(100)))
    //         {
    //             EditorGUIUtility.PingObject(prefab);
    //             selectedPrefab = prefab;
    //             selectedIndex = i;
    //
    //             DisplayInScene(selectedPrefab);
    //         }
    //
    //         EditorGUILayout.EndHorizontal();
    //
    //         EditorGUILayout.Space();
    //     }
    //
    //     EditorGUILayout.EndVertical();
    //
    //     EditorGUILayout.EndScrollView();
    //
    //     EditorGUILayout.Space();
    // }
    //
//     private void DisplayTextureList()
// {
//     List<Texture2D> textures = new List<Texture2D>(textureReferences.Keys);
//     for (int i = 0; i < textures.Count; i++)
//     {
//         EditorGUILayout.BeginHorizontal();
//         Texture2D oldTexture = textures[i];
//         textures[i] = (Texture2D)EditorGUILayout.ObjectField(textures[i], typeof(Texture2D), false);
//
//         UpdateTextureReference(oldTexture, textures[i]);
//
//         if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
//         {
//             textureReferences.Remove(textures[i]);
//             i--;
//         }
//         EditorGUILayout.EndHorizontal();
//
//         DisplayPrefabList(textures[i]);
//     }
// }
//
// private void UpdateTextureReference(Texture2D oldTexture, Texture2D newTexture)
// {
//     // 检测纹理是否发生变化
//     if (oldTexture != newTexture)
//     {
//         // 更新字典键，并查找新纹理的引用
//         textureReferences.Remove(oldTexture);
//         if (newTexture != null)
//         {
//             if (!textureReferences.ContainsKey(newTexture))
//             {
//                 textureReferences[newTexture] = FindRefrencedPrefabs(newTexture);
//             }
//         }
//     }
// }
//
// private void DisplayPrefabList(Texture2D texture)
// {
//     if (texture != null && textureReferences.ContainsKey(texture))
//     {
//         List<GameObject> referencedPrefabs = textureReferences[texture];
//         for (int j = 0; j < referencedPrefabs.Count; j++)
//         {
//             EditorGUILayout.BeginHorizontal();
//
//             EditorGUILayout.ObjectField(referencedPrefabs[j], typeof(GameObject), false);
//
//             if (DisplayLocatePrefabButton(j))
//             {
//                 EditorGUIUtility.PingObject(referencedPrefabs[j]);
//                 selectedPrefab = referencedPrefabs[j];
//                 selectedIndex = j;
//
//                 DisplayInScene(selectedPrefab);
//             }
//
//             EditorGUILayout.EndHorizontal();
//         }
//     }
// }
//
// private bool DisplayLocatePrefabButton(int index)
// {
//     // Highlight the selected button
//     GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
//     if (index == selectedIndex)
//     {
//         buttonStyle.normal.background = Texture2D.whiteTexture;
//         buttonStyle.normal.textColor = new Color(0.1f, 0.5f, 0.1f);
//     }
//
//     return GUILayout.Button("定位预制体", buttonStyle, GUILayout.MaxWidth(100), GUILayout.Height(30));
// }
//
// private void OnGUI()
// {
//     EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.25f, 0.25f, 0.25f)); // Set background color
//
//     EditorGUILayout.Space();
//
//     // Change the title style
//     GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
//     titleStyle.fontSize = 24;
//     titleStyle.normal.textColor = new Color32(46, 139, 87, 255); // Dark green color (#2E8B57)
//
//     EditorGUILayout.LabelField("纹理引用结果", titleStyle, GUILayout.Height(40));
//     EditorGUILayout.Space();
//
//     DisplayTextureList();
//
//     // 添加新纹理按钮
//     if (GUILayout.Button("添加纹理"))
//     {
//         textureReferences.Add(null, new List<GameObject>());
//     }
//
//     EditorGUILayout.Space();
//
//     ShowPreview(selectedPrefab);
// }

    private void DisplayInScene(GameObject prefab)
    {
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);

        Component targetComponent = FindComponentUsingTexture(root);

        if (targetComponent != null)
        {
            Selection.activeGameObject = targetComponent.gameObject;
        }
        else
        {
            Selection.activeObject = prefab;
        }

        // Open the prefab asset for editing, which mimics double-clicking the prefab
        AssetDatabase.OpenAsset(prefab);

        PrefabUtility.UnloadPrefabContents(root);
    }

    private Component FindComponentUsingTexture(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
        Image[] images = obj.GetComponentsInChildren<Image>(true);

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            foreach (Material material in materials)
            {
                if (material != null && material.mainTexture == selectedTexture)
                {
                    return renderer;
                }
            }
        }

        foreach (Image image in images)
        {
            if (image.sprite != null && image.sprite.texture == selectedTexture)
            {
                return image;
            }
        }

        return null;
    }

    public void FindReferencedPrefabs(string searchPath)
    {
        string filter = $"t:Prefab";
        string[] prefabGUIDs = AssetDatabase.FindAssets(filter, new[] {searchPath});
        Debug.Log($"找到 {prefabGUIDs.Length} 个预制体。");

        referencedPrefabs = new List<GameObject>();
        string selectedTextureGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selectedTexture));
        Regex guidRegex = new Regex($"guid: ({selectedTextureGUID})", RegexOptions.Compiled);

        foreach (string prefabGUID in prefabGUIDs)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGUID);
            string prefabData = File.ReadAllText(prefabPath);

            if (guidRegex.IsMatch(prefabData))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                referencedPrefabs.Add(prefab);
            }
        }
    }
}
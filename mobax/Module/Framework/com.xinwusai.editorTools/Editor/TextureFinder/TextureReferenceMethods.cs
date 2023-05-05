using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public static class TextureReferenceMethods
{
    public static List<GameObject> FindRefrencedPrefabs(Texture2D texture)
    {
        string[] allPrefabPaths = AssetDatabase.FindAssets("t:GameObject", new[] {"Assets"})
            .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
            .ToArray();

        List<GameObject> referencedPrefabs = new List<GameObject>();

        foreach (string prefabPath in allPrefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer.sharedMaterials.Any(material => material.mainTexture == texture))
                {
                    referencedPrefabs.Add(prefab);
                    break;
                }
            }
        }

        return referencedPrefabs;
    }

    public static void DisplayInScene(GameObject prefab)
    {
        if (!prefab) return;

        if (Selection.activeGameObject != prefab)
        {
            Selection.activeGameObject = prefab;
            SceneView.FrameLastActiveSceneView();
        }
    }

    public static void UpdateTextureReference(Texture2D oldTexture, Texture2D newTexture,
        ref Dictionary<Texture2D, List<GameObject>> textureReferences)
    {
        // 检测纹理是否发生变化
        if (oldTexture != newTexture)
        {
            // 更新字典键，并查找新纹理的引用
            textureReferences.Remove(oldTexture);
            if (newTexture != null)
            {
                if (!textureReferences.ContainsKey(newTexture))
                {
                    textureReferences[newTexture] = FindRefrencedPrefabs(newTexture);
                }
            }
        }
    }

    public static void DisplayPrefabList(Texture2D texture, int selectedIndex, ref GameObject selectedPrefab,
        Dictionary<Texture2D, List<GameObject>> textureReferences)
    {
        if (texture != null && textureReferences.ContainsKey(texture))
        {
            List<GameObject> referencedPrefabs = textureReferences[texture];
            for (int j = 0; j < referencedPrefabs.Count; j++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(referencedPrefabs[j], typeof(GameObject), false);

                if (DisplayLocatePrefabButton(j, selectedIndex))
                {
                    EditorGUIUtility.PingObject(referencedPrefabs[j]);
                    selectedPrefab = referencedPrefabs[j];
                    selectedIndex = j;

                    DisplayInScene(selectedPrefab);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private static bool DisplayLocatePrefabButton(int index, int selectedIndex)
    {
        // Highlight the selected button
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        if (index == selectedIndex)
        {
            buttonStyle.normal.background = Texture2D.whiteTexture;
            buttonStyle.normal.textColor = new Color(0.1f, 0.5f, 0.1f);
        }

        return GUILayout.Button("定位预制体", buttonStyle, GUILayout.MaxWidth(100), GUILayout.Height(30));
    }
}
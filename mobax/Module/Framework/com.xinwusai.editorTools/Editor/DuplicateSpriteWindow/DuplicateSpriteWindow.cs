using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class DuplicateSpriteWindow : EditorWindow
{
    private string directoryPath = "Assets/UISprites"; // 指定目录路径
    private Dictionary<string, List<string>> spriteNames = new Dictionary<string, List<string>>(); // 储存Sprite名称和路径的字典
    private List<string> duplicateNames = new List<string>(); // 重复名称列表
    private Vector2 scrollPosition = Vector2.zero; // 滚动位置
    private Texture2D selectedTexture = null; // 选中的Texture
    private bool changeReferences = false; // 是否改变引用

    [MenuItem("Tools/新屋赛/Duplicate Sprite Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DuplicateSpriteWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("Duplicate Sprite Window", EditorStyles.boldLabel);

        // 指定目录路径
        GUILayout.BeginHorizontal();
        GUILayout.Label("Directory Path:");
        directoryPath = GUILayout.TextField(directoryPath);
        GUILayout.EndHorizontal();

        // 检查按钮
        if (GUILayout.Button("Check"))
        {
            CheckSpriteNames();
        }

        // 显示重复名称列表
        if (duplicateNames.Count > 0)
        {
            GUILayout.Label("Duplicate Sprite Names:", EditorStyles.boldLabel);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width),
                GUILayout.Height(position.height - 90));

            foreach (string name in duplicateNames)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(name);
                foreach (string path in spriteNames[name])
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    if (texture != null)
                    {
                        if (GUILayout.Button(texture, GUILayout.Width(50), GUILayout.Height(50)))
                        {
                            selectedTexture = texture;
                            if (EditorUtility.DisplayDialog("Change References",
                                "Do you want to change references to this texture?", "Yes", "No"))
                            {
                                ChangeReferences();
                            }
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            // 选中的Texture
            GUILayout.Label("Selected Texture:");
            if (selectedTexture != null)
            {
                GUILayout.Label(selectedTexture, GUILayout.Width(50), GUILayout.Height(50));
            }
            else
            {
                GUILayout.Label("None");
            }

            // 是否改变引用
            GUILayout.BeginHorizontal();
            changeReferences = GUILayout.Toggle(changeReferences, "Change References");
            GUILayout.EndHorizontal();

            // 改变引用按钮
            if (GUILayout.Button("Change References"))
            {
                if (selectedTexture != null)
                {
                    if (EditorUtility.DisplayDialog("Change References", "Are you sure you want to change references?",
                        "Yes", "No"))
                    {
                        ChangeReferences();
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please select a texture.", "OK");
                }
            }
        }
        else
        {
            GUILayout.Label("No duplicate sprite names found.");
        }
    }

    private void CheckSpriteNames()
    {
        spriteNames.Clear();
        duplicateNames.Clear();

        // 获取指定目录下所有Sprite资源的路径
        string[] spritePaths = Directory.GetFiles(directoryPath, "*.png", SearchOption.AllDirectories);

        foreach (string path in spritePaths)
        {
            // 获取Sprite名称
            string spriteName = Path.GetFileNameWithoutExtension(path);

            // 将Sprite名称和路径添加到字典中
            if (!spriteNames.ContainsKey(spriteName))
            {
                spriteNames.Add(spriteName, new List<string>());
            }

            spriteNames[spriteName].Add(path);
        }

        // 找出重复的Sprite名称
        foreach (KeyValuePair<string, List<string>> pair in spriteNames)
        {
            if (pair.Value.Count > 1)
            {
                duplicateNames.Add(pair.Key);
            }
        }

        // 清空选中的Texture
        selectedTexture = null;
    }

    private void ChangeReferences()
    {
        // 获取所有的图片资源的路径
        string[] spritePaths = AssetDatabase.GetAllAssetPaths()
            .Where(path => path.EndsWith(".png") || path.EndsWith(".jpg")).ToArray();

        // 获取所有Prefab的路径
        string[] prefabPaths = Directory.GetFiles("Assets/res", "*.prefab", SearchOption.AllDirectories);

        for (int i = 0; i < spritePaths.Length; i++)
        {
            // 加载图片资源
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePaths[i]);

            // 如果图片资源与选中的Texture2D不同
            if (texture != selectedTexture)
            {
                // 获取图片资源的名称
                string spriteName = Path.GetFileNameWithoutExtension(spritePaths[i]);

                // 获取所有引用了图片资源的Prefab
                List<string> prefabs = new List<string>();
                foreach (string prefabPath in prefabPaths)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab.GetComponentsInChildren<Image>(true)
                        .Any(image => image.sprite != null && image.sprite.name == spriteName))
                    {
                        prefabs.Add(prefabPath);
                    }
                }

                // 替换所有引用了图片资源的Prefab中的引用
                for (int j = 0; j < prefabs.Count; j++)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[j]);

                    // 检查Prefab是否使用了我们未选中的图片资源
                    Image[] images = prefab.GetComponentsInChildren<Image>(true);
                    bool containsOtherSprite = false;
                    foreach (Image image in images)
                    {
                        if (image.sprite != null && image.sprite.name != spriteName)
                        {
                            containsOtherSprite = true;
                            break;
                        }
                    }

                    // 如果Prefab没有使用我们未选中的图片资源，则替换其中所有引用
                    if (!containsOtherSprite)
                    {
                        foreach (Image image in images)
                        {
                            if (image.sprite != null && image.sprite.name == spriteName)
                            {
                                Undo.RecordObject(image, "Change Image Sprite");
                                image.sprite = Sprite.Create(selectedTexture,
                                    new Rect(0, 0, selectedTexture.width, selectedTexture.height), Vector2.zero);
                            }
                        }

                        EditorUtility.SetDirty(prefab);
                    }
                }
            }
        }

        // 更新AssetDatabase
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
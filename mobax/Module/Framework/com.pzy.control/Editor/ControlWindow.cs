using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ControlWindow : EditorWindow
{
    private List<ControlData> controlDataList = new List<ControlData>();
    private float imageSize = 100f; // 每个图片的大小
    private float padding = 10f; // 图片之间的间距
    private float buttonHeight = 20f; // 按钮的高度

    [MenuItem("pzy.com.*/Control Window")]
    public static void ShowWindow()
    {
        var window = EditorWindow.GetWindow<ControlWindow>("ControlWindow", true) as ControlWindow;
        window.Show();
    }

    private void OnEnable()
    {
        minSize = new Vector2(300f, 200f);
    }
    private void OnGUI()
    {
        GUILayout.Space(10f); // 添加一些空白
        DrawGenerateButton();
        GUILayout.Space(10f); // 添加一些空白
        DrawControlList();
    }

    private void DrawGenerateButton()
    {
        Rect rect = new Rect(padding, padding, position.width - padding * 2, buttonHeight);
        if (GUI.Button(rect, "Create Control Data"))
        {
            GenerateControlData();
        }
    }
    private void DrawControlList()
    {
        float totalWidth = position.width - padding * 2;
        int columns = Mathf.FloorToInt(totalWidth / (imageSize + padding));
        if (columns < 1)
        {
            columns = 1;
        }

        float imageWidth = (totalWidth - padding * (columns - 1)) / columns;
        float imageHeight = imageSize * (imageWidth / imageSize);

        int rowIndex = 0;
        int columnIndex = 0;

        // 绘制每个图片
        foreach (ControlData controlData in controlDataList)
        {
            Rect rect = new Rect(
                padding + columnIndex * (imageWidth + padding),
                padding * 2 + buttonHeight + rowIndex * (imageHeight + padding + buttonHeight),
                imageWidth,
                imageHeight
            );

            // 绘制预览图
            GUI.DrawTexture(rect, controlData.texture, ScaleMode.ScaleToFit);

            // 绘制预制件名字
            Rect labelRect = new Rect(
                rect.x,
                rect.y + rect.height + padding,
                rect.width,
                buttonHeight
            );
            GUI.Label(labelRect, controlData.prefab.name, EditorStyles.centeredGreyMiniLabel);

            // 检查鼠标是否点击在了预览图上
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                // 选中对应的预制件
                Selection.activeObject = controlData.prefab;
                EditorGUIUtility.PingObject(controlData.prefab);
            }

            columnIndex++;
            if (columnIndex >= columns)
            {
                rowIndex++;
                columnIndex = 0;
            }
        }

        // 调整窗口大小
        float totalHeight = padding * 2 + buttonHeight + (rowIndex + 1) * (imageHeight + padding + buttonHeight);
        minSize = new Vector2(totalWidth, Mathf.Min(totalHeight, imageSize + padding * 2 + buttonHeight));
        maxSize = new Vector2(totalWidth, Mathf.Min(totalHeight, imageSize + padding * 2 + buttonHeight));
    }
    private void GenerateControlData()
    {
        controlDataList.Clear();

        // 获取模块列表
        List<string> modulePaths = ModuleWindow.ModulePathList;
        if (modulePaths == null || modulePaths.Count == 0)
        {
            Debug.LogWarning("No module found.");
            return;
        }

        // 搜索控件和预览图
        HashSet<string> previewPaths = new HashSet<string>();
        foreach (string modulePath in modulePaths)
        {
            string[] assetPaths = AssetDatabase.FindAssets("t:Prefab t:Texture", new string[] { modulePath });
            foreach (string assetPath in assetPaths)
            {
                string assetFullPath = AssetDatabase.GUIDToAssetPath(assetPath);
                if(!assetFullPath.EndsWith(".prefab"))
                {
                    continue;
                }
                string assetDirectory = Path.GetDirectoryName(assetFullPath);
                string assetName = Path.GetFileNameWithoutExtension(assetFullPath);

                if (File.Exists(Path.Combine(assetDirectory, assetName + ".prefab")) &&
                    File.Exists(Path.Combine(assetDirectory, assetName + ".png")))
                {
                    string previewPath = Path.Combine(assetDirectory, assetName + ".png");
                    if (!previewPaths.Contains(previewPath))
                    {
                        previewPaths.Add(previewPath);

                        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(previewPath);
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(assetDirectory, assetName + ".prefab"));
                        if (texture != null && prefab != null)
                        {
                            ControlData controlData = CreateControlData(texture, prefab, modulePath);
                            controlDataList.Add(controlData);
                        }
                    }
                }
            }
        }
    }

    private ControlData CreateControlData(Texture2D texture, GameObject prefab, string modulePath)
    {
        ControlData controlData = new ControlData();
        controlData.texture = texture;
        controlData.prefab = prefab;
        controlData.moduleName = Path.GetFileName(modulePath);

        // 缩放预览图到指定大小
        controlData.imageWidth = imageSize;
        controlData.imageHeight = imageSize;
        if (texture.width > texture.height)
        {
            controlData.imageHeight = imageSize * ((float)texture.height / texture.width);
        }
        else if (texture.width < texture.height)
        {
            controlData.imageWidth = imageSize * ((float)texture.width / texture.height);
        }

        return controlData;
    }

    private class ControlData
    {
        public Texture2D texture;
        public GameObject prefab;
        public string moduleName;
        public float imageWidth;
        public float imageHeight;
    }
}
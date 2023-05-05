using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class TexturePickerWindow : EditorWindow
{
    public string SearchPath { get; set; }
    public System.Action<Texture2D> OnTextureSelected;

    private List<Texture2D> _textures = new List<Texture2D>();
    private Vector2 _scrollPosition;

    public static void ShowWindow(string searchPath, System.Action<Texture2D> onTextureSelected)
    {
        var window = GetWindow<TexturePickerWindow>("选择纹理");
        window.SearchPath = searchPath;
        window.OnTextureSelected = onTextureSelected;
        window.RetrieveTextures();
    }

    private void RetrieveTextures()
    {
        if (string.IsNullOrEmpty(SearchPath))
            return;

        string[] files = Directory.GetFiles(SearchPath, "*.png", SearchOption.AllDirectories)
                                  .Concat(Directory.GetFiles(SearchPath, "*.jpg", SearchOption.AllDirectories))
                                  .Concat(Directory.GetFiles(SearchPath, "*.jpeg", SearchOption.AllDirectories))
                                  .ToArray();

        _textures.Clear();

        foreach (var file in files)
        {
            string assetPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture != null)
                _textures.Add(texture);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("纹理列表：", EditorStyles.boldLabel);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int i = 0; i < _textures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(_textures[i], typeof(Texture2D), false);

            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                OnTextureSelected?.Invoke(_textures[i]);
                Close();
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }
}
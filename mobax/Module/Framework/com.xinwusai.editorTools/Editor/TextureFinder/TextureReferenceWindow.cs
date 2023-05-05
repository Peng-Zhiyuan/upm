using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class TextureReferenceWindow : OdinEditorWindow
{
    [FolderPath, LabelText("搜索路径")]
    private string textureSearchPath = "Assets/Textures";

    private Dictionary<Texture2D, List<GameObject>> textureReferences = new Dictionary<Texture2D, List<GameObject>>();

    private GameObject selectedPrefab;
    private int selectedIndex;

    [MenuItem("Tools/纹理引用查找器")]
    private static void ShowWindow()
    {
        GetWindow<TextureReferenceWindow>("纹理引用查找器");
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        EditorGUILayout.Space();
        GUILayout.Label("纹理引用结果", SirenixGUIStyles.SectionHeader);
        EditorGUILayout.Space();

        DisplayTextureList();
        DisplayAddTextureButton();
        EditorGUILayout.Space();
        ShowPreview(selectedPrefab);
    }

    private void DisplayTextureList()
    {
        List<Texture2D> textures = new List<Texture2D>(textureReferences.Keys);
        for (int i = 0; i < textures.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            Texture2D oldTexture = textures[i];
            textures[i] = (Texture2D)EditorGUILayout.ObjectField(textures[i], typeof(Texture2D), false);

            TextureReferenceMethods.UpdateTextureReference(oldTexture, textures[i], ref textureReferences);

            if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(20)))
            {
                textureReferences.Remove(textures[i]);
                i--;
            }
            EditorGUILayout.EndHorizontal();

            TextureReferenceMethods.DisplayPrefabList(textures[i], selectedIndex, ref selectedPrefab, textureReferences);
        }
    }

    private void DisplayAddTextureButton()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("添加纹理", GUILayout.MaxWidth(100)))
        {
            TexturePickerWindow.ShowWindow(textureSearchPath, selectedTexture =>
            {
                if (!textureReferences.ContainsKey(selectedTexture))
                {
                    textureReferences.Add(selectedTexture, TextureReferenceMethods.FindRefrencedPrefabs(selectedTexture));
                }
            });
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    private void ShowPreview(GameObject prefab)
    {
        if (prefab == null) return;

        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("预览:", AssetPreview.GetAssetPreview(prefab), typeof(Texture2D), false, GUILayout.Height(100));
        EditorGUI.EndDisabledGroup();
    }
}
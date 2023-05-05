using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;


public class ButtonBrushToDefaultWwise
{
    private static string[] foldersToRefresh = new string[] {"Assets/res"};

    [MenuItem("Tools/新屋赛/刷新UI按钮默认音效")]
    public static void BrushToDefault()
    {
        foreach (string folder in foldersToRefresh)
        {
            string[] prefabs = Directory.GetFiles(folder, "*.prefab", SearchOption.AllDirectories);
            foreach (string prefabPath in prefabs)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    var buttons = prefab.GetComponentsInChildren<Button>();
                    foreach (var b in buttons)
                    {
                        var buttonExtra = b.gameObject.GetComponent<ButtonExtra>();
                        if (buttonExtra == null)
                        {
                            buttonExtra = b.gameObject.AddComponent<ButtonExtra>();
                            buttonExtra.action = ButtonAction.Default;
                        }
                    }
                }

                EditorUtility.SetDirty(prefab);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
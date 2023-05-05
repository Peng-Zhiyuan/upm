using UnityEngine;
using System;
using UnityEngine;
using UnityEditor;
using ElementSystem;

[InitializeOnLoad]
public class HierarchyIconDrawer
{

    private static Texture2D _elementIcon;
    private static Texture2D ElementIcon {
        get {
            if (_elementIcon == null)
            {
                _elementIcon = (Texture2D)Resources.Load("ElementSystem/ElementIcon");
            }
            return _elementIcon;
        }
    }   

    private static Texture2D _elementDesginerIcon;
    private static Texture2D ElementDesginerIcon {
        get {
            if (_elementDesginerIcon == null)
            {
                _elementDesginerIcon = (Texture2D)Resources.Load("ElementSystem/DesignerIcon");
            }
            return _elementDesginerIcon;
        }
    }


    static HierarchyIconDrawer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyIcon;
    }

    // 绘制icon方法
    private static void DrawHierarchyIcon(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (gameObject == null)
        {
            return;
        }
        var isDesigner = gameObject.GetComponent<CodeLinkerObject>() != null;

        if (isDesigner)
        {
            // 设置icon的位置与尺寸（Hierarchy窗口的左上角是起点）
            //Rect rect = new Rect(selectionRect.x + selectionRect.width - 24f, selectionRect.y - 4, 24f, 24f);
            // 画icon
            //GUI.DrawTexture(rect, HierarchyIconDrawer.ElementDesginerIcon);

            DrawIconAtSelectionRect(selectionRect, ElementDesginerIcon);
        }
    

    }

    private static void DrawIconAtSelectionRect(Rect selectionRect, Texture2D texture, Color? color = null, int slot = 1)
    {
        if (slot < 1)
        {
            slot = 1;
        }
        var beforeColor = GUI.color;
        color = color ?? beforeColor;
        Rect iconRect = new Rect(selectionRect.x + selectionRect.width - slot * 24f, selectionRect.y - 4, 24f, 24f);
        GUI.color = color.Value;
        GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit, true);
        GUI.color = beforeColor;
    }

}
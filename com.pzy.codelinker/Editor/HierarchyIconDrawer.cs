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
                _elementIcon = (Texture2D)Resources.Load("ElementSystem/hunter");
            }
            return _elementIcon;
        }
    }   

    private static Texture2D _elementDesginerIcon;
    private static Texture2D ElementDesginerIcon {
        get {
            if (_elementDesginerIcon == null)
            {
                _elementDesginerIcon = (Texture2D)Resources.Load( "ElementSystem/mage");
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
        //var isElement = gameObject.GetComponent<Element>() != null;
        var isCreator = false;//gameObject.GetComponent<ElementCreator>() != null;

        if (isDesigner)
        {
            // 设置icon的位置与尺寸（Hierarchy窗口的左上角是起点）
            //Rect rect = new Rect(selectionRect.x + selectionRect.width - 24f, selectionRect.y - 4, 24f, 24f);
            // 画icon
            //GUI.DrawTexture(rect, HierarchyIconDrawer.ElementDesginerIcon);

            DrawIconAtSelectionRect(selectionRect, ElementDesginerIcon);
        }
        // else if (!isDesigner && isElement)
        // {
        //     var isPreviewInstance = gameObject.transform.parent.name == "<previewRoot>";
        //     var isRuntimeInstance = gameObject.transform.parent.name == "<runtimeRoot>";
        //     var isPrototype = !isPreviewInstance && !isRuntimeInstance;
        //     Color color;
 
        //     if (isPrototype)
        //     {
        //         color = Color.white;
        //     }
        //     else
        //     {
        //         color = new Color(1, 1, 1, 0.35f);
        //     }
        //     DrawIconAtSelectionRect(selectionRect, ElementIcon, color);
        // }
        // else if (isDesigner && isElement)
        // {
        //     var isPreviewInstance = gameObject.transform.parent.name == "<previewRoot>";
        //     var isRuntimeInstance = gameObject.transform.parent.name == "<runtimeRoot>";
        //     var isPrototype = !isPreviewInstance && !isRuntimeInstance;
        //     Color color;

        //     if (isPrototype)
        //     {
        //         color = Color.white;
        //     }
        //     else
        //     {
        //         color = new Color(1, 1, 1, 0.35f);
        //     }
        //     /*
        //     GUI.color = color;
        //     Rect iconRect = new Rect(selectionRect.x + selectionRect.width - 24f - 24f, selectionRect.y - 4, 24f, 24f);
        //     GUI.DrawTexture(iconRect, ElementDesginerIcon, ScaleMode.ScaleToFit, true);
        //     Rect iconRect2 = new Rect(selectionRect.x + selectionRect.width - 24f , selectionRect.y - 4, 24f, 24f);
        //     GUI.DrawTexture(iconRect2, ElementIcon, ScaleMode.ScaleToFit, true);
        //     GUI.color = Color.white;
        //     */
        //     DrawIconAtSelectionRect(selectionRect, ElementDesginerIcon, color, 2);
        //     DrawIconAtSelectionRect(selectionRect, ElementIcon, color, 1);
        // }

        if (isCreator)
        {
            GUI.color = new Color(0.65f, 0.65f, 0.65f);
            int rectWidth = 50;
            Rect iconRect = new Rect(selectionRect.x + selectionRect.width - rectWidth, selectionRect.y, rectWidth, 24f);
            GUI.Label(iconRect, "Creator");
            GUI.color = Color.white;
        }


    }

    private static void DrawIconAtSelectionRect(Rect selectionRect, Texture2D texture, Color? color = null, int slot = 1)
    {
        if (slot < 1)
        {
            slot = 1;
        }
        color = color ?? Color.white;
        Rect iconRect = new Rect(selectionRect.x + selectionRect.width - slot * 24f, selectionRect.y - 4, 24f, 24f);
        GUI.color = color.Value;
        GUI.DrawTexture(iconRect, texture, ScaleMode.ScaleToFit, true);
        GUI.color = Color.white;
    }

}
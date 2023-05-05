
using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;


[UnityEditor.InitializeOnLoad]
public class CoreEngineHierarchyIcon
{
    private static Texture2D icon = Resources.Load<Texture2D>("CoreObjectDebugerIcon");

    static CoreEngineHierarchyIcon()
    {
        if (icon != null)
        {
            UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
        }
    }

    private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        //if ((obj2 != null) && (obj2.GetComponent<Behavior>() != null))

        if (go != null)
        {
            var coreObjectDebugger = go.GetComponent<CoreObjectDebuger>();
            if (coreObjectDebugger != null)
            {
                // 绘制 icon
                Rect rect = new Rect(selectionRect);
                //rect.x = rect.width + (selectionRect.x - 16f);
                rect.width = 16f;
                rect.height = 16f;
                GUI.DrawTexture(rect, icon);
            }


        }
    }
}


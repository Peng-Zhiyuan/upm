namespace BehaviorDesigner.Editor
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;


    using BehaviorDesigner.Runtime;


    [UnityEditor.InitializeOnLoad]
    public class HierarchyIcon : ScriptableObject
    {
        private static Texture2D icon = (UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/Gizmos/Behavior Designer Hier Icon.png", typeof(Texture2D)) as Texture2D);

        static HierarchyIcon()
        {
            if (icon != null)
            {
                UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
            }
        }

        private static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.ShowHierarchyIcon))
            {
                GameObject go = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;
                //if ((obj2 != null) && (obj2.GetComponent<Behavior>() != null))

                if(go != null)
                {
                    var coreObjectDebugger = go.GetComponent<CoreObjectDebuger>();
                    if(coreObjectDebugger != null)
                    {
                        var co = coreObjectDebugger.co;
                        var behaviorComponent = co.GetComponent<Behavior>();
                        if(behaviorComponent != null)
                        {
                            Rect rect = new Rect(selectionRect);
                            rect.x = rect.width + (selectionRect.x - 16f);
                            rect.width = 16f;
                            rect.height = 16f;
                            GUI.DrawTexture(rect, icon);
                        }

                    }


                }

            }
        }
    }
}


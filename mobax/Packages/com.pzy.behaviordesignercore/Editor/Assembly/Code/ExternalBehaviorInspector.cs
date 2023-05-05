namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditor.Callbacks;
    using UnityEngine;

    [UnityEditor.CustomEditor(typeof(BehaviorDesigner.Runtime.ExternalBehavior))]
    public class ExternalBehaviorInspector : UnityEditor.Editor
    {
        private bool mShowVariables;
        private static List<float> variablePosition;
        private static int selectedVariableIndex = -1;
        private static string selectedVariableName;
        private static int selectedVariableTypeIndex;

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        public static bool ClickAction(int instanceID, int line)
        {
            BehaviorDesigner.Runtime.ExternalBehavior behavior = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as BehaviorDesigner.Runtime.ExternalBehavior;
            if (behavior == null)
            {
                return false;
            }
            BehaviorDesignerWindow.ShowWindow();
            BehaviorDesignerWindow.instance.LoadBehavior(behavior.BehaviorSource, false, true);
            return true;
        }

        public static bool DrawInspectorGUI(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, bool fromInspector, ref bool showVariables)
        {
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(120f) };
            UnityEditor.EditorGUILayout.LabelField("Behavior Name", optionArray1);
            behaviorSource.behaviorName = UnityEditor.EditorGUILayout.TextField(behaviorSource.behaviorName, Array.Empty<GUILayoutOption>());
            if (fromInspector && GUILayout.Button("Open", Array.Empty<GUILayoutOption>()))
            {
                BehaviorDesignerWindow.ShowWindow();
                BehaviorDesignerWindow.instance.LoadBehavior(behaviorSource, false, true);
            }
            GUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.LabelField("Behavior Description", Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Height(48f) };
            behaviorSource.behaviorDescription = UnityEditor.EditorGUILayout.TextArea(behaviorSource.behaviorDescription, optionArray2);
            if (fromInspector)
            {
                bool flag;
                string key = "BehaviorDesigner.VariablesFoldout." + behaviorSource.GetHashCode();
                showVariables = flag = UnityEditor.EditorGUILayout.Foldout(UnityEditor.EditorPrefs.GetBool(key, true), "Variables");
                if (flag)
                {
                    UnityEditor.EditorGUI.indentLevel++;
                    List<BehaviorDesigner.Runtime.SharedVariable> allVariables = behaviorSource.GetAllVariables();
                    if ((allVariables != null) && VariableInspector.DrawAllVariables(false, behaviorSource, ref allVariables, false, ref variablePosition, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, true, false))
                    {
                        if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                        {
                            BinarySerialization.Save(behaviorSource);
                        }
                        else
                        {
                            JSONSerialization.Save(behaviorSource);
                        }
                        return true;
                    }
                    UnityEditor.EditorGUI.indentLevel--;
                }
                UnityEditor.EditorPrefs.SetBool(key, showVariables);
            }
            return UnityEditor.EditorGUI.EndChangeCheck();
        }

        public override void OnInspectorGUI()
        {
            BehaviorDesigner.Runtime.ExternalBehavior behavior = base.target as BehaviorDesigner.Runtime.ExternalBehavior;
            if (behavior != null)
            {
                if (behavior.BehaviorSource.Owner == null)
                {
                    behavior.BehaviorSource.Owner = behavior;
                }
                if (DrawInspectorGUI(behavior.BehaviorSource, true, ref this.mShowVariables))
                {
                    BehaviorDesignerUtility.SetObjectDirty((UnityEngine.Object) behavior);
                }
            }
        }

        public void Reset()
        {
            BehaviorDesigner.Runtime.ExternalBehavior behavior = base.target as BehaviorDesigner.Runtime.ExternalBehavior;
            if ((behavior != null) && (behavior.BehaviorSource.Owner == null))
            {
                behavior.BehaviorSource.Owner = behavior;
            }
        }
    }
}


namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [CustomCoreComponentInspectorAttribute(typeof(Behavior))]
    public class BehaviorInspector : CoreComponentInspector
    {
        private bool mShowOptions = true;
        private bool mShowVariables;
        private static List<float> variablePosition;
        private static int selectedVariableIndex = -1;
        private static string selectedVariableName;
        private static int selectedVariableTypeIndex;

        public static bool DrawInspectorGUI(BehaviorDesigner.Runtime.Behavior behavior, bool fromInspector, ref bool externalModification, ref bool showOptions, ref bool showVariables)
        {
            string str;
            bool flag;
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.Space(3f);
            GUILayout.BeginHorizontal();

            UnityEditor.EditorGUILayout.LabelField("Behavior Name", GUILayout.Width(120f));


            behavior.GetBehaviorSource().behaviorName = EditorGUILayout.TextField(behavior.GetBehaviorSource().behaviorName);

            if (fromInspector && GUILayout.Button("Open"))
            {
                BehaviorDesignerWindow.ShowWindow();
                BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, true);
            }

            GUILayout.EndHorizontal();

            UnityEditor.EditorGUILayout.LabelField("Behavior Description", Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Height(48f) };
            behavior.GetBehaviorSource().behaviorDescription = UnityEditor.EditorGUILayout.TextArea(behavior.GetBehaviorSource().behaviorDescription, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, optionArray2);
            
            //serializedObject.Update();

            UnityEditor.EditorGUI.BeginChangeCheck();

            // 不可编辑预制件
            //GUI.enabled =(BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances) || ((UnityEditor.PrefabUtility.GetPrefabAssetType((UnityEngine.Object) behavior) != UnityEditor.PrefabAssetType.Regular) && (UnityEditor.PrefabUtility.GetPrefabAssetType((UnityEngine.Object) behavior) != UnityEditor.PrefabAssetType.Variant)));
            GUI.enabled = BehaviorDesignerPreferences.GetBool(BDPreferences.EditablePrefabInstances);

            // 绘制行为树数据的引用
            //UnityEditor.SerializedProperty property = serializedObject.FindProperty("externalBehavior");
            //BehaviorDesigner.Runtime.ExternalBehavior behavior2 = property.objectReferenceValue as BehaviorDesigner.Runtime.ExternalBehavior;
            //UnityEditor.EditorGUILayout.PropertyField(property, true, Array.Empty<GUILayoutOption>());
            var behavior2 = behavior.ExternalBehavior;
            behavior.ExternalBehavior = EditorGUILayout.ObjectField(behavior2, typeof(ExternalBehavior), false) as ExternalBehavior;


            //if (UnityEditor.EditorGUI.EndChangeCheck())
            //{
            //    serializedObject.ApplyModifiedProperties();
            //}
            if ((!ReferenceEquals(behavior.ExternalBehavior, null) && !behavior.ExternalBehavior.Equals(behavior2)) || (!ReferenceEquals(behavior2, null) && !behavior2.Equals(behavior.ExternalBehavior)))
            {
                if (!ReferenceEquals(behavior.ExternalBehavior, null))
                {
                    behavior.ExternalBehavior.BehaviorSource.Owner = behavior.ExternalBehavior;
                    behavior.ExternalBehavior.BehaviorSource.CheckForSerialization(true, behavior.GetBehaviorSource());
                }
                else
                {
                    behavior.GetBehaviorSource().EntryTask = null;
                    behavior.GetBehaviorSource().RootTask = null;
                    behavior.GetBehaviorSource().DetachedTasks = null;
                    behavior.GetBehaviorSource().Variables = null;
                    behavior.GetBehaviorSource().CheckForSerialization(true, null);
                    behavior.GetBehaviorSource().Variables = null;
                    if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                    {
                        BinarySerialization.Save(behavior.GetBehaviorSource());
                    }
                    else
                    {
                        JSONSerialization.Save(behavior.GetBehaviorSource());
                    }
                }
                externalModification = true;
            }
            GUI.enabled =(true);

            // 绘制分组选项
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("group"), true, Array.Empty<GUILayoutOption>());
            behavior.Group = EditorGUILayout.IntField("Group", behavior.Group);


            if (fromInspector)
            {
                str = "BehaviorDesigner.VariablesFoldout." + behavior.GetHashCode();
                showVariables = flag = UnityEditor.EditorGUILayout.Foldout(UnityEditor.EditorPrefs.GetBool(str, true), "Variables");
                if (flag)
                {
                    UnityEditor.EditorGUI.indentLevel++;
                    bool flag2 = false;
                    BehaviorDesigner.Runtime.BehaviorSource behaviorSource = behavior.GetBehaviorSource();
                    List<BehaviorDesigner.Runtime.SharedVariable> allVariables = behaviorSource.GetAllVariables();
                    if ((allVariables == null) || (allVariables.Count <= 0))
                    {
                        UnityEditor.EditorGUILayout.LabelField("There are no variables to display", Array.Empty<GUILayoutOption>());
                    }
                    else if (VariableInspector.DrawAllVariables(false, behaviorSource, ref allVariables, false, ref variablePosition, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, false, true))
                    {
                        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && (behavior.ExternalBehavior != null))
                        {
                            BehaviorDesigner.Runtime.BehaviorSource localBehaviorSource = behavior.ExternalBehavior.GetBehaviorSource();
                            localBehaviorSource.CheckForSerialization(true, null);
                            if (VariableInspector.SyncVariables(localBehaviorSource, allVariables))
                            {
                                if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                                {
                                    BinarySerialization.Save(localBehaviorSource);
                                }
                                else
                                {
                                    JSONSerialization.Save(localBehaviorSource);
                                }
                            }
                        }
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                        {
                            BinarySerialization.Save(behaviorSource);
                        }
                        else
                        {
                            JSONSerialization.Save(behaviorSource);
                        }
                    }
                    UnityEditor.EditorGUI.indentLevel--;
                }
                UnityEditor.EditorPrefs.SetBool(str, showVariables);
            }
            str = "BehaviorDesigner.OptionsFoldout." + behavior.GetHashCode();
            if (fromInspector)
            {
                showOptions = flag = UnityEditor.EditorGUILayout.Foldout(UnityEditor.EditorPrefs.GetBool(str, true), "Options");
                if (!flag)
                {
                    goto TR_0004;
                }
            }
            if (fromInspector)
            {
                UnityEditor.EditorGUI.indentLevel++;
            }

            // 绘制选项
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("startWhenEnabled"), true, Array.Empty<GUILayoutOption>());
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("asynchronousLoad"), true, Array.Empty<GUILayoutOption>());
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("pauseWhenDisabled"), true, Array.Empty<GUILayoutOption>());
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("restartWhenComplete"), true, Array.Empty<GUILayoutOption>());
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("resetValuesOnRestart"), true, Array.Empty<GUILayoutOption>());
            //UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("logTaskChanges"), true, Array.Empty<GUILayoutOption>());


            behavior.StartWhenEnabled = EditorGUILayout.Toggle("Start When Enabled", behavior.StartWhenEnabled);
            behavior.AsynchronousLoad = EditorGUILayout.Toggle("Asynchronous Load", behavior.AsynchronousLoad);
            behavior.PauseWhenDisabled = EditorGUILayout.Toggle("Pause When Disabled", behavior.PauseWhenDisabled);
            behavior.RestartWhenComplete = EditorGUILayout.Toggle("Restart When Complete", behavior.RestartWhenComplete);
            behavior.ResetValuesOnRestart = EditorGUILayout.Toggle("Reset Values On Restart", behavior.ResetValuesOnRestart);
            behavior.LogTaskChanges = EditorGUILayout.Toggle("Log Task Changes", behavior.LogTaskChanges);


            if (fromInspector)
            {
                UnityEditor.EditorGUI.indentLevel--;
            }
        TR_0004:
            if (fromInspector)
            {
                UnityEditor.EditorPrefs.SetBool(str, showOptions);
            }
            if (!UnityEditor.EditorGUI.EndChangeCheck())
            {
                return false;
            }
            //serializedObject.ApplyModifiedProperties();
            return true;
        }

        public override void OnEnable()
        {
            var behavior = base.target as Behavior;
            if (behavior != null)
            {
                GizmoManager.UpdateGizmo(behavior);
                if (Application.isPlaying && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    BehaviorDesigner.Runtime.BehaviorManager.IsPlaying =(true);
                }
                behavior.CheckForSerialization(((BehaviorDesignerWindow.instance == null) && !Application.isPlaying) && (behavior.ExternalBehavior != null));
            }
        }

        public override void OnInspectorGUI()
        {
            var behavior = base.target as Behavior;
            if (behavior != null)
            {
                bool externalModification = false;
                if (DrawInspectorGUI(behavior, true, ref externalModification, ref this.mShowOptions, ref this.mShowVariables))
                {
                    // 行为树组件不会存在于预制件中
                    //BehaviorDesignerUtility.SetObjectDirty((UnityEngine.Object) behavior);

                    if (externalModification && ((BehaviorDesignerWindow.instance != null) && (behavior.GetBehaviorSource().BehaviorID == BehaviorDesignerWindow.instance.ActiveBehaviorID)))
                    {
                        BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, false);
                    }
                }
            }
        }
    }
}


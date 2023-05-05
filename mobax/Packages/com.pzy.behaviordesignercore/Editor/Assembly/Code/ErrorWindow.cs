namespace BehaviorDesigner.Editor
{
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class ErrorWindow : UnityEditor.EditorWindow
    {
        private List<BehaviorDesigner.Editor.ErrorDetails> mErrorDetails;
        private Vector2 mScrollPosition;
        public static ErrorWindow instance;

        public void OnFocus()
        {
            instance = this;
            if (BehaviorDesignerWindow.instance != null)
            {
                this.mErrorDetails = BehaviorDesignerWindow.instance.ErrorDetails;
            }
        }

        public void OnGUI()
        {
            this.mScrollPosition = UnityEditor.EditorGUILayout.BeginScrollView(this.mScrollPosition, Array.Empty<GUILayoutOption>());
            if ((this.mErrorDetails == null) || (this.mErrorDetails.Count <= 0))
            {
                if (!BehaviorDesignerPreferences.GetBool(BDPreferences.ErrorChecking))
                {
                    UnityEditor.EditorGUILayout.LabelField("Enable realtime error checking from the preferences to view the errors.", BehaviorDesignerUtility.ErrorListLightBackground, Array.Empty<GUILayoutOption>());
                }
                else
                {
                    UnityEditor.EditorGUILayout.LabelField("The behavior tree has no errors.", BehaviorDesignerUtility.ErrorListLightBackground, Array.Empty<GUILayoutOption>());
                }
            }
            else
            {
                for (int i = 0; i < this.mErrorDetails.Count; i++)
                {
                    BehaviorDesigner.Editor.ErrorDetails details = this.mErrorDetails[i];
                    if ((details != null) && ((details.Type == BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidVariableReference) || ((details.NodeDesigner != null) && (details.NodeDesigner.Task != null))))
                    {
                        string str = string.Empty;
                        switch (details.Type)
                        {
                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.RequiredField:
                                str = string.Format("The task {0} ({1}, index {2}) requires a value for the field {3}.", new object[] { details.TaskFriendlyName, details.TaskType, details.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(details.FieldName) });
                                break;

                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.SharedVariable:
                                str = string.Format("The task {0} ({1}, index {2}) has a Shared Variable field ({3}) that is marked as shared but is not referencing a Shared Variable.", new object[] { details.TaskFriendlyName, details.TaskType, details.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(details.FieldName) });
                                break;

                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.NonUniqueDynamicVariable:
                                str = string.Format("The task {0} ({1}, index {2}) has a dynamic Shared Variable ({3}) but the name matches an existing Shared Varaible.", new object[] { details.TaskFriendlyName, details.TaskType, details.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(details.FieldName) });
                                break;

                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.MissingChildren:
                                str = string.Format("The {0} task ({1}, index {2}) is a parent task which does not have any children", details.TaskFriendlyName, details.TaskType, details.NodeDesigner.Task.ID);
                                break;

                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.UnknownTask:
                                str = string.Format("The task at index {0} is unknown. Has a task been renamed or deleted?", details.NodeDesigner.Task.ID);
                                break;

                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidTaskReference:
                                str = string.Format("The task {0} ({1}, index {2}) has a field ({3}) which is referencing an object within the scene. Behavior tree variables at the project level cannot reference objects within a scene.", new object[] { details.TaskFriendlyName, details.TaskType, details.NodeDesigner.Task.ID, BehaviorDesignerUtility.SplitCamelCase(details.FieldName) });
                                break;

                            case BehaviorDesigner.Editor.ErrorDetails.ErrorType.InvalidVariableReference:
                                str = string.Format("The variable {0} is referencing an object within the scene. Behavior tree variables at the project level cannot reference objects within a scene.", details.FieldName);
                                break;

                            default:
                                break;
                        }
                        GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Height(30f), GUILayout.Width((float) (Screen.width - 7)) };
                        UnityEditor.EditorGUILayout.LabelField(str, ((i % 2) != 0) ? BehaviorDesignerUtility.ErrorListDarkBackground : BehaviorDesignerUtility.ErrorListLightBackground, optionArray1);
                    }
                }
            }
            UnityEditor.EditorGUILayout.EndScrollView();
        }

        [UnityEditor.MenuItem("Tools/Behavior Designer/Error List", false, 2)]
        public static void ShowWindow()
        {
            ErrorWindow window = GetWindow<ErrorWindow>(false, "Error List");
            window.minSize = (new Vector2(400f, 200f));
            window.wantsMouseMove = true;
        }

        public List<BehaviorDesigner.Editor.ErrorDetails> ErrorDetails
        {
            set
            {
                this.mErrorDetails = value;
            }
        }
    }
}


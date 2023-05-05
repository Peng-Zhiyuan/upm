namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public static class ErrorCheck
    {
        private static HashSet<int> fieldHashes = new HashSet<int>();

        private static void AddError(ref List<ErrorDetails> errorDetails, ErrorDetails.ErrorType type, BehaviorDesigner.Runtime.Tasks.Task task, string fieldName)
        {
            if (errorDetails == null)
            {
                errorDetails = new List<ErrorDetails>();
            }
            errorDetails.Add(new ErrorDetails(type, task, fieldName));
        }

        private static void CheckField(BehaviorDesigner.Runtime.Tasks.Task task, bool projectLevelBehavior, ref List<ErrorDetails> errorDetails, FieldInfo field, int hashPrefix, object value)
        {
            if (value != null)
            {
                int item = (hashPrefix + field.Name.GetHashCode()) + field.GetHashCode();
                if (!fieldHashes.Contains(item))
                {
                    fieldHashes.Add(item);
                    if (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) && !IsRequiredFieldValid(field.FieldType, value))
                    {
                        AddError(ref errorDetails, ErrorDetails.ErrorType.RequiredField, task, field.Name);
                    }
                    if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(field.FieldType))
                    {
                        BehaviorDesigner.Runtime.SharedVariable variable = value as BehaviorDesigner.Runtime.SharedVariable;
                        if (variable != null)
                        {
                            BehaviorDesigner.Runtime.SharedVariable variable2;
                            if (variable.IsShared && (!variable.IsDynamic && (string.IsNullOrEmpty(variable.Name) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))))
                            {
                                AddError(ref errorDetails, ErrorDetails.ErrorType.SharedVariable, task, field.Name);
                            }
                            if (!Application.isPlaying && (variable.IsShared && (variable.IsDynamic && (!string.IsNullOrEmpty(variable.Name) && ((task.Owner != null) && (((variable2 = task.Owner.GetBehaviorSource().GetVariable(variable.Name)) != null) && !variable2.IsDynamic))))))
                            {
                                AddError(ref errorDetails, ErrorDetails.ErrorType.NonUniqueDynamicVariable, task, field.Name);
                            }
                            object obj2 = variable.GetValue();
                            if ((!UnityEditor.EditorApplication.isPlaying && (projectLevelBehavior && (!variable.IsShared && (obj2 is Object)))) && (UnityEditor.AssetDatabase.GetAssetPath(obj2 as Object).Length <= 0))
                            {
                                AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.Name);
                            }
                        }
                    }
                    else if (value is Object)
                    {
                        bool flag2 = UnityEditor.AssetDatabase.GetAssetPath(value as Object).Length > 0;
                        if (!UnityEditor.EditorApplication.isPlaying && (projectLevelBehavior && !flag2))
                        {
                            AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidTaskReference, task, field.Name);
                        }
                    }
                    else if (!typeof(Delegate).IsAssignableFrom(field.FieldType) && (!typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(field.FieldType) && (!typeof(BehaviorDesigner.Runtime.Behavior).IsAssignableFrom(field.FieldType) && (field.FieldType.IsClass || (field.FieldType.IsValueType && !field.FieldType.IsPrimitive)))))
                    {
                        FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(field.FieldType);
                        for (int i = 0; i < serializableFields.Length; i++)
                        {
                            CheckField(task, projectLevelBehavior, ref errorDetails, serializableFields[i], item, serializableFields[i].GetValue(value));
                        }
                    }
                }
            }
        }

        // 判断类型是否继承自 UnityObject
        static bool IsUnitObject(object obj)
        {
            var ret = obj is UnityEngine.Object;
            return ret;
        }

        /// <summary>
        /// 如果是 Unity 对象，且是资产，则尝试在 AssetDatabase 中寻找路径
        /// 否则返回空字符串
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        static string FindAssetPathIfObjectIsUnityObject(object obj)
        {
            if(IsUnitObject(obj))
            {
                var unityObject = obj as UnityEngine.Object;
                var path = AssetDatabase.GetAssetPath(unityObject);
                return path;
            }
            return "";
        }

        public static List<ErrorDetails> CheckForErrors(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            if ((behaviorSource == null) || (behaviorSource.Owner == null))
            {
                return null;
            }
            List<ErrorDetails> errorDetails = null;
            fieldHashes.Clear();
            BehaviorDesigner.Runtime.BehaviorSource source = behaviorSource;
            if (!Application.isPlaying && ((behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior) && ((behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).ExternalBehavior != null)))
            {
                behaviorSource = (behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior).ExternalBehavior.BehaviorSource;
            }
            // behaviorSource.Owner 可能是行为树数据或者行为树组件
            // 在之前他们总是能转换成 Unity.Object
            // 现在仅在是行为树数据的情况下可以进行转换
            //bool projectLevelBehavior = UnityEditor.AssetDatabase.GetAssetPath(behaviorSource.Owner.GetObject()).Length > 0;

            bool projectLevelBehavior = false;
            if (IsUnitObject(behaviorSource.Owner))
            {
                projectLevelBehavior = UnityEditor.AssetDatabase.GetAssetPath(behaviorSource.Owner as UnityEngine.Object).Length > 0;
            }

            if (behaviorSource.EntryTask != null)
            {
                CheckTaskForErrors(behaviorSource.EntryTask, projectLevelBehavior, ref errorDetails);
                if (behaviorSource.RootTask == null)
                {
                    AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, behaviorSource.EntryTask, null);
                }
            }
            if (behaviorSource.RootTask != null)
            {
                CheckTaskForErrors(behaviorSource.RootTask, projectLevelBehavior, ref errorDetails);
            }
            //if (!UnityEditor.EditorApplication.isPlaying && ((UnityEditor.AssetDatabase.GetAssetPath(source.Owner.GetObject()).Length > 0) && (source.Variables != null)))
            if (!UnityEditor.EditorApplication.isPlaying && ((FindAssetPathIfObjectIsUnityObject(source.Owner).Length > 0) && (source.Variables != null)))
            {
                for (int i = 0; i < source.Variables.Count; i++)
                {
                    if (source.Variables[i] != null)
                    {
                        object obj2 = source.Variables[i].GetValue();
                        if ((obj2 != null) && ((obj2 is Object) && (UnityEditor.AssetDatabase.GetAssetPath(obj2 as Object).Length == 0)))
                        {
                            AddError(ref errorDetails, ErrorDetails.ErrorType.InvalidVariableReference, null, source.Variables[i].Name);
                        }
                    }
                }
            }
            return errorDetails;
        }

        private static void CheckTaskForErrors(BehaviorDesigner.Runtime.Tasks.Task task, bool projectLevelBehavior, ref List<ErrorDetails> errorDetails)
        {
            if (!task.Disabled)
            {
                if ((task is BehaviorDesigner.Runtime.Tasks.UnknownTask) || (task is BehaviorDesigner.Runtime.Tasks.UnknownParentTask))
                {
                    AddError(ref errorDetails, ErrorDetails.ErrorType.UnknownTask, task, null);
                }
                if (task.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.SkipErrorCheckAttribute), false).Length == 0)
                {
                    FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(task.GetType());
                    for (int i = 0; i < serializableFields.Length; i++)
                    {
                        CheckField(task, projectLevelBehavior, ref errorDetails, serializableFields[i], 0, serializableFields[i].GetValue(task));
                    }
                }
                if ((task is BehaviorDesigner.Runtime.Tasks.ParentTask) && ((task.NodeData.NodeDesigner != null) && !(task.NodeData.NodeDesigner as NodeDesigner).IsEntryDisplay))
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task2 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if ((task2.Children == null) || (task2.Children.Count == 0))
                    {
                        AddError(ref errorDetails, ErrorDetails.ErrorType.MissingChildren, task, null);
                    }
                    else
                    {
                        for (int i = 0; i < task2.Children.Count; i++)
                        {
                            CheckTaskForErrors(task2.Children[i], projectLevelBehavior, ref errorDetails);
                        }
                    }
                }
            }
        }

        public static bool IsRequiredFieldValid(System.Type fieldType, object value)
        {
            if ((value == null) || value.Equals(null))
            {
                return false;
            }
            if (typeof(IList).IsAssignableFrom(fieldType))
            {
                IList list = value as IList;
                if (list.Count == 0)
                {
                    return false;
                }
                for (int i = 0; i < list.Count; i++)
                {
                    if ((list[i] == null) || list[i].Equals(null))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}


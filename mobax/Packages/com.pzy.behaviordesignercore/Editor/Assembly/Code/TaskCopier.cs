namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class TaskCopier : UnityEditor.Editor
    {
        private static void CheckSharedVariableFields(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.Tasks.Task task, object obj, HashSet<object> visitedObjects)
        {
            if ((obj != null) && !visitedObjects.Contains(obj))
            {
                visitedObjects.Add(obj);
                FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(obj.GetType());
                for (int i = 0; i < serializableFields.Length; i++)
                {
                    if (!typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        if (serializableFields[i].FieldType.IsClass && (!serializableFields[i].FieldType.Equals(typeof(System.Type)) && !typeof(Delegate).IsAssignableFrom(serializableFields[i].FieldType)))
                        {
                            CheckSharedVariableFields(behaviorSource, task, serializableFields[i].GetValue(obj), visitedObjects);
                        }
                    }
                    else
                    {
                        BehaviorDesigner.Runtime.SharedVariable sharedVariable = serializableFields[i].GetValue(obj) as BehaviorDesigner.Runtime.SharedVariable;
                        if (sharedVariable != null)
                        {
                            if (sharedVariable.IsShared && (!sharedVariable.IsGlobal && (!string.IsNullOrEmpty(sharedVariable.Name) && (behaviorSource.GetVariable(sharedVariable.Name) == null))))
                            {
                                behaviorSource.SetVariable(sharedVariable.Name, sharedVariable);
                            }
                            CheckSharedVariableFields(behaviorSource, task, sharedVariable, visitedObjects);
                        }
                    }
                }
            }
        }

        private static void CheckSharedVariables(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.Tasks.Task task)
        {
            if (task != null)
            {
                CheckSharedVariableFields(behaviorSource, task, task, new HashSet<object>());
                if (task is BehaviorDesigner.Runtime.Tasks.ParentTask)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task2 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (task2.Children != null)
                    {
                        for (int i = 0; i < task2.Children.Count; i++)
                        {
                            CheckSharedVariables(behaviorSource, task2.Children[i]);
                        }
                    }
                }
            }
        }

        public static TaskSerializer CopySerialized(BehaviorDesigner.Runtime.Tasks.Task task)
        {
            TaskSerializer serializer = new TaskSerializer();
            serializer.offset = (task.NodeData.NodeDesigner as NodeDesigner).GetAbsolutePosition() + new Vector2(10f, 10f);
            serializer.unityObjects = new List<ICoreEngineSystemObject>();
            serializer.serialization = BehaviorDesigner.Runtime.MiniJSON.Serialize(JSONSerialization.SerializeTask(task, false, ref serializer.unityObjects));
            return serializer;
        }

        public static BehaviorDesigner.Runtime.Tasks.Task PasteTask(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, TaskSerializer serializer)
        {
            Dictionary<int, BehaviorDesigner.Runtime.Tasks.Task> dictionary = new Dictionary<int, BehaviorDesigner.Runtime.Tasks.Task>();
            BehaviorDesigner.Runtime.JSONDeserialization.TaskIDs = new Dictionary<BehaviorDesigner.Runtime.JSONDeserialization.TaskField, List<int>>();
            BehaviorDesigner.Runtime.Tasks.Task task = BehaviorDesigner.Runtime.JSONDeserialization.DeserializeTask(behaviorSource, BehaviorDesigner.Runtime.MiniJSON.Deserialize(serializer.serialization) as Dictionary<string, object>, ref dictionary, serializer.unityObjects);
            CheckSharedVariables(behaviorSource, task);
            if (BehaviorDesigner.Runtime.JSONDeserialization.TaskIDs.Count > 0)
            {
                foreach (BehaviorDesigner.Runtime.JSONDeserialization.TaskField field in BehaviorDesigner.Runtime.JSONDeserialization.TaskIDs.Keys)
                {
                    List<int> list = BehaviorDesigner.Runtime.JSONDeserialization.TaskIDs[field];
                    System.Type fieldType = field.fieldInfo.FieldType;
                    if (!field.fieldInfo.FieldType.IsArray)
                    {
                        BehaviorDesigner.Runtime.Tasks.Task task4 = TaskWithID(behaviorSource, list[0]);
                        if ((task4 == null) || (!task4.GetType().Equals(field.fieldInfo.FieldType) && !task4.GetType().IsSubclassOf(field.fieldInfo.FieldType)))
                        {
                            continue;
                        }
                        field.fieldInfo.SetValue(field.task, task4);
                        continue;
                    }
                    int length = 0;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= list.Count)
                        {
                            Array array = Array.CreateInstance(fieldType.GetElementType(), length);
                            int index = 0;
                            int num4 = 0;
                            while (true)
                            {
                                if (num4 >= list.Count)
                                {
                                    field.fieldInfo.SetValue(field.task, array);
                                    break;
                                }
                                BehaviorDesigner.Runtime.Tasks.Task task3 = TaskWithID(behaviorSource, list[num4]);
                                if ((task3 != null) && (task3.GetType().Equals(fieldType.GetElementType()) || task3.GetType().IsSubclassOf(fieldType.GetElementType())))
                                {
                                    array.SetValue(task3, index);
                                    index++;
                                }
                                num4++;
                            }
                            break;
                        }
                        BehaviorDesigner.Runtime.Tasks.Task task2 = TaskWithID(behaviorSource, list[num2]);
                        if ((task2 != null) && (task2.GetType().Equals(fieldType.GetElementType()) || task2.GetType().IsSubclassOf(fieldType.GetElementType())))
                        {
                            length++;
                        }
                        num2++;
                    }
                }
                BehaviorDesigner.Runtime.JSONDeserialization.TaskIDs = null;
            }
            return task;
        }

        private static BehaviorDesigner.Runtime.Tasks.Task TaskWithID(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, int id)
        {
            BehaviorDesigner.Runtime.Tasks.Task task = null;
            if (behaviorSource.RootTask != null)
            {
                task = TaskWithID(id, behaviorSource.RootTask);
            }
            if ((task == null) && (behaviorSource.DetachedTasks != null))
            {
                for (int i = 0; (i < behaviorSource.DetachedTasks.Count) && ((task = TaskWithID(id, behaviorSource.DetachedTasks[i])) == null); i++)
                {
                }
            }
            return task;
        }

        private static BehaviorDesigner.Runtime.Tasks.Task TaskWithID(int id, BehaviorDesigner.Runtime.Tasks.Task task)
        {
            if (task != null)
            {
                if (task.ID == id)
                {
                    return task;
                }
                if (task is BehaviorDesigner.Runtime.Tasks.ParentTask)
                {
                    BehaviorDesigner.Runtime.Tasks.ParentTask task2 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                    if (task2.Children != null)
                    {
                        for (int i = 0; i < task2.Children.Count; i++)
                        {
                            BehaviorDesigner.Runtime.Tasks.Task task3 = TaskWithID(id, task2.Children[i]);
                            if (task3 != null)
                            {
                                return task3;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}


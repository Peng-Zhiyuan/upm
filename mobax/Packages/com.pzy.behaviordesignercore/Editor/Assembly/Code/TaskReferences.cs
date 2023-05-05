namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public class TaskReferences : MonoBehaviour
    {
        public static void CheckReferences(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            if (behaviorSource.RootTask != null)
            {
                CheckReferences(behaviorSource, behaviorSource.RootTask);
            }
            if (behaviorSource.DetachedTasks != null)
            {
                for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
                {
                    CheckReferences(behaviorSource, behaviorSource.DetachedTasks[i]);
                }
            }
        }

        public static void CheckReferences(BehaviorDesigner.Runtime.Behavior behavior, List<BehaviorDesigner.Runtime.Tasks.Task> taskList)
        {
            for (int i = 0; i < taskList.Count; i++)
            {
                CheckReferences(behavior, taskList[i], taskList);
            }
        }

        private static void CheckReferences(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.Tasks.Task task)
        {
            FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(task.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if (!serializableFields[i].FieldType.IsArray && (serializableFields[i].FieldType.Equals(typeof(BehaviorDesigner.Runtime.Tasks.Task)) || serializableFields[i].FieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Task))))
                {
                    BehaviorDesigner.Runtime.Tasks.Task referencedTask = serializableFields[i].GetValue(task) as BehaviorDesigner.Runtime.Tasks.Task;
                    if (referencedTask != null)
                    {
                        BehaviorDesigner.Runtime.Tasks.Task task3 = FindReferencedTask(behaviorSource, referencedTask);
                        if (task3 != null)
                        {
                            serializableFields[i].SetValue(task, task3);
                        }
                    }
                }
                else if (serializableFields[i].FieldType.IsArray && (serializableFields[i].FieldType.GetElementType().Equals(typeof(BehaviorDesigner.Runtime.Tasks.Task)) || serializableFields[i].FieldType.GetElementType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Task))))
                {
                    BehaviorDesigner.Runtime.Tasks.Task[] taskArray = serializableFields[i].GetValue(task) as BehaviorDesigner.Runtime.Tasks.Task[];
                    if (taskArray != null)
                    {
                        Type[] typeArguments = new Type[] { serializableFields[i].FieldType.GetElementType() };
                        IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments)) as IList;
                        if (!BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(BehaviorDesigner.Runtime.Tasks.InspectTaskAttribute)))
                        {
                            int index = 0;
                            while (true)
                            {
                                if (index >= taskArray.Length)
                                {
                                    Array array = Array.CreateInstance(serializableFields[i].FieldType.GetElementType(), list.Count);
                                    list.CopyTo(array, 0);
                                    serializableFields[i].SetValue(task, array);
                                    break;
                                }
                                BehaviorDesigner.Runtime.Tasks.Task task4 = FindReferencedTask(behaviorSource, taskArray[index]);
                                if (task4 != null)
                                {
                                    list.Add(task4);
                                }
                                index++;
                            }
                        }
                    }
                }
            }
            if (task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ParentTask)))
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task5 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task5.Children != null)
                {
                    for (int j = 0; j < task5.Children.Count; j++)
                    {
                        CheckReferences(behaviorSource, task5.Children[j]);
                    }
                }
            }
        }

        private static void CheckReferences(BehaviorDesigner.Runtime.Behavior behavior, BehaviorDesigner.Runtime.Tasks.Task task, List<BehaviorDesigner.Runtime.Tasks.Task> taskList)
        {
            if (BehaviorDesigner.Runtime.TaskUtility.CompareType(task.GetType(), "BehaviorDesigner.Runtime.Tasks.ConditionalEvaluator"))
            {
                object obj2 = task.GetType().GetField("conditionalTask").GetValue(task);
                if (obj2 != null)
                {
                    task = obj2 as BehaviorDesigner.Runtime.Tasks.Task;
                }
            }
            FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(task.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if (!serializableFields[i].FieldType.IsArray && (serializableFields[i].FieldType.Equals(typeof(BehaviorDesigner.Runtime.Tasks.Task)) || serializableFields[i].FieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Task))))
                {
                    BehaviorDesigner.Runtime.Tasks.Task referencedTask = serializableFields[i].GetValue(task) as BehaviorDesigner.Runtime.Tasks.Task;
                    if ((referencedTask != null) && !referencedTask.Owner.Equals(behavior))
                    {
                        BehaviorDesigner.Runtime.Tasks.Task task3 = FindReferencedTask(referencedTask, taskList);
                        if (task3 != null)
                        {
                            serializableFields[i].SetValue(task, task3);
                        }
                    }
                }
                else if (serializableFields[i].FieldType.IsArray && (serializableFields[i].FieldType.GetElementType().Equals(typeof(BehaviorDesigner.Runtime.Tasks.Task)) || serializableFields[i].FieldType.GetElementType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Task))))
                {
                    BehaviorDesigner.Runtime.Tasks.Task[] taskArray = serializableFields[i].GetValue(task) as BehaviorDesigner.Runtime.Tasks.Task[];
                    if (taskArray != null)
                    {
                        Type[] typeArguments = new Type[] { serializableFields[i].FieldType.GetElementType() };
                        IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments)) as IList;
                        int index = 0;
                        while (true)
                        {
                            if (index >= taskArray.Length)
                            {
                                Array array = Array.CreateInstance(serializableFields[i].FieldType.GetElementType(), list.Count);
                                list.CopyTo(array, 0);
                                serializableFields[i].SetValue(task, array);
                                break;
                            }
                            BehaviorDesigner.Runtime.Tasks.Task task4 = FindReferencedTask(taskArray[index], taskList);
                            if (task4 != null)
                            {
                                list.Add(task4);
                            }
                            index++;
                        }
                    }
                }
            }
        }

        private static BehaviorDesigner.Runtime.Tasks.Task FindReferencedTask(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.Tasks.Task referencedTask)
        {
            if (referencedTask != null)
            {
                BehaviorDesigner.Runtime.Tasks.Task task;
                int iD = referencedTask.ID;
                if ((behaviorSource.RootTask != null) && ((task = FindReferencedTask(behaviorSource.RootTask, iD)) != null))
                {
                    return task;
                }
                if (behaviorSource.DetachedTasks != null)
                {
                    for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
                    {
                        task = FindReferencedTask(behaviorSource.DetachedTasks[i], iD);
                        if (task != null)
                        {
                            return task;
                        }
                    }
                }
            }
            return null;
        }

        private static BehaviorDesigner.Runtime.Tasks.Task FindReferencedTask(BehaviorDesigner.Runtime.Tasks.Task referencedTask, List<BehaviorDesigner.Runtime.Tasks.Task> taskList)
        {
            int referenceID = referencedTask.ReferenceID;
            for (int i = 0; i < taskList.Count; i++)
            {
                if (taskList[i].ReferenceID == referenceID)
                {
                    return taskList[i];
                }
            }
            return null;
        }

        private static BehaviorDesigner.Runtime.Tasks.Task FindReferencedTask(BehaviorDesigner.Runtime.Tasks.Task task, int referencedTaskID)
        {
            if (task.ID == referencedTaskID)
            {
                return task;
            }
            if (task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ParentTask)))
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task2 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if (task2.Children != null)
                {
                    for (int i = 0; i < task2.Children.Count; i++)
                    {
                        BehaviorDesigner.Runtime.Tasks.Task task3 = FindReferencedTask(task2.Children[i], referencedTaskID);
                        if (task3 != null)
                        {
                            return task3;
                        }
                    }
                }
            }
            return null;
        }
    }
}


namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public class JSONSerialization : UnityEngine.Object
    {
        private static BehaviorDesigner.Runtime.TaskSerializationData taskSerializationData;
        private static BehaviorDesigner.Runtime.FieldSerializationData fieldSerializationData;
        private static BehaviorDesigner.Runtime.VariableSerializationData variableSerializationData;

        public static void Save(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            behaviorSource.CheckForSerialization(false, null);
            taskSerializationData = new BehaviorDesigner.Runtime.TaskSerializationData();
            fieldSerializationData = taskSerializationData.fieldSerializationData;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            if (behaviorSource.EntryTask != null)
            {
                dictionary.Add("EntryTask", SerializeTask(behaviorSource.EntryTask, true, ref fieldSerializationData.unityObjects));
            }
            if (behaviorSource.RootTask != null)
            {
                dictionary.Add("RootTask", SerializeTask(behaviorSource.RootTask, true, ref fieldSerializationData.unityObjects));
            }
            if ((behaviorSource.DetachedTasks != null) && (behaviorSource.DetachedTasks.Count > 0))
            {
                Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[behaviorSource.DetachedTasks.Count];
                int index = 0;
                while (true)
                {
                    if (index >= behaviorSource.DetachedTasks.Count)
                    {
                        dictionary.Add("DetachedTasks", dictionaryArray);
                        break;
                    }
                    dictionaryArray[index] = SerializeTask(behaviorSource.DetachedTasks[index], true, ref fieldSerializationData.unityObjects);
                    index++;
                }
            }
            if ((behaviorSource.Variables != null) && (behaviorSource.Variables.Count > 0))
            {
                dictionary.Add("Variables", SerializeVariables(behaviorSource.Variables, ref fieldSerializationData.unityObjects));
            }
            taskSerializationData.Version = "1.7.2";
            taskSerializationData.JSONSerialization = BehaviorDesigner.Runtime.MiniJSON.Serialize(dictionary);
            behaviorSource.TaskData = taskSerializationData;
            if ((behaviorSource.Owner != null) && !behaviorSource.Owner.Equals(null))
            {
                //throw new Exception("[JSONSerialization] not implement yet");
                // pzy:
                // 此处尚未实现
                //BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner.GetObject());

                if (behaviorSource.OwnerAsBehaviorData != null)
                {
                    BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner as ExternalBehaviorTree);
                }
            }
        }

        public static void Save(BehaviorDesigner.Runtime.GlobalVariables variables)
        {
            if (variables != null)
            {
                variableSerializationData = new BehaviorDesigner.Runtime.VariableSerializationData();
                fieldSerializationData = variableSerializationData.fieldSerializationData;
                Dictionary<string, object> dictionary = new Dictionary<string, object>();
                dictionary.Add("Variables", SerializeVariables(variables.Variables, ref fieldSerializationData.unityObjects));
                variableSerializationData.JSONSerialization = BehaviorDesigner.Runtime.MiniJSON.Serialize(dictionary);
                variables.VariableData = variableSerializationData;
                variables.Version = "1.7.2";
                BehaviorDesignerUtility.SetObjectDirty((UnityEngine.Object) variables);
            }
        }

        private static void SerializeFields(object obj, ref Dictionary<string, object> dict, ref List<ICoreEngineSystemObject> unityObjects)
        {
            FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(obj.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if ((!BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(NonSerializedAttribute)) && (((!serializableFields[i].IsPrivate && !serializableFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(SerializeField))) && (!(obj is BehaviorDesigner.Runtime.Tasks.ParentTask) || !serializableFields[i].Name.Equals("children")))) && (serializableFields[i].GetValue(obj) != null))
                {
                    string key = (serializableFields[i].FieldType.Name + serializableFields[i].Name).ToString();
                    if (typeof(IList).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        IList list = serializableFields[i].GetValue(obj) as IList;
                        if (list != null)
                        {
                            List<object> list2 = new List<object>();
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= list.Count)
                                {
                                    if (list2 != null)
                                    {
                                        dict.Add(key, list2);
                                    }
                                    break;
                                }
                                if (list[num2] == null)
                                {
                                    list2.Add(null);
                                }
                                else
                                {
                                    Type type = list[num2].GetType();
                                    if ((list[num2] is BehaviorDesigner.Runtime.Tasks.Task) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(serializableFields[i], typeof(BehaviorDesigner.Runtime.Tasks.InspectTaskAttribute)))
                                    {
                                        list2.Add((list[num2] as BehaviorDesigner.Runtime.Tasks.Task).ID);
                                    }
                                    else if (list[num2] is BehaviorDesigner.Runtime.SharedVariable)
                                    {
                                        list2.Add(SerializeVariable(list[num2] as BehaviorDesigner.Runtime.SharedVariable, ref unityObjects));
                                    }
                                    else if (list[num2] is ICoreEngineSystemObject)
                                    {
                                        // 现在 Task 仅可包含 CoreEngine 系统中对象的引用
                                        //UnityEngine.Object objA = list[num2] as UnityEngine.Object;

                                        var objA = list[num2] as ICoreEngineSystemObject;
                                        if (!object.ReferenceEquals(objA, null) && (objA != null))
                                        {
                                            list2.Add(unityObjects.Count);
                                            unityObjects.Add(objA);
                                        }
                                    }
                                    else if (type.Equals(typeof(LayerMask)))
                                    {
                                        list2.Add(((LayerMask) list[num2]).value);
                                    }
                                    else if (type.IsPrimitive || (type.IsEnum || (type.Equals(typeof(string)) || (type.Equals(typeof(Vector2)) || (type.Equals(typeof(Vector2Int)) || (type.Equals(typeof(Vector3)) || (type.Equals(typeof(Vector3Int)) || (type.Equals(typeof(Vector4)) || (type.Equals(typeof(Quaternion)) || (type.Equals(typeof(Matrix4x4)) || (type.Equals(typeof(Color)) || type.Equals(typeof(Rect)))))))))))))
                                    {
                                        list2.Add(list[num2]);
                                    }
                                    else
                                    {
                                        Dictionary<string, object> dictionary = new Dictionary<string, object>();
                                        SerializeFields(list[num2], ref dictionary, ref unityObjects);
                                        Dictionary<string, object> item = new Dictionary<string, object>();
                                        item.Add("Type", list[num2].GetType().FullName);
                                        item.Add("Value", dictionary);
                                        list2.Add(item);
                                    }
                                }
                                num2++;
                            }
                        }
                    }
                    else if (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        BehaviorDesigner.Runtime.Tasks.Task task2 = serializableFields[i].GetValue(obj) as BehaviorDesigner.Runtime.Tasks.Task;
                        if (task2 != null)
                        {
                            if (!BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(BehaviorDesigner.Runtime.Tasks.InspectTaskAttribute)))
                            {
                                dict.Add(key, task2.ID);
                            }
                            else
                            {
                                Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
                                dictionary3.Add("Type", task2.GetType());
                                SerializeFields(task2, ref dictionary3, ref unityObjects);
                                dict.Add(key, dictionary3);
                            }
                        }
                    }
                    else if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, SerializeVariable(serializableFields[i].GetValue(obj) as BehaviorDesigner.Runtime.SharedVariable, ref unityObjects));
                        }
                    }
                    else if (typeof(ICoreEngineSystemObject).IsAssignableFrom(serializableFields[i].FieldType))
                    {
                        // Task 中现在仅可包含 CoreEngine 系统中对象的引用
                        //UnityEngine.Object objA = serializableFields[i].GetValue(obj) as UnityEngine.Object;
                        var objA = serializableFields[i].GetValue(obj) as ICoreEngineSystemObject;
                        if (!object.ReferenceEquals(objA, null) && (objA != null))
                        {
                            dict.Add(key, unityObjects.Count);
                            unityObjects.Add(objA);
                        }
                    }
                    else if (serializableFields[i].FieldType.Equals(typeof(LayerMask)))
                    {
                        dict.Add(key, ((LayerMask) serializableFields[i].GetValue(obj)).value);
                    }
                    else if (serializableFields[i].FieldType.IsPrimitive || (serializableFields[i].FieldType.IsEnum || (serializableFields[i].FieldType.Equals(typeof(string)) || (serializableFields[i].FieldType.Equals(typeof(Vector2)) || (serializableFields[i].FieldType.Equals(typeof(Vector2Int)) || (serializableFields[i].FieldType.Equals(typeof(Vector3)) || (serializableFields[i].FieldType.Equals(typeof(Vector3Int)) || (serializableFields[i].FieldType.Equals(typeof(Vector4)) || (serializableFields[i].FieldType.Equals(typeof(Quaternion)) || (serializableFields[i].FieldType.Equals(typeof(Matrix4x4)) || (serializableFields[i].FieldType.Equals(typeof(Color)) || serializableFields[i].FieldType.Equals(typeof(Rect)))))))))))))
                    {
                        dict.Add(key, serializableFields[i].GetValue(obj));
                    }
                    else if (!serializableFields[i].FieldType.Equals(typeof(AnimationCurve)))
                    {
                        Dictionary<string, object> dictionary5 = new Dictionary<string, object>();
                        SerializeFields(serializableFields[i].GetValue(obj), ref dictionary5, ref unityObjects);
                        dict.Add(key, dictionary5);
                    }
                    else
                    {
                        AnimationCurve curve = serializableFields[i].GetValue(obj) as AnimationCurve;
                        Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
                        if (curve.keys != null)
                        {
                            Keyframe[] keyframeArray = curve.keys;
                            List<List<object>> list3 = new List<List<object>>();
                            int index = 0;
                            while (true)
                            {
                                if (index >= keyframeArray.Length)
                                {
                                    dictionary4.Add("Keys", list3);
                                    break;
                                }
                                List<object> item = new List<object>();
                                item.Add(keyframeArray[index].time);
                                item.Add(keyframeArray[index].value);
                                item.Add(keyframeArray[index].inTangent);
                                item.Add(keyframeArray[index].outTangent);
                                list3.Add(item);
                                index++;
                            }
                        }
                        dictionary4.Add("PreWrapMode", curve.preWrapMode);
                        dictionary4.Add("PostWrapMode", curve.postWrapMode);
                        dict.Add(key, dictionary4);
                    }
                }
            }
        }

        private static Dictionary<string, object> SerializeNodeData(BehaviorDesigner.Runtime.NodeData nodeData)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            dictionary.Add("Offset", nodeData.Offset);
            if (nodeData.Comment.Length > 0)
            {
                dictionary.Add("Comment", nodeData.Comment);
            }
            if (nodeData.IsBreakpoint)
            {
                dictionary.Add("IsBreakpoint", nodeData.IsBreakpoint);
            }
            if (nodeData.Collapsed)
            {
                dictionary.Add("Collapsed", nodeData.Collapsed);
            }
            if (nodeData.ColorIndex != 0)
            {
                dictionary.Add("ColorIndex", nodeData.ColorIndex);
            }
            if ((nodeData.WatchedFieldNames != null) && (nodeData.WatchedFieldNames.Count > 0))
            {
                dictionary.Add("WatchedFields", nodeData.WatchedFieldNames);
            }
            return dictionary;
        }

        public static Dictionary<string, object> SerializeTask(BehaviorDesigner.Runtime.Tasks.Task task, bool serializeChildren, ref List<ICoreEngineSystemObject> unityObjects)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Type", task.GetType());
            dict.Add("NodeData", SerializeNodeData(task.NodeData));
            dict.Add("ID", task.ID);
            dict.Add("Name", task.FriendlyName);
            dict.Add("Instant", task.IsInstant);
            if (task.Disabled)
            {
                dict.Add("Disabled", task.Disabled);
            }
            SerializeFields(task, ref dict, ref unityObjects);
            if (serializeChildren && (task is BehaviorDesigner.Runtime.Tasks.ParentTask))
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task2 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if ((task2.Children != null) && (task2.Children.Count > 0))
                {
                    Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[task2.Children.Count];
                    int index = 0;
                    while (true)
                    {
                        if (index >= task2.Children.Count)
                        {
                            dict.Add("Children", dictionaryArray);
                            break;
                        }
                        dictionaryArray[index] = SerializeTask(task2.Children[index], serializeChildren, ref unityObjects);
                        index++;
                    }
                }
            }
            return dict;
        }

        private static Dictionary<string, object> SerializeVariable(BehaviorDesigner.Runtime.SharedVariable sharedVariable, ref List<ICoreEngineSystemObject> unityObjects)
        {
            if (sharedVariable == null)
            {
                return null;
            }
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add("Type", sharedVariable.GetType());
            dict.Add("Name", sharedVariable.Name);
            if (sharedVariable.IsShared)
            {
                dict.Add("IsShared", sharedVariable.IsShared);
            }
            if (sharedVariable.IsGlobal)
            {
                dict.Add("IsGlobal", sharedVariable.IsGlobal);
            }
            if (sharedVariable.IsDynamic)
            {
                dict.Add("IsDynamic", sharedVariable.IsDynamic);
            }
            if (!string.IsNullOrEmpty(sharedVariable.Tooltip))
            {
                dict.Add("Tooltip", sharedVariable.Tooltip);
            }
            if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
            {
                dict.Add("PropertyMapping", sharedVariable.PropertyMapping);
                if (!object.Equals(sharedVariable.PropertyMappingOwner, null))
                {
                    dict.Add("PropertyMappingOwner", unityObjects.Count);
                    unityObjects.Add(sharedVariable.PropertyMappingOwner);
                }
            }
            SerializeFields(sharedVariable, ref dict, ref unityObjects);
            return dict;
        }

        private static Dictionary<string, object>[] SerializeVariables(List<BehaviorDesigner.Runtime.SharedVariable> variables, ref List<ICoreEngineSystemObject> unityObjects)
        {
            Dictionary<string, object>[] dictionaryArray = new Dictionary<string, object>[variables.Count];
            for (int i = 0; i < variables.Count; i++)
            {
                dictionaryArray[i] = SerializeVariable(variables[i], ref unityObjects);
            }
            return dictionaryArray;
        }
    }
}


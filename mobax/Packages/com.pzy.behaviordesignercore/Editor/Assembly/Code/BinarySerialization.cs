namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;
    using UnityEngine;

    public class BinarySerialization
    {
        private static int fieldIndex;
        private static BehaviorDesigner.Runtime.TaskSerializationData taskSerializationData;
        private static BehaviorDesigner.Runtime.FieldSerializationData fieldSerializationData;
        private static HashSet<int> fieldHashes = new HashSet<int>();

        private static void AddByteData(ICollection<byte> bytes)
        {
            fieldSerializationData.dataPosition.Add(fieldSerializationData.byteData.Count);
            if (bytes != null)
            {
                fieldSerializationData.byteData.AddRange(bytes);
            }
            fieldIndex++;
        }

        private static ICollection<byte> AnimationCurveToBytes(AnimationCurve animationCurve)
        {
            List<byte> list = new List<byte>();
            Keyframe[] keyframeArray = animationCurve.keys;
            if (keyframeArray == null)
            {
                list.AddRange(BitConverter.GetBytes(0));
            }
            else
            {
                list.AddRange(BitConverter.GetBytes(keyframeArray.Length));
                for (int i = 0; i < keyframeArray.Length; i++)
                {
                    list.AddRange(BitConverter.GetBytes(keyframeArray[i].time));
                    list.AddRange(BitConverter.GetBytes(keyframeArray[i].value));
                    list.AddRange(BitConverter.GetBytes(keyframeArray[i].inTangent));
                    list.AddRange(BitConverter.GetBytes(keyframeArray[i].outTangent));
                }
            }
            list.AddRange(BitConverter.GetBytes((int) animationCurve.preWrapMode));
            list.AddRange(BitConverter.GetBytes((int) animationCurve.postWrapMode));
            return list;
        }

        private static byte[] BoolToBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        private static byte[] ByteToBytes(byte value)
        {
            return new byte[] { value };
        }

        private static ICollection<byte> ColorToBytes(Color color)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(color.r));
            list.AddRange(BitConverter.GetBytes(color.g));
            list.AddRange(BitConverter.GetBytes(color.b));
            list.AddRange(BitConverter.GetBytes(color.a));
            return list;
        }

        private static byte[] DoubleToBytes(double value)
        {
            return BitConverter.GetBytes(value);
        }

        private static byte[] FloatToBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }

        private static byte[] Int16ToBytes(short value)
        {
            return BitConverter.GetBytes(value);
        }

        private static byte[] IntToBytes(int value)
        {
            return BitConverter.GetBytes(value);
        }

        private static byte[] LongToBytes(long value)
        {
            return BitConverter.GetBytes(value);
        }

        private static ICollection<byte> Matrix4x4ToBytes(Matrix4x4 matrix4x4)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(matrix4x4.m00));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m01));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m02));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m03));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m10));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m11));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m12));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m13));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m20));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m21));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m22));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m23));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m30));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m31));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m32));
            list.AddRange(BitConverter.GetBytes(matrix4x4.m33));
            return list;
        }

        private static ICollection<byte> QuaternionToBytes(Quaternion quaternion)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(quaternion.x));
            list.AddRange(BitConverter.GetBytes(quaternion.y));
            list.AddRange(BitConverter.GetBytes(quaternion.z));
            list.AddRange(BitConverter.GetBytes(quaternion.w));
            return list;
        }

        private static ICollection<byte> RectToBytes(Rect rect)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(rect.x));
            list.AddRange(BitConverter.GetBytes(rect.y));
            list.AddRange(BitConverter.GetBytes(rect.width));
            list.AddRange(BitConverter.GetBytes(rect.height));
            return list;
        }

        public static void Save(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            fieldIndex = 0;
            taskSerializationData = new BehaviorDesigner.Runtime.TaskSerializationData();
            fieldSerializationData = taskSerializationData.fieldSerializationData;
            if (behaviorSource.Variables != null)
            {
                for (int i = 0; i < behaviorSource.Variables.Count; i++)
                {
                    taskSerializationData.variableStartIndex.Add(fieldSerializationData.startIndex.Count);
                    SaveSharedVariable(behaviorSource.Variables[i], 0);
                }
            }
            if (!ReferenceEquals(behaviorSource.EntryTask, null))
            {
                SaveTask(behaviorSource.EntryTask, -1);
            }
            if (!ReferenceEquals(behaviorSource.RootTask, null))
            {
                SaveTask(behaviorSource.RootTask, 0);
            }
            if (behaviorSource.DetachedTasks != null)
            {
                for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
                {
                    SaveTask(behaviorSource.DetachedTasks[i], -1);
                }
            }
            taskSerializationData.Version = "1.7.2";
            taskSerializationData.fieldSerializationData.byteDataArray = taskSerializationData.fieldSerializationData.byteData.ToArray();
            taskSerializationData.fieldSerializationData.byteData = null;
            behaviorSource.TaskData = taskSerializationData;
            if ((behaviorSource.Owner != null) && !behaviorSource.Owner.Equals(null))
            {
                //BehaviorDesignerUtility.SetObjectDirty(behaviorSource.Owner.GetObject());
                BehaviorDesignerUtility.SetObjectDirty(behaviorSource.OwnerAsBehaviorData);
            }
        }

        public static void Save(BehaviorDesigner.Runtime.GlobalVariables globalVariables)
        {
            if (globalVariables != null)
            {
                fieldIndex = 0;
                globalVariables.VariableData = new BehaviorDesigner.Runtime.VariableSerializationData();
                if ((globalVariables.Variables != null) && (globalVariables.Variables.Count != 0))
                {
                    fieldSerializationData = globalVariables.VariableData.fieldSerializationData;
                    for (int i = 0; i < globalVariables.Variables.Count; i++)
                    {
                        globalVariables.VariableData.variableStartIndex.Add(fieldSerializationData.startIndex.Count);
                        SaveSharedVariable(globalVariables.Variables[i], 0);
                    }
                    globalVariables.Version = "1.7.2";
                    globalVariables.VariableData.fieldSerializationData.byteDataArray = globalVariables.VariableData.fieldSerializationData.byteData.ToArray();
                    globalVariables.VariableData.fieldSerializationData.byteData = null;
                    BehaviorDesignerUtility.SetObjectDirty((UnityEngine.Object) globalVariables);
                }
            }
        }

        private static void SaveField(Type fieldType, string fieldName, int hashPrefix, object value, FieldInfo fieldInfo)
        {
            int item = (hashPrefix + BinaryDeserialization.StringHash(fieldType.Name.ToString(), true)) + BinaryDeserialization.StringHash(fieldName, true);
            if (!fieldHashes.Contains(item))
            {
                fieldHashes.Add(item);
                fieldSerializationData.fieldNameHash.Add(item);
                fieldSerializationData.startIndex.Add(fieldIndex);
                if (typeof(IList).IsAssignableFrom(fieldType))
                {
                    Type elementType;
                    if (fieldType.IsArray)
                    {
                        elementType = fieldType.GetElementType();
                    }
                    else
                    {
                        Type baseType = fieldType;
                        while (true)
                        {
                            if (baseType.IsGenericType)
                            {
                                elementType = baseType.GetGenericArguments()[0];
                                break;
                            }
                            baseType = baseType.BaseType;
                        }
                    }
                    IList list = value as IList;
                    if (list == null)
                    {
                        AddByteData(IntToBytes(0));
                    }
                    else
                    {
                        AddByteData(IntToBytes(list.Count));
                        if (list.Count > 0)
                        {
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (ReferenceEquals(list[i], null))
                                {
                                    AddByteData(IntToBytes(-1));
                                }
                                else
                                {
                                    SaveField(elementType, i.ToString(), item / (i + 1), list[i], fieldInfo);
                                }
                            }
                        }
                    }
                }
                else if (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(fieldType))
                {
                    if ((fieldInfo == null) || !BehaviorDesignerUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.InspectTaskAttribute)))
                    {
                        AddByteData(IntToBytes((value as BehaviorDesigner.Runtime.Tasks.Task).ID));
                    }
                    else
                    {
                        AddByteData(StringToBytes(value.GetType().ToString()));
                        SaveFields(value, item);
                    }
                }
                else if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(fieldType))
                {
                    SaveSharedVariable(value as BehaviorDesigner.Runtime.SharedVariable, item);
                }
                else if (typeof(ICoreEngineSystemObject).IsAssignableFrom(fieldType))
                {
                    AddByteData(IntToBytes(fieldSerializationData.unityObjects.Count));
                    fieldSerializationData.unityObjects.Add(value as ICoreEngineSystemObject);
                }
                else if (fieldType.Equals(typeof(int)))
                {
                    AddByteData(IntToBytes((int) value));
                }
                else if (fieldType.Equals(typeof(short)))
                {
                    AddByteData(Int16ToBytes((short) value));
                }
                else if (fieldType.Equals(typeof(uint)))
                {
                    AddByteData(UIntToBytes((uint) value));
                }
                else if (fieldType.Equals(typeof(float)))
                {
                    AddByteData(FloatToBytes((float) value));
                }
                else if (fieldType.Equals(typeof(double)))
                {
                    AddByteData(DoubleToBytes((double) value));
                }
                else if (fieldType.Equals(typeof(long)))
                {
                    AddByteData(LongToBytes((long) value));
                }
                else if (fieldType.Equals(typeof(bool)))
                {
                    AddByteData(BoolToBytes((bool) value));
                }
                else if (fieldType.Equals(typeof(string)))
                {
                    AddByteData(StringToBytes((string) value));
                }
                else if (fieldType.Equals(typeof(byte)))
                {
                    AddByteData(ByteToBytes((byte) value));
                }
                else if (fieldType.IsEnum)
                {
                    SaveField(Enum.GetUnderlyingType(fieldType), fieldName, item, value, fieldInfo);
                }
                else if (fieldType.Equals(typeof(Vector2)))
                {
                    AddByteData(Vector2ToBytes((Vector2) value));
                }
                else if (fieldType.Equals(typeof(Vector2Int)))
                {
                    AddByteData(Vector2IntToBytes((Vector2Int) value));
                }
                else if (fieldType.Equals(typeof(Vector3)))
                {
                    AddByteData(Vector3ToBytes((Vector3) value));
                }
                else if (fieldType.Equals(typeof(Vector3Int)))
                {
                    AddByteData(Vector3IntToBytes((Vector3Int) value));
                }
                else if (fieldType.Equals(typeof(Vector4)))
                {
                    AddByteData(Vector4ToBytes((Vector4) value));
                }
                else if (fieldType.Equals(typeof(Quaternion)))
                {
                    AddByteData(QuaternionToBytes((Quaternion) value));
                }
                else if (fieldType.Equals(typeof(Color)))
                {
                    AddByteData(ColorToBytes((Color) value));
                }
                else if (fieldType.Equals(typeof(Rect)))
                {
                    AddByteData(RectToBytes((Rect) value));
                }
                else if (fieldType.Equals(typeof(Matrix4x4)))
                {
                    AddByteData(Matrix4x4ToBytes((Matrix4x4) value));
                }
                else if (fieldType.Equals(typeof(LayerMask)))
                {
                    AddByteData(IntToBytes(((LayerMask) value).value));
                }
                else if (fieldType.Equals(typeof(AnimationCurve)))
                {
                    AddByteData(AnimationCurveToBytes((AnimationCurve) value));
                }
                else if (!fieldType.IsClass && (!fieldType.IsValueType || fieldType.IsPrimitive))
                {
                    Debug.LogError("Missing Serialization for " + fieldType);
                }
                else
                {
                    if (ReferenceEquals(value, null))
                    {
                        value = Activator.CreateInstance(fieldType, true);
                    }
                    SaveFields(value, item);
                }
            }
        }

        private static void SaveFields(object obj, int hashPrefix)
        {
            fieldHashes.Clear();
            FieldInfo[] allFields = BehaviorDesigner.Runtime.TaskUtility.GetAllFields(obj.GetType());
            for (int i = 0; i < allFields.Length; i++)
            {
                if ((!BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(NonSerializedAttribute)) && ((!allFields[i].IsPrivate && !allFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(allFields[i], typeof(SerializeField)))) && (!(obj is BehaviorDesigner.Runtime.Tasks.ParentTask) || !allFields[i].Name.Equals("children")))
                {
                    object objA = allFields[i].GetValue(obj);
                    if (!ReferenceEquals(objA, null))
                    {
                        SaveField(allFields[i].FieldType, allFields[i].Name, hashPrefix, objA, allFields[i]);
                    }
                }
            }
        }

        private static void SaveNodeData(BehaviorDesigner.Runtime.NodeData nodeData)
        {
            SaveField(typeof(Vector2), "NodeDataOffset", 0, nodeData.Offset, null);
            SaveField(typeof(string), "NodeDataComment", 0, nodeData.Comment, null);
            SaveField(typeof(bool), "NodeDataIsBreakpoint", 0, nodeData.IsBreakpoint, null);
            SaveField(typeof(bool), "NodeDataCollapsed", 0, nodeData.Collapsed, null);
            SaveField(typeof(int), "NodeDataColorIndex", 0, nodeData.ColorIndex, null);
            SaveField(typeof(List<string>), "NodeDataWatchedFields", 0, nodeData.WatchedFieldNames, null);
        }

        private static void SaveSharedVariable(BehaviorDesigner.Runtime.SharedVariable sharedVariable, int hashPrefix)
        {
            if (sharedVariable != null)
            {
                SaveField(typeof(string), "Type", hashPrefix, sharedVariable.GetType().ToString(), null);
                SaveField(typeof(string), "Name", hashPrefix, sharedVariable.Name, null);
                if (sharedVariable.IsShared)
                {
                    SaveField(typeof(bool), "IsShared", hashPrefix, sharedVariable.IsShared, null);
                }
                if (sharedVariable.IsGlobal)
                {
                    SaveField(typeof(bool), "IsGlobal", hashPrefix, sharedVariable.IsGlobal, null);
                }
                if (sharedVariable.IsDynamic)
                {
                    SaveField(typeof(bool), "IsDynamic", hashPrefix, sharedVariable.IsDynamic, null);
                }
                if (!string.IsNullOrEmpty(sharedVariable.Tooltip))
                {
                    SaveField(typeof(string), "Tooltip", hashPrefix, sharedVariable.Tooltip, null);
                }
                if (!string.IsNullOrEmpty(sharedVariable.PropertyMapping))
                {
                    SaveField(typeof(string), "PropertyMapping", hashPrefix, sharedVariable.PropertyMapping, null);
                    if (!Equals(sharedVariable.PropertyMappingOwner, null))
                    {
                        SaveField(typeof(GameObject), "PropertyMappingOwner", hashPrefix, sharedVariable.PropertyMappingOwner, null);
                    }
                }
                SaveFields(sharedVariable, hashPrefix);
            }
        }

        private static void SaveTask(BehaviorDesigner.Runtime.Tasks.Task task, int parentTaskIndex)
        {
            taskSerializationData.types.Add(task.GetType().ToString());
            taskSerializationData.parentIndex.Add(parentTaskIndex);
            taskSerializationData.startIndex.Add(fieldSerializationData.startIndex.Count);
            SaveField(typeof(int), "ID", 0, task.ID, null);
            SaveField(typeof(string), "FriendlyName", 0, task.FriendlyName, null);
            SaveField(typeof(bool), "IsInstant", 0, task.IsInstant, null);
            SaveField(typeof(bool), "Disabled", 0, task.Disabled, null);
            SaveNodeData(task.NodeData);
            SaveFields(task, 0);
            if (task is BehaviorDesigner.Runtime.Tasks.ParentTask)
            {
                BehaviorDesigner.Runtime.Tasks.ParentTask task2 = task as BehaviorDesigner.Runtime.Tasks.ParentTask;
                if ((task2.Children != null) && (task2.Children.Count > 0))
                {
                    for (int i = 0; i < task2.Children.Count; i++)
                    {
                        SaveTask(task2.Children[i], task2.ID);
                    }
                }
            }
        }

        private static byte[] StringToBytes(string str)
        {
            if (str == null)
            {
                str = string.Empty;
            }
            return Encoding.UTF8.GetBytes(str);
        }

        private static byte[] UIntToBytes(uint value)
        {
            return BitConverter.GetBytes(value);
        }

        private static ICollection<byte> Vector2IntToBytes(Vector2Int vector2)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(vector2.x));
            list.AddRange(BitConverter.GetBytes(vector2.y));
            return list;
        }

        private static ICollection<byte> Vector2ToBytes(Vector2 vector2)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(vector2.x));
            list.AddRange(BitConverter.GetBytes(vector2.y));
            return list;
        }

        private static ICollection<byte> Vector3IntToBytes(Vector3Int vector3)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(vector3.x));
            list.AddRange(BitConverter.GetBytes(vector3.y));
            list.AddRange(BitConverter.GetBytes(vector3.z));
            return list;
        }

        private static ICollection<byte> Vector3ToBytes(Vector3 vector3)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(vector3.x));
            list.AddRange(BitConverter.GetBytes(vector3.y));
            list.AddRange(BitConverter.GetBytes(vector3.z));
            return list;
        }

        private static ICollection<byte> Vector4ToBytes(Vector4 vector4)
        {
            List<byte> list = new List<byte>();
            list.AddRange(BitConverter.GetBytes(vector4.x));
            list.AddRange(BitConverter.GetBytes(vector4.y));
            list.AddRange(BitConverter.GetBytes(vector4.z));
            list.AddRange(BitConverter.GetBytes(vector4.w));
            return list;
        }
    }
}


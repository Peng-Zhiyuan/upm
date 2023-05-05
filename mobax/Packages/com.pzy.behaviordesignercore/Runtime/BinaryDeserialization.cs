using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if NETFX_CORE && !UNITY_EDITOR
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
#else
using System.Security.Cryptography;
#endif
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public static class BinaryDeserialization
{
    private static GlobalVariables globalVariables = null;
    private class ObjectFieldMap
    {
        public ObjectFieldMap(object o, FieldInfo f) { obj = o; fieldInfo = f; }
        public object obj;
        public FieldInfo fieldInfo;
    }
    private class ObjectFieldMapComparer : IEqualityComparer<ObjectFieldMap>
    {
        public bool Equals(ObjectFieldMap a, ObjectFieldMap b)
        {
            if (ReferenceEquals(a, null)) return false;
            if (ReferenceEquals(b, null)) return false;
            return a.obj.Equals(b.obj) && a.fieldInfo.Equals(b.fieldInfo);
        }

        public int GetHashCode(ObjectFieldMap a)
        {
            return a != null ? (a.obj.ToString().GetHashCode() + a.fieldInfo.ToString().GetHashCode()) : 0;
        }
    }
    private static Dictionary<ObjectFieldMap, List<int>> taskIDs = null;

#if !NETFX_CORE || UNITY_EDITOR
    private static SHA1 shaHash;
#endif
    private static bool updatedSerialization; // 1.5.7
    private static bool shaHashSerialization; // 1.5.9
    private static bool strHashSerialization; // 1.5.11
    private static int animationCurveAdvance = 20; // 1.5.12
    private static bool enumSerialization; // 1.6.4

    private static Dictionary<uint, string> stringCache = new Dictionary<uint, string>();

    private static byte[] sBigEndianFourByteArray;
    private static byte[] sBigEndianEightByteArray;

    private static byte[] BigEndianFourByteArray { get { if (sBigEndianFourByteArray == null) { sBigEndianFourByteArray = new byte[4]; } return sBigEndianFourByteArray; } set { sBigEndianFourByteArray = value; } }
    private static byte[] BigEndianEightByteArray { get { if (sBigEndianEightByteArray == null) { sBigEndianEightByteArray = new byte[8]; } return sBigEndianEightByteArray; } set { sBigEndianEightByteArray = value; } }

    private static uint[] crcTable = new uint[]{
        0x00000000, 0x77073096, 0xEE0E612C, 0x990951BA, 0x076DC419, 0x706AF48F,
        0xE963A535, 0x9E6495A3, 0x0EDB8832, 0x79DCB8A4, 0xE0D5E91E, 0x97D2D988,
        0x09B64C2B, 0x7EB17CBD, 0xE7B82D07, 0x90BF1D91, 0x1DB71064, 0x6AB020F2,
        0xF3B97148, 0x84BE41DE, 0x1ADAD47D, 0x6DDDE4EB, 0xF4D4B551, 0x83D385C7,
        0x136C9856, 0x646BA8C0, 0xFD62F97A, 0x8A65C9EC, 0x14015C4F, 0x63066CD9,
        0xFA0F3D63, 0x8D080DF5, 0x3B6E20C8, 0x4C69105E, 0xD56041E4, 0xA2677172,
        0x3C03E4D1, 0x4B04D447, 0xD20D85FD, 0xA50AB56B, 0x35B5A8FA, 0x42B2986C,
        0xDBBBC9D6, 0xACBCF940, 0x32D86CE3, 0x45DF5C75, 0xDCD60DCF, 0xABD13D59,
        0x26D930AC, 0x51DE003A, 0xC8D75180, 0xBFD06116, 0x21B4F4B5, 0x56B3C423,
        0xCFBA9599, 0xB8BDA50F, 0x2802B89E, 0x5F058808, 0xC60CD9B2, 0xB10BE924,
        0x2F6F7C87, 0x58684C11, 0xC1611DAB, 0xB6662D3D, 0x76DC4190, 0x01DB7106,
        0x98D220BC, 0xEFD5102A, 0x71B18589, 0x06B6B51F, 0x9FBFE4A5, 0xE8B8D433,
        0x7807C9A2, 0x0F00F934, 0x9609A88E, 0xE10E9818, 0x7F6A0DBB, 0x086D3D2D,
        0x91646C97, 0xE6635C01, 0x6B6B51F4, 0x1C6C6162, 0x856530D8, 0xF262004E,
        0x6C0695ED, 0x1B01A57B, 0x8208F4C1, 0xF50FC457, 0x65B0D9C6, 0x12B7E950,
        0x8BBEB8EA, 0xFCB9887C, 0x62DD1DDF, 0x15DA2D49, 0x8CD37CF3, 0xFBD44C65,
        0x4DB26158, 0x3AB551CE, 0xA3BC0074, 0xD4BB30E2, 0x4ADFA541, 0x3DD895D7,
        0xA4D1C46D, 0xD3D6F4FB, 0x4369E96A, 0x346ED9FC, 0xAD678846, 0xDA60B8D0,
        0x44042D73, 0x33031DE5, 0xAA0A4C5F, 0xDD0D7CC9, 0x5005713C, 0x270241AA,
        0xBE0B1010, 0xC90C2086, 0x5768B525, 0x206F85B3, 0xB966D409, 0xCE61E49F,
        0x5EDEF90E, 0x29D9C998, 0xB0D09822, 0xC7D7A8B4, 0x59B33D17, 0x2EB40D81,
        0xB7BD5C3B, 0xC0BA6CAD, 0xEDB88320, 0x9ABFB3B6, 0x03B6E20C, 0x74B1D29A,
        0xEAD54739, 0x9DD277AF, 0x04DB2615, 0x73DC1683, 0xE3630B12, 0x94643B84,
        0x0D6D6A3E, 0x7A6A5AA8, 0xE40ECF0B, 0x9309FF9D, 0x0A00AE27, 0x7D079EB1,
        0xF00F9344, 0x8708A3D2, 0x1E01F268, 0x6906C2FE, 0xF762575D, 0x806567CB,
        0x196C3671, 0x6E6B06E7, 0xFED41B76, 0x89D32BE0, 0x10DA7A5A, 0x67DD4ACC,
        0xF9B9DF6F, 0x8EBEEFF9, 0x17B7BE43, 0x60B08ED5, 0xD6D6A3E8, 0xA1D1937E,
        0x38D8C2C4, 0x4FDFF252, 0xD1BB67F1, 0xA6BC5767, 0x3FB506DD, 0x48B2364B,
        0xD80D2BDA, 0xAF0A1B4C, 0x36034AF6, 0x41047A60, 0xDF60EFC3, 0xA867DF55,
        0x316E8EEF, 0x4669BE79, 0xCB61B38C, 0xBC66831A, 0x256FD2A0, 0x5268E236,
        0xCC0C7795, 0xBB0B4703, 0x220216B9, 0x5505262F, 0xC5BA3BBE, 0xB2BD0B28,
        0x2BB45A92, 0x5CB36A04, 0xC2D7FFA7, 0xB5D0CF31, 0x2CD99E8B, 0x5BDEAE1D,
        0x9B64C2B0, 0xEC63F226, 0x756AA39C, 0x026D930A, 0x9C0906A9, 0xEB0E363F,
        0x72076785, 0x05005713, 0x95BF4A82, 0xE2B87A14, 0x7BB12BAE, 0x0CB61B38,
        0x92D28E9B, 0xE5D5BE0D, 0x7CDCEFB7, 0x0BDBDF21, 0x86D3D2D4, 0xF1D4E242,
        0x68DDB3F8, 0x1FDA836E, 0x81BE16CD, 0xF6B9265B, 0x6FB077E1, 0x18B74777,
        0x88085AE6, 0xFF0F6A70, 0x66063BCA, 0x11010B5C, 0x8F659EFF, 0xF862AE69,
        0x616BFFD3, 0x166CCF45, 0xA00AE278, 0xD70DD2EE, 0x4E048354, 0x3903B3C2,
        0xA7672661, 0xD06016F7, 0x4969474D, 0x3E6E77DB, 0xAED16A4A, 0xD9D65ADC,
        0x40DF0B66, 0x37D83BF0, 0xA9BCAE53, 0xDEBB9EC5, 0x47B2CF7F, 0x30B5FFE9,
        0xBDBDF21C, 0xCABAC28A, 0x53B39330, 0x24B4A3A6, 0xBAD03605, 0xCDD70693,
        0x54DE5729, 0x23D967BF, 0xB3667A2E, 0xC4614AB8, 0x5D681B02, 0x2A6F2B94,
        0xB40BBE37, 0xC30C8EA1, 0x5A05DF1B, 0x2D02EF8D
    };

    public static void Load(BehaviorSource behaviorSource)
    {
        Load(behaviorSource.TaskData, behaviorSource);
    }

    public static void Load(TaskSerializationData taskData, BehaviorSource behaviorSource)
    {
        behaviorSource.EntryTask = null;
        behaviorSource.RootTask = null;
        behaviorSource.DetachedTasks = null;
        behaviorSource.Variables = null;

        var taskSerializationData = taskData;

        FieldSerializationData fieldSerializationData;
        if (taskSerializationData == null || 
            ((fieldSerializationData = taskSerializationData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.Count == 0) &&
            (fieldSerializationData.byteDataArray == null || fieldSerializationData.byteDataArray.Length == 0)) {
            return;
        }

        if (fieldSerializationData.byteData != null && fieldSerializationData.byteData.Count > 0) {
            fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
        }
        taskIDs = null;
        var treeVersion = new Version(taskData.Version);
        updatedSerialization = treeVersion.CompareTo(new Version("1.5.7")) >= 0;
        enumSerialization = shaHashSerialization = strHashSerialization = false;
        if (updatedSerialization) {
            shaHashSerialization = treeVersion.CompareTo(new Version("1.5.9")) >= 0;
            if (shaHashSerialization) {
                strHashSerialization = treeVersion.CompareTo(new Version("1.5.11")) >= 0;
                if (strHashSerialization) {
                    animationCurveAdvance = treeVersion.CompareTo(new Version("1.5.12")) >= 0 ? 16 : 20;
                    enumSerialization = treeVersion.CompareTo(new Version("1.6.4")) >= 0;
                }
            }
        }

        if (taskSerializationData.variableStartIndex != null) {
            var variables = new List<SharedVariable>();
            var fieldIndexMap = ObjectPool.Get<Dictionary<int, int>>();
            for (int i = 0; i < taskSerializationData.variableStartIndex.Count; ++i) {
                int startIndex = taskSerializationData.variableStartIndex[i];
                int endIndex;
                if (i + 1 < taskSerializationData.variableStartIndex.Count) {
                    endIndex = taskSerializationData.variableStartIndex[i + 1];
                } else {
                    // tasks are added after the variables
                    if (taskSerializationData.startIndex != null && taskSerializationData.startIndex.Count > 0) {
                        endIndex = taskSerializationData.startIndex[0];
                    } else {
                        endIndex = fieldSerializationData.startIndex.Count;
                    }
                }
                // build a dictionary based off of the saved fields
                fieldIndexMap.Clear();
                for (int j = startIndex; j < endIndex; ++j) {
                    fieldIndexMap.Add(fieldSerializationData.fieldNameHash[j], fieldSerializationData.startIndex[j]);
                }

                var sharedVariable = BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, taskSerializationData.variableStartIndex[i], behaviorSource, false, 0);
                if (sharedVariable != null) {
                    variables.Add(sharedVariable);
                }
            }
            ObjectPool.Return(fieldIndexMap);
            behaviorSource.Variables = variables;
        }

        var taskList = new List<Task>();
        if (taskSerializationData.types != null) {
            for (int i = 0; i < taskSerializationData.types.Count; ++i) {
                LoadTask(taskSerializationData, fieldSerializationData, ref taskList, ref behaviorSource);
            }
        }

        // determine where the tasks are positioned
        if (taskSerializationData.parentIndex.Count != taskList.Count) {
            Debug.LogError("Deserialization Error: parent index count does not match task list count");
            return;
        }

        // Determine where the task is positioned
        for (int i = 0; i < taskSerializationData.parentIndex.Count; ++i) {
            if (taskSerializationData.parentIndex[i] == -1) {
                if (behaviorSource.EntryTask == null) { // the first task is always the entry task
                    behaviorSource.EntryTask = taskList[i];
                } else {
                    if (behaviorSource.DetachedTasks == null) {
                        behaviorSource.DetachedTasks = new List<Task>();
                    }
                    behaviorSource.DetachedTasks.Add(taskList[i]);
                }
            } else if (taskSerializationData.parentIndex[i] == 0) { // if the parent is the entry task then assign it as the root task. The entry task isn't a "real" parent task
                behaviorSource.RootTask = taskList[i];
            } else {
                // Add the child to the parent (if the parent index isn't -1)
                if (taskSerializationData.parentIndex[i] != -1) {
                    var parentTask = taskList[taskSerializationData.parentIndex[i]] as ParentTask;
                    if (parentTask != null) {
                        var childIndex = parentTask.Children == null ? 0 : parentTask.Children.Count;
                        parentTask.AddChild(taskList[i], childIndex);
                    }
                }
            }
        }

        if (taskIDs != null) {
            foreach (var objFieldMap in taskIDs.Keys) {
                var ids = taskIDs[objFieldMap] as List<int>;
                var fieldType = objFieldMap.fieldInfo.FieldType;
                if (typeof(IList).IsAssignableFrom(fieldType)) { // array
                    if (fieldType.IsArray) {
                        var elementType = fieldType.GetElementType();
                        var idCount = 0; // The task may be null.
                        for (int i = 0; i < ids.Count; ++i) {
                            var task = taskList[ids[i]];
                            if (!elementType.IsAssignableFrom(task.GetType())) {
                                continue;
                            }
                            idCount++;
                        }
                        var insertIndex = 0;
                        var objectArray = Array.CreateInstance(elementType, idCount);
                        for (int i = 0; i < objectArray.Length; ++i) {
                            var task = taskList[ids[i]];
                            if (!elementType.IsAssignableFrom(task.GetType())) {
                                continue;
                            }
                            objectArray.SetValue(task, insertIndex);
                            insertIndex++;
                        }
                        objFieldMap.fieldInfo.SetValue(objFieldMap.obj, objectArray);
                    } else {
                        var elementType = fieldType.GetGenericArguments()[0];
                        var objectList = TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                        for (int i = 0; i < ids.Count; ++i) {
                            var task = taskList[ids[i]];
                            if (!elementType.IsAssignableFrom(task.GetType())) {
                                continue;
                            }
                            objectList.Add(task);
                        }
                        objFieldMap.fieldInfo.SetValue(objFieldMap.obj, objectList);
                    }
                } else {
                    objFieldMap.fieldInfo.SetValue(objFieldMap.obj, taskList[ids[0]]);
                }
            }
        }
    }

    public static void Load(GlobalVariables globalVariables, string version)
    {
        if (globalVariables == null) {
            return;
        }

        globalVariables.Variables = null;
        FieldSerializationData fieldSerializationData;
        if (globalVariables.VariableData == null || 
            ((fieldSerializationData = globalVariables.VariableData.fieldSerializationData).byteData == null || fieldSerializationData.byteData.Count == 0) &&
            (fieldSerializationData.byteDataArray == null || fieldSerializationData.byteDataArray.Length == 0)) {
            return;
        }

        var variableData = globalVariables.VariableData;
        if (fieldSerializationData.byteData != null && fieldSerializationData.byteData.Count > 0) {
            fieldSerializationData.byteDataArray = fieldSerializationData.byteData.ToArray();
        }
        var variableVersion = new Version(globalVariables.Version);
        updatedSerialization = variableVersion.CompareTo(new Version("1.5.7")) >= 0;
        enumSerialization = shaHashSerialization = strHashSerialization = false;
        if (updatedSerialization) {
            shaHashSerialization = variableVersion.CompareTo(new Version("1.5.9")) >= 0;
            if (shaHashSerialization) {
                strHashSerialization = variableVersion.CompareTo(new Version("1.5.11")) >= 0;
                if (strHashSerialization) {
                    animationCurveAdvance = variableVersion.CompareTo(new Version("1.5.12")) >= 0 ? 16 : 20;
                    enumSerialization = variableVersion.CompareTo(new Version("1.6.4")) >= 0;
                }
            }
        }

        if (variableData.variableStartIndex != null) {
            var variables = new List<SharedVariable>();
            var fieldIndexMap = ObjectPool.Get<Dictionary<int, int>>();
            for (int i = 0; i < variableData.variableStartIndex.Count; ++i) {
                int startIndex = variableData.variableStartIndex[i];
                int endIndex;
                if (i + 1 < variableData.variableStartIndex.Count) {
                    endIndex = variableData.variableStartIndex[i + 1];
                } else {
                    endIndex = fieldSerializationData.startIndex.Count;
                }
                // build a dictionary based off of the saved fields
                fieldIndexMap.Clear();
                for (int j = startIndex; j < endIndex; ++j) {
                    fieldIndexMap.Add(fieldSerializationData.fieldNameHash[j], fieldSerializationData.startIndex[j]);
                }

                var sharedVariable = BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, variableData.variableStartIndex[i], globalVariables, false, 0);
                if (sharedVariable != null) {
                    variables.Add(sharedVariable);
                }
            }
            ObjectPool.Return(fieldIndexMap);
            globalVariables.Variables = variables;
        }
    }

    public static void LoadTask(TaskSerializationData taskSerializationData, FieldSerializationData fieldSerializationData, ref List<Task> taskList, ref BehaviorSource behaviorSource)
    {
        int taskIndex = taskList.Count;
        int startIndex = taskSerializationData.startIndex[taskIndex];
        int endIndex;
        if (taskIndex + 1 < taskSerializationData.startIndex.Count) {
            endIndex = taskSerializationData.startIndex[taskIndex + 1];
        } else {
            endIndex = fieldSerializationData.startIndex.Count;
        }
        // build a dictionary based off of the saved fields
        var fieldIndexMap = ObjectPool.Get<Dictionary<int, int>>();
        fieldIndexMap.Clear(); 
        for (int i = startIndex; i < endIndex; ++i) {
            if (fieldIndexMap.ContainsKey(fieldSerializationData.fieldNameHash[i])) {
                continue;
            }
            fieldIndexMap.Add(fieldSerializationData.fieldNameHash[i], fieldSerializationData.startIndex[i]);
        }
        Task task = null;
        var type = TaskUtility.GetTypeWithinAssembly(taskSerializationData.types[taskIndex]);
        // Change the type to an unknown type if the type doesn't exist anymore.
        if (type == null) {
            bool isUnknownParent = false;
            for (int i = 0; i < taskSerializationData.parentIndex.Count; ++i) {
                if (taskIndex == taskSerializationData.parentIndex[i]) {
                    isUnknownParent = true;
                    break;
                }
            }
            if (isUnknownParent) {
                type = typeof(UnknownParentTask);
            } else {
                type = typeof(UnknownTask);
            }
        }
        task = TaskUtility.CreateInstance(type) as Task;
        if (task is UnknownTask) {
            var unknownTask = task as UnknownTask;
            for (int i = startIndex; i < endIndex; ++i) {
                unknownTask.fieldNameHash.Add(fieldSerializationData.fieldNameHash[i]);
                unknownTask.startIndex.Add(fieldSerializationData.startIndex[i] - fieldSerializationData.startIndex[startIndex]);
            }
            for (int i = fieldSerializationData.startIndex[startIndex]; i <= fieldSerializationData.startIndex[endIndex - 1]; ++i) {
                unknownTask.dataPosition.Add(fieldSerializationData.dataPosition[i] - fieldSerializationData.dataPosition[fieldSerializationData.startIndex[startIndex]]);
            }
            if (taskIndex + 1 < taskSerializationData.startIndex.Count && taskSerializationData.startIndex[taskIndex + 1] < fieldSerializationData.dataPosition.Count) {
                endIndex = fieldSerializationData.dataPosition[taskSerializationData.startIndex[taskIndex + 1]];
            } else {
                endIndex = fieldSerializationData.byteDataArray.Length;
            }
            for (int i = fieldSerializationData.dataPosition[fieldSerializationData.startIndex[startIndex]]; i < endIndex; ++i) {
                unknownTask.byteData.Add(fieldSerializationData.byteDataArray[i]);
            }
            unknownTask.unityObjects = fieldSerializationData.unityObjects;
        }

        task.Owner = behaviorSource.OwnerAsComponent;
        taskList.Add(task);

        task.ID = (int)LoadField(fieldSerializationData, fieldIndexMap, typeof(int), "ID", 0, null);
        task.FriendlyName = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "FriendlyName", 0, null) as string;
        task.IsInstant = (bool)LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsInstant", 0, null);
        object disabled;
        if ((disabled = LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "Disabled", 0, null)) != null) {
            task.Disabled = (bool)disabled;
        }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
        LoadNodeData(fieldSerializationData, fieldIndexMap, taskList[taskIndex]);

        // give a little warning if the task is an unknown type
        if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask))) {
            if (!task.FriendlyName.Contains("Unknown ")) {
                task.FriendlyName = string.Format("Unknown {0}", task.FriendlyName);
            }
            task.NodeData.Comment = "Unknown Task. Right click and Replace to locate new task.";
        }
#endif
        LoadFields(fieldSerializationData, fieldIndexMap, taskList[taskIndex], 0, behaviorSource);
        ObjectPool.Return(fieldIndexMap);
    }

#if UNITY_EDITOR || DLL_DEBUG || DLL_RELEASE
    private static void LoadNodeData(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, Task task)
    {
        var nodeData = new NodeData();
        nodeData.Offset = (Vector2)LoadField(fieldSerializationData, fieldIndexMap, typeof(Vector2), "NodeDataOffset", 0, null);
        nodeData.Comment = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "NodeDataComment", 0, null) as string;
        nodeData.IsBreakpoint = (bool)LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataIsBreakpoint", 0, null);
        nodeData.Collapsed = (bool)LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "NodeDataCollapsed", 0, null);
        var value = LoadField(fieldSerializationData, fieldIndexMap, typeof(int), "NodeDataColorIndex", 0, null);
        if (value != null) {
            nodeData.ColorIndex = (int)value;
        }
        value = LoadField(fieldSerializationData, fieldIndexMap, typeof(List<string>), "NodeDataWatchedFields", 0, null);
        if (value != null) {
            nodeData.WatchedFieldNames = new List<string>();
            nodeData.WatchedFields = new List<FieldInfo>();

            var objectValues = value as IList;
            for (int i = 0; i < objectValues.Count; ++i) {
                var field = task.GetType().GetField((string)objectValues[i], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null) {
                    nodeData.WatchedFieldNames.Add(field.Name);
                    nodeData.WatchedFields.Add(field);
                }
            }
        }
        task.NodeData = nodeData;
    }
#endif

    private static void LoadFields(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, object obj, int hashPrefix, IVariableSource variableSource)
    {
        var fields = TaskUtility.GetSerializableFields(obj.GetType());
        for (int i = 0; i < fields.Length; ++i) {
            // there are a variety of reasons why we can't deserialize a field
            if (TaskUtility.HasAttribute(fields[i], typeof(NonSerializedAttribute)) ||
                ((fields[i].IsPrivate || fields[i].IsFamily) && !TaskUtility.HasAttribute(fields[i], typeof(SerializeField))) ||
                (obj is ParentTask) && fields[i].Name.Equals("children")) {
                continue;
            }
            var value = LoadField(fieldSerializationData, fieldIndexMap, fields[i].FieldType, fields[i].Name, hashPrefix, variableSource, obj, fields[i]);
            if (value != null && !ReferenceEquals(value, null) && !value.Equals(null) && fields[i].FieldType.IsAssignableFrom(value.GetType())) {
                fields[i].SetValue(obj, value);
            }
        }
    }

    private static object LoadField(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, Type fieldType, string fieldName, int hashPrefix, IVariableSource variableSource, object obj = null, FieldInfo fieldInfo = null)
    {
        var fieldHash = hashPrefix;
        if (shaHashSerialization) {
            fieldHash += StringHash(fieldType.Name.ToString(), strHashSerialization) + StringHash(fieldName, strHashSerialization);
        } else {
            fieldHash += fieldType.Name.GetHashCode() + fieldName.GetHashCode();
        }
        int fieldIndex;
        if (!fieldIndexMap.TryGetValue(fieldHash, out fieldIndex)) {
#if NETFX_CORE && !UNITY_EDITOR
            if (fieldType.GetTypeInfo().IsAbstract) {
#else
            if (fieldType.IsAbstract) {
#endif
                return null;
            }

            if (typeof(SharedVariable).IsAssignableFrom(fieldType)) {
                var sharedVariable = TaskUtility.CreateInstance(fieldType) as SharedVariable;
                var sharedVariableValue = (fieldInfo.GetValue(obj) as SharedVariable);
                if (sharedVariableValue != null) {
                    sharedVariable.SetValue(sharedVariableValue.GetValue());
                }
                return sharedVariable;
            }

            return null;
        }
        object value = null;
        if (typeof(IList).IsAssignableFrom(fieldType)) { // array
            int elementCount = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
            if (fieldType.IsArray) {
                var elementType = fieldType.GetElementType();
                if (elementType == null) {
                    return null;
                }
                var objectArray = Array.CreateInstance(elementType, elementCount);
                for (int i = 0; i < elementCount; ++i) {
                    var objectValue = LoadField(fieldSerializationData, fieldIndexMap, elementType, i.ToString(), fieldHash / (updatedSerialization ? (i + 1) : 1), variableSource, obj, fieldInfo);
                    objectArray.SetValue((ReferenceEquals(objectValue, null) || objectValue.Equals(null)) ? null : objectValue, i);
                }
                value = objectArray;
            } else {
                var baseFieldType = fieldType;
#if NETFX_CORE && !UNITY_EDITOR
                while (!baseFieldType.IsGenericType()) {
                    baseFieldType = baseFieldType.BaseType();
                }
#else
                while (!baseFieldType.IsGenericType) {
                    baseFieldType = baseFieldType.BaseType;
                }
#endif
                var elementType = baseFieldType.GetGenericArguments()[0];
                IList objectList;
#if NETFX_CORE && !UNITY_EDITOR
                if (fieldType.IsGenericType()) {
#else
                if (fieldType.IsGenericType) {
#endif
                    objectList = TaskUtility.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                } else {
                    objectList = TaskUtility.CreateInstance(fieldType) as IList;
                }
                for (int i = 0; i < elementCount; ++i) {
                    var objectValue = LoadField(fieldSerializationData, fieldIndexMap, elementType, i.ToString(), fieldHash / (updatedSerialization ? (i + 1) : 1), variableSource, obj, fieldInfo);
                    objectList.Add((ReferenceEquals(objectValue, null) || objectValue.Equals(null)) ? null : objectValue);
                }
                value = objectList;
            }
        } else if (typeof(Task).IsAssignableFrom(fieldType)) {
            if (fieldInfo != null && TaskUtility.HasAttribute(fieldInfo, typeof(InspectTaskAttribute))) {
                var taskTypeName = BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], GetFieldSize(fieldSerializationData, fieldIndex));
                if (!string.IsNullOrEmpty(taskTypeName)) {
                    var taskType = TaskUtility.GetTypeWithinAssembly(taskTypeName);
                    if (taskType != null) {
                        value = TaskUtility.CreateInstance(taskType);
                        LoadFields(fieldSerializationData, fieldIndexMap, value, fieldHash, variableSource);
                    }
                }
            } else { // restore the task ids
                if (taskIDs == null) {
                    taskIDs = new Dictionary<ObjectFieldMap, List<int>>(new ObjectFieldMapComparer());
                }
                int taskID = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
                // Add the task id
                var map = new ObjectFieldMap(obj, fieldInfo);
                if (taskIDs.ContainsKey(map)) {
                    taskIDs[map].Add(taskID);
                } else {
                    var taskIDList = new List<int>();
                    taskIDList.Add(taskID);
                    taskIDs.Add(map, taskIDList);
                }
            }
        } else if (typeof(SharedVariable).IsAssignableFrom(fieldType)) {
            value = BytesToSharedVariable(fieldSerializationData, fieldIndexMap, fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], variableSource, true, fieldHash);
        } else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType)) {
            int unityObjectIndex = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
            value = IndexToUnityObject(unityObjectIndex, fieldSerializationData);
#if !UNITY_EDITOR && NETFX_CORE
        } else if (fieldType.Equals(typeof(int)) || (!enumSerialization && fieldType.GetTypeInfo().IsEnum)) {
#else
        } else if (fieldType.Equals(typeof(int)) || (!enumSerialization && fieldType.IsEnum)) {
#endif
            value = BytesToInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
            if (fieldType.IsEnum) {
                value = Enum.ToObject(fieldType, value);
            }
#if !UNITY_EDITOR && NETFX_CORE
        } else if (fieldType.GetTypeInfo().IsEnum) {
#else
        } else if (fieldType.IsEnum) {
#endif
            value = Enum.ToObject(fieldType, LoadField(fieldSerializationData, fieldIndexMap, Enum.GetUnderlyingType(fieldType), fieldName, fieldHash, variableSource, obj, fieldInfo));
        } else if (fieldType.Equals(typeof(uint))) {
            value = BytesToUInt(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(float))) {
            value = BytesToFloat(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(double))) {
            value = BytesToDouble(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(long))) {
            value = BytesToLong(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(bool))) {
            value = BytesToBool(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(string))) {
            value = BytesToString(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex], GetFieldSize(fieldSerializationData, fieldIndex));
        } else if (fieldType.Equals(typeof(byte))) {
            value = BytesToByte(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Vector2))) {
            value = BytesToVector2(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Vector2Int))) {
            value = BytesToVector2Int(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Vector3))) {
            value = BytesToVector3(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Vector3Int))) {
            value = BytesToVector3Int(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Vector4))) {
            value = BytesToVector4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Quaternion))) {
            value = BytesToQuaternion(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Color))) {
            value = BytesToColor(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Rect))) {
            value = BytesToRect(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(Matrix4x4))) {
            value = BytesToMatrix4x4(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(AnimationCurve))) {
            value = BytesToAnimationCurve(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
        } else if (fieldType.Equals(typeof(LayerMask))) {
            value = BytesToLayerMask(fieldSerializationData.byteDataArray, fieldSerializationData.dataPosition[fieldIndex]);
#if !UNITY_EDITOR && NETFX_CORE
        } else if (fieldType.GetTypeInfo().IsClass) {
#else
        } else if (fieldType.IsClass || (fieldType.IsValueType && !fieldType.IsPrimitive)) {
#endif
            value = TaskUtility.CreateInstance(fieldType);
            LoadFields(fieldSerializationData, fieldIndexMap, value, fieldHash, variableSource);
            return value;
        }
        return value;
    }
    
    public static int StringHash(string value, bool fastHash)
    {
        if (String.IsNullOrEmpty(value)) {
            return 0;
        }

        if (fastHash) { // 1.5.11 and later, from https://stackoverflow.com/questions/5154970/how-do-i-create-a-hashcode-in-net-c-for-a-string-that-is-safe-to-store-in-a
            var hash = 23;
            var length = value.Length;
            for (int i = 0; i < length; ++i) {
                hash = hash * 31 + value[i];
            }
            return hash;
        } else { // Pre 1.5.11
#if NETFX_CORE && !UNITY_EDITOR
            var valueData = CryptographicBuffer.ConvertStringToBinary(value, BinaryStringEncoding.Utf8);
            var shaHash = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hashBuffer = shaHash.HashData(valueData);
        
            byte[] hash;
            CryptographicBuffer.CopyToByteArray(hashBuffer, out hash);
#else
            var valueData = Encoding.UTF8.GetBytes(value);
            if (shaHash == null) {
                shaHash = new SHA1Managed();
            }
            var hash = shaHash.ComputeHash(valueData);
#endif
            return BitConverter.ToInt32(hash, 0);
        }
    }

    private static int GetFieldSize(FieldSerializationData fieldSerializationData, int fieldIndex)
    {
        return (fieldIndex + 1 < fieldSerializationData.dataPosition.Count ? fieldSerializationData.dataPosition[fieldIndex + 1] : fieldSerializationData.byteDataArray.Length) - fieldSerializationData.dataPosition[fieldIndex];
    }

    private static int BytesToInt(byte[] bytes, int dataPosition)
    {
        if (!BitConverter.IsLittleEndian) {
            Array.Copy(bytes, dataPosition, BigEndianFourByteArray, 0, 4);
            Array.Reverse(BigEndianFourByteArray);
            return BitConverter.ToInt32(BigEndianFourByteArray, 0);
        }
        return BitConverter.ToInt32(bytes, dataPosition);
    }

    private static uint BytesToUInt(byte[] bytes, int dataPosition)
    {
        if (!BitConverter.IsLittleEndian) {
            Array.Copy(bytes, dataPosition, BigEndianFourByteArray, 0, 4);
            Array.Reverse(BigEndianFourByteArray);
            return BitConverter.ToUInt32(BigEndianFourByteArray, 0);
        }
        return BitConverter.ToUInt32(bytes, dataPosition);
    }

    private static float BytesToFloat(byte[] bytes, int dataPosition)
    {
        if (!BitConverter.IsLittleEndian) {
            Array.Copy(bytes, dataPosition, BigEndianFourByteArray, 0, 4);
            Array.Reverse(BigEndianFourByteArray);
            return BitConverter.ToSingle(BigEndianFourByteArray, 0);
        }
        return BitConverter.ToSingle(bytes, dataPosition);
    }

    private static double BytesToDouble(byte[] bytes, int dataPosition)
    {
        if (!BitConverter.IsLittleEndian) {
            Array.Copy(bytes, dataPosition, BigEndianEightByteArray, 0, 8);
            Array.Reverse(BigEndianEightByteArray);
            return BitConverter.ToDouble(BigEndianEightByteArray, 0);
        }
        return BitConverter.ToDouble(bytes, dataPosition);
    }

    private static long BytesToLong(byte[] bytes, int dataPosition)
    {
        if (!BitConverter.IsLittleEndian) {
            Array.Copy(bytes, dataPosition, BigEndianEightByteArray, 0, 8);
            Array.Reverse(BigEndianEightByteArray);
            return BitConverter.ToInt64(BigEndianEightByteArray, 0);
        }
        return BitConverter.ToInt64(bytes, dataPosition);
    }

    private static bool BytesToBool(byte[] bytes, int dataPosition)
    {
        return BitConverter.ToBoolean(bytes, dataPosition);
    }

    private static string BytesToString(byte[] bytes, int dataPosition, int dataSize)
    {
        if (dataSize == 0)
            return "";
        uint crc = crc32(bytes, dataPosition, dataSize);
        string s;
        if (!stringCache.TryGetValue(crc, out s)) {
            s = Encoding.UTF8.GetString(bytes, dataPosition, dataSize);
            stringCache.Add(crc, s);
        }
        return s;
    }

    public static uint crc32(byte[] input, int dataPosition, int dataSize)
    {
        unchecked {
            uint crc = (uint)(((uint)0) ^ (-1));
            var len = input.Length;
            for (var i = dataPosition; i < dataPosition + dataSize; i++) {
                crc = (crc >> 8) ^ crcTable[
                    (crc ^ input[i]) & 0xFF
            ];
            }
            crc = (uint)(crc ^ (-1));
            if (crc < 0) {
                crc += (uint)4294967296;
            }
            return crc;
        }
    }

    private static byte BytesToByte(byte[] bytes, int dataPosition)
    {
        return bytes[dataPosition];
    }

    private static Color BytesToColor(byte[] bytes, int dataPosition)
    {
        var color = Color.black;
        color.r = BytesToFloat(bytes, dataPosition);
        color.g = BytesToFloat(bytes, dataPosition + 4);
        color.b = BytesToFloat(bytes, dataPosition + 8);
        color.a = BytesToFloat(bytes, dataPosition + 12);
        return color;
    }

    private static Vector2 BytesToVector2(byte[] bytes, int dataPosition)
    {
        var vector2 = Vector2.zero;
        vector2.x = BytesToFloat(bytes, dataPosition);
        vector2.y = BytesToFloat(bytes, dataPosition + 4);
        return vector2;
    }

    private static Vector2Int BytesToVector2Int(byte[] bytes, int dataPosition)
    {
        var vector2 = Vector2Int.zero;
        vector2.x = BytesToInt(bytes, dataPosition);
        vector2.y = BytesToInt(bytes, dataPosition + 4);
        return vector2;
    }

    private static Vector3 BytesToVector3(byte[] bytes, int dataPosition)
    {
        var vector3 = Vector3.zero;
        vector3.x = BytesToFloat(bytes, dataPosition);
        vector3.y = BytesToFloat(bytes, dataPosition + 4);
        vector3.z = BytesToFloat(bytes, dataPosition + 8);
        return vector3;
    }

    private static Vector3Int BytesToVector3Int(byte[] bytes, int dataPosition)
    {
        var vector3 = Vector3Int.zero;
        vector3.x = BytesToInt(bytes, dataPosition);
        vector3.y = BytesToInt(bytes, dataPosition + 4);
        vector3.z = BytesToInt(bytes, dataPosition + 8);
        return vector3;
    }

    private static Vector4 BytesToVector4(byte[] bytes, int dataPosition)
    {
        var vector4 = Vector4.zero;
        vector4.x = BytesToFloat(bytes, dataPosition);
        vector4.y = BytesToFloat(bytes, dataPosition + 4);
        vector4.z = BytesToFloat(bytes, dataPosition + 8);
        vector4.w = BytesToFloat(bytes, dataPosition + 12);
        return vector4;
    }

    private static Quaternion BytesToQuaternion(byte[] bytes, int dataPosition)
    {
        var quaternion = Quaternion.identity;
        quaternion.x = BytesToFloat(bytes, dataPosition);
        quaternion.y = BytesToFloat(bytes, dataPosition + 4);
        quaternion.z = BytesToFloat(bytes, dataPosition + 8);
        quaternion.w = BytesToFloat(bytes, dataPosition + 12);
        return quaternion;
    }

    private static Rect BytesToRect(byte[] bytes, int dataPosition)
    {
        var rect = new Rect();
        rect.x = BytesToFloat(bytes, dataPosition);
        rect.y = BytesToFloat(bytes, dataPosition + 4);
        rect.width = BytesToFloat(bytes, dataPosition + 8);
        rect.height = BytesToFloat(bytes, dataPosition + 12);
        return rect;
    }

    private static Matrix4x4 BytesToMatrix4x4(byte[] bytes, int dataPosition)
    {
        var matrix4x4 = Matrix4x4.identity;
        matrix4x4.m00 = BytesToFloat(bytes, dataPosition);
        matrix4x4.m01 = BytesToFloat(bytes, dataPosition + 4);
        matrix4x4.m02 = BytesToFloat(bytes, dataPosition + 8);
        matrix4x4.m03 = BytesToFloat(bytes, dataPosition + 12);
        matrix4x4.m10 = BytesToFloat(bytes, dataPosition + 16);
        matrix4x4.m11 = BytesToFloat(bytes, dataPosition + 20);
        matrix4x4.m12 = BytesToFloat(bytes, dataPosition + 24);
        matrix4x4.m13 = BytesToFloat(bytes, dataPosition + 28);
        matrix4x4.m20 = BytesToFloat(bytes, dataPosition + 32);
        matrix4x4.m21 = BytesToFloat(bytes, dataPosition + 36);
        matrix4x4.m22 = BytesToFloat(bytes, dataPosition + 40);
        matrix4x4.m23 = BytesToFloat(bytes, dataPosition + 44);
        matrix4x4.m30 = BytesToFloat(bytes, dataPosition + 48);
        matrix4x4.m31 = BytesToFloat(bytes, dataPosition + 52);
        matrix4x4.m32 = BytesToFloat(bytes, dataPosition + 56);
        matrix4x4.m33 = BytesToFloat(bytes, dataPosition + 60);
        return matrix4x4;
    }

    private static AnimationCurve BytesToAnimationCurve(byte[] bytes, int dataPosition)
    {
        var animationCurve = new AnimationCurve();
        var keyCount = BytesToInt(bytes, dataPosition);
        for (int i = 0; i < keyCount; ++i) {
            var keyframe = new Keyframe();
            keyframe.time = BytesToFloat(bytes, dataPosition + 4);
            keyframe.value = BytesToFloat(bytes, dataPosition + 8);
            keyframe.inTangent = BytesToFloat(bytes, dataPosition + 12);
            keyframe.outTangent = BitConverter.ToSingle(bytes, dataPosition + 16);
            animationCurve.AddKey(keyframe);
            dataPosition += animationCurveAdvance;
        }
        animationCurve.preWrapMode = (WrapMode)BytesToInt(bytes, dataPosition + 4);
        animationCurve.postWrapMode = (WrapMode)BytesToInt(bytes, dataPosition + 8);
        return animationCurve;
    }

    private static LayerMask BytesToLayerMask(byte[] bytes, int dataPosition)
    {
        var layerMask = new LayerMask();
        layerMask.value = BytesToInt(bytes, dataPosition);
        return layerMask;
    }

    private static object IndexToUnityObject(int index, FieldSerializationData activeFieldSerializationData)
    {
        if (index < 0 || index >= activeFieldSerializationData.unityObjects.Count) {
            return null;
        }

        return activeFieldSerializationData.unityObjects[index];
    }

    private static SharedVariable BytesToSharedVariable(FieldSerializationData fieldSerializationData, Dictionary<int, int> fieldIndexMap, byte[] bytes, int dataPosition, IVariableSource variableSource, bool fromField, int hashPrefix)
    {
        SharedVariable sharedVariable = null;
        var variableTypeName = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Type", hashPrefix, null) as string;
        if (string.IsNullOrEmpty(variableTypeName)) {
            return null;
        }
        var variableName = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Name", hashPrefix, null) as string;
        var isShared = Convert.ToBoolean(LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsShared", hashPrefix, null));
        var isGlobal = Convert.ToBoolean(LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsGlobal", hashPrefix, null));
        var isDynamic = Convert.ToBoolean(LoadField(fieldSerializationData, fieldIndexMap, typeof(bool), "IsDynamic", hashPrefix, null));

        if (isShared && (!isDynamic || BehaviorManager.IsPlaying) && fromField) {
            if (!isGlobal) {
                sharedVariable = variableSource.GetVariable(variableName);
            } else {
                if (globalVariables == null) {
                    globalVariables = GlobalVariables.Instance;
                }
                if (globalVariables != null) {
                    sharedVariable = globalVariables.GetVariable(variableName);
                }
            }
        }

        var variableType = TaskUtility.GetTypeWithinAssembly(variableTypeName);
        if (variableType == null) {
            return null;
        }

        bool typesEqual = true;
        if (sharedVariable == null || !(typesEqual = sharedVariable.GetType().Equals(variableType))) {
            sharedVariable = TaskUtility.CreateInstance(variableType) as SharedVariable;
            sharedVariable.Name = variableName;
            sharedVariable.IsShared = isShared;
            sharedVariable.IsGlobal = isGlobal;
            sharedVariable.IsDynamic = isDynamic;
#if UNITY_EDITOR
            sharedVariable.Tooltip = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "Tooltip", hashPrefix, null) as string;
#endif
            if (!isGlobal) {
                sharedVariable.PropertyMapping = LoadField(fieldSerializationData, fieldIndexMap, typeof(string), "PropertyMapping", hashPrefix, null) as string;
                sharedVariable.PropertyMappingOwner = LoadField(fieldSerializationData, fieldIndexMap, typeof(GameObject), "PropertyMappingOwner", hashPrefix, null) as CoreObject;
                sharedVariable.InitializePropertyMapping(variableSource as BehaviorSource);
            }

            // if the types are not equal then this shared variable used to be a different type so it should be shared
            if (!typesEqual) {
                sharedVariable.IsShared = true;
            }

            if (isDynamic && BehaviorManager.IsPlaying) {
                // The new dynamic variable has been created.
                variableSource.SetVariable(variableName, sharedVariable);
            }

            LoadFields(fieldSerializationData, fieldIndexMap, sharedVariable, hashPrefix, variableSource);
        }

        return sharedVariable;
    }
}

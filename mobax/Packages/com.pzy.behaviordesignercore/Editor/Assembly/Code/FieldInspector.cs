namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using UnityEditor;
    using UnityEngine;

    public static class FieldInspector
    {
        private const string c_EditorPrefsFoldoutKey = "BehaviorDesigner.Editor.Foldout.";
        private static int currentKeyboardControl = -1;
        private static bool editingArray = false;
        private static int savedArraySize = -1;
        private static int editingFieldHash;
        public static BehaviorDesigner.Runtime.BehaviorSource behaviorSource;
        private static HashSet<int> drawnObjects = new HashSet<int>();
        private static string[] layerNames;
        private static int[] maskValues;

        private static object DrawArrayField(BehaviorDesigner.Runtime.Tasks.Task task, GUIContent guiContent, FieldInfo fieldInfo, System.Type fieldType, object value)
        {
            System.Type elementType;
            IList list;
            Array array2;
            int num6;
            if (fieldType.IsArray)
            {
                elementType = fieldType.GetElementType();
            }
            else
            {
                System.Type baseType = fieldType;
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
            if (value != null)
            {
                list = (IList) value;
            }
            else
            {
                if (!fieldType.IsGenericType && !fieldType.IsArray)
                {
                    list = Activator.CreateInstance(fieldType, true) as IList;
                }
                else
                {
                    System.Type[] typeArguments = new System.Type[] { elementType };
                    list = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments), true) as IList;
                }
                if (fieldType.IsArray)
                {
                    Array array = Array.CreateInstance(elementType, list.Count);
                    list.CopyTo(array, 0);
                    list = array;
                }
                GUI.changed =(true);
            }
            UnityEditor.EditorGUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
            if (!DrawFoldout(guiContent.text.GetHashCode(), guiContent))
            {
                goto TR_0000;
            }
            else
            {
                UnityEditor.EditorGUI.indentLevel++;
                bool flag = guiContent.text.GetHashCode() == editingFieldHash;
                int num = !flag ? list.Count : savedArraySize;
                int length = UnityEditor.EditorGUILayout.IntField("Size", num, Array.Empty<GUILayoutOption>());
                if (!flag || (!editingArray || ((GUIUtility.keyboardControl == currentKeyboardControl) && ((UnityEngine.Event.current.keyCode != (KeyCode)13) && (UnityEngine.Event.current.keyCode != (KeyCode)0x10f)))))
                {
                    if (length != num)
                    {
                        if (!editingArray)
                        {
                            currentKeyboardControl = GUIUtility.keyboardControl;
                            editingArray = true;
                            editingFieldHash = guiContent.text.GetHashCode();
                        }
                        savedArraySize = length;
                    }
                    goto TR_0005;
                }
                else if (length == list.Count)
                {
                    goto TR_0006;
                }
                else
                {
                    array2 = Array.CreateInstance(elementType, length);
                    int num3 = -1;
                    for (int i = 0; i < length; i++)
                    {
                        if (i < list.Count)
                        {
                            num3 = i;
                        }
                        if (num3 == -1)
                        {
                            break;
                        }
                        object obj2 = list[num3];
                        if ((i >= list.Count) && (!typeof(UnityEngine.Object).IsAssignableFrom(elementType) && !typeof(string).IsAssignableFrom(elementType)))
                        {
                            obj2 = Activator.CreateInstance(list[num3].GetType(), true);
                        }
                        array2.SetValue(obj2, i);
                    }
                }
            }
            if (fieldType.IsArray)
            {
                list = array2;
            }
            else
            {
                if (!fieldType.IsGenericType)
                {
                    list = Activator.CreateInstance(fieldType, true) as IList;
                }
                else
                {
                    System.Type[] typeArguments = new System.Type[] { elementType };
                    list = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments), true) as IList;
                }
                for (int i = 0; i < array2.Length; i++)
                {
                    list.Add(array2.GetValue(i));
                }
            }
            goto TR_0006;
        TR_0000:
            UnityEditor.EditorGUILayout.EndVertical();
            return list;
        TR_0005:
            num6 = 0;
            while (true)
            {
                if (num6 >= list.Count)
                {
                    UnityEditor.EditorGUI.indentLevel--;
                    break;
                }
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                guiContent.text =("Element " + num6);
                list[num6] = DrawField(task, guiContent, fieldInfo, elementType, list[num6]);
                GUILayout.Space(6f);
                GUILayout.EndHorizontal();
                num6++;
            }
            goto TR_0000;
        TR_0006:
            editingArray = false;
            savedArraySize = -1;
            editingFieldHash = -1;
            GUI.changed =(true);
            goto TR_0005;
        }

        public static object DrawField(BehaviorDesigner.Runtime.Tasks.Task task, GUIContent guiContent, FieldInfo field, object value)
        {
            ObjectDrawer objectDrawer = null;
            BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute[] attributeArray = null;
            objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task, field);
            if (objectDrawer != null)
            {
                if ((value == null) && !field.FieldType.IsAbstract)
                {
                    value = !typeof(ScriptableObject).IsAssignableFrom(field.FieldType) ? Activator.CreateInstance(field.FieldType, true) : ScriptableObject.CreateInstance(field.FieldType);
                }
                objectDrawer.Value = value;
                objectDrawer.OnGUI(guiContent);
                if (objectDrawer.Value != value)
                {
                    value = objectDrawer.Value;
                    GUI.changed =(true);
                }
                return value;
            }
            if (((attributeArray = field.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute), true) as BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute[]).Length <= 0) || ((objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task, field, attributeArray[0])) == null))
            {
                return DrawField(task, guiContent, field, field.FieldType, value);
            }
            if (value == null)
            {
                value = !typeof(ScriptableObject).IsAssignableFrom(field.FieldType) ? Activator.CreateInstance(field.FieldType, true) : ScriptableObject.CreateInstance(field.FieldType);
            }
            objectDrawer.Value = value;
            objectDrawer.OnGUI(guiContent);
            if (objectDrawer.Value != value)
            {
                value = objectDrawer.Value;
                GUI.changed =(true);
            }
            return value;
        }

        private static object DrawField(BehaviorDesigner.Runtime.Tasks.Task task, GUIContent guiContent, FieldInfo fieldInfo, System.Type fieldType, object value)
        {
            return (!typeof(IList).IsAssignableFrom(fieldType) ? DrawSingleField(task, guiContent, fieldInfo, fieldType, value) : DrawArrayField(task, guiContent, fieldInfo, fieldType, value));
        }

        public static object DrawFields(BehaviorDesigner.Runtime.Tasks.Task task, object obj)
        {
            return DrawFields(task, obj, null);
        }

        public static object DrawFields(BehaviorDesigner.Runtime.Tasks.Task task, object obj, GUIContent guiContent)
        {
            if (obj == null)
            {
                return null;
            }
            List<System.Type> baseClasses = GetBaseClasses(obj.GetType());
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            int num = baseClasses.Count - 1;
            while (num > -1)
            {
                FieldInfo[] fields = baseClasses[num].GetFields(bindingAttr);
                int index = 0;
                while (true)
                {
                    if (index >= fields.Length)
                    {
                        num--;
                        break;
                    }
                    if (((!BehaviorDesignerUtility.HasAttribute(fields[index], typeof(NonSerializedAttribute)) && !BehaviorDesignerUtility.HasAttribute(fields[index], typeof(HideInInspector))) && ((!fields[index].IsPrivate && !fields[index].IsFamily) || BehaviorDesignerUtility.HasAttribute(fields[index], typeof(SerializeField)))) && (!(obj is BehaviorDesigner.Runtime.Tasks.ParentTask) || !fields[index].Name.Equals("children")))
                    {
                        if (guiContent == null)
                        {
                            BehaviorDesigner.Runtime.Tasks.TooltipAttribute[] attributeArray = null;
                            string name = fields[index].Name;
                            guiContent = ((attributeArray = fields[index].GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TooltipAttribute), false) as BehaviorDesigner.Runtime.Tasks.TooltipAttribute[]).Length <= 0) ? new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name)) : new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name), attributeArray[0].Tooltip);
                        }
                        UnityEditor.EditorGUI.BeginChangeCheck();
                        object obj2 = DrawField(task, guiContent, fields[index], fields[index].GetValue(obj));
                        if (UnityEditor.EditorGUI.EndChangeCheck())
                        {
                            fields[index].SetValue(obj, obj2);
                            GUI.changed =(true);
                        }
                        guiContent = null;
                    }
                    index++;
                }
            }
            return obj;
        }

        public static bool DrawFoldout(int hash, GUIContent guiContent)
        {
            object[] objArray1 = new object[] { "BehaviorDesigner.Editor.Foldout..", hash, ".", guiContent.text };
            string key = string.Concat(objArray1);
            bool @bool = UnityEditor.EditorPrefs.GetBool(key, true);
            bool flag2 = UnityEditor.EditorGUILayout.Foldout(@bool, guiContent);
            if (flag2 != @bool)
            {
                UnityEditor.EditorPrefs.SetBool(key, flag2);
            }
            return flag2;
        }

        private static LayerMask DrawLayerMask(GUIContent guiContent, LayerMask layerMask)
        {
            if (layerNames == null)
            {
                InitLayers();
            }
            int num = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                if ((layerMask.value & maskValues[i]) == maskValues[i])
                {
                    num |= 1 << (i & 0x1f);
                }
            }
            int num3 = UnityEditor.EditorGUILayout.MaskField(guiContent, num, layerNames, Array.Empty<GUILayoutOption>());
            if (num3 != num)
            {
                num = 0;
                int index = 0;
                while (true)
                {
                    if (index >= layerNames.Length)
                    {
                        layerMask.value =(num);
                        break;
                    }
                    if ((num3 & (1 << (index & 0x1f))) != 0)
                    {
                        num |= maskValues[index];
                    }
                    index++;
                }
            }
            return layerMask;
        }

        public static BehaviorDesigner.Runtime.SharedVariable DrawSharedVariable(BehaviorDesigner.Runtime.Tasks.Task task, GUIContent guiContent, FieldInfo fieldInfo, System.Type fieldType, BehaviorDesigner.Runtime.SharedVariable sharedVariable)
        {
            if (!fieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)))
            {
                if (sharedVariable == null)
                {
                    sharedVariable = Activator.CreateInstance(fieldType, true) as BehaviorDesigner.Runtime.SharedVariable;
                    GUI.changed =(true);
                }
                if (!sharedVariable.IsShared && (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) || BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute))))
                {
                    sharedVariable.IsShared = true;
                    GUI.changed =(true);
                }
            }
            if ((sharedVariable != null) && sharedVariable.IsDynamic)
            {
                sharedVariable.Name = UnityEditor.EditorGUILayout.TextField(guiContent, sharedVariable.Name, Array.Empty<GUILayoutOption>());
                sharedVariable = DrawSharedVariableToggleSharedButton(sharedVariable);
                if (!sharedVariable.IsDynamic && (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) || BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute))))
                {
                    sharedVariable = null;
                }
            }
            else if ((sharedVariable != null) && !sharedVariable.IsShared)
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                ObjectDrawer drawer = null;
                BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute[] attributeArray = null;
                if ((fieldInfo == null) || (((attributeArray = fieldInfo.GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute), true) as BehaviorDesigner.Runtime.Tasks.ObjectDrawerAttribute[]).Length <= 0) || ((drawer = ObjectDrawerUtility.GetObjectDrawer(task, fieldInfo, attributeArray[0])) == null)))
                {
                    DrawFields(task, sharedVariable, guiContent);
                }
                else
                {
                    drawer.Value = sharedVariable;
                    drawer.OnGUI(guiContent);
                }
                if (!BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                {
                    sharedVariable = DrawSharedVariableToggleSharedButton(sharedVariable);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                string[] names = null;
                int globalStartIndex = -1;
                bool addDynamic = !fieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable));
                int index = GetVariablesOfType((sharedVariable == null) ? null : sharedVariable.GetType().GetProperty("Value").PropertyType, (sharedVariable != null) && sharedVariable.IsGlobal, (sharedVariable == null) ? string.Empty : sharedVariable.Name, behaviorSource, out names, ref globalStartIndex, fieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)), addDynamic);
                Color color = GUI.backgroundColor;
                if ((index == 0) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                {
                    GUI.backgroundColor =(Color.red);
                }
                index = UnityEditor.EditorGUILayout.Popup(guiContent.text, index, names, BehaviorDesignerUtility.SharedVariableToolbarPopup, Array.Empty<GUILayoutOption>());
                GUI.backgroundColor =(color);
                if (index != index)
                {
                    if (index == 0)
                    {
                        if (fieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)))
                        {
                            sharedVariable = null;
                        }
                        else
                        {
                            sharedVariable = Activator.CreateInstance(fieldType, true) as BehaviorDesigner.Runtime.SharedVariable;
                            sharedVariable.IsShared = true;
                        }
                    }
                    else if (index < (names.Length - (!addDynamic ? 0 : 1)))
                    {
                        sharedVariable = ((globalStartIndex == -1) || (index < globalStartIndex)) ? behaviorSource.GetVariable(names[index]) : BehaviorDesigner.Runtime.GlobalVariables.Instance.GetVariable(names[index].Substring(8, names[index].Length - 8));
                    }
                    else
                    {
                        sharedVariable = Activator.CreateInstance(fieldType, true) as BehaviorDesigner.Runtime.SharedVariable;
                        sharedVariable.IsShared = true;
                        sharedVariable.IsDynamic = true;
                    }
                    GUI.changed =(true);
                }
                if (!fieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) && (!BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fieldInfo, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute))))
                {
                    sharedVariable = DrawSharedVariableToggleSharedButton(sharedVariable);
                    GUILayout.Space(-3f);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(3f);
            }
            return sharedVariable;
        }

        internal static BehaviorDesigner.Runtime.SharedVariable DrawSharedVariableToggleSharedButton(BehaviorDesigner.Runtime.SharedVariable sharedVariable)
        {
            if (sharedVariable == null)
            {
                return null;
            }
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(15f) };
            if (GUILayout.Button(!sharedVariable.IsShared ? BehaviorDesignerUtility.VariableButtonTexture : BehaviorDesignerUtility.VariableButtonSelectedTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray1))
            {
                bool flag = !sharedVariable.IsShared;
                sharedVariable = !sharedVariable.GetType().Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) ? (Activator.CreateInstance(sharedVariable.GetType(), true) as BehaviorDesigner.Runtime.SharedVariable) : (Activator.CreateInstance(FriendlySharedVariableName(sharedVariable.GetType().GetProperty("Value").PropertyType), true) as BehaviorDesigner.Runtime.SharedVariable);
                sharedVariable.IsShared = flag;
                if (!flag)
                {
                    sharedVariable.IsDynamic = false;
                }
            }
            return sharedVariable;
        }

        private static object DrawSingleField(BehaviorDesigner.Runtime.Tasks.Task task, GUIContent guiContent, FieldInfo fieldInfo, System.Type fieldType, object value)
        {
            if (fieldType.Equals(typeof(int)))
            {
                return UnityEditor.EditorGUILayout.IntField(guiContent, (int) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(float)))
            {
                return UnityEditor.EditorGUILayout.FloatField(guiContent, (float) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(double)))
            {
                return UnityEditor.EditorGUILayout.FloatField(guiContent, Convert.ToSingle((double) value), Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(long)))
            {
                return (long) UnityEditor.EditorGUILayout.IntField(guiContent, Convert.ToInt32((long) value), Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(bool)))
            {
                return UnityEditor.EditorGUILayout.Toggle(guiContent, (bool) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(string)))
            {
                return UnityEditor.EditorGUILayout.TextField(guiContent, (string) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(byte)))
            {
                return Convert.ToByte(UnityEditor.EditorGUILayout.IntField(guiContent, Convert.ToInt32(value), Array.Empty<GUILayoutOption>()));
            }
            if (fieldType.Equals(typeof(uint)))
            {
                int num = UnityEditor.EditorGUILayout.IntField(guiContent, Convert.ToInt32(value), Array.Empty<GUILayoutOption>());
                if (num < 0)
                {
                    num = 0;
                }
                return Convert.ToUInt32(num);
            }
            if (fieldType.Equals(typeof(Vector2)))
            {
                return UnityEditor.EditorGUILayout.Vector2Field(guiContent, (Vector2) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Vector2Int)))
            {
                return UnityEditor.EditorGUILayout.Vector2IntField(guiContent, (Vector2Int) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Vector3)))
            {
                return UnityEditor.EditorGUILayout.Vector3Field(guiContent, (Vector3) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Vector3Int)))
            {
                return UnityEditor.EditorGUILayout.Vector3IntField(guiContent, (Vector3Int) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Vector3)))
            {
                return UnityEditor.EditorGUILayout.Vector3Field(guiContent, (Vector3) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Vector4)))
            {
                return UnityEditor.EditorGUILayout.Vector4Field(guiContent.text, (Vector4) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Quaternion)))
            {
                Quaternion quaternion = (Quaternion) value;
                Vector4 vector = Vector4.zero;
                vector.Set(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
                vector = UnityEditor.EditorGUILayout.Vector4Field(guiContent.text, vector, Array.Empty<GUILayoutOption>());
                quaternion.Set(vector.x, vector.y, vector.z, vector.w);
                return quaternion;
            }
            if (fieldType.Equals(typeof(Color)))
            {
                return UnityEditor.EditorGUILayout.ColorField(guiContent, (Color) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Rect)))
            {
                return UnityEditor.EditorGUILayout.RectField(guiContent, (Rect) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(Matrix4x4)))
            {
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
                if (DrawFoldout(guiContent.text.GetHashCode(), guiContent))
                {
                    UnityEditor.EditorGUI.indentLevel++;
                    Matrix4x4 matrixx = (Matrix4x4) value;
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= 4)
                        {
                            value = matrixx;
                            UnityEditor.EditorGUI.indentLevel--;
                            break;
                        }
                        int num3 = 0;
                        while (true)
                        {
                            if (num3 >= 4)
                            {
                                num2++;
                                break;
                            }
                            UnityEditor.EditorGUI.BeginChangeCheck(); 
                            matrixx[num2, num3] = UnityEditor.EditorGUILayout.FloatField("E" + num2.ToString() + num3.ToString(), matrixx[num2, num3], Array.Empty<GUILayoutOption>());
                            if (UnityEditor.EditorGUI.EndChangeCheck())
                            {
                                GUI.changed =(true);
                            }
                            num3++;
                        }
                    }
                }
                GUILayout.EndVertical();
                return value;
            }
            if (fieldType.Equals(typeof(AnimationCurve)))
            {
                if (value == null)
                {
                    value = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
                    GUI.changed =(true);
                }
                return UnityEditor.EditorGUILayout.CurveField(guiContent, (AnimationCurve) value, Array.Empty<GUILayoutOption>());
            }
            if (fieldType.Equals(typeof(LayerMask)))
            {
                return DrawLayerMask(guiContent, (LayerMask) value);
            }
            if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(fieldType))
            {
                return DrawSharedVariable(task, guiContent, fieldInfo, fieldType, value as BehaviorDesigner.Runtime.SharedVariable);
            }
            if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                return UnityEditor.EditorGUILayout.ObjectField(guiContent, (UnityEngine.Object) value, fieldType, true, Array.Empty<GUILayoutOption>());
            }

            // 增加 CoreEngineObject 支持
            if(typeof(CoreObject).IsAssignableFrom(fieldType))
            {
                var ret = CoreEngineEditorUtil.DrawObject(guiContent, (CoreObject)value, typeof(CoreObject));
                return ret;
            }
            if (typeof(CoreComponent).IsAssignableFrom(fieldType))
            {
                var ret = CoreEngineEditorUtil.DrawObject(guiContent, (CoreComponent)value, typeof(CoreComponent));
                return ret;
            }

            if (fieldType.IsEnum)
            {
                return UnityEditor.EditorGUILayout.EnumPopup(guiContent, (Enum) value, Array.Empty<GUILayoutOption>());
            }
            if (!fieldType.IsClass && (!fieldType.IsValueType || fieldType.IsPrimitive))
            {
                UnityEditor.EditorGUILayout.LabelField("Unsupported Type: " + fieldType, Array.Empty<GUILayoutOption>());
                return null;
            }
            if (typeof(Delegate).IsAssignableFrom(fieldType))
            {
                return null;
            }
            int hashCode = guiContent.text.GetHashCode();
            if (drawnObjects.Contains(hashCode))
            {
                return null;
            }
            try
            {
                drawnObjects.Add(hashCode);
                GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
                if (value == null)
                {
                    if (fieldType.IsGenericType && (fieldType.GetGenericTypeDefinition() == typeof(Nullable<>)))
                    {
                        fieldType = Nullable.GetUnderlyingType(fieldType);
                    }
                    value = Activator.CreateInstance(fieldType, true);
                }
                if (DrawFoldout(hashCode, guiContent))
                {
                    UnityEditor.EditorGUI.indentLevel++;
                    value = DrawFields(task, value);
                    UnityEditor.EditorGUI.indentLevel--;
                }
                drawnObjects.Remove(hashCode);
                GUILayout.EndVertical();
            }
            catch (Exception)
            {
                GUILayout.EndVertical();
                drawnObjects.Remove(hashCode);
            }
            return value;
        }

        internal static System.Type FriendlySharedVariableName(System.Type type)
        {
            if (type.Equals(typeof(bool)))
            {
                return BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedBool");
            }
            if (type.Equals(typeof(int)))
            {
                return BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedInt");
            }
            if (type.Equals(typeof(float)))
            {
                return BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedFloat");
            }
            if (type.Equals(typeof(string)))
            {
                return BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString");
            }
            System.Type typeWithinAssembly = BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.Shared" + type.Name);
            if (typeWithinAssembly != null)
            {
                return typeWithinAssembly;
            }
            typeWithinAssembly = BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("Shared" + type.Name);
            return ((typeWithinAssembly == null) ? type : typeWithinAssembly);
        }

        public static List<System.Type> GetBaseClasses(System.Type t)
        {
            List<System.Type> list = new List<System.Type>();
            while ((t != null) && (!t.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ParentTask)) && (!t.Equals(typeof(BehaviorDesigner.Runtime.Tasks.Task)) && !t.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)))))
            {
                list.Add(t);
                t = t.BaseType;
            }
            return list;
        }

        public static int GetVariablesOfType(System.Type valueType, bool isGlobal, string name, BehaviorDesigner.Runtime.BehaviorSource behaviorSource, out string[] names, ref int globalStartIndex, bool getAll, bool addDynamic)
        {
            if (behaviorSource == null)
            {
                names = new string[0];
                return 0;
            }
            List<BehaviorDesigner.Runtime.SharedVariable> variables = behaviorSource.Variables;
            int num = 0;
            List<string> list2 = new List<string>();
            list2.Add("(None)");
            if (variables != null)
            {
                for (int i = 0; i < variables.Count; i++)
                {
                    if (variables[i] != null)
                    {
                        System.Type propertyType = variables[i].GetType().GetProperty("Value").PropertyType;
                        if ((valueType == null) || (getAll || valueType.IsAssignableFrom(propertyType)))
                        {
                            list2.Add(variables[i].Name);
                            if (!isGlobal && variables[i].Name.Equals(name))
                            {
                                num = list2.Count - 1;
                            }
                        }
                    }
                }
            }
            BehaviorDesigner.Runtime.GlobalVariables instance = null;
            instance = BehaviorDesigner.Runtime.GlobalVariables.Instance;
            if (instance != null)
            {
                globalStartIndex = list2.Count;
                variables = instance.Variables;
                if (variables != null)
                {
                    for (int i = 0; i < variables.Count; i++)
                    {
                        if (variables[i] != null)
                        {
                            System.Type propertyType = variables[i].GetType().GetProperty("Value").PropertyType;
                            if ((valueType == null) || (getAll || propertyType.Equals(valueType)))
                            {
                                list2.Add("Globals/" + variables[i].Name);
                                if (isGlobal && variables[i].Name.Equals(name))
                                {
                                    num = list2.Count - 1;
                                }
                            }
                        }
                    }
                }
            }
            if (addDynamic)
            {
                list2.Add("(Dynamic)");
            }
            names = list2.ToArray();
            return num;
        }

        public static void Init()
        {
            InitLayers();
        }

        private static void InitLayers()
        {
            List<string> list = new List<string>();
            List<int> list2 = new List<int>();
            for (int i = 0; i < 0x20; i++)
            {
                string str = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(str))
                {
                    list.Add(str);
                    list2.Add(1 << (i & 0x1f));
                }
            }
            layerNames = list.ToArray();
            maskValues = list2.ToArray();
        }
    }
}


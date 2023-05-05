namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using BehaviorDesigner.Runtime.Tasks;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;

    [Serializable]
    public class TaskInspector : ScriptableObject
    {
        private BehaviorDesignerWindow behaviorDesignerWindow;
        private BehaviorDesigner.Runtime.Tasks.Task activeReferenceTask;
        private FieldInfo activeReferenceTaskFieldInfo;
        private BehaviorDesigner.Runtime.Tasks.Task mActiveMenuSelectionTask;
        private Vector2 mScrollPosition = Vector2.zero;
        [CompilerGenerated]
        private static UnityEditor.GenericMenu.MenuFunction2 cache0;
        [CompilerGenerated]
        private static UnityEditor.GenericMenu.MenuFunction2 cache1;

        private void AddColorMenuItem(ref UnityEditor.GenericMenu menu, BehaviorDesigner.Runtime.Tasks.Task task, string color, int index)
        {
            menu.AddItem(new GUIContent(color), task.NodeData.ColorIndex == index, new UnityEditor.GenericMenu.MenuFunction2(this.SetTaskColor), new TaskColor(task, index));
        }

        private bool CanDrawReflectedField(object task, FieldInfo field)
        {
            if (!field.Name.Contains("parameter") && (!field.Name.Contains("storeResult") && (!field.Name.Contains("fieldValue") && (!field.Name.Contains("propertyValue") && !field.Name.Contains("compareValue")))))
            {
                return true;
            }
            if (this.IsInvokeMethodTask(task.GetType()))
            {
                if (field.Name.Contains("parameter"))
                {
                    return (task.GetType().GetField(field.Name).GetValue(task) != null);
                }
                MethodInfo info2 = null;
                return (((info2 = this.GetInvokeMethodInfo(task)) != null) ? (!field.Name.Equals("storeResult") || !info2.ReturnType.Equals(typeof(void))) : false);
            }
            if (this.IsFieldReflectionTask(task.GetType()))
            {
                BehaviorDesigner.Runtime.SharedVariable variable = task.GetType().GetField("fieldName").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
                return ((variable != null) && !string.IsNullOrEmpty((string) variable.GetValue()));
            }
            BehaviorDesigner.Runtime.SharedVariable variable2 = task.GetType().GetField("propertyName").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
            return ((variable2 != null) && !string.IsNullOrEmpty((string) variable2.GetValue()));
        }

        public void ClearFocus()
        {
            GUIUtility.keyboardControl =(0);
        }

        private void ClearInvokeVariablesTask()
        {
            for (int i = 0; i < 4; i++)
            {
                this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (i + 1)).SetValue(this.mActiveMenuSelectionTask, null);
            }
            this.mActiveMenuSelectionTask.GetType().GetField("storeResult").SetValue(this.mActiveMenuSelectionTask, null);
        }

        private void ComponentSelectionCallback(object obj)
        {
            if (this.mActiveMenuSelectionTask != null)
            {
                FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("componentName");
                BehaviorDesigner.Runtime.SharedVariable variable = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as BehaviorDesigner.Runtime.SharedVariable;
                if (obj == null)
                {
                    field.SetValue(this.mActiveMenuSelectionTask, variable);
                    variable = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as BehaviorDesigner.Runtime.SharedVariable;
                    FieldInfo info2 = null;
                    if (!this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                    {
                        info2 = !this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()) ? this.mActiveMenuSelectionTask.GetType().GetField("propertyName") : this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
                    }
                    else
                    {
                        info2 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
                        this.ClearInvokeVariablesTask();
                    }
                    info2.SetValue(this.mActiveMenuSelectionTask, variable);
                }
                else
                {
                    string str = (string) obj;
                    if (!str.Equals((string) (field.GetValue(this.mActiveMenuSelectionTask) as BehaviorDesigner.Runtime.SharedVariable).GetValue()))
                    {
                        FieldInfo info3 = null;
                        FieldInfo info4 = null;
                        if (this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                        {
                            info3 = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
                            int num = 0;
                            while (true)
                            {
                                if (num >= 4)
                                {
                                    info4 = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
                                    break;
                                }
                                this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (num + 1)).SetValue(this.mActiveMenuSelectionTask, null);
                                num++;
                            }
                        }
                        else if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
                        {
                            info3 = this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
                            info4 = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
                            if (info4 == null)
                            {
                                info4 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                            }
                        }
                        else
                        {
                            info3 = this.mActiveMenuSelectionTask.GetType().GetField("propertyName");
                            info4 = this.mActiveMenuSelectionTask.GetType().GetField("propertyValue");
                            if (info4 == null)
                            {
                                info4 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                            }
                        }
                        info3.SetValue(this.mActiveMenuSelectionTask, variable);
                        info4.SetValue(this.mActiveMenuSelectionTask, null);
                    }
                    variable = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as BehaviorDesigner.Runtime.SharedVariable;
                    variable.SetValue(str);
                    field.SetValue(this.mActiveMenuSelectionTask, variable);
                }
            }
            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        private void DrawObjectFields(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, TaskList taskList, BehaviorDesigner.Runtime.Tasks.Task task, object obj, bool enabled, bool drawWatch)
        {
            if (obj != null)
            {
                ObjectDrawer objectDrawer = ObjectDrawerUtility.GetObjectDrawer(task);
                if (objectDrawer != null)
                {
                    objectDrawer.OnGUI(new GUIContent());
                }
                else
                {
                    List<System.Type> baseClasses = FieldInspector.GetBaseClasses(obj.GetType());
                    BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                    bool isReflectionTask = this.IsReflectionTask(obj.GetType());
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
                            if (((!BehaviorDesignerUtility.HasAttribute(fields[index], typeof(NonSerializedAttribute)) && !BehaviorDesignerUtility.HasAttribute(fields[index], typeof(HideInInspector))) && (((!fields[index].IsPrivate && !fields[index].IsFamily) || BehaviorDesignerUtility.HasAttribute(fields[index], typeof(SerializeField))) && (!(obj is BehaviorDesigner.Runtime.Tasks.ParentTask) || !fields[index].Name.Equals("children")))) && ((!isReflectionTask || (!fields[index].FieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) && !fields[index].FieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable)))) || this.CanDrawReflectedField(obj, fields[index])))
                            {
                                HeaderAttribute[] attributeArray;
                                SpaceAttribute[] attributeArray2;
                                if ((attributeArray = fields[index].GetCustomAttributes(typeof(HeaderAttribute), true) as HeaderAttribute[]).Length > 0)
                                {
                                    UnityEditor.EditorGUILayout.LabelField(attributeArray[0].header, BehaviorDesignerUtility.BoldLabelGUIStyle, Array.Empty<GUILayoutOption>());
                                }
                                if ((attributeArray2 = fields[index].GetCustomAttributes(typeof(SpaceAttribute), true) as SpaceAttribute[]).Length > 0)
                                {
                                    GUILayout.Space(attributeArray2[0].height);
                                }
                                GUIContent guiContent = null;
                                BehaviorDesigner.Runtime.Tasks.TooltipAttribute[] attributeArray3 = null;
                                string name = fields[index].Name;
                                if (isReflectionTask && (fields[index].FieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) || fields[index].FieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable))))
                                {
                                    name = this.InvokeParameterName(obj, fields[index]);
                                }
                                guiContent = ((attributeArray3 = fields[index].GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TooltipAttribute), false) as BehaviorDesigner.Runtime.Tasks.TooltipAttribute[]).Length <= 0) ? new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name)) : new GUIContent(BehaviorDesignerUtility.SplitCamelCase(name), attributeArray3[0].Tooltip);
                                object obj2 = fields[index].GetValue(obj);
                                System.Type fieldType = fields[index].FieldType;
                                if (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(fieldType) || (typeof(IList).IsAssignableFrom(fieldType) && (typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(fieldType.GetElementType()) || (fieldType.IsGenericType && typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(fieldType.GetGenericArguments()[0])))))
                                {
                                    UnityEditor.EditorGUI.BeginChangeCheck();
                                    this.DrawTaskValue(behaviorSource, taskList, fields[index], guiContent, task, obj2 as BehaviorDesigner.Runtime.Tasks.Task, enabled);
                                    if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index].Name))
                                    {
                                        GUILayout.Space(-3f);
                                        GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(20f) };
                                        GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, optionArray1);
                                    }
                                    if (UnityEditor.EditorGUI.EndChangeCheck())
                                    {
                                        GUI.changed =(true);
                                    }
                                }
                                else if (fieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) || fieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable)))
                                {
                                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                                    UnityEditor.EditorGUI.BeginChangeCheck();
                                    if (drawWatch)
                                    {
                                        this.DrawWatchedButton(task, fields[index]);
                                    }
                                    BehaviorDesigner.Runtime.SharedVariable variable = this.DrawSharedVariableValue(behaviorSource, fields[index], guiContent, task, obj2 as BehaviorDesigner.Runtime.SharedVariable, isReflectionTask, enabled, drawWatch);
                                    if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index].Name))
                                    {
                                        GUILayout.Space(-3f);
                                        GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(20f) };
                                        GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, optionArray2);
                                    }
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(4f);
                                    if (UnityEditor.EditorGUI.EndChangeCheck())
                                    {
                                        fields[index].SetValue(obj, variable);
                                        GUI.changed =(true);
                                    }
                                }
                                else
                                {
                                    GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                                    UnityEditor.EditorGUI.BeginChangeCheck();
                                    if (drawWatch)
                                    {
                                        this.DrawWatchedButton(task, fields[index]);
                                    }
                                    object obj3 = FieldInspector.DrawField(task, guiContent, fields[index], obj2);
                                    if (BehaviorDesignerWindow.instance.ContainsError(task, fields[index].Name))
                                    {
                                        GUILayout.Space(-3f);
                                        GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(20f) };
                                        GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, optionArray3);
                                    }
                                    if (UnityEditor.EditorGUI.EndChangeCheck())
                                    {
                                        fields[index].SetValue(obj, obj3);
                                        GUI.changed =(true);
                                    }
                                    if (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fields[index], typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) && !ErrorCheck.IsRequiredFieldValid(fieldType, obj2))
                                    {
                                        GUILayout.Space(-3f);
                                        GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(20f) };
                                        GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, optionArray4);
                                    }
                                    GUILayout.EndHorizontal();
                                    GUILayout.Space(4f);
                                }
                            }
                            index++;
                        }
                    }
                }
            }
        }

        private void DrawReflectionField(BehaviorDesigner.Runtime.Tasks.Task task, GUIContent guiContent, bool drawComponentField, FieldInfo field)
        {
            BehaviorDesigner.Runtime.SharedVariable variable = task.GetType().GetField("targetGameObject").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
            if (drawComponentField)
            {
                GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(146f) };
                GUILayout.Label(guiContent, optionArray1);
                BehaviorDesigner.Runtime.SharedVariable variable2 = field.GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
                string str = string.Empty;
                if ((variable2 == null) || string.IsNullOrEmpty((string) variable2.GetValue()))
                {
                    str = "Select";
                }
                else
                {
                    char[] separator = new char[] { '.' };
                    string[] strArray = ((string) variable2.GetValue()).Split(separator);
                    str = strArray[strArray.Length - 1];
                }
                GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(92f) };
                if (GUILayout.Button(str, UnityEditor.EditorStyles.toolbarPopup, optionArray2))
                {
                    UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
                    menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty((string) variable2.GetValue()), new UnityEditor.GenericMenu.MenuFunction2(this.ComponentSelectionCallback), null);
                    GameObject obj2 = null;
                    if ((variable != null) && (((GameObject) variable.GetValue()) != null))
                    {
                        obj2 = (GameObject) variable.GetValue();
                    }
                    else if (task.Owner != null)
                    {
                        throw new Exception("[TaskInspector] 不太明白这里在干什么");
                        //obj2 = task.Owner.gameObject;
                    }
                    if (obj2 != null)
                    {
                        Component[] components = obj2.GetComponents<Component>();
                        int index = 0;
                        while (true)
                        {
                            if (index >= components.Length)
                            {
                                menu.ShowAsContext();
                                this.mActiveMenuSelectionTask = task;
                                break;
                            }
                            menu.AddItem(new GUIContent(components[index].GetType().Name), components[index].GetType().FullName.Equals((string) variable2.GetValue()), new UnityEditor.GenericMenu.MenuFunction2(this.ComponentSelectionCallback), components[index].GetType().FullName);
                            index++;
                        }
                    }
                }
            }
            else
            {
                GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(146f) };
                GUILayout.Label(guiContent, optionArray3);
                BehaviorDesigner.Runtime.SharedVariable variable3 = task.GetType().GetField("componentName").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
                BehaviorDesigner.Runtime.SharedVariable variable4 = field.GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
                string str3 = string.Empty;
                str3 = ((variable3 == null) || string.IsNullOrEmpty((string) variable3.GetValue())) ? "Component Required" : (!string.IsNullOrEmpty((string) variable4.GetValue()) ? ((string) variable4.GetValue()) : "Select");
                GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(92f) };
                if (GUILayout.Button(str3, UnityEditor.EditorStyles.toolbarPopup, optionArray4) && !string.IsNullOrEmpty((string) variable3.GetValue()))
                {
                    UnityEditor.GenericMenu menu2 = new UnityEditor.GenericMenu();
                    menu2.AddItem(new GUIContent("None"), string.IsNullOrEmpty((string) variable4.GetValue()), new UnityEditor.GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), null);
                    GameObject obj3 = null;
                    if ((variable != null) && (((GameObject) variable.GetValue()) != null))
                    {
                        obj3 = (GameObject) variable.GetValue();
                    }
                    else if (task.Owner != null)
                    {
                        throw new Exception("[TaskInspector] 不太明白这里在干什么");
                        //obj3 = task.Owner.gameObject;
                    }
                    if (obj3 != null)
                    {
                        Component component = obj3.GetComponent(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly((string) variable3.GetValue()));
                        List<System.Type> sharedVariableTypes = VariableInspector.FindAllSharedVariableTypes(false);
                        if (!this.IsInvokeMethodTask(task.GetType()))
                        {
                            if (this.IsFieldReflectionTask(task.GetType()))
                            {
                                FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                                for (int i = 0; i < fields.Length; i++)
                                {
                                    if (!fields[i].IsSpecialName && this.SharedVariableTypeExists(sharedVariableTypes, fields[i].FieldType))
                                    {
                                        menu2.AddItem(new GUIContent(fields[i].Name), fields[i].Name.Equals((string) variable4.GetValue()), new UnityEditor.GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), fields[i]);
                                    }
                                }
                            }
                            else
                            {
                                PropertyInfo[] properties = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                for (int i = 0; i < properties.Length; i++)
                                {
                                    if (!properties[i].IsSpecialName && this.SharedVariableTypeExists(sharedVariableTypes, properties[i].PropertyType))
                                    {
                                        menu2.AddItem(new GUIContent(properties[i].Name), properties[i].Name.Equals((string) variable4.GetValue()), new UnityEditor.GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), properties[i]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MethodInfo[] methods = component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                            for (int i = 0; i < methods.Length; i++)
                            {
                                if (!methods[i].IsSpecialName && (!methods[i].IsGenericMethod && (methods[i].GetParameters().Length <= 4)))
                                {
                                    System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();
                                    bool flag = true;
                                    int index = 0;
                                    while (true)
                                    {
                                        if (index < parameters.Length)
                                        {
                                            if (this.SharedVariableTypeExists(sharedVariableTypes, parameters[index].ParameterType))
                                            {
                                                index++;
                                                continue;
                                            }
                                            flag = false;
                                        }
                                        if (flag && (methods[i].ReturnType.Equals(typeof(void)) || this.SharedVariableTypeExists(sharedVariableTypes, methods[i].ReturnType)))
                                        {
                                            menu2.AddItem(new GUIContent(methods[i].Name), methods[i].Name.Equals((string) variable4.GetValue()), new UnityEditor.GenericMenu.MenuFunction2(this.SecondaryReflectionSelectionCallback), methods[i]);
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                        menu2.ShowAsContext();
                        this.mActiveMenuSelectionTask = task;
                    }
                }
            }
            GUILayout.Space(8f);
        }

        private BehaviorDesigner.Runtime.SharedVariable DrawSharedVariableValue(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, FieldInfo field, GUIContent guiContent, BehaviorDesigner.Runtime.Tasks.Task task, BehaviorDesigner.Runtime.SharedVariable sharedVariable, bool isReflectionTask, bool enabled, bool drawWatch)
        {
            if (!isReflectionTask)
            {
                sharedVariable = FieldInspector.DrawSharedVariable(task, guiContent, field, field.FieldType, sharedVariable);
            }
            else
            {
                if (!field.FieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) && (sharedVariable == null))
                {
                    sharedVariable = Activator.CreateInstance(field.FieldType) as BehaviorDesigner.Runtime.SharedVariable;
                    if (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) || BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                    {
                        sharedVariable.IsShared = true;
                    }
                    GUI.changed =(true);
                }
                if (sharedVariable == null)
                {
                    this.mActiveMenuSelectionTask = task;
                    this.SecondaryReflectionSelectionCallback(null);
                    this.ClearInvokeVariablesTask();
                    return null;
                }
                if (!sharedVariable.IsShared)
                {
                    bool drawComponentField = false;
                    if (!(drawComponentField = field.Name.Equals("componentName")) && (!field.Name.Equals("methodName") && (!field.Name.Equals("fieldName") && !field.Name.Equals("propertyName"))))
                    {
                        FieldInspector.DrawFields(task, sharedVariable, guiContent);
                    }
                    else
                    {
                        this.DrawReflectionField(task, guiContent, drawComponentField, field);
                    }
                }
                else
                {
                    GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(126f) };
                    GUILayout.Label(guiContent, optionArray1);
                    string[] names = null;
                    int globalStartIndex = -1;
                    int index = FieldInspector.GetVariablesOfType(sharedVariable.GetType().GetProperty("Value").PropertyType, sharedVariable.IsGlobal, sharedVariable.Name, behaviorSource, out names, ref globalStartIndex, false, true);
                    Color color = GUI.backgroundColor;
                    if ((index == 0) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                    {
                        GUI.backgroundColor =(Color.red);
                    }
                    index = UnityEditor.EditorGUILayout.Popup(index, names, UnityEditor.EditorStyles.toolbarPopup, Array.Empty<GUILayoutOption>());
                    GUI.backgroundColor =(color);
                    if (index != index)
                    {
                        if (index != 0)
                        {
                            sharedVariable = ((globalStartIndex == -1) || (index < globalStartIndex)) ? behaviorSource.GetVariable(names[index]) : BehaviorDesigner.Runtime.GlobalVariables.Instance.GetVariable(names[index].Substring(8, names[index].Length - 8));
                        }
                        else
                        {
                            sharedVariable = !field.FieldType.Equals(typeof(BehaviorDesigner.Runtime.SharedVariable)) ? (Activator.CreateInstance(field.FieldType) as BehaviorDesigner.Runtime.SharedVariable) : (Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(sharedVariable.GetType().GetProperty("Value").PropertyType)) as BehaviorDesigner.Runtime.SharedVariable);
                            sharedVariable.IsShared = true;
                        }
                    }
                    GUILayout.Space(8f);
                }
                if (!BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) && !BehaviorDesigner.Runtime.TaskUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.SharedRequiredAttribute)))
                {
                    sharedVariable = FieldInspector.DrawSharedVariableToggleSharedButton(sharedVariable);
                }
                else if (!sharedVariable.IsShared)
                {
                    sharedVariable.IsShared = true;
                }
            }
            GUILayout.Space(8f);
            return sharedVariable;
        }

        private bool DrawTaskFields(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, TaskList taskList, BehaviorDesigner.Runtime.Tasks.Task task, bool enabled)
        {
            if (task == null)
            {
                return false;
            }
            UnityEditor.EditorGUI.BeginChangeCheck();
            FieldInspector.behaviorSource = behaviorSource;
            this.DrawObjectFields(behaviorSource, taskList, task, task, enabled, true);
            return UnityEditor.EditorGUI.EndChangeCheck();
        }

        public bool DrawTaskInspector(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, TaskList taskList, BehaviorDesigner.Runtime.Tasks.Task task, bool enabled)
        {
            if ((task == null) || (task.NodeData.NodeDesigner as NodeDesigner).IsEntryDisplay)
            {
                return false;
            }
            this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, Array.Empty<GUILayoutOption>());
            GUI.enabled =(enabled);
            if (this.behaviorDesignerWindow == null)
            {
                this.behaviorDesignerWindow = BehaviorDesignerWindow.instance;
            }
            GUILayout.Space(6f);
            UnityEditor.EditorGUIUtility.labelWidth = 150f;
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(90f) };
            UnityEditor.EditorGUILayout.LabelField("Name", optionArray1);
            task.FriendlyName = UnityEditor.EditorGUILayout.TextField(task.FriendlyName, Array.Empty<GUILayoutOption>());
            if (GUILayout.Button(BehaviorDesignerUtility.DocTexture, BehaviorDesignerUtility.TransparentButtonOffsetGUIStyle, Array.Empty<GUILayoutOption>()))
            {
                this.OpenHelpURL(task);
            }
            if (GUILayout.Button(BehaviorDesignerUtility.ColorSelectorTexture(task.NodeData.ColorIndex), BehaviorDesignerUtility.TransparentButtonOffsetGUIStyle, Array.Empty<GUILayoutOption>()))
            {
                UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
                this.AddColorMenuItem(ref menu, task, "Default", 0);
                this.AddColorMenuItem(ref menu, task, "Red", 1);
                this.AddColorMenuItem(ref menu, task, "Pink", 2);
                this.AddColorMenuItem(ref menu, task, "Brown", 3);
                this.AddColorMenuItem(ref menu, task, "Orange", 4);
                this.AddColorMenuItem(ref menu, task, "Turquoise", 5);
                this.AddColorMenuItem(ref menu, task, "Cyan", 6);
                this.AddColorMenuItem(ref menu, task, "Blue", 7);
                this.AddColorMenuItem(ref menu, task, "Purple", 8);
                menu.ShowAsContext();
            }
            if (GUILayout.Button(BehaviorDesignerUtility.GearTexture, BehaviorDesignerUtility.TransparentButtonOffsetGUIStyle, Array.Empty<GUILayoutOption>()))
            {
                UnityEditor.GenericMenu menu2 = new UnityEditor.GenericMenu();
                if (cache0 == null)
                {
                    cache0 = new UnityEditor.GenericMenu.MenuFunction2(TaskInspector.OpenInFileEditor);
                }
                menu2.AddItem(new GUIContent("Edit Script"), false, cache0, task);
                if (cache1 == null)
                {
                    cache1 = new UnityEditor.GenericMenu.MenuFunction2(TaskInspector.SelectInProject);
                }
                menu2.AddItem(new GUIContent("Locate Script"), false, cache1, task);
                menu2.AddItem(new GUIContent("Reset"), false, new UnityEditor.GenericMenu.MenuFunction2(this.ResetTask), task);
                menu2.ShowAsContext();
            }
            GUILayout.EndHorizontal();
            string str = BehaviorDesignerUtility.SplitCamelCase(task.GetType().Name.ToString());
            if (!task.FriendlyName.Equals(str))
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(90f) };
                UnityEditor.EditorGUILayout.LabelField("Type", optionArray2);
                GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.MaxWidth(170f) };
                UnityEditor.EditorGUILayout.LabelField(str, optionArray3);
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(90f) };
            UnityEditor.EditorGUILayout.LabelField("Instant", optionArray4);
            task.IsInstant = UnityEditor.EditorGUILayout.Toggle(task.IsInstant, Array.Empty<GUILayoutOption>());
            GUILayout.EndHorizontal();
            UnityEditor.EditorGUILayout.LabelField("Comment", Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray5 = new GUILayoutOption[] { GUILayout.Height(48f) };
            task.NodeData.Comment = UnityEditor.EditorGUILayout.TextArea(task.NodeData.Comment, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, optionArray5);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                // 不支持撤销
                //BehaviorUndo.RegisterUndo("Inspector", behaviorSource.Owner.GetObject());
                GUI.changed =(true);
            }
            BehaviorDesignerUtility.DrawContentSeperator(2);
            GUILayout.Space(6f);
            if (this.DrawTaskFields(behaviorSource, taskList, task, enabled))
            {
                // 不支持撤销
                //BehaviorUndo.RegisterUndo("Inspector", behaviorSource.Owner.GetObject());
                GUI.changed =(true);
            }
            GUI.enabled =(true);
            GUILayout.EndScrollView();
            return GUI.changed;
        }

        private void DrawTaskValue(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, TaskList taskList, FieldInfo field, GUIContent guiContent, BehaviorDesigner.Runtime.Tasks.Task parentTask, BehaviorDesigner.Runtime.Tasks.Task task, bool enabled)
        {
            if (BehaviorDesignerUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.InspectTaskAttribute)))
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(144f) };
                GUILayout.Label(guiContent, optionArray1);
                GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(134f) };
                if (GUILayout.Button((task == null) ? "Select" : BehaviorDesignerUtility.SplitCamelCase(task.GetType().Name.ToString()), UnityEditor.EditorStyles.toolbarPopup, optionArray2))
                {
                    UnityEditor.GenericMenu genericMenu = new UnityEditor.GenericMenu();
                    genericMenu.AddItem(new GUIContent("None"), object.ReferenceEquals(task, null), new UnityEditor.GenericMenu.MenuFunction2(this.InspectedTaskCallback), null);
                    taskList.AddTaskTypesToMenu(2, ref genericMenu, (task == null) ? null : task.GetType(), null, string.Empty, true, new UnityEditor.GenericMenu.MenuFunction2(this.InspectedTaskCallback));
                    genericMenu.ShowAsContext();
                    this.mActiveMenuSelectionTask = parentTask;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
                this.DrawObjectFields(behaviorSource, taskList, task, task, enabled, false);
            }
            else
            {
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                this.DrawWatchedButton(parentTask, field);
                GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(165f) };
                GUILayout.Label(guiContent, BehaviorDesignerUtility.TaskInspectorGUIStyle, optionArray3);
                bool flag = this.behaviorDesignerWindow.IsReferencingField(field);
                Color color = GUI.backgroundColor;
                if (flag)
                {
                    GUI.backgroundColor =(new Color(0.5f, 1f, 0.5f));
                }
                GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(80f) };
                if (GUILayout.Button(!flag ? "Select" : "Done", UnityEditor.EditorStyles.miniButtonMid, optionArray4))
                {
                    if (this.behaviorDesignerWindow.IsReferencingTasks() && !flag)
                    {
                        this.behaviorDesignerWindow.ToggleReferenceTasks();
                    }
                    this.behaviorDesignerWindow.ToggleReferenceTasks(parentTask, field);
                }
                GUI.backgroundColor =(color);
                UnityEditor.EditorGUILayout.EndHorizontal();
                if (!typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    BehaviorDesigner.Runtime.Tasks.Task task2 = field.GetValue(parentTask) as BehaviorDesigner.Runtime.Tasks.Task;
                    GUILayoutOption[] optionArray8 = new GUILayoutOption[] { GUILayout.Width(232f) };
                    GUILayout.Label((task2 == null) ? "No Tasks Referenced" : task2.NodeData.NodeDesigner.ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, optionArray8);
                    if (task2 != null)
                    {
                        GUILayoutOption[] optionArray9 = new GUILayoutOption[] { GUILayout.Width(14f) };
                        if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray9))
                        {
                            this.ReferenceTasks(parentTask, null, field);
                            GUI.changed =(true);
                        }
                        GUILayout.Space(3f);
                        GUILayoutOption[] optionArray10 = new GUILayoutOption[] { GUILayout.Width(14f) };
                        if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray10))
                        {
                            this.behaviorDesignerWindow.IdentifyNode(task2.NodeData.NodeDesigner as NodeDesigner);
                        }
                    }
                    UnityEditor.EditorGUILayout.EndHorizontal();
                }
                else
                {
                    IList list = field.GetValue(parentTask) as IList;
                    if ((list == null) || (list.Count == 0))
                    {
                        GUILayout.Label("No Tasks Referenced", BehaviorDesignerUtility.TaskInspectorGUIStyle, Array.Empty<GUILayoutOption>());
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i] is BehaviorDesigner.Runtime.Tasks.Task)
                            {
                                UnityEditor.EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                                GUILayoutOption[] optionArray5 = new GUILayoutOption[] { GUILayout.Width(232f) };
                                GUILayout.Label((list[i] as BehaviorDesigner.Runtime.Tasks.Task).NodeData.NodeDesigner.ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, optionArray5);
                                GUILayoutOption[] optionArray6 = new GUILayoutOption[] { GUILayout.Width(14f) };
                                if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray6))
                                {
                                    this.ReferenceTasks(parentTask, ((list[i] as BehaviorDesigner.Runtime.Tasks.Task).NodeData.NodeDesigner as NodeDesigner).Task, field);
                                    GUI.changed =(true);
                                }
                                GUILayout.Space(3f);
                                GUILayoutOption[] optionArray7 = new GUILayoutOption[] { GUILayout.Width(14f) };
                                if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray7))
                                {
                                    this.behaviorDesignerWindow.IdentifyNode((list[i] as BehaviorDesigner.Runtime.Tasks.Task).NodeData.NodeDesigner as NodeDesigner);
                                }
                                UnityEditor.EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                }
            }
        }

        private bool DrawWatchedButton(BehaviorDesigner.Runtime.Tasks.Task task, FieldInfo field)
        {
            GUILayout.Space(3f);
            bool flag = task.NodeData.GetWatchedFieldIndex(field) != -1;
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(15f) };
            if (!GUILayout.Button(!flag ? BehaviorDesignerUtility.VariableWatchButtonTexture : BehaviorDesignerUtility.VariableWatchButtonSelectedTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray1))
            {
                return false;
            }
            if (flag)
            {
                task.NodeData.RemoveWatchedField(field);
            }
            else
            {
                task.NodeData.AddWatchedField(field);
            }
            return true;
        }

        private MethodInfo GetInvokeMethodInfo(object task)
        {
            BehaviorDesigner.Runtime.SharedVariable variable = task.GetType().GetField("targetGameObject").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
            GameObject obj2 = null;
            if ((variable != null) && (((GameObject) variable.GetValue()) != null))
            {
                obj2 = (GameObject) variable.GetValue();
            }
            else if ((task as BehaviorDesigner.Runtime.Tasks.Task).Owner != null)
            {
                throw new Exception("[TaskInspector] 不太明白这里在干什么");
                //obj2 = (task as BehaviorDesigner.Runtime.Tasks.Task).Owner.gameObject;
            }
            if (obj2 == null)
            {
                return null;
            }
            BehaviorDesigner.Runtime.SharedVariable variable2 = task.GetType().GetField("componentName").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
            if ((variable2 == null) || string.IsNullOrEmpty((string) variable2.GetValue()))
            {
                return null;
            }
            BehaviorDesigner.Runtime.SharedVariable variable3 = task.GetType().GetField("methodName").GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
            if ((variable3 == null) || string.IsNullOrEmpty((string) variable3.GetValue()))
            {
                return null;
            }
            List<System.Type> list = new List<System.Type>();
            BehaviorDesigner.Runtime.SharedVariable variable4 = null;
            int num = 0;
            while (true)
            {
                if (num < 4)
                {
                    FieldInfo field = task.GetType().GetField("parameter" + (num + 1));
                    variable4 = field.GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
                    if (variable4 != null)
                    {
                        list.Add(variable4.GetType().GetProperty("Value").PropertyType);
                        num++;
                        continue;
                    }
                }
                Component component = obj2.GetComponent(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly((string) variable2.GetValue()));
                return ((component != null) ? component.GetType().GetMethod((string) variable3.GetValue(), list.ToArray()) : null);
            }
        }

        public static List<BehaviorDesigner.Runtime.Tasks.Task> GetReferencedTasks(BehaviorDesigner.Runtime.Tasks.Task task)
        {
            List<BehaviorDesigner.Runtime.Tasks.Task> list = new List<BehaviorDesigner.Runtime.Tasks.Task>();
            FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(task.GetType());
            for (int i = 0; i < serializableFields.Length; i++)
            {
                if ((!serializableFields[i].IsPrivate && !serializableFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(SerializeField)))
                {
                    if (!typeof(IList).IsAssignableFrom(serializableFields[i].FieldType) || (!typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(serializableFields[i].FieldType.GetElementType()) && (!serializableFields[i].FieldType.IsGenericType || !typeof(BehaviorDesigner.Runtime.Tasks.Task).IsAssignableFrom(serializableFields[i].FieldType.GetGenericArguments()[0]))))
                    {
                        if (serializableFields[i].FieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.Task)) && (serializableFields[i].GetValue(task) != null))
                        {
                            list.Add(serializableFields[i].GetValue(task) as BehaviorDesigner.Runtime.Tasks.Task);
                        }
                    }
                    else
                    {
                        BehaviorDesigner.Runtime.Tasks.Task[] taskArray = serializableFields[i].GetValue(task) as BehaviorDesigner.Runtime.Tasks.Task[];
                        if (taskArray != null)
                        {
                            for (int j = 0; j < taskArray.Length; j++)
                            {
                                list.Add(taskArray[j]);
                            }
                        }
                    }
                }
            }
            return ((list.Count <= 0) ? null : list);
        }

        public bool HasFocus()
        {
            return (GUIUtility.keyboardControl != 0);
        }

        private void InspectedTaskCallback(object obj)
        {
            if (this.mActiveMenuSelectionTask != null)
            {
                FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("conditionalTask");
                if (obj == null)
                {
                    field.SetValue(this.mActiveMenuSelectionTask, null);
                }
                else
                {
                    System.Type type = (System.Type) obj;
                    BehaviorDesigner.Runtime.Tasks.Task task = Activator.CreateInstance(type, true) as BehaviorDesigner.Runtime.Tasks.Task;
                    field.SetValue(this.mActiveMenuSelectionTask, task);
                    FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(type);
                    for (int i = 0; i < serializableFields.Length; i++)
                    {
                        if ((serializableFields[i].FieldType.IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable)) && (!BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(HideInInspector)) && !BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(NonSerializedAttribute)))) && ((!serializableFields[i].IsPrivate && !serializableFields[i].IsFamily) || BehaviorDesignerUtility.HasAttribute(serializableFields[i], typeof(SerializeField))))
                        {
                            BehaviorDesigner.Runtime.SharedVariable variable = Activator.CreateInstance(serializableFields[i].FieldType) as BehaviorDesigner.Runtime.SharedVariable;
                            variable.IsShared = false;
                            serializableFields[i].SetValue(task, variable);
                        }
                    }
                }
            }
            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        private string InvokeParameterName(object task, FieldInfo field)
        {
            if (!field.Name.Contains("parameter"))
            {
                return field.Name;
            }
            MethodInfo invokeMethodInfo = null;
            invokeMethodInfo = this.GetInvokeMethodInfo(task);
            if (invokeMethodInfo == null)
            {
                return field.Name;
            }
            System.Reflection.ParameterInfo[] parameters = invokeMethodInfo.GetParameters();
            int index = int.Parse(field.Name.Substring(9)) - 1;
            return ((index >= parameters.Length) ? field.Name : parameters[index].Name);
        }

        public bool IsActiveTaskArray()
        {
            return this.activeReferenceTaskFieldInfo.FieldType.IsArray;
        }

        public bool IsActiveTaskNull()
        {
            return (this.activeReferenceTaskFieldInfo.GetValue(this.activeReferenceTask) == null);
        }

        public static bool IsFieldLinked(FieldInfo field)
        {
            return BehaviorDesignerUtility.HasAttribute(field, typeof(BehaviorDesigner.Runtime.Tasks.LinkedTaskAttribute));
        }

        private bool IsFieldReflectionTask(System.Type type)
        {
            return ((BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") || BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetFieldValue")) || BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.CompareFieldValue"));
        }

        private bool IsInvokeMethodTask(System.Type type)
        {
            return BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.InvokeMethod");
        }

        private bool IsPropertyReflectionTask(System.Type type)
        {
            return ((BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue") || BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.SetPropertyValue")) || BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.ComparePropertyValue"));
        }

        private bool IsReflectionGetterTask(System.Type type)
        {
            return (BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetFieldValue") || BehaviorDesigner.Runtime.TaskUtility.CompareType(type, "BehaviorDesigner.Runtime.Tasks.GetPropertyValue"));
        }

        private bool IsReflectionTask(System.Type type)
        {
            return ((this.IsInvokeMethodTask(type) || this.IsFieldReflectionTask(type)) || this.IsPropertyReflectionTask(type));
        }

        public void OnEnable()
        {
            base.hideFlags = (HideFlags)(0x3d);
        }

        private void OpenHelpURL(BehaviorDesigner.Runtime.Tasks.Task task)
        {
            BehaviorDesigner.Runtime.Tasks.HelpURLAttribute[] attributeArray = null;
            if ((attributeArray = task.GetType().GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.HelpURLAttribute), false) as BehaviorDesigner.Runtime.Tasks.HelpURLAttribute[]).Length > 0)
            {
                Application.OpenURL(attributeArray[0].URL);
            }
        }

        public static void OpenInFileEditor(object task)
        {
            UnityEditor.MonoScript[] scriptArray = (UnityEditor.MonoScript[]) Resources.FindObjectsOfTypeAll(typeof(UnityEditor.MonoScript));
            int index = 0;
            while (true)
            {
                if (index < scriptArray.Length)
                {
                    if ((scriptArray[index] == null) || ((scriptArray[index].GetClass() == null) || !scriptArray[index].GetClass().Equals(task.GetType())))
                    {
                        index++;
                        continue;
                    }
                    UnityEditor.AssetDatabase.OpenAsset((UnityEngine.Object) scriptArray[index]);
                }
                return;
            }
        }

        private void PerformFullSync(BehaviorDesigner.Runtime.Tasks.Task task)
        {
            List<BehaviorDesigner.Runtime.Tasks.Task> referencedTasks = GetReferencedTasks(task);
            if (referencedTasks != null)
            {
                FieldInfo[] serializableFields = BehaviorDesigner.Runtime.TaskUtility.GetSerializableFields(task.GetType());
                for (int i = 0; i < serializableFields.Length; i++)
                {
                    if (!IsFieldLinked(serializableFields[i]))
                    {
                        for (int j = 0; j < referencedTasks.Count; j++)
                        {
                            FieldInfo field = referencedTasks[j].GetType().GetField(serializableFields[i].Name);
                            if (field != null)
                            {
                                field.SetValue(referencedTasks[j], serializableFields[i].GetValue(task));
                            }
                        }
                    }
                }
            }
        }

        public bool ReferenceTasks(BehaviorDesigner.Runtime.Tasks.Task referenceTask)
        {
            return this.ReferenceTasks(this.activeReferenceTask, referenceTask, this.activeReferenceTaskFieldInfo);
        }

        private bool ReferenceTasks(BehaviorDesigner.Runtime.Tasks.Task sourceTask, BehaviorDesigner.Runtime.Tasks.Task referenceTask, FieldInfo sourceFieldInfo)
        {
            bool fullSync = false;
            bool doReference = false;
            if (!ReferenceTasks(sourceTask, referenceTask, sourceFieldInfo, ref fullSync, ref doReference, true, false))
            {
                return false;
            }
            if (referenceTask != null)
            {
                (referenceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = doReference;
                if (fullSync)
                {
                    this.PerformFullSync(this.activeReferenceTask);
                }
            }
            return true;
        }

        public static bool ReferenceTasks(BehaviorDesigner.Runtime.Tasks.Task sourceTask, BehaviorDesigner.Runtime.Tasks.Task referenceTask, FieldInfo sourceFieldInfo, ref bool fullSync, ref bool doReference, bool synchronize, bool unreferenceAll)
        {
            if (referenceTask == null)
            {
                BehaviorDesigner.Runtime.Tasks.Task task = sourceFieldInfo.GetValue(sourceTask) as BehaviorDesigner.Runtime.Tasks.Task;
                if (task != null)
                {
                    (task.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = false;
                }
                sourceFieldInfo.SetValue(sourceTask, null);
                return true;
            }
            if ((referenceTask.Equals(sourceTask) || ((sourceFieldInfo == null) || (!typeof(IList).IsAssignableFrom(sourceFieldInfo.FieldType) && !sourceFieldInfo.FieldType.IsAssignableFrom(referenceTask.GetType())))) || (typeof(IList).IsAssignableFrom(sourceFieldInfo.FieldType) && ((sourceFieldInfo.FieldType.IsGenericType && !sourceFieldInfo.FieldType.GetGenericArguments()[0].IsAssignableFrom(referenceTask.GetType())) || (!sourceFieldInfo.FieldType.IsGenericType && !sourceFieldInfo.FieldType.GetElementType().IsAssignableFrom(referenceTask.GetType())))))
            {
                return false;
            }
            if (synchronize && !IsFieldLinked(sourceFieldInfo))
            {
                synchronize = false;
            }
            if (unreferenceAll)
            {
                sourceFieldInfo.SetValue(sourceTask, null);
                (sourceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = false;
            }
            else
            {
                doReference = true;
                bool flag = false;
                if (!typeof(IList).IsAssignableFrom(sourceFieldInfo.FieldType))
                {
                    BehaviorDesigner.Runtime.Tasks.Task task2 = sourceFieldInfo.GetValue(sourceTask) as BehaviorDesigner.Runtime.Tasks.Task;
                    doReference = !referenceTask.Equals(task2);
                    if (IsFieldLinked(sourceFieldInfo) && (task2 != null))
                    {
                        ReferenceTasks(task2, sourceTask, task2.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, true);
                    }
                    if (synchronize)
                    {
                        ReferenceTasks(referenceTask, sourceTask, referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, !doReference);
                    }
                    sourceFieldInfo.SetValue(sourceTask, !doReference ? null : referenceTask);
                }
                else
                {
                    System.Type elementType;
                    BehaviorDesigner.Runtime.Tasks.Task[] taskArray = sourceFieldInfo.GetValue(sourceTask) as BehaviorDesigner.Runtime.Tasks.Task[];
                    if (sourceFieldInfo.FieldType.IsArray)
                    {
                        elementType = sourceFieldInfo.FieldType.GetElementType();
                    }
                    else
                    {
                        System.Type fieldType = sourceFieldInfo.FieldType;
                        while (true)
                        {
                            if (fieldType.IsGenericType)
                            {
                                elementType = fieldType.GetGenericArguments()[0];
                                break;
                            }
                            fieldType = fieldType.BaseType;
                        }
                    }
                    System.Type[] typeArguments = new System.Type[] { elementType };
                    IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(typeArguments)) as IList;
                    if (taskArray != null)
                    {
                        for (int i = 0; i < taskArray.Length; i++)
                        {
                            if (referenceTask.Equals(taskArray[i]))
                            {
                                doReference = false;
                            }
                            else
                            {
                                list.Add(taskArray[i]);
                            }
                        }
                    }
                    if (synchronize)
                    {
                        if ((taskArray != null) && (taskArray.Length > 0))
                        {
                            for (int i = 0; i < taskArray.Length; i++)
                            {
                                ReferenceTasks(taskArray[i], referenceTask, taskArray[i].GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, false);
                                if (doReference)
                                {
                                    ReferenceTasks(referenceTask, taskArray[i], referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, false);
                                }
                            }
                        }
                        else if (doReference)
                        {
                            FieldInfo field = referenceTask.GetType().GetField(sourceFieldInfo.Name);
                            if (field != null)
                            {
                                taskArray = field.GetValue(referenceTask) as BehaviorDesigner.Runtime.Tasks.Task[];
                                if (taskArray != null)
                                {
                                    int index = 0;
                                    while (true)
                                    {
                                        if (index >= taskArray.Length)
                                        {
                                            doReference = true;
                                            break;
                                        }
                                        list.Add(taskArray[index]);
                                        (taskArray[index].NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = true;
                                        ReferenceTasks(taskArray[index], sourceTask, taskArray[index].GetType().GetField(sourceFieldInfo.Name), ref doReference, ref flag, false, false);
                                        index++;
                                    }
                                }
                            }
                        }
                        ReferenceTasks(referenceTask, sourceTask, referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, !doReference);
                    }
                    if (doReference)
                    {
                        list.Add(referenceTask);
                    }
                    if (!sourceFieldInfo.FieldType.IsArray)
                    {
                        sourceFieldInfo.SetValue(sourceTask, list);
                    }
                    else
                    {
                        Array array = Array.CreateInstance(sourceFieldInfo.FieldType.GetElementType(), list.Count);
                        list.CopyTo(array, 0);
                        sourceFieldInfo.SetValue(sourceTask, array);
                    }
                }
                if (synchronize)
                {
                    (referenceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = doReference;
                }
                fullSync = doReference && synchronize;
            }
            return true;
        }

        private void ResetTask(object task)
        {
            (task as BehaviorDesigner.Runtime.Tasks.Task).OnReset();
            List<System.Type> baseClasses = FieldInspector.GetBaseClasses(task.GetType());
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
                    if (typeof(BehaviorDesigner.Runtime.SharedVariable).IsAssignableFrom(fields[index].FieldType))
                    {
                        BehaviorDesigner.Runtime.SharedVariable variable = fields[index].GetValue(task) as BehaviorDesigner.Runtime.SharedVariable;
                        if (BehaviorDesigner.Runtime.TaskUtility.HasAttribute(fields[index], typeof(BehaviorDesigner.Runtime.Tasks.RequiredFieldAttribute)) && ((variable != null) && !variable.IsShared))
                        {
                            variable.IsShared = true;
                        }
                    }
                    index++;
                }
            }
        }

        private void SecondaryReflectionSelectionCallback(object obj)
        {
            if (this.mActiveMenuSelectionTask != null)
            {
                BehaviorDesigner.Runtime.SharedVariable variable = Activator.CreateInstance(BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly("BehaviorDesigner.Runtime.SharedString")) as BehaviorDesigner.Runtime.SharedVariable;
                FieldInfo field = null;
                if (!this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                {
                    field = !this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()) ? this.mActiveMenuSelectionTask.GetType().GetField("propertyName") : this.mActiveMenuSelectionTask.GetType().GetField("fieldName");
                }
                else
                {
                    this.ClearInvokeVariablesTask();
                    field = this.mActiveMenuSelectionTask.GetType().GetField("methodName");
                }
                if (obj == null)
                {
                    field.SetValue(this.mActiveMenuSelectionTask, variable);
                }
                else if (!this.IsInvokeMethodTask(this.mActiveMenuSelectionTask.GetType()))
                {
                    if (this.IsFieldReflectionTask(this.mActiveMenuSelectionTask.GetType()))
                    {
                        // pzy:
                        // 这里有变量名重复
                        // 猜测这样改
                        //FieldInfo info5 = (FieldInfo) obj;
                        //variable.SetValue(info5.Name);
                        //field.SetValue(this.mActiveMenuSelectionTask, variable);
                        //FieldInfo field = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
                        //if (field == null)
                        //{
                        //    field = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                        //}
                        //variable = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(info5.FieldType)) as BehaviorDesigner.Runtime.SharedVariable;
                        //variable.IsShared = this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType());
                        //field.SetValue(this.mActiveMenuSelectionTask, variable);

                        FieldInfo info5 = (FieldInfo)obj;
                        variable.SetValue(info5.Name);
                        field.SetValue(this.mActiveMenuSelectionTask, variable);
                        FieldInfo field2 = this.mActiveMenuSelectionTask.GetType().GetField("fieldValue");
                        if (field2 == null)
                        {
                            field2 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                        }
                        variable = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(info5.FieldType)) as BehaviorDesigner.Runtime.SharedVariable;
                        variable.IsShared = this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType());
                        field2.SetValue(this.mActiveMenuSelectionTask, variable);
                    }
                    else
                    {
                        PropertyInfo info7 = (PropertyInfo) obj;
                        variable.SetValue(info7.Name);
                        field.SetValue(this.mActiveMenuSelectionTask, variable);
                        FieldInfo field2 = this.mActiveMenuSelectionTask.GetType().GetField("propertyValue");
                        if (field2 == null)
                        {
                            field2 = this.mActiveMenuSelectionTask.GetType().GetField("compareValue");
                        }
                        variable = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(info7.PropertyType)) as BehaviorDesigner.Runtime.SharedVariable;
                        variable.IsShared = this.IsReflectionGetterTask(this.mActiveMenuSelectionTask.GetType());
                        field2.SetValue(this.mActiveMenuSelectionTask, variable);
                    }
                }
                else
                {
                    MethodInfo info2 = (MethodInfo) obj;
                    variable.SetValue(info2.Name);
                    field.SetValue(this.mActiveMenuSelectionTask, variable);
                    System.Reflection.ParameterInfo[] parameters = info2.GetParameters();
                    int index = 0;
                    while (true)
                    {
                        if (index >= 4)
                        {
                            if (!info2.ReturnType.Equals(typeof(void)))
                            {
                                FieldInfo info4 = this.mActiveMenuSelectionTask.GetType().GetField("storeResult");
                                variable = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(info2.ReturnType)) as BehaviorDesigner.Runtime.SharedVariable;
                                variable.IsShared = true;
                                info4.SetValue(this.mActiveMenuSelectionTask, variable);
                            }
                            break;
                        }
                        FieldInfo field2 = this.mActiveMenuSelectionTask.GetType().GetField("parameter" + (index + 1));
                        if (index >= parameters.Length)
                        {
                            field2.SetValue(this.mActiveMenuSelectionTask, null);
                        }
                        else
                        {
                            variable = Activator.CreateInstance(FieldInspector.FriendlySharedVariableName(parameters[index].ParameterType)) as BehaviorDesigner.Runtime.SharedVariable;
                            field2.SetValue(this.mActiveMenuSelectionTask, variable);
                        }
                        index++;
                    }
                }
            }
            BehaviorDesignerWindow.instance.SaveBehavior();
        }

        public static void SelectInProject(object task)
        {
            UnityEditor.MonoScript[] scriptArray = (UnityEditor.MonoScript[]) Resources.FindObjectsOfTypeAll(typeof(UnityEditor.MonoScript));
            int index = 0;
            while (true)
            {
                if (index < scriptArray.Length)
                {
                    if ((scriptArray[index] == null) || ((scriptArray[index].GetClass() == null) || !scriptArray[index].GetClass().Equals(task.GetType())))
                    {
                        index++;
                        continue;
                    }
                    UnityEditor.Selection.activeObject = ((UnityEngine.Object) scriptArray[index]);
                }
                return;
            }
        }

        public void SetActiveReferencedTasks(BehaviorDesigner.Runtime.Tasks.Task referenceTask, FieldInfo fieldInfo)
        {
            this.activeReferenceTask = referenceTask;
            this.activeReferenceTaskFieldInfo = fieldInfo;
        }

        private void SetTaskColor(object value)
        {
            TaskColor color = value as TaskColor;
            if (color.task.NodeData.ColorIndex != color.colorIndex)
            {
                color.task.NodeData.ColorIndex = color.colorIndex;
                BehaviorDesignerWindow.instance.SaveBehavior();
            }
        }

        private bool SharedVariableTypeExists(List<System.Type> sharedVariableTypes, System.Type type)
        {
            System.Type type2 = FieldInspector.FriendlySharedVariableName(type);
            for (int i = 0; i < sharedVariableTypes.Count; i++)
            {
                if (type2.IsAssignableFrom(sharedVariableTypes[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public BehaviorDesigner.Runtime.Tasks.Task ActiveReferenceTask
        {
            get
            {
                return this.activeReferenceTask;
            }
        }

        public FieldInfo ActiveReferenceTaskFieldInfo
        {
            get
            {
                return this.activeReferenceTaskFieldInfo;
            }
        }

        private class TaskColor
        {
            public BehaviorDesigner.Runtime.Tasks.Task task;
            public int colorIndex;

            public TaskColor(BehaviorDesigner.Runtime.Tasks.Task task, int colorIndex)
            {
                this.task = task;
                this.colorIndex = colorIndex;
            }
        }
    }
}


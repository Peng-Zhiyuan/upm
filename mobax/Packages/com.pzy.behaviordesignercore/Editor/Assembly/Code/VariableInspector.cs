namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;


    public class VariableInspector : ScriptableObject
    {
        private static string[] sharedVariableStrings;
        private static List<System.Type> sharedVariableTypes;
        private static Dictionary<string, int> sharedVariableTypesDict;
        private string mVariableName = string.Empty;
        private int mVariableTypeIndex;
        private Vector2 mScrollPosition = Vector2.zero;
        private bool mFocusNameField;
        [SerializeField]
        private float mVariableStartPosition = -1f;
        [SerializeField]
        private List<float> mVariablePosition;
        [SerializeField]
        private int mSelectedVariableIndex = -1;
        [SerializeField]
        private string mSelectedVariableName;
        [SerializeField]
        private int mSelectedVariableTypeIndex;
        private static BehaviorDesigner.Runtime.SharedVariable mPropertyMappingVariable;
        private static BehaviorDesigner.Runtime.BehaviorSource mPropertyMappingBehaviorSource;
        private static UnityEditor.GenericMenu mPropertyMappingMenu;
        [CompilerGenerated]
        private static UnityEditor.GenericMenu.MenuFunction2 cache0;

        private static int AddPropertyName(BehaviorDesigner.Runtime.SharedVariable sharedVariable, CoreObject gameObject, ref List<string> propertyNames, ref List<CoreObject> propertyGameObjects, bool behaviorGameObject)
        {
            int count = -1;
            CoreComponent[] components = null;
            if (gameObject != null)
            {
                components = gameObject.GetComponents(typeof(CoreComponent));
                System.Type propertyType = sharedVariable.GetType().GetProperty("Value").PropertyType;
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] != null)
                    {
                        PropertyInfo[] properties = components[i].GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                        for (int j = 0; j < properties.Length; j++)
                        {
                            if (properties[j].PropertyType.Equals(propertyType) && !properties[j].IsSpecialName)
                            {
                                string item = components[i].GetType().FullName + "/" + properties[j].Name;
                                if (item.Equals(sharedVariable.PropertyMapping) && (object.Equals(sharedVariable.PropertyMappingOwner, gameObject) || (object.Equals(sharedVariable.PropertyMappingOwner, null) && behaviorGameObject)))
                                {
                                    count = propertyNames.Count;
                                }
                                propertyNames.Add(item);
                                propertyGameObjects.Add(gameObject);
                            }
                        }
                    }
                }
            }
            return count;
        }

        private static bool AddVariable(BehaviorDesigner.Runtime.IVariableSource variableSource, string variableName, int variableTypeIndex, bool fromGlobalVariablesWindow)
        {
            BehaviorDesigner.Runtime.SharedVariable item = CreateVariable(variableTypeIndex, variableName, fromGlobalVariablesWindow);
            List<BehaviorDesigner.Runtime.SharedVariable> variables = (variableSource == null) ? null : variableSource.GetAllVariables();
            if (variables == null)
            {
                variables = new List<BehaviorDesigner.Runtime.SharedVariable>();
            }
            variables.Add(item);
            variableSource.SetAllVariables(variables);
            return true;
        }

        private static bool CanNetworkSync(System.Type type)
        {
            return ((type == typeof(bool)) || ((type == typeof(Color)) || ((type == typeof(float)) || ((type == typeof(GameObject)) || ((type == typeof(int)) || ((type == typeof(Quaternion)) || ((type == typeof(Rect)) || ((type == typeof(string)) || ((type == typeof(Transform)) || ((type == typeof(Vector2)) || ((type == typeof(Vector3)) || (type == typeof(Vector4)))))))))))));
        }

        public bool ClearFocus(bool addVariable, BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(string.Empty);
            bool flag = false;
            if (addVariable && (!string.IsNullOrEmpty(this.mVariableName) && VariableNameValid(behaviorSource, this.mVariableName)))
            {
                flag = AddVariable(behaviorSource, this.mVariableName, this.mVariableTypeIndex, false);
                this.mVariableName = string.Empty;
            }
            return flag;
        }

        private static BehaviorDesigner.Runtime.SharedVariable CreateVariable(int index, string name, bool global)
        {
            BehaviorDesigner.Runtime.SharedVariable variable = Activator.CreateInstance(sharedVariableTypes[index]) as BehaviorDesigner.Runtime.SharedVariable;
            variable.Name = name;
            variable.IsShared = true;
            variable.IsGlobal = global;
            return variable;
        }

        public static bool DrawAllVariables(bool showFooter, BehaviorDesigner.Runtime.IVariableSource variableSource, ref List<BehaviorDesigner.Runtime.SharedVariable> variables, bool canSelect, ref List<float> variablePosition, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex, bool drawRemoveButton, bool drawLastSeparator)
        {
            if (variables == null)
            {
                return false;
            }
            bool flag = false;
            if (canSelect && (variablePosition == null))
            {
                variablePosition = new List<float>();
            }
            int index = 0;
            goto TR_0037;
        TR_0008:
            if (canSelect && (variables.Count < variablePosition.Count))
            {
                for (int i = variablePosition.Count - 1; i >= variables.Count; i--)
                {
                    variablePosition.RemoveAt(i);
                }
            }
            if (showFooter && (variables.Count > 0))
            {
                GUI.enabled = true;
                GUILayout.Label("Select a variable to change its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, Array.Empty<GUILayoutOption>());
            }
            return flag;
        TR_0011:
            index++;
            goto TR_0037;
        TR_0015:
            GUILayout.Space(4f);
            if (canSelect && (UnityEngine.Event.current.type == (EventType)7))
            {
                if (variablePosition.Count <= index)
                {
                    variablePosition.Add(GUILayoutUtility.GetLastRect().yMax);
                }
                else
                {
                    variablePosition[index] = GUILayoutUtility.GetLastRect().yMax;
                }
            }
            goto TR_0011;
        TR_0037:
            while (true)
            {
                if (index < variables.Count)
                {
                    BehaviorDesigner.Runtime.SharedVariable sharedVariable = variables[index];
                    if ((sharedVariable != null) && !sharedVariable.IsDynamic)
                    {
                        if (!canSelect || (selectedVariableIndex != index))
                        {
                            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                            if (DrawSharedVariable(variableSource, sharedVariable, false))
                            {
                                flag = true;
                            }
                            if (!drawRemoveButton)
                            {
                                break;
                            }
                            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(19f) };
                            if (!GUILayout.Button(BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray1) || !UnityEditor.EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
                            {
                                break;
                            }
                            if (BehaviorDesignerWindow.instance != null)
                            {
                                if (BehaviorDesignerWindow.instance.ActiveBehaviorSource != null)
                                {
                                    // 不支持撤销
                                    //BehaviorUndo.RegisterUndo("Delete Variable", BehaviorDesignerWindow.instance.ActiveBehaviorSource.Owner.GetObject());
                                }
                                BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
                            }
                            variables.RemoveAt(index);
                            if (canSelect)
                            {
                                if (selectedVariableIndex == index)
                                {
                                    selectedVariableIndex = -1;
                                }
                                else if (selectedVariableIndex > index)
                                {
                                    selectedVariableIndex--;
                                }
                            }
                            flag = true;
                        }
                        else
                        {
                            if (index == 0)
                            {
                                GUILayout.Space(2f);
                            }
                            bool deleted = false;
                            if (DrawSelectedVariable(variableSource, ref variables, sharedVariable, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, ref deleted))
                            {
                                flag = true;
                            }
                            if (!deleted)
                            {
                                goto TR_0015;
                            }
                            else
                            {
                                if (BehaviorDesignerWindow.instance != null)
                                {
                                    BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
                                }
                                variables.RemoveAt(index);
                                if (selectedVariableIndex == index)
                                {
                                    selectedVariableIndex = -1;
                                }
                                else if (selectedVariableIndex > index)
                                {
                                    selectedVariableIndex--;
                                }
                                flag = true;
                            }
                        }
                        goto TR_0008;
                    }
                    goto TR_0011;
                }
                else
                {
                    goto TR_0008;
                }
                goto TR_0015;
            }
            if ((BehaviorDesignerWindow.instance != null) && BehaviorDesignerWindow.instance.ContainsError(null, variables[index].Name))
            {
                GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(20f) };
                GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, optionArray2);
            }
            GUILayout.Space(10f);
            GUILayout.EndHorizontal();
            if ((index != (variables.Count - 1)) || drawLastSeparator)
            {
                BehaviorDesignerUtility.DrawContentSeperator(2, 7);
            }
            goto TR_0015;
        }

        private static bool DrawHeader(BehaviorDesigner.Runtime.IVariableSource variableSource, bool fromGlobalVariablesWindow, ref float variableStartPosition, ref string variableName, ref bool focusNameField, ref int variableTypeIndex, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex)
        {
            if (sharedVariableTypes == null)
            {
                FindAllSharedVariableTypes(true);
            }
            GUILayout.Space(6f);
            UnityEditor.EditorGUIUtility.labelWidth = 150f;
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayout.Space(4f);
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(70f) };
            UnityEditor.EditorGUILayout.LabelField("Name", optionArray1);
            GUI.SetNextControlName("Name");
            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(212f) };
            variableName = UnityEditor.EditorGUILayout.TextField(variableName, optionArray2);
            if (focusNameField)
            {
                GUI.FocusControl("Name");
                focusNameField = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayout.Space(4f);
            GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(70f) };
            GUILayout.Label("Type", optionArray3);
            GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(163f) };
            variableTypeIndex = UnityEditor.EditorGUILayout.Popup(variableTypeIndex, sharedVariableStrings, UnityEditor.EditorStyles.popup, optionArray4);
            GUILayout.Space(4f);
            bool flag = false;
            bool flag2 = VariableNameValid(variableSource, variableName);
            bool flag3 = GUI.enabled;
            GUI.enabled = flag2 && flag3;
            GUI.SetNextControlName("Add");
            GUILayoutOption[] optionArray5 = new GUILayoutOption[] { GUILayout.Width(40f) };
            if (GUILayout.Button("Add", UnityEditor.EditorStyles.miniButton, optionArray5) && flag2)
            {
                if (fromGlobalVariablesWindow && (variableSource == null))
                {
                    BehaviorDesigner.Runtime.GlobalVariables variables = ScriptableObject.CreateInstance(typeof(BehaviorDesigner.Runtime.GlobalVariables)) as BehaviorDesigner.Runtime.GlobalVariables;
                    string str = BehaviorDesignerUtility.GetEditorBaseDirectory(null).Substring(6, BehaviorDesignerUtility.GetEditorBaseDirectory(null).Length - 13);
                    string str2 = str + "/Resources/BehaviorDesignerGlobalVariables.asset";
                    if (!Directory.Exists(Application.dataPath + str + "/Resources"))
                    {
                        Directory.CreateDirectory(Application.dataPath + str + "/Resources");
                    }
                    if (!File.Exists(Application.dataPath + str2))
                    {
                        UnityEditor.AssetDatabase.CreateAsset((UnityEngine.Object) variables, "Assets" + str2);
                        UnityEditor.EditorUtility.DisplayDialog("Created Global Variables", "Behavior Designer Global Variables asset created:\n\nAssets" + str + "/Resources/BehaviorDesignerGlobalVariables.asset\n\nNote: Copy this file to transfer global variables between projects.", "OK");
                    }
                    variableSource = variables;
                }
                flag = AddVariable(variableSource, variableName, variableTypeIndex, fromGlobalVariablesWindow);
                if (flag)
                {
                    selectedVariableIndex = variableSource.GetAllVariables().Count - 1;
                    selectedVariableName = variableName;
                    selectedVariableTypeIndex = variableTypeIndex;
                    variableName = string.Empty;
                    GUI.FocusControl(string.Empty);
                }
            }
            GUILayout.Space(6f);
            GUILayout.EndHorizontal();
            if (!fromGlobalVariablesWindow)
            {
                GUI.enabled = true;
                GUILayout.Space(3f);
                GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                GUILayout.Space(5f);
                GUILayoutOption[] optionArray6 = new GUILayoutOption[] { GUILayout.Width(284f) };
                if (GUILayout.Button("Global Variables", UnityEditor.EditorStyles.miniButton, optionArray6))
                {
                    GlobalVariablesWindow.ShowWindow();
                }
                GUILayout.EndHorizontal();
            }
            BehaviorDesignerUtility.DrawContentSeperator(2);
            GUILayout.Space(4f);
            if ((variableStartPosition == -1f) && ((int)UnityEngine.Event.current.type == 7))
            {
                variableStartPosition = GUILayoutUtility.GetLastRect().yMax;
            }
            GUI.enabled = flag3;
            return flag;
        }

        private static bool DrawSelectedVariable(BehaviorDesigner.Runtime.IVariableSource variableSource, ref List<BehaviorDesigner.Runtime.SharedVariable> variables, BehaviorDesigner.Runtime.SharedVariable sharedVariable, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex, ref bool deleted)
        {
            bool flag = false;
            GUILayout.BeginVertical(BehaviorDesignerUtility.SelectedBackgroundGUIStyle, Array.Empty<GUILayoutOption>());
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(70f) };
            GUILayout.Label("Name", optionArray1);
            UnityEditor.EditorGUI.BeginChangeCheck();
            if (string.IsNullOrEmpty(selectedVariableName))
            {
                selectedVariableName = sharedVariable.Name;
            }
            GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(140f) };
            selectedVariableName = UnityEditor.EditorGUILayout.TextField(selectedVariableName, optionArray2);
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                if (VariableNameValid(variableSource, selectedVariableName))
                {
                    variableSource.UpdateVariableName(sharedVariable, selectedVariableName);
                }
                flag = true;
            }
            GUILayout.Space(10f);
            bool flag2 = GUI.enabled;
            GUI.enabled = flag2 && (selectedVariableIndex < (variables.Count - 1));
            GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.Width(19f) };
            if (GUILayout.Button(BehaviorDesignerUtility.DownArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray3))
            {
                BehaviorDesigner.Runtime.SharedVariable variable = variables[selectedVariableIndex + 1];
                variables[selectedVariableIndex + 1] = variables[selectedVariableIndex];
                variables[selectedVariableIndex] = variable;
                selectedVariableIndex++;
                flag = true;
            }
            GUI.enabled = flag2 && ((selectedVariableIndex < (variables.Count - 1)) || (selectedVariableIndex != 0));
            GUI.enabled = flag2 && (selectedVariableIndex != 0);
            GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(20f) };
            if (GUILayout.Button(BehaviorDesignerUtility.UpArrowButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray4))
            {
                BehaviorDesigner.Runtime.SharedVariable variable2 = variables[selectedVariableIndex - 1];
                variables[selectedVariableIndex - 1] = variables[selectedVariableIndex];
                variables[selectedVariableIndex] = variable2;
                selectedVariableIndex--;
                flag = true;
            }
            GUI.enabled = flag2;
            GUILayoutOption[] optionArray5 = new GUILayoutOption[] { GUILayout.Width(19f) };
            if (GUILayout.Button(BehaviorDesignerUtility.VariableDeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray5) && UnityEditor.EditorUtility.DisplayDialog("Delete Variable", "Are you sure you want to delete this variable?", "Yes", "No"))
            {
                deleted = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray6 = new GUILayoutOption[] { GUILayout.Width(70f) };
            GUILayout.Label("Type", optionArray6);
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayoutOption[] optionArray7 = new GUILayoutOption[] { GUILayout.Width(200f) };
            selectedVariableTypeIndex = UnityEditor.EditorGUILayout.Popup(selectedVariableTypeIndex, sharedVariableStrings, UnityEditor.EditorStyles.toolbarPopup, optionArray7);
            if (UnityEditor.EditorGUI.EndChangeCheck() && (sharedVariableTypesDict[sharedVariable.GetType().Name] != selectedVariableTypeIndex))
            {
                if (BehaviorDesignerWindow.instance != null)
                {
                    BehaviorDesignerWindow.instance.RemoveSharedVariableReferences(sharedVariable);
                }
                sharedVariable = CreateVariable(selectedVariableTypeIndex, sharedVariable.Name, sharedVariable.IsGlobal);
                variables[selectedVariableIndex] = sharedVariable;
                flag = true;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2f);
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUILayoutOption[] optionArray8 = new GUILayoutOption[] { GUILayout.Width(70f) };
            GUILayout.Label("Tooltip", optionArray8);
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayoutOption[] optionArray9 = new GUILayoutOption[] { GUILayout.Width(200f) };
            sharedVariable.Tooltip = UnityEditor.EditorGUILayout.TextField(sharedVariable.Tooltip, optionArray9);
            GUILayout.EndHorizontal();
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                flag = true;
            }
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            GUI.enabled = CanNetworkSync(sharedVariable.GetType().GetProperty("Value").PropertyType);
            UnityEditor.EditorGUI.BeginChangeCheck();
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                flag = true;
            }
            GUILayout.EndHorizontal();
            GUI.enabled = flag2;
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            if (DrawSharedVariable(variableSource, sharedVariable, true))
            {
                flag = true;
            }
            if ((BehaviorDesignerWindow.instance != null) && BehaviorDesignerWindow.instance.ContainsError(null, variables[selectedVariableIndex].Name))
            {
                GUILayoutOption[] optionArray10 = new GUILayoutOption[] { GUILayout.Width(20f) };
                GUILayout.Box(BehaviorDesignerUtility.ErrorIconTexture, BehaviorDesignerUtility.PlainTextureGUIStyle, optionArray10);
            }
            GUILayout.EndHorizontal();
            BehaviorDesignerUtility.DrawContentSeperator(4, 7);
            GUILayout.EndVertical();
            GUILayout.Space(3f);
            return flag;
        }

        private static bool DrawSharedVariable(BehaviorDesigner.Runtime.IVariableSource variableSource, BehaviorDesigner.Runtime.SharedVariable sharedVariable, bool selected)
        {
            if ((sharedVariable == null) || (sharedVariable.GetType().GetProperty("Value") == null))
            {
                return false;
            }
            GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
            bool flag = false;
            if (string.IsNullOrEmpty(sharedVariable.PropertyMapping))
            {
                UnityEditor.EditorGUI.BeginChangeCheck();
                FieldInspector.DrawFields(null, sharedVariable, new GUIContent(sharedVariable.Name, sharedVariable.Tooltip));
                flag = UnityEditor.EditorGUI.EndChangeCheck();
            }
            else
            {
                if (selected)
                {
                    GUILayout.Label("Property", Array.Empty<GUILayoutOption>());
                }
                else
                {
                    GUILayout.Label(new GUIContent(sharedVariable.Name, sharedVariable.Tooltip), Array.Empty<GUILayoutOption>());
                }
                char[] separator = new char[] { '.' };
                string[] strArray = sharedVariable.PropertyMapping.Split(separator);
                GUILayout.Label(strArray[strArray.Length - 1].Replace('/', '.'), Array.Empty<GUILayoutOption>());
            }
            if (!sharedVariable.IsGlobal)
            {
                GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.Width(19f) };
                if (GUILayout.Button(BehaviorDesignerUtility.VariableMapButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, optionArray1))
                {
                    ShowPropertyMappingMenu(variableSource as BehaviorDesigner.Runtime.BehaviorSource, sharedVariable);
                }
            }
            GUILayout.EndHorizontal();
            return flag;
        }

        public bool DrawVariables(BehaviorDesigner.Runtime.BehaviorSource behaviorSource)
        {
            return DrawVariables(behaviorSource, behaviorSource, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
        }

        public static bool DrawVariables(BehaviorDesigner.Runtime.IVariableSource variableSource, BehaviorDesigner.Runtime.BehaviorSource behaviorSource, ref string variableName, ref bool focusNameField, ref int variableTypeIndex, ref Vector2 scrollPosition, ref List<float> variablePosition, ref float variableStartPosition, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, Array.Empty<GUILayoutOption>());
            bool flag = false;
            bool flag2 = false;
            if (DrawHeader(variableSource, object.ReferenceEquals(behaviorSource, null), ref variableStartPosition, ref variableName, ref focusNameField, ref variableTypeIndex, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex))
            {
                flag = true;
            }
            List<BehaviorDesigner.Runtime.SharedVariable> variables = (variableSource == null) ? null : variableSource.GetAllVariables();
            if ((variables != null) && (variables.Count > 0))
            {
                GUI.enabled = !flag2;
                if (DrawAllVariables(true, variableSource, ref variables, true, ref variablePosition, ref selectedVariableIndex, ref selectedVariableName, ref selectedVariableTypeIndex, true, true))
                {
                    flag = true;
                }
            }
            if (flag && (variableSource != null))
            {
                variableSource.SetAllVariables(variables);
            }
            GUI.enabled = true;
            GUILayout.EndScrollView();
            if (flag && (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && ((behaviorSource != null) && (behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior))))
            {
                BehaviorDesigner.Runtime.Behavior owner = behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior;
                if (owner.ExternalBehavior != null)
                {
                    if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
                    {
                        BinarySerialization.Save(behaviorSource);
                    }
                    else
                    {
                        JSONSerialization.Save(behaviorSource);
                    }
                    BehaviorDesigner.Runtime.BehaviorSource localBehaviorSource = owner.ExternalBehavior.GetBehaviorSource();
                    localBehaviorSource.CheckForSerialization(true, null);
                    SyncVariables(localBehaviorSource, variables);
                }
            }
            return flag;
        }

        public static List<System.Type> FindAllSharedVariableTypes(bool removeShared)
        {
            if (sharedVariableTypes == null)
            {
                sharedVariableTypes = new List<System.Type>();
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    try
                    {
                        System.Type[] types = assemblies[i].GetTypes();
                        for (int k = 0; k < types.Length; k++)
                        {
                            if (types[k].IsSubclassOf(typeof(BehaviorDesigner.Runtime.SharedVariable)) && !types[k].IsAbstract)
                            {
                                sharedVariableTypes.Add(types[k]);
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                sharedVariableTypes.Sort(new AlphanumComparator<System.Type>());
                sharedVariableStrings = new string[sharedVariableTypes.Count];
                sharedVariableTypesDict = new Dictionary<string, int>();
                for (int j = 0; j < sharedVariableTypes.Count; j++)
                {
                    string name = sharedVariableTypes[j].Name;
                    sharedVariableTypesDict.Add(name, j);
                    if (removeShared && ((name.Length > 6) && name.Substring(0, 6).Equals("Shared")))
                    {
                        name = name.Substring(6, name.Length - 6);
                    }
                    sharedVariableStrings[j] = name;
                }
            }
            return sharedVariableTypes;
        }

        public void FocusNameField()
        {
            this.mFocusNameField = true;
        }

        //private static string GetFullPath(Transform transform)
        //{
        //    return ((transform.parent != null) ? (GetFullPath(transform.parent) + "/" + transform.name) : transform.name);
        //}

        /// <summary>
        /// CoreEngine Transform 并不支持子节点，因此全路径就是名称
        /// </summary>
        /// <param name="transform"></param>
        /// <returns></returns>
        private static string GetFullPath(CoreTransform transform)
        {
            return transform.Name;
        }

        public bool HasFocus()
        {
            return ((GUIUtility.keyboardControl != 0) && !string.IsNullOrEmpty(this.mVariableName));
        }

        public bool LeftMouseDown(BehaviorDesigner.Runtime.IVariableSource variableSource, BehaviorDesigner.Runtime.BehaviorSource behaviorSource, Vector2 mousePosition)
        {
            return LeftMouseDown(variableSource, behaviorSource, mousePosition, this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex);
        }

        public static bool LeftMouseDown(BehaviorDesigner.Runtime.IVariableSource variableSource, BehaviorDesigner.Runtime.BehaviorSource behaviorSource, Vector2 mousePosition, List<float> variablePosition, float variableStartPosition, Vector2 scrollPosition, ref int selectedVariableIndex, ref string selectedVariableName, ref int selectedVariableTypeIndex)
        {
            if ((variablePosition != null) && ((mousePosition.y > variableStartPosition) && (variableSource != null)))
            {
                List<BehaviorDesigner.Runtime.SharedVariable> allVariables = null;
                if (Application.isPlaying || ((behaviorSource == null) || !(behaviorSource.Owner is BehaviorDesigner.Runtime.Behavior)))
                {
                    allVariables = variableSource.GetAllVariables();
                }
                else
                {
                    BehaviorDesigner.Runtime.Behavior owner = behaviorSource.Owner as BehaviorDesigner.Runtime.Behavior;
                    if (owner.ExternalBehavior == null)
                    {
                        allVariables = variableSource.GetAllVariables();
                    }
                    else
                    {
                        BehaviorDesigner.Runtime.BehaviorSource source = owner.GetBehaviorSource();
                        source.CheckForSerialization(true, null);
                        allVariables = source.GetAllVariables();
                        BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = owner.ExternalBehavior;
                        externalBehavior.BehaviorSource.Owner = externalBehavior;
                        externalBehavior.BehaviorSource.CheckForSerialization(true, behaviorSource);
                    }
                }
                if ((allVariables == null) || (allVariables.Count != variablePosition.Count))
                {
                    return false;
                }
                for (int i = 0; i < variablePosition.Count; i++)
                {
                    if (mousePosition.y < (variablePosition[i] - scrollPosition.y))
                    {
                        if (i == selectedVariableIndex)
                        {
                            return false;
                        }
                        selectedVariableIndex = i;
                        selectedVariableName = allVariables[i].Name;
                        selectedVariableTypeIndex = sharedVariableTypesDict[allVariables[i].GetType().Name];
                        return true;
                    }
                }
            }
            if (selectedVariableIndex == -1)
            {
                return false;
            }
            selectedVariableIndex = -1;
            return true;
        }

        public void OnEnable()
        {
            base.hideFlags = (HideFlags)0x3d;
        }

        private static void PropertySelected(object selected)
        {
            SelectedPropertyMapping mapping = selected as SelectedPropertyMapping;
            if (mapping.Property.Equals("None"))
            {
                mPropertyMappingVariable.PropertyMapping = string.Empty;
                mPropertyMappingVariable.PropertyMappingOwner = null;
            }
            else
            {
                mPropertyMappingVariable.PropertyMapping = mapping.Property;
                mPropertyMappingVariable.PropertyMappingOwner = mapping.GameObject;
            }
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
            {
                BinarySerialization.Save(mPropertyMappingBehaviorSource);
            }
            else
            {
                JSONSerialization.Save(mPropertyMappingBehaviorSource);
            }
        }

        public void ResetSelectedVariableIndex()
        {
            this.mSelectedVariableIndex = -1;
            this.mVariableStartPosition = -1f;
            if (this.mVariablePosition != null)
            {
                this.mVariablePosition.Clear();
            }
        }

        private static void ShowPropertyMappingMenu(BehaviorDesigner.Runtime.BehaviorSource behaviorSource, BehaviorDesigner.Runtime.SharedVariable sharedVariable)
        {
            mPropertyMappingVariable = sharedVariable;
            mPropertyMappingBehaviorSource = behaviorSource;
            mPropertyMappingMenu = new UnityEditor.GenericMenu();
            List<string> propertyNames = new List<string>();
            var propertyGameObjects = new List<CoreObject>();
            propertyNames.Add("None");
            propertyGameObjects.Add(null);
            int num = 0;
            if (behaviorSource.Owner.GetObject() is BehaviorDesigner.Runtime.Behavior)
            {
                CoreObject[] objArray;
                var gameObject = (behaviorSource.Owner.GetObject() as BehaviorDesigner.Runtime.Behavior).coreObject;
                int num2 = AddPropertyName(sharedVariable, gameObject, ref propertyNames, ref propertyGameObjects, true);
                if (num2 != -1)
                {
                    num = num2;
                }

                // 判断是否来源于资产
                // CoreObject 不会出现在预制件里
                // 因此不会出现在资产中，因此不需要判断
                //if (UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object) gameObject).Length == 0)
                //{
                //    objArray = UnityEngine.Object.FindObjectsOfType<GameObject>();
                //}
                //else
                //{
                //    Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
                //    objArray = new GameObject[componentsInChildren.Length];
                //    for (int k = 0; k < componentsInChildren.Length; k++)
                //    {
                //        objArray[k] = componentsInChildren[k].gameObject;
                //    }
                //}
                objArray = CoreEngine.lastestInstance.GetCoreObjectList();

                for (int j = 0; j < objArray.Length; j++)
                {
                    if (!objArray[j].Equals(gameObject) && ((num2 = AddPropertyName(sharedVariable, objArray[j], ref propertyNames, ref propertyGameObjects, false)) != -1))
                    {
                        num = num2;
                    }
                }
            }
            for (int i = 0; i < propertyNames.Count; i++)
            {
                char[] separator = new char[] { '.' };
                string[] strArray = propertyNames[i].Split(separator);
                if (propertyGameObjects[i] != null)
                {
                    strArray[strArray.Length - 1] = GetFullPath(propertyGameObjects[i].Transform) + "/" + strArray[strArray.Length - 1];
                }
                if (cache0 == null)
                {
                    cache0 = new UnityEditor.GenericMenu.MenuFunction2(VariableInspector.PropertySelected);
                }
                mPropertyMappingMenu.AddItem(new GUIContent(strArray[strArray.Length - 1]), i == num, cache0, new SelectedPropertyMapping(propertyNames[i], propertyGameObjects[i]));
            }
            mPropertyMappingMenu.ShowAsContext();
        }

        public static bool SyncVariables(BehaviorDesigner.Runtime.BehaviorSource localBehaviorSource, List<BehaviorDesigner.Runtime.SharedVariable> variables)
        {
            List<BehaviorDesigner.Runtime.SharedVariable> allVariables = localBehaviorSource.GetAllVariables();
            if (variables == null)
            {
                if ((allVariables == null) || (allVariables.Count <= 0))
                {
                    return false;
                }
                allVariables.Clear();
                return true;
            }
            bool flag = false;
            if (allVariables == null)
            {
                allVariables = new List<BehaviorDesigner.Runtime.SharedVariable>();
                localBehaviorSource.SetAllVariables(allVariables);
                flag = true;
            }
            for (int i = 0; i < variables.Count; i++)
            {
                if (variables[i] != null)
                {
                    if ((allVariables.Count - 1) < i)
                    {
                        BehaviorDesigner.Runtime.SharedVariable item = Activator.CreateInstance(variables[i].GetType()) as BehaviorDesigner.Runtime.SharedVariable;
                        item.Name = variables[i].Name;
                        item.IsShared = true;
                        item.SetValue(variables[i].GetValue());
                        allVariables.Add(item);
                        flag = true;
                    }
                    else if ((allVariables[i].Name != variables[i].Name) || (allVariables[i].GetType() != variables[i].GetType()))
                    {
                        BehaviorDesigner.Runtime.SharedVariable variable2 = Activator.CreateInstance(variables[i].GetType()) as BehaviorDesigner.Runtime.SharedVariable;
                        variable2.Name = variables[i].Name;
                        variable2.IsShared = true;
                        variable2.SetValue(variables[i].GetValue());
                        allVariables[i] = variable2;
                        flag = true;
                    }
                }
            }
            for (int j = allVariables.Count - 1; j > (variables.Count - 1); j--)
            {
                allVariables.RemoveAt(j);
                flag = true;
            }
            return flag;
        }

        private static bool VariableNameValid(BehaviorDesigner.Runtime.IVariableSource variableSource, string variableName)
        {
            return (!variableName.Equals(string.Empty) && ((variableSource == null) || object.ReferenceEquals(variableSource.GetVariable(variableName), null)));
        }

        private class SelectedPropertyMapping
        {
            private string mProperty;
            //private UnityEngine.GameObject mGameObject;
            private CoreObject mGameObject;

            public SelectedPropertyMapping(string property, CoreObject gameObject)
            {
                this.mProperty = property;
                this.mGameObject = gameObject;
            }

            public string Property
            {
                get
                {
                    return this.mProperty;
                }
            }

            public CoreObject GameObject
            {
                get
                {
                    return this.mGameObject;
                }
            }
        }
    }
}


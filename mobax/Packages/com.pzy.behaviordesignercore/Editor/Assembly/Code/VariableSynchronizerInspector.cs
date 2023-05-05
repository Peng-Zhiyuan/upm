namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    [UnityEditor.CustomEditor(typeof(BehaviorDesigner.Runtime.VariableSynchronizer))]
    public class VariableSynchronizerInspector : UnityEditor.Editor
    {
        [SerializeField]
        private Synchronizer sharedVariableSynchronizer = new Synchronizer();
        [SerializeField]
        private string sharedVariableValueTypeName;
        private System.Type sharedVariableValueType;
        [SerializeField]
        private BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType synchronizationType;
        [SerializeField]
        private bool setVariable;
        [SerializeField]
        private Synchronizer targetSynchronizer;
        private Action<Synchronizer, System.Type> thirdPartySynchronizer;
        private System.Type playMakerSynchronizationType;
        private System.Type uFrameSynchronizationType;

        private void DrawAnimatorSynchronizer(Synchronizer synchronizer)
        {
            DrawComponentSelector(synchronizer, typeof(Animator), ComponentListType.Instant);
            synchronizer.targetName = UnityEditor.EditorGUILayout.TextField("Parameter Name", synchronizer.targetName, Array.Empty<GUILayoutOption>());
        }

        public static void DrawComponentSelector(Synchronizer synchronizer, System.Type componentType, ComponentListType listType)
        {
            bool flag = false;
            UnityEditor.EditorGUI.BeginChangeCheck();

            // 这里绘制 core object 的调试 game object，并且不可更改
            //synchronizer.gameObject = UnityEditor.EditorGUILayout.ObjectField("GameObject", synchronizer.gameObject, typeof(GameObject), true, Array.Empty<GUILayoutOption>()) as GameObject;
            var debbbuger = synchronizer.gameObject.debbugerGameObject as GameObject;
            var ret = EditorGUILayout.ObjectField("CoreObject", debbbuger, typeof(GameObject), true);
            if(ret != debbbuger)
            {
                Debug.Log("[VariableSynchronizerInspector] 这里暂时修改为不可更改，因为不太明白这是干什么的");
            }

            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                flag = true;
            }
            if (synchronizer.gameObject == null)
            {
                GUI.enabled =(false);
            }
            if (listType == ComponentListType.Instant)
            {
                if (flag)
                {
                    synchronizer.component = (synchronizer.gameObject == null) ? null : synchronizer.gameObject.GetComponent(componentType);
                }
            }
            else if (listType == ComponentListType.BehaviorDesignerGroup)
            {
                if (synchronizer.gameObject != null)
                {
                    BehaviorDesigner.Runtime.Behavior[] components = synchronizer.gameObject.GetComponents<BehaviorDesigner.Runtime.Behavior>();
                    if ((components != null) && (components.Length > 1))
                    {
                        synchronizer.componentGroup = UnityEditor.EditorGUILayout.IntField("Behavior Tree Group", synchronizer.componentGroup, Array.Empty<GUILayoutOption>());
                    }
                    //synchronizer.component = (Component) GetBehaviorWithGroup(components, synchronizer.componentGroup);
                    synchronizer.component = GetBehaviorWithGroup(components, synchronizer.componentGroup);
                }
            }
            else if (listType == ComponentListType.Popup)
            {
                int count = 0;
                List<string> list = new List<string>();
                //Component[] components = null;
                CoreComponent[] components = null;
                list.Add("None");
                if (synchronizer.gameObject != null)
                {
                    components = synchronizer.gameObject.GetComponents(componentType);
                    int index = 0;
                    while (index < components.Length)
                    {
                        if (components[index].Equals(synchronizer.component))
                        {
                            count = list.Count;
                        }
                        string item = BehaviorDesignerUtility.SplitCamelCase(components[index].GetType().Name);
                        int num3 = 0;
                        int num4 = 0;
                        while (true)
                        {
                            if (num4 >= list.Count)
                            {
                                if (num3 > 0)
                                {
                                    item = item + " " + num3;
                                }
                                list.Add(item);
                                index++;
                                break;
                            }
                            if (list[index].Equals(item))
                            {
                                num3++;
                            }
                            num4++;
                        }
                    }
                }
                UnityEditor.EditorGUI.BeginChangeCheck();
                count = UnityEditor.EditorGUILayout.Popup("Component", count, list.ToArray(), Array.Empty<GUILayoutOption>());
                if (UnityEditor.EditorGUI.EndChangeCheck())
                {
                    synchronizer.component = (count == 0) ? null : components[count - 1];
                }
            }
        }

        private void DrawPlayMakerSynchronizer(Synchronizer synchronizer, System.Type valueType)
        {
            if (this.playMakerSynchronizationType == null)
            {
                this.playMakerSynchronizationType = System.Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_PlayMaker, Assembly-CSharp-Editor");
                if (this.playMakerSynchronizationType == null)
                {
                    UnityEditor.EditorGUILayout.LabelField("Unable to find PlayMaker inspector task.", Array.Empty<GUILayoutOption>());
                    return;
                }
            }
            if (this.thirdPartySynchronizer == null)
            {
                MethodInfo method = this.playMakerSynchronizationType.GetMethod("DrawPlayMakerSynchronizer");
                if (method != null)
                {
                    this.thirdPartySynchronizer = (Action<Synchronizer, System.Type>) Delegate.CreateDelegate(typeof(Action<Synchronizer, System.Type>), method);
                }
            }
            this.thirdPartySynchronizer.Invoke(synchronizer, valueType);
        }

        private void DrawPropertySynchronizer(Synchronizer synchronizer, System.Type valueType)
        {
            DrawComponentSelector(synchronizer, typeof(Component), ComponentListType.Popup);
            int count = 0;
            List<string> list = new List<string>();
            PropertyInfo[] properties = null;
            list.Add("None");
            if (synchronizer.component != null)
            {
                properties = synchronizer.component.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < properties.Length; i++)
                {
                    if (properties[i].PropertyType.Equals(valueType) && !properties[i].IsSpecialName)
                    {
                        if (properties[i].Name.Equals(synchronizer.targetName))
                        {
                            count = list.Count;
                        }
                        list.Add(properties[i].Name);
                    }
                }
            }
            UnityEditor.EditorGUI.BeginChangeCheck();
            count = UnityEditor.EditorGUILayout.Popup("Property", count, list.ToArray(), Array.Empty<GUILayoutOption>());
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                synchronizer.targetName = (count == 0) ? string.Empty : list[count];
            }
        }

        private bool DrawSharedVariableSynchronizer(Synchronizer synchronizer, System.Type valueType)
        {
            DrawComponentSelector(synchronizer, typeof(BehaviorDesigner.Runtime.Behavior), ComponentListType.BehaviorDesignerGroup);
            int index = 0;
            int globalStartIndex = -1;
            string[] names = null;
            if (synchronizer.component != null)
            {
                index = FieldInspector.GetVariablesOfType(valueType, synchronizer.global, synchronizer.targetName, (synchronizer.component as BehaviorDesigner.Runtime.Behavior).GetBehaviorSource(), out names, ref globalStartIndex, valueType == null, false);
            }
            else
            {
                names = new string[] { "None" };
            }
            UnityEditor.EditorGUI.BeginChangeCheck();
            index = UnityEditor.EditorGUILayout.Popup("Shared Variable", index, names, Array.Empty<GUILayoutOption>());
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                if (index == 0)
                {
                    synchronizer.targetName = null;
                }
                else
                {
                    if ((globalStartIndex != -1) && (index >= globalStartIndex))
                    {
                        synchronizer.targetName = names[index].Substring(8, names[index].Length - 8);
                        synchronizer.global = true;
                    }
                    else
                    {
                        synchronizer.targetName = names[index];
                        synchronizer.global = false;
                    }
                    if (valueType == null)
                    {
                        BehaviorDesigner.Runtime.SharedVariable variable = !synchronizer.global ? (synchronizer.component as BehaviorDesigner.Runtime.Behavior).GetVariable(names[index]) : BehaviorDesigner.Runtime.GlobalVariables.Instance.GetVariable(synchronizer.targetName);
                        this.sharedVariableValueTypeName = variable.GetType().GetProperty("Value").PropertyType.FullName;
                        this.sharedVariableValueType = null;
                    }
                }
            }
            if (string.IsNullOrEmpty(synchronizer.targetName))
            {
                GUI.enabled =(false);
            }
            return GUI.enabled;
        }

        private unsafe void DrawSynchronizedVariables(BehaviorDesigner.Runtime.VariableSynchronizer variableSynchronizer)
        {
            GUI.enabled =(true);
            if ((variableSynchronizer.SynchronizedVariables != null) && (variableSynchronizer.SynchronizedVariables.Count != 0))
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                lastRect.x = (-5f);
                lastRect.y = (lastRect.y + (lastRect.height + 1f));
                lastRect.height =(2f);
                lastRect.width = (lastRect.width + 20f);

                GUI.DrawTexture(lastRect, BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", true, (UnityEngine.Object) this));
                GUILayout.Space(6f);
                for (int i = 0; i < variableSynchronizer.SynchronizedVariables.Count; i++)
                {
                    BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizedVariable variable = variableSynchronizer.SynchronizedVariables[i];
                    if (variable.global)
                    {
                        if (BehaviorDesigner.Runtime.GlobalVariables.Instance.GetVariable(variable.variableName) == null)
                        {
                            variableSynchronizer.SynchronizedVariables.RemoveAt(i);
                            break;
                        }
                    }
                    else if (variable.behavior.GetVariable(variable.variableName) == null)
                    {
                        variableSynchronizer.SynchronizedVariables.RemoveAt(i);
                        break;
                    }
                    UnityEditor.EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.MaxWidth(120f) };
                    UnityEditor.EditorGUILayout.LabelField(variable.variableName, optionArray1);
                    GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(22f) };
                    if (GUILayout.Button(BehaviorDesignerUtility.LoadTexture(!variable.setVariable ? "RightArrowButton.png" : "LeftArrowButton.png", true, (UnityEngine.Object) this), BehaviorDesignerUtility.ButtonGUIStyle, optionArray2) && !Application.isPlaying)
                    {
                        variable.setVariable = !variable.setVariable;
                    }
                    GUILayoutOption[] optionArray3 = new GUILayoutOption[] { GUILayout.MinWidth(120f) };
                    UnityEditor.EditorGUILayout.LabelField(string.Format("{0} ({1})", variable.targetName, variable.synchronizationType.ToString()), optionArray3);
                    GUILayout.FlexibleSpace();
                    GUILayoutOption[] optionArray4 = new GUILayoutOption[] { GUILayout.Width(22f) };
                    if (GUILayout.Button(BehaviorDesignerUtility.LoadTexture("DeleteButton.png", true, (UnityEngine.Object) this), BehaviorDesignerUtility.ButtonGUIStyle, optionArray4))
                    {
                        variableSynchronizer.SynchronizedVariables.RemoveAt(i);
                        UnityEditor.EditorGUILayout.EndHorizontal();
                        break;
                    }
                    GUILayout.Space(2f);
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2f);
                }
                GUILayout.Space(4f);
            }
        }

        private void DrawuFrameSynchronizer(Synchronizer synchronizer, System.Type valueType)
        {
            if (this.uFrameSynchronizationType == null)
            {
                this.uFrameSynchronizationType = System.Type.GetType("BehaviorDesigner.Editor.VariableSynchronizerInspector_uFrame, Assembly-CSharp-Editor");
                if (this.uFrameSynchronizationType == null)
                {
                    UnityEditor.EditorGUILayout.LabelField("Unable to find uFrame inspector task.", Array.Empty<GUILayoutOption>());
                    return;
                }
            }
            if (this.thirdPartySynchronizer == null)
            {
                MethodInfo method = this.uFrameSynchronizationType.GetMethod("DrawSynchronizer");
                if (method != null)
                {
                    this.thirdPartySynchronizer = (Action<Synchronizer, System.Type>) Delegate.CreateDelegate(typeof(Action<Synchronizer, System.Type>), method);
                }
            }
            this.thirdPartySynchronizer.Invoke(synchronizer, valueType);
        }

        private static BehaviorDesigner.Runtime.Behavior GetBehaviorWithGroup(BehaviorDesigner.Runtime.Behavior[] behaviors, int group)
        {
            if ((behaviors == null) || (behaviors.Length == 0))
            {
                return null;
            }
            if (behaviors.Length != 1)
            {
                for (int i = 0; i < behaviors.Length; i++)
                {
                    if (behaviors[i].Group == group)
                    {
                        return behaviors[i];
                    }
                }
            }
            return behaviors[0];
        }

        public override void OnInspectorGUI()
        {
            BehaviorDesigner.Runtime.VariableSynchronizer variableSynchronizer = base.target as BehaviorDesigner.Runtime.VariableSynchronizer;
            if (variableSynchronizer != null)
            {
                GUILayout.Space(5f);
                variableSynchronizer.UpdateInterval = (BehaviorDesigner.Runtime.UpdateIntervalType) UnityEditor.EditorGUILayout.EnumPopup("Update Interval", variableSynchronizer.UpdateInterval, Array.Empty<GUILayoutOption>());
                //if (variableSynchronizer.UpdateInterval == BehaviorDesigner.Runtime.UpdateIntervalType.SpecifySeconds)
                //{
                //    variableSynchronizer.UpdateIntervalSeconds = UnityEditor.EditorGUILayout.FloatField("Seconds", variableSynchronizer.UpdateIntervalSeconds, Array.Empty<GUILayoutOption>());
                //}
                GUILayout.Space(5f);
                GUI.enabled =(!Application.isPlaying);
                this.DrawSharedVariableSynchronizer(this.sharedVariableSynchronizer, null);
                if (string.IsNullOrEmpty(this.sharedVariableSynchronizer.targetName))
                {
                    this.DrawSynchronizedVariables(variableSynchronizer);
                }
                else
                {
                    UnityEditor.EditorGUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
                    GUILayoutOption[] optionArray1 = new GUILayoutOption[] { GUILayout.MaxWidth(146f) };
                    UnityEditor.EditorGUILayout.LabelField("Direction", optionArray1);
                    GUILayoutOption[] optionArray2 = new GUILayoutOption[] { GUILayout.Width(22f) };
                    if (GUILayout.Button(BehaviorDesignerUtility.LoadTexture(!this.setVariable ? "RightArrowButton.png" : "LeftArrowButton.png", true, (UnityEngine.Object) this), BehaviorDesignerUtility.ButtonGUIStyle, optionArray2))
                    {
                        this.setVariable = !this.setVariable;
                    }
                    UnityEditor.EditorGUILayout.EndHorizontal();
                    UnityEditor.EditorGUI.BeginChangeCheck();
                    this.synchronizationType = (BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType) UnityEditor.EditorGUILayout.EnumPopup("Type", this.synchronizationType, Array.Empty<GUILayoutOption>());
                    if (UnityEditor.EditorGUI.EndChangeCheck())
                    {
                        this.targetSynchronizer = new Synchronizer();
                    }
                    if (this.targetSynchronizer == null)
                    {
                        this.targetSynchronizer = new Synchronizer();
                    }
                    if ((this.sharedVariableValueType == null) && !string.IsNullOrEmpty(this.sharedVariableValueTypeName))
                    {
                        this.sharedVariableValueType = BehaviorDesigner.Runtime.TaskUtility.GetTypeWithinAssembly(this.sharedVariableValueTypeName);
                    }
                    switch (this.synchronizationType)
                    {
                        case BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType.BehaviorDesigner:
                            this.DrawSharedVariableSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
                            break;

                        case BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType.Property:
                            this.DrawPropertySynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
                            break;

                        case BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType.Animator:
                            this.DrawAnimatorSynchronizer(this.targetSynchronizer);
                            break;

                        case BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType.PlayMaker:
                            this.DrawPlayMakerSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
                            break;

                        case BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizationType.uFrame:
                            this.DrawuFrameSynchronizer(this.targetSynchronizer, this.sharedVariableValueType);
                            break;

                        default:
                            break;
                    }
                    if (string.IsNullOrEmpty(this.targetSynchronizer.targetName))
                    {
                        GUI.enabled =(false);
                    }
                    if (GUILayout.Button("Add", Array.Empty<GUILayoutOption>()))
                    {
                        BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizedVariable item = new BehaviorDesigner.Runtime.VariableSynchronizer.SynchronizedVariable(this.synchronizationType, this.setVariable, this.sharedVariableSynchronizer.component as BehaviorDesigner.Runtime.Behavior, this.sharedVariableSynchronizer.targetName, this.sharedVariableSynchronizer.global, this.targetSynchronizer.component, this.targetSynchronizer.targetName, this.targetSynchronizer.global);
                        variableSynchronizer.SynchronizedVariables.Add(item);
                        BehaviorDesignerUtility.SetObjectDirty((UnityEngine.Object) variableSynchronizer);
                        this.sharedVariableSynchronizer = new Synchronizer();
                        this.targetSynchronizer = new Synchronizer();
                    }
                    GUI.enabled =(true);
                    this.DrawSynchronizedVariables(variableSynchronizer);
                }
            }
        }

        public enum ComponentListType
        {
            Instant,
            Popup,
            BehaviorDesignerGroup,
            None
        }

        [Serializable]
        public class Synchronizer
        {
            public CoreObject gameObject;
            public CoreComponent component;
            public string targetName;
            public bool global;
            public int componentGroup;
            public string componentName;
        }
    }
}


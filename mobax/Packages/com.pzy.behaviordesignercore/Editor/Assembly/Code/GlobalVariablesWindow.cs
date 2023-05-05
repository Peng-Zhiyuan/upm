namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class GlobalVariablesWindow : UnityEditor.EditorWindow
    {
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
        private BehaviorDesigner.Runtime.GlobalVariables mVariableSource;
        public static GlobalVariablesWindow instance;

        public void OnFocus()
        {
            instance = this;
            this.mVariableSource = BehaviorDesigner.Runtime.GlobalVariables.Instance;
            if (this.mVariableSource != null)
            {
                this.mVariableSource.CheckForSerialization(!Application.isPlaying);
            }
            FieldInspector.Init();
        }

        public void OnGUI()
        {
            if (this.mVariableSource == null)
            {
                this.mVariableSource = BehaviorDesigner.Runtime.GlobalVariables.Instance;
            }
            if (VariableInspector.DrawVariables(this.mVariableSource, null, ref this.mVariableName, ref this.mFocusNameField, ref this.mVariableTypeIndex, ref this.mScrollPosition, ref this.mVariablePosition, ref this.mVariableStartPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex))
            {
                this.SerializeVariables();
            }
            if ((UnityEngine.Event.current.type == null) && VariableInspector.LeftMouseDown(this.mVariableSource, null, UnityEngine.Event.current.mousePosition, this.mVariablePosition, this.mVariableStartPosition, this.mScrollPosition, ref this.mSelectedVariableIndex, ref this.mSelectedVariableName, ref this.mSelectedVariableTypeIndex))
            {
                UnityEngine.Event.current.Use();
                base.Repaint();
            }
        }

        private void SerializeVariables()
        {
            if (this.mVariableSource == null)
            {
                this.mVariableSource = BehaviorDesigner.Runtime.GlobalVariables.Instance;
            }
            if (BehaviorDesignerPreferences.GetBool(BDPreferences.BinarySerialization))
            {
                BinarySerialization.Save(this.mVariableSource);
            }
            else
            {
                JSONSerialization.Save(this.mVariableSource);
            }
        }

        [UnityEditor.MenuItem("Tools/Behavior Designer/Global Variables", false, 1)]
        public static void ShowWindow()
        {
            GlobalVariablesWindow window = GetWindow<GlobalVariablesWindow>(false, "Global Variables");
            window.minSize = (new Vector2(300f, 410f));
            window.minSize = (new Vector2(300f, float.MaxValue));
            window.wantsMouseMove = true;
        }
    }
}


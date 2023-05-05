namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using UnityEditor;

    [CustomCoreComponentInspector(typeof(BehaviorManager))]
    public class BehaviorManagerInspector : CoreComponentInspector
    {
        public override void OnInspectorGUI()
        {
            BehaviorDesigner.Runtime.BehaviorManager manager = base.target as BehaviorDesigner.Runtime.BehaviorManager;

            manager.UpdateInterval = (BehaviorDesigner.Runtime.UpdateIntervalType) UnityEditor.EditorGUILayout.EnumPopup("Update Interval", manager.UpdateInterval);
            //if (manager.UpdateInterval == BehaviorDesigner.Runtime.UpdateIntervalType.SpecifySeconds)
            //{
            //    UnityEditor.EditorGUI.indentLevel++;
            //    manager.UpdateIntervalSeconds = UnityEditor.EditorGUILayout.FloatField("Seconds", manager.UpdateIntervalSeconds);
            //    UnityEditor.EditorGUI.indentLevel--;
            //}
            manager.ExecutionsPerTick = (BehaviorDesigner.Runtime.BehaviorManager.ExecutionsPerTickType) UnityEditor.EditorGUILayout.EnumPopup("Task Execution Type", manager.ExecutionsPerTick);
            if (manager.ExecutionsPerTick == BehaviorDesigner.Runtime.BehaviorManager.ExecutionsPerTickType.Count)
            {
                UnityEditor.EditorGUI.indentLevel++;
                manager.MaxTaskExecutionsPerTick = UnityEditor.EditorGUILayout.IntField("Max Execution Count", manager.MaxTaskExecutionsPerTick);
                UnityEditor.EditorGUI.indentLevel--;
            }
        }
    }
}


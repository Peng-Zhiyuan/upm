namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;


    [UnityEditor.InitializeOnLoad]
    public class GizmoManager
    {
        private static string currentScene = SceneManager.GetActiveScene().name;
        [CompilerGenerated]
        private static Action cache;
        [CompilerGenerated]
        private static Action<UnityEditor.PlayModeStateChange> cache1;
        [CompilerGenerated]
        private static BehaviorDesigner.Runtime.BehaviorManager.BehaviorManagerHandler cache2;

        static GizmoManager()
        {
            if (cache == null)
            {
                cache = HierarchyChange;
            }
            UnityEditor.EditorApplication.hierarchyChanged += cache;
            if (!Application.isPlaying)
            {
                UpdateAllGizmos();
                if (cache1 == null)
                {
                    cache1 = new Action<UnityEditor.PlayModeStateChange>(GizmoManager.UpdateAllGizmos);
                }
                UnityEditor.EditorApplication.playModeStateChanged += cache1;
            }
        }

        public static void HierarchyChange()
        {
            BehaviorDesigner.Runtime.BehaviorManager instance = BehaviorDesigner.Runtime.BehaviorManager.latestInstance;
            if (!Application.isPlaying)
            {
                string str = SceneManager.GetActiveScene().name;
                if (currentScene != str)
                {
                    currentScene = str;
                    UpdateAllGizmos();
                }
            }
            else if (instance != null)
            {
                if (cache2 == null)
                {
                    cache2 = new BehaviorDesigner.Runtime.BehaviorManager.BehaviorManagerHandler(GizmoManager.UpdateBehaviorManagerGizmos);
                }
                instance.onEnableBehavior = cache2;
            }
        }

        public static void UpdateAllGizmos()
        {
            // 获得场景中所有行为树组件
            //BehaviorDesigner.Runtime.Behavior[] behaviorArray = UnityEngine.Object.FindObjectsOfType<BehaviorDesigner.Runtime.Behavior>();
            if(CoreEngine.lastestInstance != null)
            {
                var behaviorArray = CoreEngine.lastestInstance.FindObjectsOfType<Behavior>();
                for (int i = 0; i < behaviorArray.Length; i++)
                {
                    UpdateGizmo(behaviorArray[i]);
                }
            }


        }

        public static void UpdateAllGizmos(UnityEditor.PlayModeStateChange change)
        {
            UpdateAllGizmos();
        }

        private static void UpdateBehaviorManagerGizmos()
        {
            BehaviorDesigner.Runtime.BehaviorManager instance = BehaviorDesigner.Runtime.BehaviorManager.latestInstance;
            if (instance != null)
            {
                for (int i = 0; i < instance.BehaviorTrees.Count; i++)
                {
                    UpdateGizmo(instance.BehaviorTrees[i].behavior);
                }
            }
        }

        public static void UpdateGizmo(BehaviorDesigner.Runtime.Behavior behavior)
        {
            behavior.gizmoViewMode = (BehaviorDesigner.Runtime.Behavior.GizmoViewMode) BehaviorDesignerPreferences.GetInt(BDPreferences.GizmosViewMode);
            behavior.showBehaviorDesignerGizmo = BehaviorDesignerPreferences.GetBool(BDPreferences.ShowSceneIcon);
        }
    }
}


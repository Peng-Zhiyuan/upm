namespace BehaviorDesigner.Editor
{
    using BehaviorDesigner.Runtime;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class BehaviorDesignerPreferences : UnityEditor.Editor
    {
        private static string[] prefString;
        private static string[] serializationString = new string[] { "Binary", "JSON" };
        private static Dictionary<BDPreferences, object> prefToValue = new Dictionary<BDPreferences, object>();

        private static void DrawBoolPref(BDPreferences pref, string text, PreferenceChangeHandler callback)
        {
            bool @bool = GetBool(pref);
            bool flag2 = GUILayout.Toggle(@bool, text, Array.Empty<GUILayoutOption>());
            if (flag2 != @bool)
            {
                SetBool(pref, flag2);
                callback(pref, flag2);
            }
        }

        public static void DrawPreferencesPane(PreferenceChangeHandler callback)
        {
            DrawBoolPref(BDPreferences.ShowWelcomeScreen, "Show welcome screen", callback);
            DrawBoolPref(BDPreferences.ShowSceneIcon, "Show Behavior Designer icon in the scene", callback);
            DrawBoolPref(BDPreferences.ShowHierarchyIcon, "Show Behavior Designer icon in the hierarchy window", callback);
            DrawBoolPref(BDPreferences.OpenInspectorOnTaskSelection, "Open inspector on single task selection", callback);
            DrawBoolPref(BDPreferences.OpenInspectorOnTaskDoubleClick, "Open inspector on task double click", callback);
            DrawBoolPref(BDPreferences.FadeNodes, "Fade tasks after they are done running", callback);
            DrawBoolPref(BDPreferences.EditablePrefabInstances, "Allow edit of prefab instances", callback);
            DrawBoolPref(BDPreferences.PropertiesPanelOnLeft, "Position properties panel on the left", callback);
            DrawBoolPref(BDPreferences.MouseWhellScrolls, "Mouse wheel scrolls graph view", callback);
            DrawBoolPref(BDPreferences.FoldoutFields, "Grouped fields start visible", callback);
            DrawBoolPref(BDPreferences.CompactMode, "Compact mode", callback);
            DrawBoolPref(BDPreferences.SnapToGrid, "Snap to grid", callback);
            DrawBoolPref(BDPreferences.ShowTaskDescription, "Show selected task description", callback);
            DrawBoolPref(BDPreferences.UndoRedo, "Record undo/redo", callback);
            DrawBoolPref(BDPreferences.ErrorChecking, "Realtime error checking", callback);
            DrawBoolPref(BDPreferences.SelectOnBreakpoint, "Select GameObject if a breakpoint is hit", callback);
            DrawBoolPref(BDPreferences.UpdateCheck, "Check for updates", callback);
            DrawBoolPref(BDPreferences.AddGameGUIComponent, "Add Game GUI Component", callback);
            bool @bool = GetBool(BDPreferences.BinarySerialization);
            if (UnityEditor.EditorGUILayout.Popup("Serialization", !@bool ? 1 : 0, serializationString, Array.Empty<GUILayoutOption>()) != (!@bool ? 1 : 0))
            {
                SetBool(BDPreferences.BinarySerialization, !@bool);
                callback(BDPreferences.BinarySerialization, !@bool);
            }
            int @int = GetInt(BDPreferences.GizmosViewMode);
            int num2 = (int) ((BehaviorDesigner.Runtime.Behavior.GizmoViewMode) UnityEditor.EditorGUILayout.EnumPopup("Gizmos View Mode", (BehaviorDesigner.Runtime.Behavior.GizmoViewMode) @int, Array.Empty<GUILayoutOption>()));
            if (num2 != @int)
            {
                SetInt(BDPreferences.GizmosViewMode, num2);
                callback(BDPreferences.GizmosViewMode, num2);
            }
            @int = GetInt(BDPreferences.QuickSearchKeyCode);

            //num2 = (KeyCode) UnityEditor.EditorGUILayout.EnumPopup("Quick Search Key Code", (KeyCode) @int, new GUILayoutOption[] { });
            num2 = (int)(KeyCode)UnityEditor.EditorGUILayout.EnumPopup("Quick Search Key Code", (KeyCode)@int, new GUILayoutOption[] { });
            if (num2 != @int)
            {
                SetInt(BDPreferences.QuickSearchKeyCode, num2);
                callback(BDPreferences.QuickSearchKeyCode, num2);
            }
            float @float = GetFloat(BDPreferences.ZoomSpeedMultiplier);
            float num4 = UnityEditor.EditorGUILayout.Slider("Zoom Speed Multiplier", @float, 0.1f, 4f, Array.Empty<GUILayoutOption>());
            if (@float != num4)
            {
                SetFloat(BDPreferences.ZoomSpeedMultiplier, num4);
                callback(BDPreferences.ZoomSpeedMultiplier, num4);
            }
            if (GUILayout.Button("Restore to Defaults", UnityEditor.EditorStyles.miniButtonMid, Array.Empty<GUILayoutOption>()))
            {
                ResetPrefs();
            }
        }

        public static bool GetBool(BDPreferences pref)
        {
            object @bool;
            if (!prefToValue.TryGetValue(pref, out @bool))
            {
                @bool = UnityEditor.EditorPrefs.GetBool(PrefString[(int) pref]);
                prefToValue.Add(pref, @bool);
            }
            return (bool) @bool;
        }

        public static float GetFloat(BDPreferences pref)
        {
            object @float;
            if (!prefToValue.TryGetValue(pref, out @float))
            {
                @float = UnityEditor.EditorPrefs.GetFloat(PrefString[(int) pref], 1f);
                prefToValue.Add(pref, @float);
            }
            return (float) @float;
        }

        public static int GetInt(BDPreferences pref)
        {
            object @int;
            if (!prefToValue.TryGetValue(pref, out @int))
            {
                @int = UnityEditor.EditorPrefs.GetInt(PrefString[(int) pref]);
                prefToValue.Add(pref, @int);
            }
            return (int) @int;
        }

        public static void InitPrefernces()
        {
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[0]))
            {
                SetBool(BDPreferences.ShowWelcomeScreen, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[1]))
            {
                SetBool(BDPreferences.ShowSceneIcon, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[2]))
            {
                SetBool(BDPreferences.ShowHierarchyIcon, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[3]))
            {
                SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[3]))
            {
                SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[5]))
            {
                SetBool(BDPreferences.FadeNodes, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[6]))
            {
                SetBool(BDPreferences.EditablePrefabInstances, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[7]))
            {
                SetBool(BDPreferences.PropertiesPanelOnLeft, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[8]))
            {
                SetBool(BDPreferences.MouseWhellScrolls, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[9]))
            {
                SetBool(BDPreferences.FoldoutFields, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[10]))
            {
                SetBool(BDPreferences.CompactMode, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[11]))
            {
                SetBool(BDPreferences.SnapToGrid, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[12]))
            {
                SetBool(BDPreferences.ShowTaskDescription, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[13]))
            {
                SetBool(BDPreferences.BinarySerialization, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[14]))
            {
                SetBool(BDPreferences.UndoRedo, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[15]))
            {
                SetBool(BDPreferences.ErrorChecking, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[0x10]))
            {
                SetBool(BDPreferences.SelectOnBreakpoint, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[0x11]))
            {
                SetBool(BDPreferences.UpdateCheck, true);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[0x12]))
            {
                SetBool(BDPreferences.AddGameGUIComponent, false);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[0x13]))
            {
                SetInt(BDPreferences.GizmosViewMode, 2);
            }
            if (!UnityEditor.EditorPrefs.HasKey(PrefString[20]))
            {
                SetInt(BDPreferences.QuickSearchKeyCode, 0x20);
            }
            if (GetBool(BDPreferences.EditablePrefabInstances) && GetBool(BDPreferences.BinarySerialization))
            {
                SetBool(BDPreferences.BinarySerialization, false);
            }
        }

        private static void InitPrefString()
        {
            prefString = new string[0x16];
            for (int i = 0; i < prefString.Length; i++)
            {
                prefString[i] = string.Format("BehaviorDesigner{0}", (BDPreferences) i);
            }
        }

        private static void ResetPrefs()
        {
            SetBool(BDPreferences.ShowWelcomeScreen, true);
            SetBool(BDPreferences.ShowSceneIcon, true);
            SetBool(BDPreferences.ShowHierarchyIcon, true);
            SetBool(BDPreferences.OpenInspectorOnTaskSelection, false);
            SetBool(BDPreferences.OpenInspectorOnTaskDoubleClick, false);
            SetBool(BDPreferences.FadeNodes, true);
            SetBool(BDPreferences.EditablePrefabInstances, false);
            SetBool(BDPreferences.PropertiesPanelOnLeft, true);
            SetBool(BDPreferences.MouseWhellScrolls, false);
            SetBool(BDPreferences.FoldoutFields, true);
            SetBool(BDPreferences.CompactMode, false);
            SetBool(BDPreferences.SnapToGrid, true);
            SetBool(BDPreferences.ShowTaskDescription, true);
            SetBool(BDPreferences.BinarySerialization, false);
            SetBool(BDPreferences.UndoRedo, true);
            SetBool(BDPreferences.ErrorChecking, true);
            SetBool(BDPreferences.SelectOnBreakpoint, false);
            SetBool(BDPreferences.UpdateCheck, true);
            SetBool(BDPreferences.AddGameGUIComponent, false);
            SetInt(BDPreferences.GizmosViewMode, 2);
            SetInt(BDPreferences.QuickSearchKeyCode, 0x20);
            SetInt(BDPreferences.ZoomSpeedMultiplier, 1);
        }

        public static void SetBool(BDPreferences pref, bool value)
        {
            UnityEditor.EditorPrefs.SetBool(PrefString[(int) pref], value);
            prefToValue[pref] = value;
        }

        public static void SetFloat(BDPreferences pref, float value)
        {
            UnityEditor.EditorPrefs.SetFloat(PrefString[(int) pref], value);
            prefToValue[pref] = value;
        }

        public static void SetInt(BDPreferences pref, int value)
        {
            UnityEditor.EditorPrefs.SetInt(PrefString[(int) pref], value);
            prefToValue[pref] = value;
        }

        private static string[] PrefString
        {
            get
            {
                if (prefString == null)
                {
                    InitPrefString();
                }
                return prefString;
            }
        }
    }
}


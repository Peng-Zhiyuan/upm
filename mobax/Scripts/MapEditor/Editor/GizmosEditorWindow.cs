namespace Dragonli.MapEditor
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;

    public class GizmosEditorWindow : OdinMenuEditorWindow
    {
        Dictionary<Type, List<GizmosMonoBehaviour>> StateMap = new Dictionary<Type, List<GizmosMonoBehaviour>>();
        Dictionary<Type, bool> StateGroupMap = new Dictionary<Type, bool>();
        //Dictionary<Type, bool> StateGroupFoldMap = new Dictionary<Type, bool>();

        private void Awake()
        {
            InitStates();
        }
        [MenuItem("Window/Map Editor/Gizmo List")]
        public static void ShowWindow()
        {
            GetWindow<GizmosEditorWindow>().Show();
        }
        //protected override void OnGUI()
        //{
        //    base.OnGUI();
        //    OnBuildInGUI();
        //}
        void OnBuildInGUI ()
        {
            // group
            var modified = new Dictionary<Type, bool>();
            var modifiedFold = new Dictionary<Type, bool>();
            var uncheckChild = new Dictionary<Type, bool>();
            var modifiedNodes = new Dictionary<GizmosMonoBehaviour, bool>();
            foreach (var state in StateGroupMap) {
                var newVal = EditorGUILayout.Toggle(state.Key.Name, state.Value);
                if (newVal != state.Value) {
                    modified.Add(state.Key, newVal);
                    //Debug.LogFormat("{0}: {1}", state.Key.Name, newVal);
                }
                //var fold = EditorGUILayout.BeginFoldoutHeaderGroup(StateGroupFoldMap[state.Key], state.Key.Name);
                //if (fold != StateGroupFoldMap[state.Key])
                //    modifiedFold.Add(state.Key, fold);
                var allShown = true;
                foreach (var gizmo in StateMap[state.Key]) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    var val = EditorGUILayout.Toggle(gizmo.gameObject.name, gizmo.IsShown);
                    EditorGUILayout.EndHorizontal();
                    if (val != gizmo.IsShown) {
                        modifiedNodes.Add(gizmo, val);
                        Debug.LogFormat("{0}: {1}", gizmo.gameObject.name, val);
                    }
                    if (!val)
                        allShown = false;
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
                if (allShown != newVal) {
                    uncheckChild.Add(state.Key, allShown);
                }
            }
            foreach (var state in modified) {
                EditorGUILayout.Toggle(state.Key.Name, state.Value);
                StateGroupMap[state.Key] = state.Value;
                foreach (var gizmo in StateMap[state.Key])
                    gizmo.IsShown = state.Value;
            }
            foreach (var state in uncheckChild) {
                EditorGUILayout.Toggle(state.Key.Name, state.Value);
                StateGroupMap[state.Key] = state.Value;
            }
            //foreach (var state in modifiedFold) {
            //    StateGroupFoldMap[state.Key] = state.Value;
            //}

            // children
            //foreach (var state in StateGroupMap) {

            //}
            foreach (var state in modifiedNodes) {
                state.Key.IsShown = state.Value;
            }

            if (GUILayout.Button("Refresh")) {
                InitStates();
            }
        }
        public void InitStates()
        {
            StateMap.Clear();
            StateGroupMap.Clear();

            GizmosHelper.SwitchAllGizmos(false);
            var gizmoObjects = GameObject.FindObjectsOfType<GizmosMonoBehaviour>();
            foreach (var gizmo in gizmoObjects) {
                if (!StateMap.ContainsKey(gizmo.GetType())) {
                    StateMap.Add(gizmo.GetType(), new List<GizmosMonoBehaviour>());
                    GizmosHelper.SwitchGizmo(gizmo.GetType().Name, gizmo.IsShown);
                }
                StateMap[gizmo.GetType()].Add(gizmo);
                if (!StateGroupMap.ContainsKey(gizmo.GetType())) {
                    StateGroupMap.Add(gizmo.GetType(), true);
                }
                if (!gizmo.IsShown)
                StateGroupMap[gizmo.GetType()] = false;
                //if (!StateGroupFoldMap.ContainsKey(gizmo.GetType())) {
                //    StateGroupFoldMap.Add(gizmo.GetType(), false);
                //}
            }
        }
        public void UpdateStates()
        {

        }

        protected override OdinMenuTree BuildMenuTree()
        {
            // Test menu
            //var toggledMenu = new ToggledOdinMenuItem(tree, new ToggledMenuItemModel() { Name = "QQ", Value = new GUIContent ()});
            //var toggledSubmenu = new ToggledOdinMenuItem(tree, new ToggledMenuItemModel() { Name = "WWW", Value = new GUIContent() });
            //tree.MenuItems.Insert(0, toggledMenu);
            //toggledMenu.ChildMenuItems.Add(toggledSubmenu);
            InitStates();
            var tree = new OdinMenuTree();
            foreach (var state in StateGroupMap) {
                var toggledMenu = new ToggledOdinMenuItem(tree, new ToggledMenuItemModel() {
                    Name = state.Key.ToString(),
                    Enabled = state.Value,
                    Value = null,
                    OnEnableChange = OnMenuEnableChanged,
                });
                tree.MenuItems.Insert(0, toggledMenu);

                foreach (var gizmo in StateMap[state.Key]) {
                    var toggledSubmenu = new ToggledOdinMenuItem(tree, new ToggledMenuItemModel() {
                        Name = gizmo.name,
                        Value = gizmo,
                        Enabled = gizmo.IsShown,
                        OnEnableChange = OnMenuEnableChanged,
                    }); 
                    toggledMenu.ChildMenuItems.Add(toggledSubmenu);
                }
            }

            return tree;
        }
        void OnMenuEnableChanged(OdinMenuItem menu)
        {
            // group
            if (menu.Parent == null) {
                var submenus = menu.ChildMenuItems;
                var parentItem = menu as ToggledOdinMenuItem;
                foreach (var item in submenus) {
                    var toggleMenu = item as ToggledOdinMenuItem;
                    toggleMenu.instance.Enabled = parentItem.instance.Enabled;
                    (toggleMenu.Value as GizmosMonoBehaviour).IsShown = toggleMenu.instance.Enabled;
                }
            }
            // submenu
            else {
                var allShown = true;
                var submenus = menu.Parent.ChildMenuItems;
                var parentItem = menu.Parent as ToggledOdinMenuItem;
                var curItem = menu as ToggledOdinMenuItem;
                (curItem.Value as GizmosMonoBehaviour).IsShown = curItem.instance.Enabled;
                foreach (var item in submenus) {
                    var toggleMenu = item as ToggledOdinMenuItem;
                    if (!toggleMenu.instance.Enabled)
                        allShown = false;
                }
                if (allShown != parentItem.instance.Enabled) {
                    parentItem.instance.Enabled = allShown;
                }
            }
        }

        protected override void OnBeginDrawEditors()
        {
            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();
                if (SirenixEditorGUI.ToolbarButton ("Refresh")) {
                    ForceMenuTreeRebuild();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private class ToggledOdinMenuItem : OdinMenuItem
        {
            public readonly ToggledMenuItemModel instance;
            public ToggledOdinMenuItem(OdinMenuTree tree, ToggledMenuItemModel instance) : base(tree, instance.Name, instance)
            {
                this.instance = instance;
                this.Value = instance.Value;
            }
            protected override void OnDrawMenuItem(Rect rect, Rect labelRect)
            {
                labelRect.x -= 16;
                var newVal = GUI.Toggle(labelRect.AlignMiddle(18).AlignLeft(16), this.instance.Enabled, GUIContent.none);
                if (newVal != instance.Enabled) {
                    instance.Enabled = newVal;
                    if (instance.OnEnableChange != null)
                        instance.OnEnableChange(this);
                }
            }
            public override string SmartName => base.SmartName;

            public override bool Toggled { 
                get => base.Toggled; 
                set { 
                    base.Toggled = value;
                    if (value && instance != null) {
                        foreach (var submenu in ChildMenuItems) {
                            if (submenu.IsSelected && submenu.Value != null) {
                                var comp = (submenu.Value as Component);
                                if (comp != null)
                                    Selection.objects = new UnityEngine.Object[] { comp.gameObject };
                            }
                        }
                    }
                } 
            }
        }
        private class ToggledMenuItemModel
        {
            public bool Enabled = true;
            public string Name;
            public object Value;
            public Action<OdinMenuItem> OnEnableChange;
        }

    }
}
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using ObjectFieldAlignment = Sirenix.OdinInspector.ObjectFieldAlignment;

namespace Plot.Runtime
{
    [PlotComicsConfigElementItem("场景模型-其他(炸弹&猫等)", (int)EPlotComicsElementType.SceneModelEnv, (int) EConfigPriority.SceneModelEnv)]
    public class PlotComicsSceneEnvModelElement : PlotComicsConfigElementItem
    {
        #region ---抽象---

        public override string Label => "场景模型-其他(炸弹&猫等)";

        #endregion

        [ToggleGroup("enabled"), LabelText("模型资源:")]
        [LabelWidth(65)]
        //TODO:这里最好不要用0.000多位小数点
        [HorizontalGroup("enabled/model", 0.720f)]
        [VerticalGroup("enabled/model/leftInfo")]
        [HorizontalGroup("enabled/model/leftInfo/button", 0.92f)]
        [OnValueChanged("InitPreviewModel")]
        [Sirenix.OdinInspector.FilePath(ParentFolder = "Assets/Arts/Env", Extensions = "prefab")]
        public string modelRes;

        [HorizontalGroup("enabled/model")]
        [PreviewField(ObjectFieldAlignment.Left, Height = 120)]
        [ReadOnly]
        [HideLabel]
        public GameObject modelPrefab;

        // [ToggleGroup("enabled"), LabelText("模型动作:")]
        // [VerticalGroup("enabled/model/leftInfo")]
        // [ValueDropdown("ModelActionList")]
        // [LabelWidth(65)]
        // [ReadOnly]
        [HideInInspector] public string actionName = CharacterActionConst.Idle;

        [ToggleGroup("enabled"), LabelText("唯一ID:")] [VerticalGroup("enabled/model/leftInfo")] [LabelWidth(65)]
        public int id;

        [ToggleGroup("enabled"), LabelText("锁定不再修改:")] [VerticalGroup("enabled/model/leftInfo")] [LabelWidth(80)]
        public bool isLock = false;

        [ToggleGroup("enabled"), LabelText(" 模型位置:")] [VerticalGroup("enabled/model/leftInfo")] [LabelWidth(80)]
        public Vector3 pos;

        [ToggleGroup("enabled"), LabelText(" 模型旋转:")] [VerticalGroup("enabled/model/leftInfo")] [LabelWidth(80)]
        public Vector3 rotation;

        [ToggleGroup("enabled"), LabelText(" 模型缩放:")] [VerticalGroup("enabled/model/leftInfo")] [LabelWidth(80)]
        public Vector3 scale = Vector3.one;

        private IEnumerable<string> ModelActionList()
        {
            var results = PlotDefineUtil.ActionStyles;
            results.Insert(0, "无");
            return results;
        }

        private void InitActionSumTime()
        {
            var animator = this.modelPrefab.GetComponent<Animator>();

            var animationClips = animator.runtimeAnimatorController.animationClips;
            var clip = Array.Find(animationClips, val => val.name.Equals(this.actionName));
            if (clip == null) return;
        }

        [UsedImplicitly]
        [OnInspectorInit]
        private void InitPreviewModel()
        {
            var pathName = $"{PlotDefineUtil.PLOT_MODEL_ENV_PATH_FOLDER}/{this.modelRes}";
#if UNITY_EDITOR
            this.modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{pathName}");
#endif
        }

#if UNITY_EDITOR

        [CustomValueDrawer("DoSelection")] [HorizontalGroup("enabled/model/leftInfo/button", 0.1f)] [LabelWidth(80)]
        public bool selection;

        private bool DoSelection(bool value, GUIContent label)
        {
            var button = GUILayout.Button(EditorGUIUtility.IconContent("Pick"));
            //     , new GUIStyle()
            // {
            //     fixedWidth = 20,
            //     fixedHeight = 20,
            // });
            if (button)
            {
                this.selectionTime = EditorApplication.timeSinceStartup;
            }

            return button;
        }
#endif
    }
}
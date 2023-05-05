using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("移动(相对)", (int) EPlotActionType.Move)]
    public class PlotComicsMoveActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "移动(相对)";

        [HideInInspector] private GameObject targetObj;

        [ValueDropdown("GetTargetObjNames")]
        [ToggleGroup("enabled"), LabelText("目标组件:")]
        [OnValueChanged("SetTargetObj")]
        [LabelWidth(80)]
        public string targetObjName = "(场景模型)-未选中";

        [ToggleGroup("enabled"), LabelText("移动类型:")] [LabelWidth(80)]
        public EPlotMoveType PlotMoveType;

        public bool IsImmediateMoveModeType
        {
            get { return PlotMoveType == EPlotMoveType.Immediate; }
        }

        public bool IsSpeedMoveModeType
        {
            get { return PlotMoveType == EPlotMoveType.Speed; }
        }

        /// <summary>
        /// 瞬间移动参数
        /// </summary>
        [ToggleGroup("enabled"), LabelText("相对位移距离:"), ShowIf("IsImmediateMoveModeType")] [LabelWidth(100)]
        public float immediateOffsetDistance = 0.0f;

        [ToggleGroup("enabled"), LabelText("相对位移高度:"), ShowIf("IsImmediateMoveModeType")] [LabelWidth(100)]
        public float immediateOffsetY = 0.0f;

        /// <summary>
        /// 速度移动参数
        /// </summary>
        [ToggleGroup("enabled"), LabelText("移动方向:"), ShowIf("IsSpeedMoveModeType")] [LabelWidth(80)]
        public EPlotMoveDir PlotMoveDir;

        [ToggleGroup("enabled"), LabelText("移动速度:"), ShowIf("IsSpeedMoveModeType")] [LabelWidth(80)]
        public float moveSpeed = 1;

        [ToggleGroup("enabled"), LabelText("最大距离:"), ShowIf("IsSpeedMoveModeType")] [LabelWidth(80)]
        public float moveMaxDis = 10;

        [ToggleGroup("enabled"), LabelText("返回初始位置:")] [LabelWidth(100)]
        public bool IsBackStart = false;

        public void Init()
        {
            this.SetTargetObj();
        }

        /// <summary>
        /// 获取场景内所有的目标组件
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetTargetObjNames()
        {
            GameObject modelRoot = GameObject.Find("ModelRoot");
            List<string> targetNames = new List<string>();
            targetNames.Insert(0, "(场景模型)-未选中");
            if (modelRoot.transform.childCount <= 0) return targetNames;

            for (int i = 0; i < modelRoot.transform.childCount; i++)
            {
                var go = modelRoot.transform.GetChild(i);
                targetNames.Add(go.name);
            }

            return targetNames;
        }

        private void SetTargetObj()
        {
            this.targetObj = this.targetObjName.Equals("(场景模型)-未选中")
                ? null
                : GameObject.Find($"ModelRoot/{this.targetObjName}");
        }
    }
}
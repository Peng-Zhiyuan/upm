using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class PlotComicsActionElementItemAttribute : Attribute
    {
        readonly string _actionType;
        readonly int _order;

        public PlotComicsActionElementItemAttribute(string actionType, int order)
        {
            this._actionType = actionType;
            this._order = order;
        }

        public string ActionType => _actionType;

        public int Order => _order;
    }

    public abstract class PlotComicsActionElementItem
    {
        /// <summary>
        /// 标识-记录当前帧动画行为类型
        /// </summary>
        [HideInInspector]
        public virtual string Label => "PlotComicsActionElementItem";
        private void SetSelection()
        {
            Debug.Log(" this.enabled = " + this.enabled);
        }

        [OnValueChanged("SetSelection")] [ToggleGroup("enabled", "$Label")]
        public bool enabled;

        [ToggleGroup("enabled"), LabelText("行为类型:"), GUIColor("GetColor")] [LabelWidth(100)]
        [ReadOnly]
        public EPlotActionType type;

        [HorizontalGroup("Frame", 0.5f, LabelWidth = 50)] [LabelText("开始帧")]
        public int startFrame = 0;

        [HorizontalGroup("Frame", 0.5f, LabelWidth = 50)] [LabelText("结束帧")]
        public int endFrame = 20;

        [LabelText("备注")] public string desc = "";

        public virtual int High()
        {
            return 69;
        }

        public Color GetColor()
        {
            if (this.type == EPlotActionType.Animation)
            {
                return Colors.AnimationFrame;
            }
            else if (this.type == EPlotActionType.Move)
            {
                return Colors.MoveFrame;
            }
            else if (this.type == EPlotActionType.TimeLine)
            {
                return Colors.TimeLineFrame;
            }

            return Colors.ChoosedFrame;
        }
    }
}
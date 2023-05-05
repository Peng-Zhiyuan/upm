using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class PlotComicsConfigElementItemAttribute : Attribute
    {
        readonly string _elementType;
        readonly int _order;
        readonly int _priority;

        public PlotComicsConfigElementItemAttribute(string elementType, int order, int priority)
        {
            this._elementType = elementType;
            this._order = order;
            this._priority = priority;
        }

        public string ElementType => _elementType;

        public int Order => _order;
        public int Priority => _priority;
    }

    public abstract class PlotComicsConfigElementItem
    {
        // 继承标识
        [HideInInspector] public virtual string Label => "PlotComicsConfigElementItem";

        // 设置预览的时间
        [HideInInspector] public double selectionTime;

        [ToggleGroup("enabled", "$Label")] public bool enabled;

        [ToggleGroup("enabled"), LabelText("节点类型:")]
        // [ GUIColor("GetColor")]
        [VerticalGroup("enabled/preview")]
        [LabelWidth(65)]
        [ReadOnly]
        public EPlotComicsElementType type;

        [ToggleGroup("enabled"), LabelText("Priority")]
        // [ GUIColor("GetColor")]
        [VerticalGroup("enabled/preview")]
        [LabelWidth(65)]
        [ReadOnly]
        public EConfigPriority priority;
    }
}
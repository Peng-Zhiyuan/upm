using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class SkillActionElementItemAttribute : Attribute
    {
        readonly string elementType;
        readonly int order;

        public SkillActionElementItemAttribute(string effectType, int order)
        {
            this.elementType = effectType;
            this.order = order;
        }

        public string ElementType
        {
            get { return elementType; }
        }
        public int Order
        {
            get { return order; }
        }
    }

    [Serializable]
    public abstract class SkillActionElementItem
    {
        [HideInInspector]
        public virtual string Label => "SkillActionElementItem";
        [ToggleGroup("Enabled", "$Label")]
        public bool Enabled = true;
        [ToggleGroup("Enabled"), LabelText("行为类型"), GUIColor("GetColor")]
        [LabelWidth(80)]
        public SKILL_ACTION_ELEMENT_TYPE type;
        [HorizontalGroup("Frame", 0.5f, LabelWidth = 50)]
        [LabelText("开始帧")]
        [OnValueChanged("StartFrameChanged")]
        public int startFrame = 0;
        [HorizontalGroup("Frame", 0.5f, LabelWidth = 50)]
        [LabelText("结束帧")]
        public int endFrame = 20;
        [LabelText("备注")]
        public string desc = "";

        [HideInInspector]
        public int initFrame = 0;

        public virtual int High()
        {
            return 69;
        }

        public virtual void StartFrameChanged() { }

        public virtual List<string> GetPreLoadAssetLst()
        {
            return null;
        }

        public virtual Color GetColor()
        {
            return Color.white;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.Reflection;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("DOT", 19)]
    public sealed class CreatDotActionElement : SkillActionElementItem
    {
        public override string Label => "DOT";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string effectPrefab;
        [ToggleGroup("Enabled"), LabelText("位置偏移"), LabelWidth(120)]
        public Vector3 offset = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("角度偏移"), LabelWidth(120)]
        public Vector3 angleOffset = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("缩放"), LabelWidth(120)]
        public Vector3 scale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("是否跟着角色旋转"), LabelWidth(120)]
        public bool isAttachLookAt = true;
        [ToggleGroup("Enabled"), SuffixLabel("毫秒", true), LabelText("持续时间"), LabelWidth(120)]
        public int durationTime = 1;
        [ToggleGroup("Enabled"), SuffixLabel("毫秒", true), LabelText("触发间隔"), LabelWidth(120)]
        public int intervalTime = 1;
        [ToggleGroup("Enabled"), LabelText("范围"), LabelWidth(120)]
        public float radius = 1;
        [ToggleGroup("Enabled"), LabelText("只生效一次"), LabelWidth(120)]
        public bool effectiveOnce = false;
        [ToggleGroup("Enabled"), LabelText("是否直接执行Effect"), LabelWidth(120)]
        public bool applyEffectDirect = false;
        [ToggleGroup("Enabled"), LabelText("效果列表"), Space(10)]
        [ListDrawerSettings(Expanded = true, DraggableItems = true, ShowItemCount = false, HideAddButton = true)]
        [HideReferenceObjectPicker]
        public List<Effect> Effects = new List<Effect>();
        [HideLabel]
        [ToggleGroup("Enabled"), EnumPaging, OnValueChanged("AddEffect")]
        [ValueDropdown("EffectTypeSelect")]
        public string EffectTypeName = "(添加效果)";

        public IEnumerable<string> EffectTypeSelect()
        {
            var types = typeof(Effect).Assembly.GetTypes().Where(x => !x.IsAbstract).Where(x => typeof(Effect).IsAssignableFrom(x)).Where(x => x.GetCustomAttribute<EffectAttribute>() != null).OrderBy(x => x.GetCustomAttribute<EffectAttribute>().Order).Select(x => x.GetCustomAttribute<EffectAttribute>().EffectType);
            var results = types.ToList();
            results.Insert(0, "(添加效果)");
            return results;
        }

        private void AddEffect()
        {
            if (EffectTypeName != "(添加效果)")
            {
                var effectType = typeof(Effect).Assembly.GetTypes().Where(x => !x.IsAbstract).Where(x => typeof(Effect).IsAssignableFrom(x)).Where(x => x.GetCustomAttribute<EffectAttribute>() != null).Where(x => x.GetCustomAttribute<EffectAttribute>().EffectType == EffectTypeName).First();
                var effect = Activator.CreateInstance(effectType) as Effect;
                effect.Enabled = true;
                effect.IsSkillEffect = true;
                Effects.Add(effect);
                EffectTypeName = "(添加效果)";
            }
        }
        
        public override Color GetColor()
        {
            return Colors.DotFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(effectPrefab));
            return addResPathLst;
        }
    }
}
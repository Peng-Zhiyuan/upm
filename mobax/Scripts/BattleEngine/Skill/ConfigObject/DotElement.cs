using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.Reflection;

namespace BattleEngine.Logic
{
    [Serializable]
    public sealed class DotElement
    {
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [LabelText("特效")]
        public string effectPrefab;
        [LabelText("缩放"), LabelWidth(120)]
        public Vector3 scale = Vector3.one;
        [SuffixLabel("毫秒", true), LabelText("持续时间"), LabelWidth(120)]
        public int durationTime = 1;
        [SuffixLabel("毫秒", true), LabelText("触发间隔"), LabelWidth(120)]
        public int intervalTime = 1;
        [LabelText("只生效一次"), LabelWidth(120)]
        public bool effectiveOnce = true;
        [LabelText("是否直接执行Effect"), LabelWidth(120)]
        public bool applyEffectDirect = false;
        [LabelText("范围"), LabelWidth(120)]
        public float radius = 1;
        [LabelText("效果列表"), Space(10)]
        [ListDrawerSettings(Expanded = true, DraggableItems = true, ShowItemCount = false, HideAddButton = true)]
        [HideReferenceObjectPicker]
        public List<Effect> Effects = new List<Effect>();
        [HideLabel]
        [EnumPaging, OnValueChanged("AddEffect")]
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

        public List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(effectPrefab));
            return addResPathLst;
        }
    }
}
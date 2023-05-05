using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("相机特效", 30)]
    public sealed class CameraEffectElement : SkillActionElementItem
    {
        public override string Label => "相机特效";

        [ToggleGroup("Enabled"), LabelText("特效名称")]
        public string effect;

        [ToggleGroup("Enabled"), LabelText("位置偏移")]
        public Vector3 offset;

        [ToggleGroup("Enabled"), LabelText("比例")]
        public Vector3 scale;

        [ToggleGroup("Enabled"), LabelText("旋转")]
        public Vector3 rot;

        public override Color GetColor()
        {
            return Colors.EffectFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(effect));
            return addResPathLst;
        }
    }
}
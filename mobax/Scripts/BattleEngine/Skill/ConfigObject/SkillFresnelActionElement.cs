/* Created:Loki Date:2023-02-02*/

using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("Fresnel效果", 31)]
    public sealed class SkillFresnelActionElement : SkillActionElementItem
    {
        public override string Label => "Fresnel效果";
        [ToggleGroup("Enabled"), LabelText("蒙皮节点"), LabelWidth(100)]
        public string skinMeshRoot;
        [ToggleGroup("Enabled"), LabelText("颜色参数"), LabelWidth(100)]
        public string hexColor;
        [ToggleGroup("Enabled"), LabelText("淡入帧数"), LabelWidth(100)]
        public int FadeInFrame;
        [ToggleGroup("Enabled"), LabelText("淡出帧数"), LabelWidth(100)]
        public int FadeOutFrame;

        public override Color GetColor()
        {
            return Colors.FresnelFrame;
        }
    }
}
using Sirenix.OdinInspector;
using UnityEngine;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("弹道", (int) EPlotActionType.Trajectory)]
    public class PlotComicsTrajectoryActionElement : PlotComicsActionElementItem
    {
        public override string Label => "弹道";

        [LabelWidth(100)] [ToggleGroup("enabled"), LabelText("绑点目标ID")]
        public int attachTargetId = 0;

        [FilePath(ParentFolder = "Assets/Arts/FX")] [ToggleGroup("enabled"), LabelText("子弹路径"), LabelWidth(100)]
        public string effectPath = "";

        [ToggleGroup("enabled"), LabelText(" 子弹大小"), LabelWidth(100)]
        public Vector3 effectScale = Vector3.one;

        [FilePath(ParentFolder = "Assets/Arts/FX")] [ToggleGroup("enabled"), LabelText("命中特效"), LabelWidth(100)]
        public string destroyEffect = "";

        [FilePath(ParentFolder = "Assets/res/$Sound")] [ToggleGroup("enabled"), LabelText("命中音效"), LabelWidth(100)]
        public string destroyAudio;

        [ToggleGroup("enabled"), LabelText(" 特效大小"), LabelWidth(100)]
        public Vector3 destroyEffectScale = Vector3.one;

        [ToggleGroup("enabled"), LabelText(" 发射偏移"), LabelWidth(100)]
        public Vector3 fireOffset = Vector3.zero;

        [ToggleGroup("enabled"), LabelText(" 中心偏移"), LabelWidth(100)]
        public Vector3 centerOffset = Vector3.zero;

        [ToggleGroup("enabled"), LabelText("消失延迟"), LabelWidth(100)]
        public float destroyDelay = 0;

        //[HorizontalGroup("2", 0.5f, LabelWidth = 50)]
        [ToggleGroup("enabled"), LabelText("子弹数量"), LabelWidth(100)]
        public int flyCount = 1;

        //[HorizontalGroup("3", 0.5f, LabelWidth = 50)]
        [ToggleGroup("enabled"), LabelText("间隔时间"), LabelWidth(100)]
        public int flyTimeOffset;

        //[HorizontalGroup("3", 0.5f, LabelWidth = 50)]
        [ToggleGroup("enabled"), LabelText("角度偏差"), LabelWidth(100)]
        public int flyAngleOffset = 0;

        //[HorizontalGroup("4", 0.5f, LabelWidth = 50)]
        [ToggleGroup("enabled"), LabelText("碰撞大小"), LabelWidth(100)]
        public float colliderRadius = 1;

        //[HorizontalGroup("1", 0.5f, LabelWidth = 50)]
        [ToggleGroup("enabled"), LabelText("伤害倍率"), LabelWidth(100)]
        public float hurtRatio = 1.0f;

        //[HorizontalGroup("1", 0.5f, LabelWidth = 50)]
        [ToggleGroup("enabled"), LabelText("发射速度"), LabelWidth(100)]
        public int flySpeed = 0;

        [ToggleGroup("enabled"), LabelText("速度曲线"), LabelWidth(100)]
        public AnimationCurve speedCurve = AnimationCurve.Constant(0, 1, 1);

        [ToggleGroup("enabled"), SuffixLabel("毫秒", true), LabelText("加速到最大速度时间"), LabelWidth(150)]
        public int cureTime = 1000;

        // [ToggleGroup("enabled"), LabelText("是否飞行固定时间"), LabelWidth(150)]
        // public bool flyFixedTime = false;

        [ToggleGroup("enabled"), LabelText("影响目标ID"), LabelWidth(100)]
        public int hurtTargetId = 0;

        [ToggleGroup("enabled"), LabelText("抛物线高度"), LabelWidth(100)]
        public float parabolaHeight;

        [ToggleGroup("enabled"), LabelText("抛物线曲线"), LabelWidth(100)]
        public AnimationCurve parabolaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    }
}
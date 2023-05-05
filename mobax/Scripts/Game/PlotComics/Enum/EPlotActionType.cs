using Sirenix.OdinInspector;

namespace Plot.Runtime
{
    /// <summary>
    /// timeline类型
    /// </summary>
    public enum EPlotActionType
    {
        [LabelText("动画")] Animation = 1,
        [LabelText("移动(相对)")] Move = 2,
        [LabelText("对话气泡")] Bubble = 3,
        [LabelText("镜头动画")] Camera = 4,
        [LabelText("遮罩框动画")] Mask = 5,
        [LabelText("遮罩框动画(溶解)")] MaskDissolve = 6,
        [LabelText("位移&旋转&缩放动画")] TransAni = 7,
        [LabelText("移动(绝对)")] MoveAbsolute = 8,
        [LabelText("特效")] Effect = 9,
        [LabelText("TimeLine")] TimeLine = 10,
        [LabelText("屏幕震动")] ScreenShark = 11,
        [LabelText("气泡动画(固定模板放大->停留->变小)")] BubbleTemplate = 12,
        [LabelText("弹道")] Trajectory = 13,
    }
}
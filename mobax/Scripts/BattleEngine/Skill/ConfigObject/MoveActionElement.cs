using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("移动", 17)]
    public sealed class MoveActionElement : SkillActionElementItem
    {
        public override string Label => "移动";
        [ToggleGroup("Enabled"), LabelText("目标选定")]
        [LabelWidth(80)]
        public SKILL_MOVE_TARGET_TYPE MoveTargetType = SKILL_MOVE_TARGET_TYPE.SkillTarget;
        public bool IsNoneTarget
        {
            get { return MoveTargetType == SKILL_MOVE_TARGET_TYPE.Self; }
        }
        [ToggleGroup("Enabled"), LabelText("移动类型")]
        [LabelWidth(80)]
        public MOVE_MODE_TYPE MoveModeType = MOVE_MODE_TYPE.Immediate;
        public bool IsImmediateMoveModeType
        {
            get { return MoveModeType == MOVE_MODE_TYPE.Immediate; }
        }
        public bool IsSpeedMoveModeType
        {
            get { return MoveModeType == MOVE_MODE_TYPE.Speed; }
        }
        /// <summary>
        /// 瞬间移动参数
        /// </summary>
        [ToggleGroup("Enabled"), LabelText("相对位移距离"), ShowIf("IsImmediateMoveModeType")]
        [LabelWidth(100)]
        public float immediateOffsetDistance = 0.0f;
        [ToggleGroup("Enabled"), LabelText("相对位移高度"), ShowIf("IsImmediateMoveModeType")]
        [LabelWidth(100)]
        public float immediateOffsetY = 0.0f;

        /// <summary>
        /// 速度移动参数
        /// </summary>
        [ToggleGroup("Enabled"), LabelText("移动方向"), ShowIf("@this.IsNoneTarget && this.IsSpeedMoveModeType")]
        [LabelWidth(80)]
        public MOVE_DIR_TYPE moveDir;
        [ToggleGroup("Enabled"), LabelText("移动速度"), ShowIf("IsSpeedMoveModeType")]
        [LabelWidth(80)]
        public float moveSpeed = 1;
        [ToggleGroup("Enabled"), LabelText("最大距离"), ShowIf("IsSpeedMoveModeType")]
        [LabelWidth(80)]
        public float moveMaxDis = 10;

        [ToggleGroup("Enabled"), LabelText("返回初始位置")]
        [LabelWidth(100)]
        public bool IsBackStart = false;

        public override Color GetColor()
        {
            return Colors.MoveFrame;
        }
    }
}
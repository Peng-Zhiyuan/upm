using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("技能生效检测", 3)]
    public class CreateTriggerActionElement : SkillActionElementItem
    {
        public override string Label => "技能生效检测";
        [ToggleGroup("Enabled"), LabelText("检测次数")]
        [LabelWidth(80)]
        public int triggerTimes;
        [ToggleGroup("Enabled"), LabelText("检测间隔")]
        [LabelWidth(80)]
        public int intervalTime;
        [ToggleGroup("Enabled"), LabelText("一击必杀")]
        [LabelWidth(80)]
        public bool oneHitKill = false;
        [ToggleGroup("Enabled"), LabelText("伤害倍率")]
        [LabelWidth(80)]
        public float hurtRatio = 1;
        [ToggleGroup("Enabled"), LabelText("技能检测方式"), LabelWidth(100)]
        public SKILL_JUDGEX_TYPE JudgexType;
        public bool NeedCenter
        {
            get { return JudgexType == SKILL_JUDGEX_TYPE.AreaSelect || JudgexType == SKILL_JUDGEX_TYPE.TargetDirAreaSelect; }
        }
        public bool NeedJudgeShapeType
        {
            get { return JudgexType == SKILL_JUDGEX_TYPE.AreaSelect || JudgexType == SKILL_JUDGEX_TYPE.TargetAreaSelect || JudgexType == SKILL_JUDGEX_TYPE.PreWarningAreaSelect || JudgexType == SKILL_JUDGEX_TYPE.InputAreaSelect; }
        }
        [ToggleGroup("Enabled"), LabelText("区域场类型"), ShowIf("NeedJudgeShapeType"), LabelWidth(100)]
        public SKILL_JUDGEX_SHAPE JudgeShapeType;
        public bool NeedPartName
        {
            get { return JudgexType == SKILL_JUDGEX_TYPE.TargetDirAreaSelect; }
        }
        [ToggleGroup("Enabled"), LabelText("相对部件"), ShowIf("NeedPartName"), LabelWidth(100)]
        public string AttachPart = "";

        [ToggleGroup("Enabled"), LabelText("偏移S"), ShowIf("NeedCenter")]
        public Vector3 CenterStart;
        [ToggleGroup("Enabled"), LabelText("偏移E"), ShowIf("NeedCenter")]
        public Vector3 CenterEnd;
        [ToggleGroup("Enabled"), LabelText("半径S"), ShowIf("ShowRadius")]
        public float RadiusStart;
        [ToggleGroup("Enabled"), LabelText("半径E"), ShowIf("ShowRadius")]
        public float RadiusEnd;
        [ToggleGroup("Enabled"), LabelText("半径变化曲线"), ShowIf("ShowRadius"), LabelWidth(100)]
        public AnimationCurve radiusDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        [ToggleGroup("Enabled"), LabelText("外圈半径S"), ShowIf("ShowOuterRadius")]
        [LabelWidth(80)]
        public float OuterRadiusStart;
        [ToggleGroup("Enabled"), LabelText("外圈半径E"), ShowIf("ShowOuterRadius")]
        [LabelWidth(80)]
        public float OuterRadiusEnd;

        [ToggleGroup("Enabled"), LabelText("旋转角度S"), ShowIf("ShowSectorAreaRadius")]
        [LabelWidth(80)]
        public float RotYStart;
        [ToggleGroup("Enabled"), LabelText("旋转角度E"), ShowIf("ShowSectorAreaRadius")]
        [LabelWidth(80)]
        public float RotYEnd;
        [ToggleGroup("Enabled"), LabelText("角度S"), ShowIf("ShowSectorAreaRadius")]
        public float AngleStart;
        [ToggleGroup("Enabled"), LabelText("角度E"), ShowIf("ShowSectorAreaRadius")]
        public float AngleEnd;
        public bool ShowOuterRadius
        {
            get { return (JudgeShapeType == SKILL_JUDGEX_SHAPE.Annular) && NeedJudgeShapeType; }
        }
        public bool ShowRadius
        {
            get { return (JudgeShapeType == SKILL_JUDGEX_SHAPE.Cylinder || JudgeShapeType == SKILL_JUDGEX_SHAPE.Sector || JudgeShapeType == SKILL_JUDGEX_SHAPE.Annular) && NeedJudgeShapeType; }
        }
        public bool ShowSectorAreaRadius
        {
            get { return JudgeShapeType == SKILL_JUDGEX_SHAPE.Sector && NeedJudgeShapeType; }
        }
        [ToggleGroup("Enabled"), LabelText("初始大小")]
        [LabelWidth(80)]
        public Vector3 SizeStart = new Vector3(0, 6, 0);
        [ToggleGroup("Enabled"), LabelText("最终大小")]
        [LabelWidth(80)]
        public Vector3 SizeEnd = new Vector3(0, 6, 0);

        public override Color GetColor()
        {
            return Colors.AttackBoxFrame;
        }
    }
}
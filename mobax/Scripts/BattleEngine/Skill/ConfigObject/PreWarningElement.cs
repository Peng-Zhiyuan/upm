namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;
    using Sirenix.OdinInspector;
    using System;

    [Serializable]
    [SkillActionElementItem("预警", 18)]
    public sealed class PreWarningElement : SkillActionElementItem
    {
        public override string Label => "预警";
        [ToggleGroup("Enabled"), LabelText("预警类型"), LabelWidth(100)]
        public SKILL_PRE_WARING_EFFECT_TYPE waringEffectType = SKILL_PRE_WARING_EFFECT_TYPE.Circle;

        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string waringEffect;
        [ToggleGroup("Enabled"), LabelText("是否隐藏外圈"), LabelWidth(120)]
        public bool hideOutDecals = true;

        [ToggleGroup("Enabled"), LabelText("特效大小"), LabelWidth(100)]
        [InfoBox("圆形预警圈半径一米大约是0.05,箭形RayLine位置偏移是指箭形预警下RayLine的Position，箭形RayLine缩放是指箭形预警下RayLine的Scale")]
        public Vector3 effectScale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("箭形RayLine位置偏移"), LabelWidth(120)]
        public Vector3 rayLocalPos = new Vector3(0.14f, 2.57f, 0);
        [ToggleGroup("Enabled"), LabelText("箭形RayLine缩放"), LabelWidth(120)]
        public Vector3 rayLocalScale = new Vector3(1, 1, 1);

        [ToggleGroup("Enabled"), LabelText("预警数量"), LabelWidth(100)]
        public int effectNum = 1;
        [ToggleGroup("Enabled"), LabelText("持续时间(帧)"), LabelWidth(100)]
        public int perDuration = 5;
        [ToggleGroup("Enabled"), LabelText("触发时间(帧)"), LabelWidth(100)]
        public List<int> triggerTimes = new List<int>();
        [ToggleGroup("Enabled"), LabelText("影响半径"), SuffixLabel("随机位置使用", true), LabelWidth(100)]
        public int radius = 1;
        [ToggleGroup("Enabled"), LabelText("预警位置模式"), LabelWidth(100)]
        public SKILL_PRE_WARING_TYPE waringType = SKILL_PRE_WARING_TYPE.Random;
        [ToggleGroup("Enabled"), LabelText("是否跟着角色旋转"), ShowIf("NeedOffset"), LabelWidth(120)]
        public bool isAttachLookAt = true;
        public bool NeedOffset
        {
            get { return waringType == SKILL_PRE_WARING_TYPE.FixedPosition; }
        }
        [ToggleGroup("Enabled"), LabelText("位置偏移"), ShowIf("NeedOffset"), LabelWidth(100)]
        public List<Vector3> offset = new List<Vector3>();
        [ToggleGroup("Enabled"), LabelText("扩散速度"), LabelWidth(100)]
        public float spreadSpeed = 1;
        public bool NeedMove
        {
            get { return waringType == SKILL_PRE_WARING_TYPE.RandomTarget || waringType == SKILL_PRE_WARING_TYPE.CurTarget; }
        }

        [ToggleGroup("Enabled"), LabelText("跟随速度"), ShowIf("NeedMove"), LabelWidth(120)]
        public float followSpeed = 0;
        [ToggleGroup("Enabled"), LabelText("跟随结束帧"), ShowIf("NeedMove"), LabelWidth(120)]
        public int followEndFrame = 0;

        public override Color GetColor()
        {
            return Colors.PreWaringFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(waringEffect));
            return addResPathLst;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("弹道", 7)]
    public sealed class CastTrajectoryActionElement : SkillActionElementItem
    {
        public override string Label => "弹道";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("子弹路径"), LabelWidth(100)]
        public string effectPath = "";
        [ToggleGroup("Enabled"), LabelText("子弹大小"), LabelWidth(100)]
        public Vector3 effectScale = Vector3.one;
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("命中特效"), LabelWidth(100)]
        public string destroyEffect = "";
        [FilePath(ParentFolder = "Assets/res/$Sound")]
        [ToggleGroup("Enabled"), LabelText("命中音效"), LabelWidth(100)]
        public string destoryAudio;
        [ToggleGroup("Enabled"), LabelText("特效大小"), LabelWidth(100)]
        public Vector3 destroyEffectScale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("发射偏移"), LabelWidth(100)]
        public Vector3 fireOffset = Vector3.zero;
        [ToggleGroup("Enabled"), LabelText("中心偏移"), LabelWidth(100)]
        public Vector3 centerOffset = Vector3.zero;
        [ToggleGroup("Enabled"), LabelText("弹道销毁时间(浮点秒)"), LabelWidth(130)]
        public float destoryTime;
        [ToggleGroup("Enabled"), LabelText("消失延迟(浮点秒)"), LabelWidth(130)]
        public float destoryDelay = 0;

        [ToggleGroup("Enabled"), LabelText("子弹数量"), LabelWidth(100)]
        public int flyCount = 1;
        [ToggleGroup("Enabled"), LabelText("间隔时间"), LabelWidth(100)]
        public int flyTimeOffset;
        [ToggleGroup("Enabled"), LabelText("角度偏差"), LabelWidth(100)]
        public int flyAngleOffset = 0;
        [ToggleGroup("Enabled"), LabelText("碰撞大小"), LabelWidth(100)]
        public float colliderRadius = 1;
        [ToggleGroup("Enabled"), LabelText("伤害倍率"), LabelWidth(100)]
        public float hurtRatio = 1.0f;
        [ToggleGroup("Enabled"), LabelText("是否飞行固定时间"), LabelWidth(150)]
        public bool flyFixedTime = false;
        public bool NotFlyFixedTime
        {
            get { return !flyFixedTime; }
        }
        [ToggleGroup("Enabled"), LabelText("彈道飛行時間"), SuffixLabel("毫秒", true), LabelWidth(150), ShowIf("flyFixedTime")]
        public int flyTime = 0;
        [ToggleGroup("Enabled"), LabelText("发射速度"), LabelWidth(100), ShowIf("NotFlyFixedTime")]
        public int flySpeed = 0;
        [ToggleGroup("Enabled"), LabelText("速度曲线"), LabelWidth(100), ShowIf("NotFlyFixedTime")]
        public AnimationCurve speedCurve = AnimationCurve.Constant(0, 1, 1);
        [ToggleGroup("Enabled"), SuffixLabel("毫秒", true), LabelText("加速到最大速度时间"), LabelWidth(150), ShowIf("NotFlyFixedTime")]
        public int cureTime = 1000;
        [ToggleGroup("Enabled"), LabelText("最大飛行距离"), LabelWidth(100)]
        public float flyMaxDist = 10;
        [ToggleGroup("Enabled"), LabelText("是否穿透"), LabelWidth(100)]
        public bool isPenetrate = false;
        [ToggleGroup("Enabled"), LabelText("只影响目标"), LabelWidth(100)]
        public bool onlyHurtTarget = false;

        [ToggleGroup("Enabled"), LabelText("弹道类型"), LabelWidth(100)]
        public TRAJECTORYITEM_TYPE trajectoryType;
        public bool NeedSelectTargetType
        {
            get { return trajectoryType == TRAJECTORYITEM_TYPE.SelectTarget; }
        }
        public bool NeedSelectPosType
        {
            get { return trajectoryType == TRAJECTORYITEM_TYPE.SelectPos || trajectoryType == TRAJECTORYITEM_TYPE.Parabola; }
        }
        public bool ParabolaType
        {
            get { return trajectoryType == TRAJECTORYITEM_TYPE.Parabola; }
        }
        [ToggleGroup("Enabled"), ShowIf("NeedSelectTargetType"), LabelWidth(100)]
        public TRAJECTORYITEM_TARGET_TYPE selectTargetType = TRAJECTORYITEM_TARGET_TYPE.CurTarget;
        [ToggleGroup("Enabled"), ShowIf("NeedSelectPosType"), LabelWidth(100)]
        public TRAJECTORYITEM_POS_TYPE selectPosType;
        public bool free
        {
            get { return trajectoryType == TRAJECTORYITEM_TYPE.Free; }
        }
        [ToggleGroup("Enabled"), ShowIf("free")]
        public MOVE_DIR_TYPE dirType = MOVE_DIR_TYPE.Froward;

        [ToggleGroup("Enabled"), LabelText("抛物线高度"), ShowIf("ParabolaType"), LabelWidth(120)]
        public float parabolaHeight;
        [ToggleGroup("Enabled"), LabelText("抛物线曲线"), ShowIf("ParabolaType"), LabelWidth(120)]
        public AnimationCurve parabolaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [ToggleGroup("Enabled"), LabelText("落点DOT"), LabelWidth(120)]
        public DotElement dotElement = new DotElement();
#if UNITY_EDITOR
        [ToggleGroup("Enabled"), LabelText("落点位置"), SuffixLabel("仅调试用", true), LabelWidth(120)]
        public List<Vector3> endPos = new List<Vector3>();
#endif

        public override Color GetColor()
        {
            return Colors.TrajectoryFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(effectPath));
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(destroyEffect));
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(destoryAudio));
            if (dotElement != null)
            {
                addResPathLst.AddRange(dotElement.GetPreLoadAssetLst());
            }
            return addResPathLst;
        }
    }
}
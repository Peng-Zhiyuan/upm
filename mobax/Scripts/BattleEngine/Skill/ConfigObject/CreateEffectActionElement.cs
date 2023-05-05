using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("技能特效", 2)]
    public sealed class CreateEffectActionElement : SkillActionElementItem
    {
        public override string Label => "技能特效";
        [FilePath(ParentFolder = "Assets/Arts/FX")]
        [ToggleGroup("Enabled"), LabelText("特效")]
        public string res = "";
        [ToggleGroup("Enabled"), LabelText("缩放")]
        public Vector3 scale = Vector3.one;
        [ToggleGroup("Enabled"), LabelText("速度改变"), Space(10)]
        [ListDrawerSettings(Expanded = false, DraggableItems = true, ShowItemCount = true, HideAddButton = false)]
        [HideReferenceObjectPicker]
        public List<AnimSpeed> speedModify = new List<AnimSpeed>();
        [ToggleGroup("Enabled"), LabelText("绑点类型")]
        public COORDINATE_TYPE attachType = COORDINATE_TYPE.Global;
        public bool NeedAttachPoint
        {
            get { return attachType == COORDINATE_TYPE.Global || attachType == COORDINATE_TYPE.Local || attachType == COORDINATE_TYPE.Target; }
        }
        public bool NeedOffset
        {
            get { return attachType == COORDINATE_TYPE.Global || attachType == COORDINATE_TYPE.Local || attachType == COORDINATE_TYPE.Target; }
        }
        [ToggleGroup("Enabled"), LabelText("绑点"), ShowIf("NeedAttachPoint")]
        public string attachPoint = "";
        [LabelWidth(80)]
        [ToggleGroup("Enabled"), LabelText("位置偏移"), ShowIf("NeedOffset")]
        public Vector3 posOffset = Vector3.zero;
        [LabelWidth(80)]
        [ToggleGroup("Enabled"), LabelText("角度偏移"), ShowIf("NeedOffset")]
        public Vector3 angleOffset = Vector3.zero;
        //[HorizontalGroup("attach", 0.5f, LabelWidth = 120)]
        [LabelWidth(150)]
        [ToggleGroup("Enabled"), LabelText("是否跟着角色移动")]
        public bool isAttachLock = false;
        [LabelWidth(150)]
        // [HorizontalGroup("attach", 0.5f, LabelWidth = 120)]
        [ToggleGroup("Enabled"), LabelText("是否跟着角色旋转")]
        public bool isAttachLookAt = true;
        [LabelWidth(150)]
        [ToggleGroup("Enabled"), LabelText("是否朝向目标位置")]
        public bool isLookAtTarget = false;
        [LabelWidth(150)]
        [ToggleGroup("Enabled"), LabelText("是否朝向预警位置")]
        public bool isLookAtWarningPoint = false;
        [LabelWidth(150)]
        //[HorizontalGroup("other", 0.5f, LabelWidth = 120)]
        [ToggleGroup("Enabled"), LabelText("是否循环")]
        public bool isloop = false;
        [LabelWidth(150)]
        //[HorizontalGroup("other", 0.5f, LabelWidth = 120)]
        [ToggleGroup("Enabled"), LabelText("被打断时是否销毁")]
        public bool isReleaseWhenBreaked = true;
        [LabelWidth(150)]
        [ToggleGroup("Enabled"), LabelText("是否仅自己播放")]
        public bool isSelfPlay = true;

        public override Color GetColor()
        {
            return Colors.EffectFrame;
        }

        public override List<string> GetPreLoadAssetLst()
        {
            List<string> addResPathLst = new List<string>();
            addResPathLst.Add(AddressablePathConst.SkillEditorPathParse(res));
            return addResPathLst;
        }
    }
}
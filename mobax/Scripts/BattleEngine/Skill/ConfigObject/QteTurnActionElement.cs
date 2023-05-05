using Sirenix.OdinInspector;
using UnityEngine;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("QTE转向", 10)]
    public sealed class QteTurnActionElement : SkillActionElementItem
    {
        [ToggleGroup("Enabled"), LabelText("目标死亡时重新寻找目标"), LabelWidth(200)]
        public bool ctWhenTargetDie = false;
        public override string Label => "QTE转向";
        [ToggleGroup("Enabled"), LabelText("转向朝向类型"), LabelWidth(150)]
        public QTE_TURN_FORWARD_TYPE forwardType;
        [ToggleGroup("Enabled"), LabelText("转向部位类型"), LabelWidth(100)]
        public QTE_TURN_BIND_TYPE bindType;

        [ToggleGroup("Enabled"), LabelText("转向部位"), ShowIf("NeedPartyName"), LabelWidth(100)]
        public string bindPartyName;
        [ToggleGroup("Enabled"), LabelText("看向目标偏移量"), ShowIf("NeedPartyName"), LabelWidth(150)]
        public float lookAtOffset = 15;
        [ToggleGroup("Enabled"), LabelText("转向速度"), ShowIf("NeedPartyName"), LabelWidth(100)]
        public float turnSpeed;
        public bool NeedPartyName
        {
            get { return bindType == QTE_TURN_BIND_TYPE.Party; }
        }

        public override Color GetColor()
        {
            return Colors.QteTurnFrame;
        }
    }
}
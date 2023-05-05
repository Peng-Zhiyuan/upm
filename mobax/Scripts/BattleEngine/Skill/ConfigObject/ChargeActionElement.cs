using UnityEngine;
using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [SkillActionElementItem("冲锋", 4)]
    public sealed class ChargeActionElement : SkillActionElementItem
    {
        public override string Label => "冲锋";
        [ToggleGroup("Enabled"), LabelText("冲锋类型")]
        public CHARGE_TYPE chargeType;
        [ToggleGroup("Enabled"), ShowIf("chargeType", CHARGE_TYPE.Destation), LabelText("垂直冲锋速度"), LabelWidth(120)]
        public float verMSpeed;
        [ToggleGroup("Enabled"), ShowIf("chargeType", CHARGE_TYPE.Time), LabelText("垂直冲锋时间"), LabelWidth(120)]
        public float verMdDration;
        [ToggleGroup("Enabled"), LabelText("垂直冲锋曲线"), LabelWidth(120)]
        public AnimationCurve verMoveCure;
        [ToggleGroup("Enabled"), ShowIf("chargeType", CHARGE_TYPE.Destation), LabelText("水平冲锋速度"), LabelWidth(120)]
        public float horMoveSpeed;
        [ToggleGroup("Enabled"), ShowIf("chargeType", CHARGE_TYPE.Time), LabelText("水平冲锋时间"), LabelWidth(120)]
        public float horMovedDration;
        [ToggleGroup("Enabled"), LabelText("水平冲锋曲线"), LabelWidth(120)]
        public AnimationCurve horMoveCure;
        
        public override Color GetColor()
        {
            return Colors.ImmpulseFrame;
        }
    }
}
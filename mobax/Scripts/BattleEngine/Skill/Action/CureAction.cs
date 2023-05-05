using UnityEngine;

namespace BattleEngine.Logic
{
    public class CureActionAbility : ActionAbility<CureAction> { }

    /// <summary>
    /// 治疗行动
    /// </summary>
    public class CureAction : ActionExecution<CureActionAbility>
    {
        public CureEffect CureEffect { get; set; }
        //治疗数值
        public int CureValue { get; set; }
        //产生的仇恨值
        public float OTNum { get; set; }
        //产生伤害的能力体
        public AbilityEntity abilityEntity { get; set; }
        public SkillRow cureSkillRow;

        public BehitData behitData = new BehitData();

        //伤害倍率
        public float CureRatio { get; set; }

        private int ParseCure()
        {
            return Mathf.FloorToInt(CureRatio * BattleUtil.DoDamage(Creator, Target, behitData, cureSkillRow));
        }

        //前置处理
        private void PreProcess()
        {
            CureValue = 1;
            if (CureEffect != null)
            {
                CureValue = ParseCure();
                CureValue = Mathf.Max(1, CureValue);
            }
            Creator.TriggerActionPoint(ACTION_POINT_TYPE.PreGiveCure, this);
            Target.TriggerActionPoint(ACTION_POINT_TYPE.PreReceiveCure, this);
        }

        public void ParseOT()
        {
            OTNum = 0;
            if (abilityEntity != null)
            {
                var abEntity = abilityEntity as SkillAbility;
                if (abEntity != null)
                {
                    OTNum += abEntity.SkillBaseConfig.skillTaunte;
                    OTNum += CureValue * abEntity.SkillBaseConfig.valueCoefficient * 0.01f;
                }
            }
            Creator.MaxOTNum.AddFinalAddModifier(new FloatModifier() { Value = OTNum });
        }

        public void ApplyCure()
        {
            PreProcess();
            ParseOT();
            Target.ReceiveCure(this);
            PostProcess();
        }

        //后置处理
        private void PostProcess()
        {
            Creator.TriggerActionPoint(ACTION_POINT_TYPE.PostGiveCure, this);
            Target.TriggerActionPoint(ACTION_POINT_TYPE.PostReceiveCure, this);
            Creator.battleItemInfo.battlePlayerRecord.OP_CureValue += CureValue;
            Target.battleItemInfo.battlePlayerRecord.ReceiveCureValue += CureValue;
            BattleUtil.RefreshOTNumFromCureAction(Target.UID, Creator.UID, Mathf.FloorToInt(OTNum), Creator.isAtker);
            Destroy(this);
        }
    }
}
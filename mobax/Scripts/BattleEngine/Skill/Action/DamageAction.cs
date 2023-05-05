namespace BattleEngine.Logic
{
    using UnityEngine;

    public class DamageActionAbility : ActionAbility<DamageAction> { }

    /// <summary>
    /// 伤害行动
    /// </summary>
    public class DamageAction : ActionExecution
    {
        public DamageEffect DamageEffect { get; set; }
        //伤害来源
        public DamageSource DamageSource { get; set; }
        public SkillRow DamageSkillRow { get; set; }
        //伤害数值
        public int DamageValue { get; set; }
        //耐力属性扣除
        public int VimValue { get; set; }
        //是否是暴击
        public bool IsCritical { get; set; }
        //是否是击杀
        public bool IsOneHitKill { get; set; }
        //伤害倍率
        public float HurtRatio { get; set; }
        //是否来自攻击伤害  TO DO 之后移入DamageSource
        public bool attackDamage { get; set; }
        //是否部件
        public CombatUnitEntity partEntity { get; set; }
        //产生的仇恨值
        public float OTNum { get; set; }
        //产生伤害的能力体
        public AbilityEntity abilityEntity { get; set; }

        public BehitData behitData = new BehitData();

        private int ParseDamage()
        {
            return Mathf.FloorToInt(HurtRatio * BattleUtil.DoDamage(Creator, Target, behitData, DamageSkillRow));
        }

        //前置处理
        private void PreProcess()
        {
            //触发 造成伤害后 行动点
            DamageValue = ParseDamage();
            DamageValue = Mathf.Max(1, DamageValue);
            VimValue = CacVimDeduct(Creator, Target);
            Creator.TriggerActionPoint(ACTION_POINT_TYPE.PreCauseDamage, this);
            Target.TriggerActionPoint(ACTION_POINT_TYPE.PreReceiveDamage, this);
        }

        public int CacVimDeduct(CombatActorEntity caster, CombatActorEntity defender)
        {
            float fAdd = 1 + caster.AttrData.GetValue(AttrType.PHYS) * 0.001f;
            float fJob = 1;
            if (caster.isAnitJob(defender.Job))
            {
                fJob = BattleUtil.GetGlobalK(GlobalK.PhyK_6) * 0.001f;
            }

            float vim = fAdd * fJob;
            if (DamageSkillRow != null)
            {
                vim = vim * DamageSkillRow.stamRate * HurtRatio;
            }
            else
            {
                vim = 0;
            }
            
            return Mathf.FloorToInt(vim);
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
                    OTNum += DamageValue * abEntity.SkillBaseConfig.valueCoefficient * 0.01f;
                    Creator.MaxOTNum.AddFinalAddModifier(new FloatModifier() { Value = OTNum });
                }
            }
        }

        //应用伤害
        public void ApplyDamage()
        {
            if (IsOneHitKill)
            {
                DamageValue = Target.CurrentHealth.Value;
            }
            else
            {
                PreProcess();
            }
            ParseOT();
            Target.ReceiveDamage(this, false);
            PostProcess();
        }

        //后置处理
        private void PostProcess()
        {
            //触发 造成伤害后 行动点
            Creator.TriggerActionPoint(ACTION_POINT_TYPE.PostCauseDamage, this);
            //触发 承受伤害后 行动点
            Target.TriggerActionPoint(ACTION_POINT_TYPE.PostReceiveDamage, this);
            //伤害记录
            Creator.battleItemInfo.battlePlayerRecord.OP_AttackValue += DamageValue;
            Target.battleItemInfo.battlePlayerRecord.ReceiveDamageValue += DamageValue;
            Destroy(this);
        }
    }

    public enum DamageSource
    {
        None = -1,
        Attack = 0, //普攻
        Skill = 1, //小技能
        SPSkill = 2, //大招技能
        Buff = 3, //Buff
    }
}
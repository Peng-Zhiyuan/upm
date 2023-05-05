/* Created:Loki Date:2022-08-31*/

namespace BattleEngine.Logic
{
    /// <summary>
    /// 强化技能伤害
    /// </summary>
    public class BuffSkillDamageLogic : BuffEffectLogic
    {
        private int BuffStackNum = 1;

        public override void InitData(BuffAbility _buffAbility, BuffEffect _buffEffect)
        {
            base.InitData(_buffAbility, _buffEffect);
            BuffStackNum = _buffAbility.StackNum;
        }

        public override void ExecuteLogic()
        {
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.ADD_SKILL_DAMAGE_12)
            {
                buffAbility.SelfActorEntity.AttrData.AddSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM1, buffEffect.Param2 * BuffStackNum);
                buffAbility.SelfActorEntity.AttrData.AddSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM2, buffEffect.Param3 * BuffStackNum);
            }
            else if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.ADD_SKILL_DAMAGE_45)
            {
                buffAbility.SelfActorEntity.AttrData.AddSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM4, buffEffect.Param2 * BuffStackNum);
                buffAbility.SelfActorEntity.AttrData.AddSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM5, buffEffect.Param3 * BuffStackNum);
            }
            else if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.ADD_SKILL_CRIT_DAMAGE)
            {
                buffAbility.SelfActorEntity.AttrData.AddSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.EXTCRIT, buffEffect.Param2 * BuffStackNum);
                buffAbility.SelfActorEntity.AttrData.AddSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.EXTRATE, buffEffect.Param3 * BuffStackNum);
            }
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 技能伤害改变 开始：", buffAbility.SelfActorEntity.ConfigID.ToString(), " ", ((AttrType)buffEffect.Param1).ToString(), " ", (buffEffect.Param2 * BuffStackNum).ToString(), " ", (buffEffect.Param3 * BuffStackNum).ToString()));
        }

        public override void EndLogic()
        {
            if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.ADD_SKILL_DAMAGE_12)
            {
                buffAbility.SelfActorEntity.AttrData.RemoveSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM1, buffEffect.Param2 * BuffStackNum);
                buffAbility.SelfActorEntity.AttrData.RemoveSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM2, buffEffect.Param3 * BuffStackNum);
            }
            else if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.ADD_SKILL_DAMAGE_45)
            {
                buffAbility.SelfActorEntity.AttrData.RemoveSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM4, buffEffect.Param2 * BuffStackNum);
                buffAbility.SelfActorEntity.AttrData.RemoveSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.DAM5, buffEffect.Param3 * BuffStackNum);
            }
            else if (buffEffect.Type == (int)BUFF_EFFECT_TYPE.ADD_SKILL_CRIT_DAMAGE)
            {
                buffAbility.SelfActorEntity.AttrData.RemoveSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.EXTCRIT, buffEffect.Param2 * BuffStackNum);
                buffAbility.SelfActorEntity.AttrData.RemoveSkillAttr(buffEffect.Param1, SKILL_ATTR_TYPE.EXTRATE, buffEffect.Param3 * BuffStackNum);
            }
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 技能伤害改变 结束：", buffAbility.SelfActorEntity.ConfigID.ToString(), " ", ((AttrType)buffEffect.Param1).ToString(), " ", (buffEffect.Param2 * BuffStackNum).ToString(), " ", (buffEffect.Param3 * BuffStackNum).ToString()));
        }
    }
}
using System.Reflection;

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;

    public sealed partial class CombatActorEntity
    {
        public List<SkillPassiveAbility> PassiveSkillLst = new List<SkillPassiveAbility>();

        public void InitPassiveSkill()
        {
            PassiveSkillLst.Clear();
        }

        public void AddPassiveSkillLst(List<SkillRow> passiveLst)
        {
            for (int i = 0; i < passiveLst.Count; i++)
            {
                AddPassiveSkill(passiveLst[i]);
            }
        }

        public void AddPassiveSkill(SkillRow skillRow)
        {
            if (skillRow.skillType == (int)SKILL_TYPE.Passive
                || skillRow.skillType == (int)SKILL_TYPE.STAGE_PASSIVE_SKILL
                || skillRow.skillType == (int)SKILL_TYPE.EXPLORER)
            {
                for (int j = 0; j < skillRow.BuffLsts.Count; j++)
                {
                    if (skillRow.BuffLsts[j].buffId == 0)
                    {
                        continue;
                    }
                    SkillPassiveAbilityData data = new SkillPassiveAbilityData();
                    data.skillRow = skillRow;
                    data.buffTrigger = skillRow.BuffLsts[j];
                    Type skillPassiveType = SkillPassiveAbilityManager.Instance.GetPassiveAbiltiy((BUFF_TRIGGER_TYPE)data.buffTrigger.Trigger);
                    MethodInfo mi = this.GetType().GetMethod("AttachAbility").MakeGenericMethod(new Type[] { skillPassiveType });
                    object[] args = new object[] { data };
                    var passiveSkill = mi.Invoke(this, args);
                    if (passiveSkill == null)
                    {
                        BattleLog.LogError("Cant find BuffTrigger " + data.buffTrigger);
                        continue;
                    }
                    PassiveSkillLst.Add(passiveSkill as SkillPassiveAbility);
                }
            }
            else
            {
                BattleLog.LogError("The Skill ID " + skillRow.SkillID + " is not Passive Skill");
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            for (int i = 0; i < PassiveSkillLst.Count; i++)
            {
                PassiveSkillLst[i].OnDestroy();
            }
            PassiveSkillLst.Clear();
        }

        /// <summary>
        /// 打开触发被动技
        /// </summary>
        public void OpenTriggerPassiveSkill()
        {
            for (int i = 0; i < PassiveSkillLst.Count; i++)
            {
                PassiveSkillLst[i].Enable = true;
            }
        }

        /// <summary>
        /// 禁止触发被动技
        /// </summary>
        public void CloseTriggerPassiveSkill()
        {
            for (int i = 0; i < PassiveSkillLst.Count; i++)
            {
                PassiveSkillLst[i].Enable = false;
            }
        }

        /// <summary>
        /// 是否有全局buff
        /// </summary>
        /// <returns></returns>
        public SkillPassiveAbility GetStagePassiveBuff()
        {
            for (int i = 0; i < PassiveSkillLst.Count; i++)
            {
                if (PassiveSkillLst[i].IsStagePassiveSkill())
                {
                    return PassiveSkillLst[i];
                }
            }
            return null;
        }
    }
}
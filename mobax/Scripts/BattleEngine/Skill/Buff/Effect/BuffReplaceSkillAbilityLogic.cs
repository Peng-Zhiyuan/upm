/* Created:Loki Date:2022-08-31*/

using BattleSystem.ProjectCore;

namespace BattleEngine.Logic
{
    using System.Collections.Generic;

    /// <summary>
    /// 技能替换
    /// </summary>
    public class BuffReplaceSkillAbilityLogic : BuffEffectLogic
    {
        private Dictionary<uint, SkillAbility> replaceSkillDic = new Dictionary<uint, SkillAbility>();

        public override void ExecuteLogic()
        {
            if (!Battle.HasInstance()
                || Battle.Instance.param.mode == BattleModeType.SkillView)
            {
                return;
            }
            StrBuild.Instance.ClearSB();
            StrBuild.Instance.Append("[BUFF] 技能替换 ");
            int replaceSkillID = 0;
            if (buffEffect.Param1 != 0)
            {
                replaceSkillID = ReplaceSkillAbility(buffEffect.Param1);
                if (replaceSkillID > 0)
                {
                    StrBuild.Instance.Append(replaceSkillID.ToString());
                    StrBuild.Instance.Append("  ");
                }
            }
            if (buffEffect.Param2 != 0)
            {
                replaceSkillID = ReplaceSkillAbility(buffEffect.Param2);
                if (replaceSkillID > 0)
                {
                    StrBuild.Instance.Append(replaceSkillID.ToString());
                    StrBuild.Instance.Append("  ");
                }
            }
            if (buffEffect.Param3 != 0)
            {
                replaceSkillID = ReplaceSkillAbility(buffEffect.Param3);
                if (replaceSkillID > 0)
                {
                    StrBuild.Instance.Append(replaceSkillID.ToString());
                    StrBuild.Instance.Append("  ");
                }
            }
            BattleLog.LogWarning(StrBuild.Instance.GetString());
        }

        public override void EndLogic()
        {
            if (!Battle.HasInstance()
                || Battle.Instance.param.mode == BattleModeType.SkillView)
            {
                return;
            }
            if (buffEffect.Param1 != 0)
            {
                buffAbility.SelfActorEntity.SkillSlots.Remove((uint)buffEffect.Param1);
            }
            if (buffEffect.Param2 != 0)
            {
                buffAbility.SelfActorEntity.SkillSlots.Remove((uint)buffEffect.Param2);
            }
            if (buffEffect.Param3 != 0)
            {
                buffAbility.SelfActorEntity.SkillSlots.Remove((uint)buffEffect.Param3);
            }
            var data = replaceSkillDic.GetEnumerator();
            while (data.MoveNext())
            {
                if (buffAbility.SelfActorEntity.SkillSlots.ContainsKey(data.Current.Key))
                {
                    continue;
                }
                buffAbility.SelfActorEntity.SkillSlots.Add(data.Current.Key, data.Current.Value);
                if (data.Current.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL)
                {
                    buffAbility.SelfActorEntity.SPSKL = data.Current.Value;
                }
                else if (data.Current.Value.SkillBaseConfig.skillType == (int)SKILL_TYPE.SSPMove)
                {
                    buffAbility.SelfActorEntity.SSPMove = data.Current.Value;
                }
            }
            BattleLog.LogWarning(StrBuild.Instance.ToStringAppend("[BUFF] 技能替换结束 "));
        }

        private int ReplaceSkillAbility(int skillID)
        {
            if (buffAbility.SelfActorEntity.SkillSlots.ContainsKey((uint)skillID))
            {
                return -1;
            }
            SkillAbility replaceSkillAbility = null;
            SkillRow skillRow = SkillUtil.GetSkillItem(skillID, 1);
            var data = buffAbility.SelfActorEntity.SkillSlots.GetEnumerator();
            while (data.MoveNext())
            {
                if (skillRow.skillType == data.Current.Value.SkillBaseConfig.skillType)
                {
                    SkillRow addSkill = SkillUtil.GetSkillItem(skillID, data.Current.Value.SkillBaseConfig.Level);
                    buffAbility.SelfActorEntity.AttachSkill(addSkill);
                    replaceSkillAbility = data.Current.Value;
                    break;
                }
            }
            if (replaceSkillAbility != null)
            {
                replaceSkillDic.Add((uint)replaceSkillAbility.SkillBaseConfig.SkillID, replaceSkillAbility);
                buffAbility.SelfActorEntity.SkillSlots.Remove((uint)replaceSkillAbility.SkillBaseConfig.SkillID);
                return replaceSkillAbility.SkillBaseConfig.SkillID;
            }
            else
            {
                buffAbility.SelfActorEntity.AttachSkill(skillRow);
            }
            return -1;
        }
    }
}
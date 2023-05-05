/* Created:Loki Date:2023-04-12*/

#if !SERVER
using BattleEngine.View;
#endif

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class SkillAbilityHelp : Singleton<SkillAbilityHelp>
    {
        private Dictionary<SKILL_ACTION_ELEMENT_TYPE, Type> skillTaskDataDic = new Dictionary<SKILL_ACTION_ELEMENT_TYPE, Type>();

        private void InitAbilityTaskDataDic()
        {
            skillTaskDataDic.Clear();
            List<Type> abilityLst = ReflectUtil.FromAssemblyGetTypes(typeof(AbilityTaskData), "BattleEngine.Logic");
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                List<Type> abilityViewLst = ReflectUtil.FromAssemblyGetTypes(typeof(AbilityTaskData), "BattleEngine.View");
                abilityLst.AddRange(abilityViewLst);
            }
#endif
            for (int i = 0; i < abilityLst.Count; i++)
            {
                object obj = Activator.CreateInstance(abilityLst[i]);
                MethodInfo miGetName = abilityLst[i].GetMethod("GetSkillActionElementType");
                string eventName = miGetName.Invoke(obj, null).ToString();
                SKILL_ACTION_ELEMENT_TYPE triggerType = (SKILL_ACTION_ELEMENT_TYPE)Enum.Parse(typeof(SKILL_ACTION_ELEMENT_TYPE), eventName);
                if (skillTaskDataDic.ContainsKey(triggerType))
                {
                    continue;
                }
                skillTaskDataDic.Add(triggerType, abilityLst[i]);
            }
        }

        public Type GetAbilityTaskData(SKILL_ACTION_ELEMENT_TYPE type)
        {
            if (skillTaskDataDic.Count == 0)
            {
                InitAbilityTaskDataDic();
            }
            if (skillTaskDataDic.ContainsKey(type))
            {
                return skillTaskDataDic[type];
            }
            return null;
        }
    }
}
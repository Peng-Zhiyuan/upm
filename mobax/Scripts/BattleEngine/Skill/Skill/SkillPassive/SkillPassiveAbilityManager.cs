/* Created:Loki Date:2023-01-31*/

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class SkillPassiveAbilityManager : Singleton<SkillPassiveAbilityManager>
    {
        private Dictionary<BUFF_TRIGGER_TYPE, Type> skillPassiveAbilityDic = new Dictionary<BUFF_TRIGGER_TYPE, Type>();

        private void InitPassiveDic()
        {
            skillPassiveAbilityDic.Clear();
            List<Type> abilityLst = ReflectUtil.FromAssemblyGetTypes(typeof(SkillPassiveAbility), "BattleEngine.Logic");
            for (int i = 0; i < abilityLst.Count; i++)
            {
                object obj = Activator.CreateInstance(abilityLst[i]);
                MethodInfo miGetName = abilityLst[i].GetMethod("GetPassiveType");
                string eventName = miGetName.Invoke(obj, null).ToString();
                BUFF_TRIGGER_TYPE triggerType = (BUFF_TRIGGER_TYPE)Enum.Parse(typeof(BUFF_TRIGGER_TYPE), eventName);
                if (skillPassiveAbilityDic.ContainsKey(triggerType))
                {
                    continue;
                }
                skillPassiveAbilityDic.Add(triggerType, abilityLst[i]);
                (obj as SkillPassiveAbility).OnDestroy();
            }
        }

        public override void Dispose()
        {
            skillPassiveAbilityDic.Clear();
        }

        public Type GetPassiveAbiltiy(BUFF_TRIGGER_TYPE triggerType)
        {
            if (skillPassiveAbilityDic.Count == 0)
            {
                InitPassiveDic();
            }
            if (skillPassiveAbilityDic.ContainsKey(triggerType))
            {
                return skillPassiveAbilityDic[triggerType];
            }
            return null;
        }
    }
}
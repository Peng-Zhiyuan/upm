using System.Collections.Generic;

namespace BattleEngine.Logic
{
    public sealed class BattleSkillFindRoleChooseTargetActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "SkillFindRoleChooseTargetActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleSkillEventData data = obj as BattleSkillEventData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (string.IsNullOrEmpty(actorEntity.targetKey))
            {
                return;
            }
            CombatActorEntity targetEntity = battleData.allActorDic[actorEntity.targetKey];
            actorEntity.SetSkillTargetInfos(new List<CombatActorEntity>() { targetEntity });
        }
    }
}
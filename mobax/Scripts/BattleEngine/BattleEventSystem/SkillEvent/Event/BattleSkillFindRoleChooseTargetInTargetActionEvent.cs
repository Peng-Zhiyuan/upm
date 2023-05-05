using System.Collections.Generic;

namespace BattleEngine.Logic
{
    public sealed class BattleSkillFindRoleChooseTargetInTargetActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "SkillFindRoleChooseTargetInTargetActionEvent";
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
            if (string.IsNullOrEmpty(targetEntity.targetKey))
            {
                return;
            }
            CombatActorEntity targetInTargetEntity = battleData.allActorDic[targetEntity.targetKey];
            actorEntity.SetSkillTargetInfos(new List<CombatActorEntity>() { targetInTargetEntity });
        }
    }
}
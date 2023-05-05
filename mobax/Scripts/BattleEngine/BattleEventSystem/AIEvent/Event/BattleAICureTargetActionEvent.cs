namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;
    
    public sealed class BattleAICureTargetActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AICureTargetActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (!string.IsNullOrEmpty(actorEntity.targetKey))
            {
                CombatActorEntity targetEntity = battleData.allActorDic[actorEntity.targetKey];
                if (targetEntity.CurrentHealth.Value <= 0)
                {
                    actorEntity.ClearTargetInfo();
                }
            }
            List<CombatActorEntity> targetActors = battleData.GetTeamList(actorEntity.TeamKey);
            float ratio = 1;
            CombatActorEntity expectActor = null;
            for (int i = 0; i < targetActors.Count; i++)
            {
                CombatActorEntity targetActor = targetActors[i];
                if (expectActor == null
                    && targetActor.CurrentHealth.Value > 0)
                {
                    expectActor = targetActor;
                }
                if (targetActor.CurrentHealth.Value >= targetActor.CurrentHealth.MaxValue
                    || targetActor.CurrentHealth.Value <= 0)
                {
                    continue;
                }
                float tempRatio = targetActor.CurrentHealth.Percent();
                if (tempRatio < ratio)
                {
                    ratio = tempRatio;
                    expectActor = targetActor;
                }
            }
            if (expectActor != null)
            {
                Vector3 forward = expectActor.GetPositionXZ() - actorEntity.GetPositionXZ();
                //actorEntity.KinematControl.Turn(forward.normalized, BattleConst.MoveAnagleOffset);
                actorEntity.SetForward(forward.normalized);
                actorEntity.SetAutoTargetInfo(expectActor);
            }
            else
            {
                // actorEntity.ClearTargetInfo();
                // actorEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
            }
        }
    }
}
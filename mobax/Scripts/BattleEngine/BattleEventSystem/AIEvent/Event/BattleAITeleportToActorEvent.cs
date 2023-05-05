namespace BattleEngine.Logic
{
    using UnityEngine;
    using System.Collections.Generic;

    public sealed class BattleAITeleportToActorEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "BattleAITeleportToActorEvent";
        }

        private Vector3 targetPosXZ = Vector3.zero;
        private Vector3 targetCrossPos = Vector3.zero;
        private List<CombatActorEntity> neighborList = new List<CombatActorEntity>();

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            actorEntity.SetActionState(ACTOR_ACTION_STATE.Move);
            CombatActorEntity targetActor = BattleUtil.GetTargetActorEntity(battleData, actorEntity);
            if (targetActor == null
                || targetActor.IsCantSelect)
            {
                actorEntity.ClearTargetInfo();
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
                actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.None;
                return;
            }
            float atkDis = actorEntity.GetCurrentAtkDistance();
            targetPosXZ = targetActor.GetPositionXZ();
            Vector3 forwardDir = (actorEntity.GetPositionXZ() - targetPosXZ).normalized;
            forwardDir.Normalize();
            targetCrossPos = targetPosXZ + forwardDir * (atkDis + targetActor.GetTouchRadiu() + actorEntity.GetTouchRadiu());
            var allActorData = battleData.allActorDic.GetEnumerator();
            List<CombatActorEntity> teamList = new List<CombatActorEntity>();
            while (allActorData.MoveNext())
            {
                if (allActorData.Current.Value.UID == actorEntity.UID
                    || allActorData.Current.Value.isAtker != actorEntity.isAtker)
                {
                    continue;
                }
                teamList.Add(allActorData.Current.Value);
            }
            Vector3 targetDir = targetActor.GetPositionXZ() - targetCrossPos;
            targetDir.y = 0;
            targetDir = targetDir.normalized;
            float angle = 0.0f;
            Quaternion pianyi = Quaternion.identity;
            Vector3 dir = Vector3.zero;
            float disLimit = 0.0f;
            float memberDis = 0.0f;
            float memberTargetPosDis = 0.0f;
            bool posUsed = false;
            for (int j = 0; j < 8; j++)
            {
                angle = 45 * j;
                pianyi = Quaternion.Euler(0, angle, 0);
                dir = (pianyi * targetDir).normalized;
                targetCrossPos = targetActor.GetPositionXZ() + dir * (atkDis + targetActor.GetTouchRadiu() + actorEntity.GetTouchRadiu());
                posUsed = false;
                for (int i = 0; i < teamList.Count; i++)
                {
                    disLimit = teamList[i].GetAiLocRadiu() + actorEntity.GetAiLocRadiu();
                    memberDis = MathHelper.DoubleDistanceVect3(targetCrossPos, teamList[i].GetPositionXZ());
                    memberTargetPosDis = MathHelper.DoubleDistanceVect3(targetCrossPos, teamList[i].targetPos);
                    if (disLimit * disLimit >= memberDis
                        || disLimit * disLimit >= memberTargetPosDis)
                    {
                        posUsed = true;
                        break;
                    }
                }
                if (posUsed)
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            if (!BattleUtil.IsInMap(targetCrossPos))
            {
                targetCrossPos.y = BattleUtil.GetWalkablePos(targetCrossPos).y;
            }
            actorEntity.SetPosition(targetCrossPos);
        }
    }
}
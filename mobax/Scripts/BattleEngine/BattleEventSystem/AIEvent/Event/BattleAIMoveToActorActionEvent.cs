namespace BattleEngine.Logic
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    public sealed class BattleAIMoveToActorActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIMoveToActorActionEvent";
        }

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
            if (actorEntity.CurrentHealth.Value <= 0)
            {
                return;
            }
            if (BattleControlUtil.IsForbidMove(actorEntity, false))
            {
                actorEntity.SetTargetPos(actorEntity.GetPosition());
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
                return;
            }
            CombatActorEntity targetActor = BattleUtil.GetTargetActorEntity(battleData, actorEntity, true);
            if (targetActor == null)
            {
                targetActor = battleData.GetNearestTarget(actorEntity);
            }
            actorEntity.SetAutoTargetInfo(targetActor);
            if (targetActor == null
                || targetActor.CurrentHealth.Value <= 0)
            {
                actorEntity.ClearTargetInfo();
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
                actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.None;
                return;
            }
            float atkDis = actorEntity.GetCurrentAtkDistance();
            if (targetActor.Action_actorActionState == ACTOR_ACTION_STATE.Move)
            {
                Vector3 forwardDir = (targetActor.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
                forwardDir.Normalize();
                targetCrossPos = actorEntity.GetPositionXZ() + forwardDir * (atkDis + targetActor.GetTouchRadiu() + actorEntity.GetTouchRadiu());
            }
            else
            {
                Vector3 forwardDir = (actorEntity.GetPositionXZ() - targetActor.GetPositionXZ()).normalized;
                forwardDir.Normalize();
                targetCrossPos = targetActor.GetPositionXZ() + forwardDir * (atkDis + targetActor.GetTouchRadiu() + actorEntity.GetTouchRadiu());
            }
#if !Server
            ///底层验证为了效率抛弃站位计算
            if (BattleUtil.IsInAttackRange(battleData, actorEntity, targetActor))
            {
                neighborList = GetNeighborList(battleData, actorEntity, targetActor);
                if (neighborList.Count > 0)
                {
                    Vector3 targetPos = BattleUtil.GetCheckTargetPos(battleData, actorEntity, targetActor, 0);
                    if (!BattleUtil.IsInMap(targetPos))
                    {
                        targetPos.y = BattleUtil.GetWalkablePos(targetPos).y;
                    }
                    actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.Move;
                    actorEntity.SetActionState(ACTOR_ACTION_STATE.Move);
                    actorEntity.SetTargetPos(targetPos);
                    actorEntity.LastInputMoveTime = 0;
                    battleData.PushActorOccupyVector(actorEntity);
                    return;
                }
            }
#endif
            float dis1 = MathHelper.ActorDistance(actorEntity, targetActor);
            float dis2 = MathHelper.DoubleDistanceVect3(targetCrossPos, targetActor.GetPositionXZ());
            if (Math.Round(dis1 * dis1, 2) <= Math.Round(dis2, 2)
                && neighborList.Count == 0)
            {
                actorEntity.SetTargetPos(actorEntity.GetPosition());
            }
            else
            {
                if (!BattleUtil.IsInMap(targetCrossPos))
                {
                    targetCrossPos.y = BattleUtil.GetWalkablePos(targetCrossPos).y;
                }
                actorEntity.SetTargetPos(targetCrossPos);
            }
            actorEntity.SetForward((actorEntity.targetPosXZ - actorEntity.GetPositionXZ()).normalized);
            actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.None;
            actorEntity.SetActionState(ACTOR_ACTION_STATE.Move);
            battleData.PushActorOccupyVector(actorEntity);
        }

        private List<CombatActorEntity> GetNeighborList(BattleData battleData, CombatActorEntity actorEntity, CombatActorEntity targetEntity = null)
        {
            var allActorData = battleData.allActorDic.GetEnumerator();
            List<CombatActorEntity> _neighborList = new List<CombatActorEntity>();
            float distanc = 0.0f;
            float distanceLimit = 0.0f;
            while (allActorData.MoveNext())
            {
                if (allActorData.Current.Value.UID == actorEntity.UID
                    || allActorData.Current.Value.CurrentHealth.Value <= 0
                    || allActorData.Current.Value.Action_actorActionState == ACTOR_ACTION_STATE.Move)
                {
                    continue;
                }
                distanceLimit = allActorData.Current.Value.GetAiLocRadiu() + actorEntity.GetAiLocRadiu();
                distanc = MathHelper.ActorDistance(actorEntity, allActorData.Current.Value);
                if ((Math.Round(distanc, 2) <= Math.Round(distanceLimit, 2))
                    || float.IsNaN(distanc))
                {
                    if (battleData.IsCalActorOccupy(allActorData.Current.Value))
                    {
                        _neighborList.Add(allActorData.Current.Value);
                        break;
                    }
                }
            }
            return _neighborList;
        }
    }
}
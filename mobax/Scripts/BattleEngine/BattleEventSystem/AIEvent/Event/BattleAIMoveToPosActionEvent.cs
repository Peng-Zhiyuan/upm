namespace BattleEngine.Logic
{
    using UnityEngine;
    using System;

    public sealed class BattleAIMoveToPosActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIMoveToPosActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            if (!battleData.allActorDic.ContainsKey(data.actorUID))
            {
                return;
            }
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0
                || BattleControlUtil.IsForbidMove(actorEntity, true)
                || actorEntity.AttrData.Att_Move == 0)
            {
                return;
            }
            actorEntity.SetTargetPos(actorEntity.targetPos);
            Vector3 forward = actorEntity.targetPosXZ - actorEntity.GetPositionXZ();
            actorEntity.SetForward(forward.normalized);
            float dis = MathHelper.DoubleDistanceVect3(actorEntity.GetPositionXZ(), actorEntity.targetPosXZ);
            if (Math.Round(dis, 2) <= 0.01f)
            {
                actorEntity.LastInputMoveTime = BattleTimeManager.Instance.NowTimestamp;
                actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.None;
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
            }
            else
            {
                actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.Move;
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Move);
            }
        }
    }
}
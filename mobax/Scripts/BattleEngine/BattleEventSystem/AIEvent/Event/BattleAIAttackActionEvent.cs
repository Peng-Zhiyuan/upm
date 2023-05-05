namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class BattleAIAttackActionEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "AIAttackActionEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleAIOperaterData data = obj as BattleAIOperaterData;
            CombatActorEntity actorEntity = battleData.allActorDic[data.actorUID];
            if (actorEntity.CurrentHealth.Value <= 0
                || actorEntity.IsNotOperable)
            {
                return;
            }
            CombatActorEntity targetEntity = BattleUtil.GetTargetActorEntity(battleData, actorEntity);
            if (targetEntity == null
                || targetEntity.CurrentHealth.Value <= 0)
            {
                actorEntity.ClearTargetParts();
                return;
            }
            if (targetEntity.IsCantSelect)
            {
                actorEntity.ClearTargetInfo();
                actorEntity.SetActionState(ACTOR_ACTION_STATE.Idle);
            }
            else
            {
                Vector3 forwardDir = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
                //actorEntity.KinematControl.Turn(forwardDir.normalized, actorEntity.AngleSpeed);
                actorEntity.SetForward(forwardDir);
                actorEntity.SetActionState(ACTOR_ACTION_STATE.ATK);
                actorEntity.SetAutoTargetInfo(targetEntity);
            }
            actorEntity.LastInputMoveTime = BattleTimeManager.Instance.NowTimestamp;
            actorEntity.Action_inputOperateType = INPUT_OPERATE_TYPE.None;
            actorEntity.SetTargetPos(actorEntity.GetPosition());
        }
    }
}
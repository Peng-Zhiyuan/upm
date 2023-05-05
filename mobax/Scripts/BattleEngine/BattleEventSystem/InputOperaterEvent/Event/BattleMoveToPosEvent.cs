/* Created:Loki Date:2022-09-23*/

using UnityEngine;

namespace BattleEngine.Logic
{
    public sealed class BattleMoveToPosEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "MoveToPosEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity targetActor = battleData.allActorDic[data.targetID];
            if (BattleControlUtil.IsForbidMove(targetActor, true))
            {
                return;
            }
            if (targetActor.CurrentHealth.Value >= 0
                && targetActor.Action_actorActionState != ACTOR_ACTION_STATE.Dead)
            {
                System.Action action = delegate()
                {
                    targetActor.ClearTargetInfo();
                    targetActor.Action_inputOperateType = INPUT_OPERATE_TYPE.Move;
                    targetActor.SetActionState(ACTOR_ACTION_STATE.Move);
                    Vector3 targetPosition = data.targetPos;
                    if (!BattleUtil.IsInMap(targetPosition))
                    {
                        targetPosition = BattleUtil.GetWalkablePos(targetPosition);
                    }
                    targetActor.SetTargetPos(targetPosition);
                    targetActor.LastInputMoveTime = 0;
                };
                if (targetActor.CurrentSkillExecution != null
                    && !targetActor.CurrentSkillExecution.SkillAbility.SkillConfigObject.CanbeDragged)
                {
                    targetActor.CurrentSkillExecution.BreakActions(data.SkillBreakCauseType, false, action);
                }
                else
                {
                    action();
                }
            }
        }
    }
}
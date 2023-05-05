/* Created:Loki Date:2022-09-27*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class BattleApplyFriendToBattleEvent : IBattleEvent
    {
        public string GetEventName()
        {
            return "ApplyFriendToBattleEvent";
        }

        public void Excute(BattleData battleData, object obj)
        {
            BattleInputOperaterData data = obj as BattleInputOperaterData;
            CombatActorEntity chooseActor = battleData.GetActorEntity(data.originID);
            CombatActorEntity friendActor = battleData.GetActorEntity(data.targetID);
            if (chooseActor == null
                || friendActor == null)
            {
                return;
            }
            Vector3 targetPos = Vector3.zero;
            if (!string.IsNullOrEmpty(chooseActor.targetKey))
            {
                CombatActorEntity targetActor = BattleLogicManager.Instance.BattleData.GetActorEntity(chooseActor.targetKey);
                targetPos = targetActor.GetPosition();
            }
            else
            {
                targetPos = BattleUtil.GetCheckTargetPos(BattleLogicManager.Instance.BattleData, friendActor, chooseActor, friendActor.SPSKL.SkillBaseConfig.Range * 0.01f);
            }
            if (!BattleUtil.IsInMap(targetPos))
            {
                Vector3 recpacePos = BattleUtil.GetWalkablePos(targetPos);
                targetPos.y = recpacePos.y;
            }
            LogicFrameTimerMgr.Instance.ScheduleTimer(1.3f, delegate
                            {
                                if (friendActor.CurrentSkillExecution != null)
                                {
                                    friendActor.BreakCurentSkillImmediate();
                                }
                                friendActor.CurrentMp.Add(friendActor.CurrentMp.MaxValue);
                                friendActor.SetLifeState(ACTOR_LIFE_STATE.Alive);
                                friendActor.SetPosition(targetPos);
                                friendActor.SetEulerAngles(friendActor.BornRot);
                                friendActor.ClearTargetInfo();
                                BattleManager.Instance.SendSpendSPSkill(friendActor.UID);
                            }
            );
            int waitTime = (int)BattleUtil.GetGlobalK(GlobalK.Friend_To_Battle_38);
            LogicFrameTimerMgr.Instance.ScheduleTimer(waitTime + 1.3f, () =>
                            {
                                if (friendActor.CurrentSkillExecution != null)
                                {
                                    friendActor.CurrentSkillExecution.BreakActionsImmediate(() => { BattleLogicManager.Instance.SendFriendQuitBattleInputEvent(friendActor.UID); });
                                }
                                else
                                {
                                    BattleLogicManager.Instance.SendFriendQuitBattleInputEvent(friendActor.UID);
                                }
                            }
            );
        }
    }
}
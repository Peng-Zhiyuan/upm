/* Created:Loki Date:2023-01-17*/

namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class BattleLinkerManager : Singleton<BattleLinkerManager>
    {
        public void ExcuteLinkerToBattle(CombatActorEntity linkerActor, CombatActorEntity CreateActor, int linkerIndex)
        {
            if (linkerActor == null
                || CreateActor == null)
            {
                return;
            }
            Vector3 targetPos = GetTargetPos(CreateActor, linkerActor);
            Vector3 beginPos = CreateActor.BornPos - CreateActor.BornPos.normalized * 3;
            Vector3 offsetValue = linkerIndex == 0 ? Vector3.left : Vector3.right;
            Vector3 initPos = beginPos + Quaternion.LookRotation(CreateActor.GetForward().normalized, Vector3.up) * offsetValue;
            linkerActor.SetPosition(initPos);
            linkerActor.SetForward(CreateActor.GetForward());
            LogicFrameTimerMgr.Instance.ScheduleTimer(1.3f, delegate
                            {
                                linkerActor.SetLifeState(ACTOR_LIFE_STATE.Alive);
                                linkerActor.AttachBuff((int)BUFF_COMMON_CONFIG_ID.Linker_Battle_God);
                                linkerActor.ClearTargetInfo();
                                BattleLogicManager.Instance.ActorTurnOnSkill(linkerActor.UID, targetPos);
                            }
            );
        }

        public void ExcuteLinkerQuitBattle(CombatActorEntity linkerActor)
        {
            if (linkerActor == null)
            {
                return;
            }
            linkerActor.OnBuffRemove((int)BUFF_COMMON_CONFIG_ID.Linker_Battle_God);
            linkerActor.ClearTargetInfo();
            linkerActor.SetLifeState(ACTOR_LIFE_STATE.Assist);
            if (linkerActor.CurrentSkillExecution != null)
            {
                linkerActor.CurrentSkillExecution.BreakActionsImmediate();
            }
        }

        public Vector3 GetTargetPos(CombatActorEntity actorEntity, CombatActorEntity linkerActorEntity)
        {
            Vector3 targetPos = Vector3.zero;
            if (!string.IsNullOrEmpty(actorEntity.targetKey))
            {
                CombatActorEntity targetActor = BattleLogicManager.Instance.BattleData.GetActorEntity(actorEntity.targetKey);
                targetPos = targetActor.GetPosition();
            }
            else
            {
                targetPos = BattleUtil.GetCheckTargetPos(BattleLogicManager.Instance.BattleData, linkerActorEntity, actorEntity, 0);
            }
            if (!BattleUtil.IsInMap(targetPos))
            {
                Vector3 recpacePos = BattleUtil.GetWalkablePos(targetPos);
                targetPos.y = recpacePos.y;
            }
            float radius = 1.0f;
            float angle = BattleSBManager.Instance.GetSingleAngleOffset(120);
            Vector3 centerPos = targetPos;
            float x = centerPos.x + radius * Mathf.Cos(angle);
            float z = centerPos.z + radius * Mathf.Sin(angle);
            return new Vector3(x, targetPos.y, z);
        }
    }
}
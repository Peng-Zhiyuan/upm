using PathfindingCore;

namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class BeatBackActionTaskData : AbilityTaskData
    {
        public bool freeDir;
        public float moveDuration = 1;
        public float moveMaxDis = 10;
        public SKILL_BEATBACK_TYPE beatBackType = SKILL_BEATBACK_TYPE.SkillTarget;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.BeatBack;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            BeatBackActionElement actionElement = element as BeatBackActionElement;
            beatBackType = actionElement.beatBackType;
            freeDir = actionElement.freeDir;
            moveDuration = (actionElement.endFrame - actionElement.startFrame) * (BattleLogicDefine.LogicSecTime);
            moveMaxDis = actionElement.moveMaxDis;
        }
    }

    public sealed class SkillBeatBackActionTask : AbilityTask
    {
        private BeatBackActionTaskData _taskData;
        public BeatBackActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as BeatBackActionTaskData;
                }
                return _taskData;
            }
        }
        private CombatActorEntity owner;
        private BuffAbility beatBackBuffAbility;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            owner = SkillAbilityExecution.OwnerEntity;
            if (TaskData.beatBackType == SKILL_BEATBACK_TYPE.Global)
            {
                List<CombatActorEntity> lst = BattleLogicManager.Instance.BattleData.GetEnemyLst(owner);
                for (int i = 0; i < lst.Count; i++)
                {
                    if (lst[i] == null
                        || !BattleControlUtil.RefreshBuffControlState(lst[i], ACTION_CONTROL_TYPE.control_12)
                        || !lst[i].IsCantSelect)
                    {
                        continue;
                    }
                    BeatBackActorEvent(lst[i]);
                }
            }
            else if (TaskData.beatBackType == SKILL_BEATBACK_TYPE.SkillTarget)
            {
                CombatActorEntity targetEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(owner.targetKey);
                if (targetEntity == null
                    || !BattleControlUtil.RefreshBuffControlState(targetEntity, ACTION_CONTROL_TYPE.control_12)
                    || !targetEntity.IsCantSelect)
                {
                    BeatBackActorEvent(targetEntity);
                }
            }
        }

        private void BeatBackActorEvent(CombatActorEntity targetEntity)
        {
            if (targetEntity == null
                || !BattleControlUtil.RefreshBuffControlState(targetEntity, ACTION_CONTROL_TYPE.control_12))
            {
                return;
            }
            targetEntity.BreakCurentSkillImmediate(() =>
                            {
                                Vector3 moveForward = Vector3.zero;
                                moveForward = (targetEntity.GetPositionXZ() - owner.GetPositionXZ()).normalized;
                                Vector3 endPosition = targetEntity.GetPositionXZ() + moveForward * TaskData.moveMaxDis;
                                if (!PathUtil.IsWalkable(PathFindingManager.Instance.AstarPathCore, endPosition))
                                {
                                    endPosition = PathUtil.GetNearestWalkablePos(PathFindingManager.Instance.AstarPathCore, endPosition);
                                }
                                SkillCtr.SkillActorBeatBack(targetEntity, endPosition, TaskData.moveDuration, null);
                                beatBackBuffAbility = targetEntity.AttachBuff((int)BUFF_COMMON_CONFIG_ID.BEAT_BACK);
                                beatBackBuffAbility.SetBuffTime(TaskData.moveDuration);
                            }
            );
        }
    }
}
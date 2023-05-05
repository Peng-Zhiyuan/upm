namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class MoveActionTaskData : AbilityTaskData
    {
        public SKILL_MOVE_TARGET_TYPE MoveTargetType = SKILL_MOVE_TARGET_TYPE.SkillTarget;
        public MOVE_MODE_TYPE MoveModeType = MOVE_MODE_TYPE.Speed;
        public float immediateOffsetDistance = 0.0f;
        public float immediateOffsetY = 0.0f;

        public MOVE_DIR_TYPE moveDir;
        public float moveSpeed = 1;
        public float moveMaxDis = 10;
        public bool lookAt = false;
        public bool isBackStart = false;

        public Vector3 InputTargetPos;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Move;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            MoveActionElement actionElement = element as MoveActionElement;
            MoveTargetType = actionElement.MoveTargetType;
            MoveModeType = actionElement.MoveModeType;
            immediateOffsetDistance = actionElement.immediateOffsetDistance;
            immediateOffsetY = actionElement.immediateOffsetY;
            moveDir = actionElement.moveDir;
            moveMaxDis = actionElement.moveMaxDis;
            moveSpeed = actionElement.moveSpeed;
            isBackStart = actionElement.IsBackStart;
        }
    }

    public sealed class SkillMoveActionTask : AbilityTask
    {
        private MoveActionTaskData _taskData;
        public MoveActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as MoveActionTaskData;
                }
                return _taskData;
            }
        }

        private CombatActorEntity actorEntity;

        private CombatActorEntity targetEntity;
        private Vector3 beginPos = Vector3.zero;
        private Vector3 targetPos = Vector3.zero;
        private Vector3 targetPosXZ = Vector3.zero;
        private bool isImmediateMove = false;

        private float battleDelta = 0.0f;
        private Vector3 moveDelta = Vector3.zero;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            actorEntity = SkillAbilityExecution.OwnerEntity;
            battleDelta = SkillAbilityExecution.SkillAbility.SkillFrameRate;
            moveDelta = Vector3.zero;
            targetEntity = null;
            if (TaskData.MoveTargetType == SKILL_MOVE_TARGET_TYPE.SkillTarget
                || TaskData.MoveTargetType == SKILL_MOVE_TARGET_TYPE.SkillTargetMax
                || TaskData.MoveTargetType == SKILL_MOVE_TARGET_TYPE.SkillTargetMin && !string.IsNullOrEmpty(actorEntity.targetKey))
            {
                targetEntity = BattleLogicManager.Instance.BattleData.GetActorEntity(actorEntity.targetKey);
                if (targetEntity != null)
                {
                    Vector3 dir = (targetEntity.GetPositionXZ() - actorEntity.GetPositionXZ()).normalized;
                    targetPos = targetEntity.GetPosition() + dir * TaskData.immediateOffsetDistance + new Vector3(0, TaskData.immediateOffsetY, 0);
                }
                else
                {
                    targetPos = actorEntity.GetPosition() + actorEntity.GetForward() * TaskData.immediateOffsetDistance + new Vector3(0, TaskData.immediateOffsetY, 0);
                }
            }
            else if (TaskData.MoveTargetType == SKILL_MOVE_TARGET_TYPE.Self)
            {
                targetPos = actorEntity.GetPosition() + actorEntity.GetForward() * TaskData.immediateOffsetDistance + new Vector3(0, TaskData.immediateOffsetY, 0);
            }
            else if (TaskData.MoveTargetType == SKILL_MOVE_TARGET_TYPE.MoveInputPos)
            {
                targetPos = TaskData.InputTargetPos;
            }
            else
            {
                targetPos = actorEntity.GetPosition();
            }
            if (!BattleUtil.IsInMap(targetPos)
                && !TaskData.isBackStart)
            {
                targetPos.y = BattleUtil.GetWalkablePos(targetPos).y;
                if (TaskData.immediateOffsetY > 0)
                {
                    targetPos.y += TaskData.immediateOffsetY;
                }
            }
            targetPosXZ = new Vector3(targetPos.x, 0, targetPos.z);
            if (targetPos != Vector3.zero
                && TaskData.MoveModeType == MOVE_MODE_TYPE.TargetPos)
            {
                float distance = MathHelper.DistanceVec3(targetPosXZ, actorEntity.GetPositionXZ());
                int offsetFrame = TaskData.endFrame - TaskData.startFrame;
                Vector3 dir = (targetPosXZ - actorEntity.GetPositionXZ()).normalized;
                moveDelta = dir * (distance / offsetFrame);
                moveDelta.y = 0;
            }
            if (TaskData.isBackStart)
            {
                beginPos = actorEntity.GetPosition();
            }
            else
            {
                beginPos = Vector3.zero;
            }
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            if (TaskData.MoveModeType == MOVE_MODE_TYPE.Speed)
            {
                switch (TaskData.moveDir)
                {
                    case MOVE_DIR_TYPE.Back:
                        if (TaskData.lookAt)
                        {
                            actorEntity.SetForward((actorEntity.GetPositionXZ() - 1 * actorEntity.GetForward()).normalized);
                        }
                        moveDelta = -1 * actorEntity.GetForward() * TaskData.moveSpeed * battleDelta;
                        break;
                    case MOVE_DIR_TYPE.Froward:
                        if (TaskData.lookAt)
                        {
                            actorEntity.SetForward((actorEntity.GetPositionXZ() + 1 * actorEntity.GetForward()).normalized);
                        }
                        moveDelta = actorEntity.GetForward() * TaskData.moveSpeed * battleDelta;
                        break;
                    case MOVE_DIR_TYPE.Left:
                        if (TaskData.lookAt)
                        {
                            actorEntity.SetForward((actorEntity.GetPositionXZ() - 1 * actorEntity.GetRight()).normalized);
                        }
                        moveDelta = -1 * actorEntity.GetRight() * TaskData.moveSpeed * battleDelta;
                        break;
                    case MOVE_DIR_TYPE.Right:
                        if (TaskData.lookAt)
                        {
                            actorEntity.SetForward((actorEntity.GetPositionXZ() + 1 * actorEntity.GetRight()).normalized);
                        }
                        moveDelta = actorEntity.GetRight() * TaskData.moveSpeed * battleDelta;
                        break;
                }
                SkillCtr.SkillActorMove(SkillAbilityExecution.OwnerEntity, moveDelta, battleDelta);
            }
            else if (TaskData.MoveModeType == MOVE_MODE_TYPE.Immediate
                     && !isImmediateMove)
            {
                Vector3 forward = (targetPosXZ - actorEntity.GetPositionXZ()).normalized;
                actorEntity.SetForward(forward);
                SkillCtr.SkillSetActorMove(actorEntity, targetPos);
                isImmediateMove = true;
            }
            else if (TaskData.MoveModeType == MOVE_MODE_TYPE.TargetPos)
            {
                Vector3 forward = (targetPosXZ - actorEntity.GetPositionXZ()).normalized;
                actorEntity.SetForward(forward);
                SkillCtr.SkillActorMove(actorEntity, moveDelta, battleDelta);
            }
        }

        public override void BreakExecute(int frameIdx)
        {
            if (TaskData.isBackStart)
            {
                SkillCtr.SkillSetActorMove(actorEntity, beginPos);
            }
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            if (TaskData.isBackStart)
            {
                SkillCtr.SkillSetActorMove(actorEntity, beginPos);
            }
            base.EndExecute();
        }
    }
}
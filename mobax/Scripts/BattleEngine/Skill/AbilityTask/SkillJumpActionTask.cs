namespace BattleEngine.Logic
{
    using UnityEngine;
    using System.Collections.Generic;

    public sealed class JumpTaskData : AbilityTaskData
    {
        public SKILL_JUMP_TARGET_TYPE jumpType;
        public float verHeight;
        public float verDistance;
        public AnimationCurve verCure;
        public AnimationCurve horCure;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Jump;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            JumpActionElement actionElement = element as JumpActionElement;
            jumpType = actionElement.MoveTargetType;
            verHeight = actionElement.verHeight;
            verDistance = actionElement.verDistance;
            verCure = actionElement.verCure;
            horCure = actionElement.horCure;
        }
    }

    public sealed class SkillJumpActionTask : AbilityTask
    {
        private JumpTaskData _taskData;
        public JumpTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as JumpTaskData;
                }
                return _taskData;
            }
        }
        private CombatActorEntity Creator;
        private CombatActorEntity target;
        private Vector3 moveDelta;
        private float delta = 0.0f;
        private float speed = 0.0f;
        private Vector3 dir = Vector3.zero;
        private int frameOffset = 0;
        private List<Vector3> moveList = new List<Vector3>();
        private int curFrame = 0;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            Creator = SkillAbilityExecution.OwnerEntity;
            target = SkillAbilityExecution.targetActorEntity;
            delta = SkillAbilityExecution.SkillAbility.SkillFrameRate;
            frameOffset = TaskData.endFrame - TaskData.startFrame;
            moveDelta = Vector3.zero;
            if (TaskData.jumpType == SKILL_JUMP_TARGET_TYPE.SkillTarget)
            {
                float distance = MathHelper.ActorDistance(Creator, target) - (target.GetTouchRadiu() + Creator.GetTouchRadiu() + BattleConst.MinAttackDistance);
                if (distance < 0)
                {
                    distance = 0;
                }
                if (distance > BattleConst.MinAttackDistance)
                {
                    dir = (target.GetPositionXZ() - Creator.GetPositionXZ()).normalized;
                    speed = distance / (frameOffset + 1);
                    moveDelta = dir * speed;
                }
            }
            else if (TaskData.jumpType == SKILL_JUMP_TARGET_TYPE.Self)
            {
                Vector3 forward = Creator.GetForward();
                Vector3 startPos = Creator.GetPositionXZ();
                Vector3 deltaVer = Vector3.zero;
                Vector3 deltaHor = Vector3.zero;
                Vector3 targetPosXZ = Vector3.zero;
                for (int i = 0; i < frameOffset; i++)
                {
                    if (TaskData.verCure != null)
                    {
                        float sample = TaskData.verCure.Evaluate(i * 1f / frameOffset);
                        float value = sample * TaskData.verHeight;
                        deltaVer = Vector3.up * value;
                    }
                    if (TaskData.horCure != null)
                    {
                        float sample = TaskData.horCure.Evaluate(i * 1f / frameOffset);
                        float value = sample * TaskData.verDistance;
                        deltaHor = forward * value;
                    }
                    targetPosXZ = startPos + deltaHor;
                    if (!BattleUtil.IsInMap(targetPosXZ))
                    {
                        targetPosXZ = BattleUtil.GetWalkablePos(targetPosXZ);
                    }
                    moveList.Add(targetPosXZ + deltaVer);
                }
            }
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            if (TaskData.jumpType == SKILL_JUMP_TARGET_TYPE.SkillTarget)
            {
                if (target != null)
                {
                    float preheight = TaskData.verCure.Evaluate((frameIdx - TaskData.startFrame) * 1f / frameOffset) * TaskData.verHeight;
                    float height = TaskData.verCure.Evaluate((frameIdx - TaskData.startFrame + 1) * 1f / frameOffset) * TaskData.verHeight;
                    moveDelta.y = height - preheight;
                }
                if (Creator != null)
                    SkillCtr.SkillActorMove(Creator, moveDelta, delta);
            }
            else if (TaskData.jumpType == SKILL_JUMP_TARGET_TYPE.Self)
            {
                curFrame = frameIdx - TaskData.startFrame;
                if (curFrame >= moveList.Count)
                {
                    return;
                }
                SkillCtr.SkillActorMove(Creator, moveList[curFrame], delta);
            }
        }
    }
}
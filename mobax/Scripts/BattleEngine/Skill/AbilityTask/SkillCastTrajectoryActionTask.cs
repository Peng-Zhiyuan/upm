namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;
    using System;

    public sealed class CastTrajectoryTaskData : AbilityTaskData
    {
        public string effectPath;
        public Vector3 fireOffset, effectScale, destoryEffectScale, centerOffset;
        public bool flyFixedTime = false;
        public int flyTime = 0;
        public float destoryTime = 0;
        public float destoryDelay = 0;
        public string destroyEffect;
        public string destoryAudio;

        public float flySpeed;
        public float hurtRatio;
        public float flyMaxDist;
        public int flyCount;
        public int flyTimeOffset;
        public int flyAngleOffset;
        public float colliderRadius;
        public bool isPenetrate;
        public bool onlyHurtTarget = false;
        public TRAJECTORYITEM_TYPE trajectoryType;
        public TRAJECTORYITEM_TARGET_TYPE selectTargetType;
        public TRAJECTORYITEM_POS_TYPE selectPosType;
        public MOVE_DIR_TYPE dirType;
        public float parabolaHeight;
        public AnimationCurve speedCurve = AnimationCurve.Constant(0, 1, 1);
        public int cureTime = 1000;
        public AnimationCurve parabolaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public DotElement dotElement;
        public Action<CombatActorEntity, CombatUnitEntity, float> OnTriggerEnterCallback;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Trajectory;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }
        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CastTrajectoryActionElement actionElement = (element as CastTrajectoryActionElement);
            effectPath = AddressablePathConst.SkillEditorPathParse(actionElement.effectPath);
            destroyEffect = !string.IsNullOrEmpty(actionElement.destroyEffect) ? AddressablePathConst.SkillEditorPathParse(actionElement.destroyEffect) : "";
            effectScale = actionElement.effectScale;
            destoryEffectScale = actionElement.destroyEffectScale;
            fireOffset = actionElement.fireOffset;
            centerOffset = actionElement.centerOffset;
            destoryTime = actionElement.destoryTime;
            destoryDelay = actionElement.destoryDelay;
            flySpeed = actionElement.flySpeed;
            hurtRatio = actionElement.hurtRatio;
            flyMaxDist = actionElement.flyMaxDist;
            flyFixedTime = actionElement.flyFixedTime;
            flyTime = actionElement.flyTime;
            flyCount = actionElement.flyCount;
            flyTimeOffset = actionElement.flyTimeOffset;
            flyAngleOffset = actionElement.flyAngleOffset;
            colliderRadius = actionElement.colliderRadius;
            isPenetrate = actionElement.isPenetrate;
            onlyHurtTarget = actionElement.onlyHurtTarget;
            trajectoryType = actionElement.trajectoryType;
            selectTargetType = actionElement.selectTargetType;
            selectPosType = actionElement.selectPosType;
            parabolaHeight = actionElement.parabolaHeight;
            dirType = actionElement.dirType;
            if (actionElement.speedCurve != null)
            {
                speedCurve = actionElement.speedCurve;
            }
            if (parabolaCurve != null)
            {
                parabolaCurve = actionElement.parabolaCurve;
            }
            cureTime = actionElement.cureTime;
            dotElement = actionElement.dotElement;
            destoryAudio = !string.IsNullOrEmpty(actionElement.destoryAudio) ? AddressablePathConst.SkillEditorPathParse(actionElement.destoryAudio) : "";
        }
    }

    public sealed class SkillCastTrajectoryActionTask : AbilityTask
    {
        private CastTrajectoryTaskData _taskData;
        public CastTrajectoryTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CastTrajectoryTaskData;
                }
                return _taskData;
            }
        }
        private int bulletCount = 0;
        private CombatActorEntity owner;
        private int onceCount;
        private List<BulletEntity> bulletEntities = new List<BulletEntity>();

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            owner = SkillAbilityExecution.OwnerEntity;
            bulletEntities = new List<BulletEntity>();
            if (TaskData.flyTimeOffset == 0)
            {
                fireBullet(TaskData.flyCount);
            }
            else
            {
                int fireTimes = (TaskData.endFrame - TaskData.startFrame) / TaskData.flyTimeOffset;
                if (TaskData.flyCount < fireTimes)
                {
                    onceCount = 1;
                    TaskData.flyCount = fireTimes;
                }
                else
                {
                    onceCount = TaskData.flyCount / fireTimes;
                }
                fireBullet(onceCount);
            }
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            int startFrame = TaskData.startFrame;
            int endFrame = TaskData.endFrame;
            if (frameIdx > startFrame
                && frameIdx <= endFrame)
            {
                if (bulletCount < TaskData.flyCount)
                {
                    if (TaskData.flyTimeOffset > 0
                        && (frameIdx - startFrame) % TaskData.flyTimeOffset == 0)
                    {
                        fireBullet(onceCount);
                    }
                }
            }
        }

        public override void EndExecute()
        {
            bulletCount = 0;
            bulletEntities.Clear();
            base.EndExecute();
        }

        int targetIndex = 0;

        private void fireBullet(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = Quaternion.Euler(owner.GetEulerAngles()) * TaskData.fireOffset + owner.GetPosition();
                Vector3 rot = owner.GetEulerAngles();
                if (TaskData.trajectoryType == TRAJECTORYITEM_TYPE.Free)
                {
                    float angle = 0;
                    if (count == 1
                        && TaskData.flyTimeOffset != 0)
                    {
                        angle = getAngle(bulletCount);
                    }
                    else
                    {
                        angle = getAngle(i);
                    }
                    if (TaskData.dirType == MOVE_DIR_TYPE.Froward)
                    {
                        rot = Quaternion.Euler(0, angle, 0) * owner.GetForward();
                    }
                    else if (TaskData.dirType == MOVE_DIR_TYPE.Back)
                    {
                        rot = Quaternion.Euler(0, angle, 0) * owner.GetForward() * -1;
                    }
                    else if (TaskData.dirType == MOVE_DIR_TYPE.Left)
                    {
                        rot = Quaternion.Euler(0, angle, 0) * owner.GetRight() * -1;
                    }
                    else if (TaskData.dirType == MOVE_DIR_TYPE.Right)
                    {
                        rot = Quaternion.Euler(0, angle, 0) * owner.GetRight();
                    }
                }
                BulletEntity be = null;
                if (SkillAbilityExecution.AllTargetActorEntity.Count > targetIndex)
                {
                    be = owner.CreatBullet(TaskData, SkillAbilityExecution.AllTargetActorEntity[targetIndex], pos, rot, SkillAbilityExecution.GetWarningPoint(bulletCount), SkillAbilityExecution.SkillAbility);
                    targetIndex += 1;
                    if (SkillAbilityExecution.AllTargetActorEntity.Count <= targetIndex)
                    {
                        targetIndex = 0;
                    }
                }
                else
                {
                    be = owner.CreatBullet(TaskData, SkillAbilityExecution.targetActorEntity, pos, rot, SkillAbilityExecution.GetWarningPoint(bulletCount), SkillAbilityExecution.SkillAbility);
                }
                bulletEntities.Add(be);
                bulletCount++;
            }
        }

        private float getAngle(int num)
        {
            float angle = 0;
            if (TaskData.trajectoryType == TRAJECTORYITEM_TYPE.Free)
            {
                if (TaskData.flyCount % 2 != 0)
                {
                    if (num % 2 == 0)
                        angle = num / 2f * TaskData.flyAngleOffset;
                    else
                        angle = -(num + 1) / 2f * TaskData.flyAngleOffset;
                }
                else
                {
                    if (num % 2 == 0)
                        angle = num / 2f * TaskData.flyAngleOffset + TaskData.flyAngleOffset / 2f;
                    else
                        angle = -(num / 2f * TaskData.flyAngleOffset + TaskData.flyAngleOffset / 2f);
                }
            }
            return angle;
        }
    }
}
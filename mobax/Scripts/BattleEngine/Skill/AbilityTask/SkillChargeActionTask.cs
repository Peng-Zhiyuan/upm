namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class ChargeActionTaskData : AbilityTaskData
    {
        public CHARGE_TYPE chargeType;
        public float verMoveSpeed;

        public float verMoveDration;

        public AnimationCurve verMoveCure;

        public float horMoveSpeed;

        public float horMovedDration;

        public AnimationCurve horMoveCure;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Charge;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            ChargeActionElement actionElement = element as ChargeActionElement;
            chargeType = actionElement.chargeType;
            horMoveSpeed = actionElement.horMoveSpeed;
            horMovedDration = actionElement.horMovedDration;
            horMoveCure = actionElement.horMoveCure;
            verMoveSpeed = actionElement.verMSpeed;
            verMoveDration = actionElement.verMdDration;
            verMoveCure = actionElement.verMoveCure;
        }
    }

    public sealed class SkillChargeActionTask : AbilityTask
    {
        private ChargeActionTaskData _taskData;
        public ChargeActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as ChargeActionTaskData;
                }
                return _taskData;
            }
        }

        public void CheckFrame()
        {
            switch (TaskData.chargeType)
            {
                case CHARGE_TYPE.Time:
                    break;
                case CHARGE_TYPE.Destation:
                    float delta = SkillAbilityExecution.SkillAbility.SkillFrameRate;
                    CombatActorEntity Creator = SkillAbilityExecution.OwnerEntity;
                    CombatActorEntity target = SkillAbilityExecution.targetActorEntity;
                    if (target != null)
                    {
                        float distance = MathHelper.ActorDistance(Creator, target) - (target.GetTouchRadiu() + Creator.GetTouchRadiu() + BattleConst.MinAttackDistance);
                        if (distance < 0)
                        {
                            distance = 0;
                        }
                        float durantion = distance / TaskData.horMoveSpeed;
                        int frameLength = Mathf.FloorToInt(durantion / delta) + 1;
                        SkillAbilityExecution.frameInputIndex = TaskData.endFrame;
                        SkillAbilityExecution.frameInputOffset = frameLength - (TaskData.endFrame - TaskData.startFrame) + 1;
                    }
                    break;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            float delta = SkillAbilityExecution.SkillAbility.SkillFrameRate;
            CombatActorEntity Creator = SkillAbilityExecution.OwnerEntity;
            switch (TaskData.chargeType)
            {
                case CHARGE_TYPE.Time:
                    Creator.KinematControl.HorizontalCharge(TaskData.horMoveSpeed, TaskData.horMoveCure, TaskData.horMovedDration);
                    Creator.KinematControl.VerticalCharge(TaskData.verMoveSpeed, TaskData.verMoveCure, TaskData.verMoveDration);
                    break;
                case CHARGE_TYPE.Destation:
                    AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 1) });
                    CombatActorEntity target = SkillAbilityExecution.targetActorEntity;
                    if (target != null)
                    {
                        float distance = MathHelper.ActorDistance(Creator, target) - (target.GetTouchRadiu() + Creator.GetTouchRadiu() + 1);
                        Vector3 dir = (target.GetPositionXZ() - Creator.GetPositionXZ()).normalized;
                        float durantion = distance / TaskData.horMoveSpeed;
                        SkillAbilityExecution.OwnerEntity.KinematControl.HorizontalCharge(dir * TaskData.horMoveSpeed, curve, durantion, 0);
                        SkillAbilityExecution.OwnerEntity.SetForward(dir);
                    }
                    break;
            }
        }

        public override void EndExecute()
        {
            CombatActorEntity entity = SkillAbilityExecution.OwnerEntity;
            if (entity != null)
            {
                entity.KinematControl.StopCharge();
            }
            base.EndExecute();
        }
    }
}
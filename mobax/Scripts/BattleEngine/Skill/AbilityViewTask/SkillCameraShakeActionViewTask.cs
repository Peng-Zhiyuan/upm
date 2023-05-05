namespace BattleEngine.View
{
    using Logic;

    public sealed class CameraShakeTaskData : AbilityTaskData
    {
        public float Duration = 1;
        public float ShakeIntensity = 1;
        public float Frenquence = 1;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.CameraShake;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CameraShakeElement actionElement = element as CameraShakeElement;
            Duration = actionElement.Duration;
            ShakeIntensity = actionElement.ShakeIntensity;
            Frenquence = actionElement.Frenquence;
        }
    }

    public sealed class SkillCameraShakeActionViewTask : AbilityViewTask
    {
        private CameraShakeTaskData _taskData;
        public CameraShakeTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CameraShakeTaskData;
                }
                return _taskData;
            }
        }
        float curDisRatio = 0;
        float deltaDisRatio = 0;

        private bool isPlayEnable = true;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CombatActorEntity actorEntity = SkillAbilityExecution.OwnerEntity;
            isPlayEnable = actorEntity.isAtker && actorEntity.PosIndex != BattleConst.SSPAssistPosIndexStart;
            if (!isPlayEnable)
            {
                return;
            }
            CameraSetting.Ins.Shake(TaskData.ShakeIntensity, TaskData.Frenquence, TaskData.Duration);
        }

        // public override void DoExecute(int frameIdx)
        // {
        //     base.DoExecute(frameIdx);
        //     if (TaskData == null)
        //     {
        //         EndExecute();
        //     }
        // }
    }
}
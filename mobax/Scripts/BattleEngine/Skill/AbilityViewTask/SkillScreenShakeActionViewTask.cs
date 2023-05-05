namespace BattleEngine.View
{
    using Logic;

    /// <summary>
    /// 震屏
    /// </summary>
    public sealed class ScreenShakeActionTaskData : AbilityTaskData
    {
        public float shakeTime;
        public float shakePower;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.ScreenShake;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            ScreenShakeActionElement actionElement = element as ScreenShakeActionElement;
            shakePower = actionElement.shakePower;
            shakeTime = actionElement.shakeTime;
        }
    }

    public sealed class SkillScreenShakeActionViewTask : AbilityViewTask
    {
        private ScreenShakeActionTaskData _taskData;
        public ScreenShakeActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as ScreenShakeActionTaskData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CombatActorEntity actorEntity = SkillAbilityExecution.OwnerEntity;
            if (!actorEntity.isAtker)
            {
                return;
            }
            CameraSetting.Ins.Shake(TaskData.shakePower, 10f, TaskData.shakeTime);
        }
    }
}
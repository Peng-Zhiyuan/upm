namespace BattleEngine.View
{
    using UnityEngine;
    using Logic;

    public sealed class CameraAnimTaskData : AbilityTaskData
    {
        public SKILL_CAM_POSITION_TYPE animType = SKILL_CAM_POSITION_TYPE.Local;
        public string cameraAnimPath = "";
        public Vector3 offsetVec = Vector3.zero;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.CameraAni;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CameraAnimActionElement actionElement = element as CameraAnimActionElement;
            animType = actionElement.type;
            offsetVec = actionElement.offset;
            if (!string.IsNullOrEmpty(actionElement.camPathPrefab))
            {
                cameraAnimPath = AddressablePathConst.SkillEditorPathParse(actionElement.camPathPrefab);
            }
        }
    }

    public sealed class SkillCameraAnimActionViewTask : AbilityViewTask
    {
        private CameraAnimTaskData _taskData;
        public CameraAnimTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CameraAnimTaskData;
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
            if (TaskData.animType == SKILL_CAM_POSITION_TYPE.Local)
            {
                Creature actor = BattleManager.Instance.ActorMgr.GetActor(actorEntity.UID);
                SkillCameraCtr.Instance.PlayCameraAnim(TaskData.cameraAnimPath, actor.SelfTrans, TaskData.offsetVec);
            }
            else
            {
                SkillCameraCtr.Instance.PlayCameraAnim(TaskData.cameraAnimPath, null, TaskData.offsetVec);
            }
        }

        public override void BreakExecute(int frameIdx)
        {
            if (!isPlayEnable)
            {
                return;
            }
            SkillCameraCtr.Instance.HideCameraAnim(TaskData.cameraAnimPath);
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            if (!isPlayEnable)
            {
                return;
            }
            SkillCameraCtr.Instance.HideCameraAnim(TaskData.cameraAnimPath);
            base.EndExecute();
        }
    }
}
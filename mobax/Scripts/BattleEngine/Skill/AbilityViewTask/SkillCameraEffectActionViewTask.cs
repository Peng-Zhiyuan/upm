namespace BattleEngine.View
{
    using UnityEngine;
    using Logic;

    public sealed class SkillCameraEffectData : AbilityTaskData
    {
        public string effectName;
        public Vector3 offset;
        public Vector3 scale;
        public Vector3 rot;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.CameraEffect;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CameraEffectElement actionElement = element as CameraEffectElement;
            effectName = actionElement.effect;
            offset = actionElement.offset;
            scale = actionElement.scale;
            rot = actionElement.rot;
        }
    }

    public sealed class SkillCameraEffectActionViewTask : AbilityViewTask
    {
        private SkillCameraEffectData _taskData;
        public SkillCameraEffectData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as SkillCameraEffectData;
                }
                return _taskData;
            }
        }
        private CombatActorEntity Creator;
        private GameObject VMCamera;

        private bool isPlayEnable = true;
        private int effectID;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CombatActorEntity actorEntity = SkillAbilityExecution.OwnerEntity;
            isPlayEnable = actorEntity.isAtker && actorEntity.PosIndex != BattleConst.SSPAssistPosIndexStart;
            if (!isPlayEnable)
            {
                return;
            }
            string path = AddressablePathConst.SkillEditorPathParse(TaskData.effectName);
            effectID = EffectManager.Instance.CreateUICameraEffect(path, 100, TaskData.offset, TaskData.scale, TaskData.rot);
        }

        public override void BreakExecute(int frameIdx)
        {
            if (!isPlayEnable)
            {
                return;
            }
            EffectManager.Instance.RemoveEffect(effectID);
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            if (!isPlayEnable)
            {
                return;
            }
            EffectManager.Instance.RemoveEffect(effectID);
            base.EndExecute();
        }
    }
}
namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class CreatDotTaskData : AbilityTaskData
    {
        public string effectPrefab;
        public Vector3 scale = Vector3.one;
        public Vector3 offset = Vector3.one;
        public Vector3 angleOffset = Vector3.one;
        public Dictionary<int, float> speedModify = new Dictionary<int, float>();
        public bool isAttachLookAt = true;
        public int durationTime = 1;
        public int intervalTime = 1;
        public float radius = 1;
        public float height = 6;
        public List<Effect> Effects = new List<Effect>();
        public bool effectiveOnce = false;
        public bool applyEffectDirect = false;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Dot;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CreatDotActionElement actionElement = element as CreatDotActionElement;
            effectPrefab = AddressablePathConst.SkillEditorPathParse(actionElement.effectPrefab);
            scale = actionElement.scale;
            offset = actionElement.offset;
            angleOffset = actionElement.angleOffset;
            isAttachLookAt = actionElement.isAttachLookAt;
            durationTime = actionElement.durationTime;
            intervalTime = actionElement.intervalTime;
            radius = actionElement.radius;
            Effects = actionElement.Effects;
            effectiveOnce = actionElement.effectiveOnce;
            applyEffectDirect = actionElement.applyEffectDirect;
        }
    }

    public sealed class SkillCreatDotActionTask : AbilityTask
    {
        private CreatDotTaskData _taskData;
        public CreatDotTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CreatDotTaskData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            DotCtr.CreatDot(TaskData, SkillAbilityExecution.OwnerEntity, SkillAbilityExecution.SkillAbility.SkillConfigObject.AffectTargetType);
        }
    }
}
namespace BattleEngine.Logic
{
    using UnityEngine;

    /// <summary>
    /// 盾墙技能
    /// </summary>
    public sealed class CreatShieldWallData : AbilityTaskData
    {
        public string effectPrefab;
        public string hitEffectPrefab;
        public string destoryEffectPrefab;
        public Vector3 scale = Vector3.one;
        public Vector3 offset = Vector3.one;
        public Vector3 angleOffset = Vector3.one;
        public bool isAttachLookAt = true;
        public bool isAttachLock = false;
        public int durationTime = 1; //毫秒
        public int times = 1;
        public float radius = 1;
        public Vector3 size = Vector3.one;
        public SHIELD_WALL_TYPE wallTYpe;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.ShieldWall;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CreatShieldWallActionElement actionElement = element as CreatShieldWallActionElement;
            wallTYpe = actionElement.wallTYpe;
            scale = actionElement.scale;
            offset = actionElement.offset;
            angleOffset = actionElement.angleOffset;
            isAttachLookAt = actionElement.isAttachLookAt;
            isAttachLock = actionElement.isAttachLock;
            durationTime = actionElement.durationTime;
            radius = actionElement.radius;
            size = actionElement.size;
            times = actionElement.times;
            effectPrefab = AddressablePathConst.SkillEditorPathParse(actionElement.effectPrefab);
            hitEffectPrefab = AddressablePathConst.SkillEditorPathParse(actionElement.hitEffectPrefab);
            destoryEffectPrefab = AddressablePathConst.SkillEditorPathParse(actionElement.destoryEffectPrefab);
            if (times <= 0)
            {
                times = 1;
            }
        }
    }

    public sealed class SkillCreatShieldWallActionTask : AbilityTask
    {
        private CreatShieldWallData _taskData;
        public CreatShieldWallData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CreatShieldWallData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            ShieldWallCtr.CreatShieldWall(TaskData, SkillAbilityExecution.OwnerEntity);
        }
    }
}
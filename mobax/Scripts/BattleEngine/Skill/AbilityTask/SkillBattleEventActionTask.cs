namespace BattleEngine.Logic
{
    using UnityEngine;

    public sealed class BattleEventActionTaskData : AbilityTaskData
    {
        public SKILL_EVENT_TYPE eventType = SKILL_EVENT_TYPE.NONE;
        public string param = "";
        public int param2;
        public int param3;
        public int param4;
        public Color color;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.BattleEvent;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.BOTH;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            BattleEventActionElement actioneEement = element as BattleEventActionElement;
            eventType = actioneEement.eventType;
            param = actioneEement.param;
            param2 = actioneEement.param2;
            param3 = actioneEement.param3;
            param4 = actioneEement.param4;
            color = ColorUtil.HexToColor(actioneEement.hexColor);
        }
    }

    public sealed class SkillBattleEventActionTask : AbilityTask
    {
        private BattleEventActionTaskData _taskData;
        public BattleEventActionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as BattleEventActionTaskData;
                }
                return _taskData;
            }
        }

        private bool isPlayEnable = true;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CombatActorEntity actorEntity = SkillAbilityExecution.OwnerEntity;
            isPlayEnable = actorEntity.isAtker;
            if (!isPlayEnable)
            {
                return;
            }
            switch (TaskData.eventType)
            {
                case SKILL_EVENT_TYPE.CantControl:
                    SkillAbilityExecution.OwnerEntity.AttachBuff((int)BUFF_COMMON_CONFIG_ID.SSPSkill_Cant_Control);
                    break;
            }
        }

        public override void EndExecute()
        {
            if (!isPlayEnable)
            {
                return;
            }
            switch (TaskData.eventType)
            {
                case SKILL_EVENT_TYPE.CantControl:
                    SkillAbilityExecution.OwnerEntity.OnBuffRemove((int)BUFF_COMMON_CONFIG_ID.SSPSkill_Cant_Control);
                    break;
            }
            base.EndExecute();
        }
    }
}
namespace BattleEngine.View
{
    using UnityEngine;
    using System;
    using Logic;

    public sealed class CreateExplosionTaskData : AbilityTaskData
    {
        public Vector3 TargetPoint;
        public string ExplosionPrefabName;
        public Vector3 fxScale;
        public float radiusStart;
        public float radiusEnd;
        public Action<CombatActorEntity, CombatUnitEntity> OnTriggerEnterCallback;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Explosion;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CreateExplosionActionElement actionElement = element as CreateExplosionActionElement;
            ExplosionPrefabName = AddressablePathConst.SkillEditorPathParse(actionElement.res);
            fxScale = actionElement.scale;
            radiusStart = actionElement.radiusStart;
            radiusEnd = actionElement.radiusEnd;
        }
    }

    public sealed class SkillCreateExplosionActionViewTask : AbilityViewTask
    {
        private CreateExplosionTaskData _taskData;
        public CreateExplosionTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CreateExplosionTaskData;
                }
                return _taskData;
            }
        }
        private GameObject fx;

        public override async void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            Vector3 targetPos = TaskData.TargetPoint;
            string EffectPrefabName = TaskData.ExplosionPrefabName;
            fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName);
            if (fx == null)
            {
                return;
            }
            fx.transform.position = targetPos;
        }

        public override void EndExecute()
        {
            BattleResManager.Instance.RecycleEffect(TaskData.ExplosionPrefabName, fx);
            base.EndExecute();
        }
    }
}
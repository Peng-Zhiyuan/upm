namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using Logic;

    public sealed class SkillCreateEffectTaskData : AbilityTaskData
    {
        public Vector3 Offset;
        public string effectPrefabName;
        /// <summary>
        /// 特效绑定方式
        /// </summary>
        public COORDINATE_TYPE attachType;
        /// <summary>
        /// 特效绑定点
        /// </summary>
        public string attachPoint;
        /// <summary>
        /// 是否绑定锁定
        /// </summary>
        public bool isAttachLock;
        /// <summary>
        /// 是否根据人物朝向
        /// </summary>
        public bool isAttachLookAt;
        public bool isLookAtTarget = false;
        public bool isLookAtWarningPoint = false;

        /// <summary>
        /// 是否循环
        /// </summary>
        public bool isLoop;
        /// <summary>
        /// 旋转
        /// </summary>
        public Vector3 rotation;
        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 scale;
        /// <summary>
        /// 技能被打断时是否销毁
        /// </summary>
        public bool isReleaseWhenBreaked;
        public List<AnimSpeed> speedModify = new List<AnimSpeed>();

        //是否自己播放
        public bool isSelfPlay;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.Effect;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CreateEffectActionElement actionElement = (element as CreateEffectActionElement);
            Offset = actionElement.posOffset;
            speedModify = actionElement.speedModify;
            if (speedModify == null)
            {
                speedModify = new List<AnimSpeed>();
            }
            effectPrefabName = AddressablePathConst.SkillEditorPathParse(actionElement.res);
            scale = actionElement.scale;
            Offset = actionElement.posOffset;
            attachType = actionElement.attachType;
            attachPoint = actionElement.attachPoint;
            isAttachLock = actionElement.isAttachLock;
            isAttachLookAt = actionElement.isAttachLookAt;
            isLookAtTarget = actionElement.isLookAtTarget;
            isLookAtWarningPoint = actionElement.isLookAtWarningPoint;
            isLoop = actionElement.isloop;
            rotation = actionElement.angleOffset;
            isReleaseWhenBreaked = actionElement.isReleaseWhenBreaked;
            isSelfPlay = actionElement.isSelfPlay;
        }
    }

    public sealed class SkillCreateEffectActionViewTask : AbilityViewTask
    {
        private SkillCreateEffectTaskData _taskData;
        public SkillCreateEffectTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as SkillCreateEffectTaskData;
                }
                return _taskData;
            }
        }

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CreatEffect(SkillAbilityExecution.SkillAbility.SkillBaseConfig, SkillAbilityExecution.OwnerEntity, SkillAbilityExecution.targetActorEntity, SkillAbilityExecution.GetWarningPoint(0), SkillAbilityExecution.InputPoint);
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            Update(frameIdx, SkillAbilityExecution.targetActorEntity, SkillAbilityExecution.GetWarningPoint(0));
        }

        public override void PauseExecute(int frameIdx)
        {
            base.PauseExecute(frameIdx);
            Pause();
        }

        public override void BreakExecute(int frameIdx)
        {
            BreakEffect();
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            RecycleEffect();
            base.EndExecute();
        }

        private GameObject fx;
        private string EffectPrefabName;
        private bool hasbreak = false;

        public async void CreatEffect(SkillRow skillRow, CombatActorEntity caster, CombatActorEntity target, Vector3 warningPoint, Vector3 inputPoint)
        {
            if (TaskData == null
                || string.IsNullOrEmpty(TaskData.effectPrefabName)
                || caster == null)
            {
                return;
            }
            EffectPrefabName = TaskData.effectPrefabName;
            Creature actorCreature = BattleManager.Instance.ActorMgr.GetActor(caster.UID);
            Transform actorTrans = actorCreature.transform;
            Vector3 initPos = Vector3.zero;
            Vector3 initForward = Vector3.zero;
            switch (TaskData.attachType)
            {
                case COORDINATE_TYPE.Global:
                    if (!string.IsNullOrEmpty(TaskData.attachPoint))
                    {
                        Transform attachTrans = GameObjectHelper.FindChild(actorTrans, TaskData.attachPoint);
                        initPos = attachTrans.transform.position;
                        initForward = Vector3.zero;
                    }
                    else if (TaskData.isAttachLookAt)
                    {
                        initPos = actorTrans.position + Quaternion.Euler(actorTrans.eulerAngles) * TaskData.Offset;
                        initForward = Quaternion.Euler(TaskData.rotation) * actorTrans.forward;
                    }
                    else
                    {
                        initPos = actorTrans.position + TaskData.Offset;
                        initForward = Quaternion.Euler(TaskData.rotation) * actorTrans.forward;
                    }
                    fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, TaskData.isLoop);
                    if (fx == null)
                    {
                        return;
                    }
                    fx.name = EffectPrefabName;
                    fx.transform.position = initPos;
                    fx.transform.forward = initForward;
                    fx.transform.localScale = TaskData.scale;
                    break;
                case COORDINATE_TYPE.Local:
                    Transform parentTrans = actorTrans;
                    if (string.IsNullOrEmpty(TaskData.attachPoint))
                    {
                        parentTrans = actorTrans;
                        fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, parentTrans, Vector3.zero, TaskData.isLoop);
                    }
                    else
                    {
                        parentTrans = GameObjectHelper.FindChild(actorTrans, TaskData.attachPoint);
                        fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, parentTrans, Vector3.zero, TaskData.isLoop);
                    }
                    if (fx == null)
                    {
                        return;
                    }
                    fx.name = EffectPrefabName;
                    if (TaskData == null)
                    {
                        BattleResManager.Instance.RecycleEffect(EffectPrefabName, fx);
                        return;
                    }
                    if (!TaskData.isAttachLock)
                    {
                        if (TaskData.isAttachLookAt)
                        {
                            fx.transform.localPosition = TaskData.Offset;
                            fx.transform.localRotation = Quaternion.Euler(TaskData.rotation);
                        }
                        else
                        {
                            fx.transform.position = parentTrans.position + TaskData.Offset;
                            fx.transform.rotation = Quaternion.Euler(TaskData.rotation);
                        }
                    }
                    else
                    {
                        if (TaskData.isAttachLookAt)
                        {
                            fx.transform.localPosition = TaskData.Offset;
                            fx.transform.localRotation = Quaternion.Euler(TaskData.rotation);
                        }
                        else
                        {
                            fx.transform.position = parentTrans.position + TaskData.Offset;
                            fx.transform.rotation = Quaternion.Euler(TaskData.rotation);
                        }
                    }
                    fx.transform.localScale = TaskData.scale;
                    break;
                case COORDINATE_TYPE.Target:
                    if (string.IsNullOrEmpty(TaskData.attachPoint))
                    {
                        initPos = target.GetPosition();
                        fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName);
                        if (fx == null)
                        {
                            return;
                        }
                        fx.name = EffectPrefabName;
                        fx.transform.position = initPos + TaskData.Offset;
                        fx.transform.rotation = Quaternion.Euler(target.GetEulerAngles() + TaskData.rotation);
                        fx.transform.localScale = TaskData.scale;
                    }
                    else
                    {
                        Creature targetCreature = BattleManager.Instance.ActorMgr.GetActor(target.UID);
                        parentTrans = GameObjectHelper.FindChild(targetCreature.transform, TaskData.attachPoint);
                        fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName, parentTrans, Vector3.zero, TaskData.isLoop);
                        if (fx == null)
                        {
                            return;
                        }
                        fx.name = EffectPrefabName;
                        fx.transform.localPosition = TaskData.Offset;
                        fx.transform.localRotation = Quaternion.Euler(TaskData.rotation);
                    }
                    break;
                case COORDINATE_TYPE.PreWarning:
                    initPos = warningPoint;
                    fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName);
                    if (fx == null)
                    {
                        return;
                    }
                    fx.name = EffectPrefabName;
                    fx.transform.position = initPos;
                    fx.transform.localScale = TaskData.scale;
                    break;
                case COORDINATE_TYPE.InputPoint:
                    fx = await BattleResManager.Instance.CreatorFx(EffectPrefabName);
                    if (fx == null)
                    {
                        return;
                    }
                    fx.name = EffectPrefabName;
                    fx.transform.position = inputPoint;
                    fx.transform.localScale = TaskData.scale;
                    break;
            }
            if (caster.CurrentHealth.Value <= 0 || hasbreak)
            {
                RecycleEffect();
            }
        }

        public void Update(int frameIdx, CombatActorEntity target, Vector3 warningPoint)
        {
            ChangeSpeed(GetCurSpeed(TaskData.speedModify, frameIdx));
            if (TaskData.isLookAtTarget)
            {
                if (fx != null
                    && target != null)
                    fx.transform.LookAt(target.GetPosition());
            }
            if (TaskData.isLookAtWarningPoint)
            {
                if (fx != null)
                    fx.transform.LookAt(warningPoint);
            }
        }

        float GetCurSpeed(List<AnimSpeed> speedModify, int frame)
        {
            if (speedModify == null)
            {
                return 1.0f;
            }
            float speed = 1;
            for (int i = 0; i < speedModify.Count; i++)
            {
                if (speedModify.Count > 0)
                {
                    for (int j = 0; j < speedModify.Count; j++)
                    {
                        if (speedModify[j].Frame == i)
                        {
                            speed = speedModify[j].value;
                        }
                    }
                }
            }
            return speed;
        }

        private void ChangeSpeed(float speed)
        {
            if (fx == null) return;
            // ParticleSystem[] particles = fx.GetComponentsInChildren<ParticleSystem>(false);
            // for (int i = 0; i < particles.Length; i++)
            // {
            //     ParticleSystem.MainModule main = particles[i].main;
            //     main.simulationSpeed = speed;
            // }
        }

        public void Pause()
        {
            ParticleSystem[] particles = fx.GetComponentsInChildren<ParticleSystem>(false);
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Pause();
            }
        }

        public void BreakEffect()
        {
            hasbreak = true;
            if (TaskData.isReleaseWhenBreaked
                && fx != null)
            {
                RecycleEffect();
            }
        }

        public void RecycleEffect()
        {
            hasbreak = true;
            if (!string.IsNullOrEmpty(EffectPrefabName)
                && fx != null
                && !TaskData.isLoop)
            {
                BattleResManager.Instance.RecycleEffect(EffectPrefabName, fx);
            }
        }
    }
}
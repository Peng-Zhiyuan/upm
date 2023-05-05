namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using Logic;

    public sealed class PreWarningTaskData : AbilityTaskData
    {
        public string waringEffect;
        public bool hideOutDecals = true;
        public Vector3 effectScale = Vector3.one;
        public Vector3 rayLocalPos = Vector3.one;
        public Vector3 rayLocalScale = Vector3.one;

        public int effectNum = 1;
        public int radius = 1;
        public int perDuration = 5;
        public List<int> triggerTimes = new List<int>();
        public SKILL_PRE_WARING_TYPE waringType = SKILL_PRE_WARING_TYPE.Random;
        public List<Vector3> offset = new List<Vector3>();
        public bool isAttachLookAt = true;
        public float spreadSpeed = 1;
        public SKILL_PRE_WARING_EFFECT_TYPE waringEffectType = SKILL_PRE_WARING_EFFECT_TYPE.Circle;

        public float followSpeed = 0;
        public int followEndFrame = 0;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.PreWaring;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            PreWarningElement actionElement = element as PreWarningElement;
            effectNum = actionElement.effectNum;
            effectScale = actionElement.effectScale;
            rayLocalPos = actionElement.rayLocalPos;
            rayLocalScale = actionElement.rayLocalScale;
            offset = actionElement.offset;
            waringEffect = AddressablePathConst.SkillEditorPathParse(actionElement.waringEffect);
            perDuration = actionElement.perDuration;
            hideOutDecals = actionElement.hideOutDecals;
            waringType = actionElement.waringType;
            radius = actionElement.radius;
            triggerTimes = actionElement.triggerTimes;
            isAttachLookAt = actionElement.isAttachLookAt;
            spreadSpeed = actionElement.spreadSpeed;
            waringEffectType = actionElement.waringEffectType;
            followSpeed = actionElement.followSpeed;
            followEndFrame = actionElement.followEndFrame;
        }
    }

    /// <summary>
    /// 预警圈
    /// </summary>
    public sealed class SkillPreWarningActionViewTask : AbilityViewTask
    {
        private PreWarningTaskData _taskData;
        public PreWarningTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as PreWarningTaskData;
                }
                return _taskData;
            }
        }

        private List<int> endFrames = new List<int>();
        private List<Vector3> warningPoints = new List<Vector3>();

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            SetWarningPoints();
            GetParent<SkillAbilityExecution>().warningPoints.Clear();
            for (int i = 0; i < warningPoints.Count; i++)
            {
                GetParent<SkillAbilityExecution>().warningPoints.Add(warningPoints[i]);
            }
            InitTriggerTimes();
            if (!SkillAbilityExecution.OwnerEntity.isAtker)
            {
                EventManager.Instance.SendEvent("BattleRefreshPreWarning", this);
            }
        }

        private void SetWarningPoints()
        {
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            List<CombatActorEntity> enemys = new List<CombatActorEntity>();
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (enemy.IsCantSelect
                    || enemy.isAtker == SkillAbilityExecution.OwnerEntity.isAtker)
                {
                    continue;
                }
                enemys.Add(enemy);
            }
            warningPoints.Clear();
            for (int i = 0; i < TaskData.effectNum; i++)
            {
                Vector3 point = Vector3.zero;
                switch (TaskData.waringType)
                {
                    case SKILL_PRE_WARING_TYPE.Random:
                        CombatActorEntity owner = SkillAbilityExecution.OwnerEntity;
                        point = owner.GetPosition() + new Vector3(BattleLogicManager.Instance.Rand.RandomVaule(-20, 20) + i, 1, BattleLogicManager.Instance.Rand.RandomVaule(-20, 20) + i);
                        warningPoints.Add(point);
                        break;
                    case SKILL_PRE_WARING_TYPE.RandomTarget:
                        if (enemys.Count <= 0)
                        {
                            break;
                        }
                        if (TaskData.effectNum == 1)
                        {
                            int index = BattleLogicManager.Instance.Rand.RandomVaule(0, enemys.Count - 1);
                            point = enemys[index].GetPosition() + Vector3.up;
                            warningPoints.Add(point);
                            break;
                        }
                        for (int j = 0; j < enemys.Count; j++)
                        {
                            point = enemys[j].GetPosition() + Vector3.up;
                            bool posOK = true;
                            for (int k = 0; k < warningPoints.Count; k++)
                            {
                                if (MathHelper.DoubleDistanceVect3(point, warningPoints[k]) <= TaskData.radius * TaskData.radius)
                                {
                                    posOK = false;
                                    break;
                                }
                            }
                            if (posOK)
                            {
                                warningPoints.Add(point);
                                break;
                            }
                        }
                        break;
                    case SKILL_PRE_WARING_TYPE.FixedPosition:
                        CombatActorEntity actorTrans = SkillAbilityExecution.OwnerEntity;
                        Vector3 offset = TaskData.offset.Count > 0 ? TaskData.offset[i] : Vector3.zero;
                        point = actorTrans.GetPosition() + (TaskData.isAttachLookAt ? Quaternion.Euler(actorTrans.GetEulerAngles()) * offset : offset);
                        point = new Vector3(point.x, point.y + 0.1f, point.z);
                        warningPoints.Add(point);
                        break;
                    case SKILL_PRE_WARING_TYPE.CurTarget:
                        actorTrans = SkillAbilityExecution.targetActorEntity;
                        point = actorTrans.GetPosition();
                        point = new Vector3(point.x, point.y + 0.1f, point.z);
                        warningPoints.Add(point);
                        break;
                }
            }
            for (int i = warningPoints.Count; i < TaskData.effectNum; i++)
            {
                warningPoints.Add(new Vector3(BattleLogicManager.Instance.Rand.RandomVaule(-8, 8) + i, 1f, BattleLogicManager.Instance.Rand.RandomVaule(-8, 8) + i));
            }
        }

        private void InitTriggerTimes()
        {
            if (TaskData.triggerTimes == null)
            {
                TaskData.triggerTimes = new List<int>();
            }
            if (TaskData.triggerTimes.Count <= 0)
            {
                for (int i = 0; i < TaskData.effectNum; i++)
                {
                    TaskData.triggerTimes.Add(TaskData.startFrame);
                }
            }
            endFrames = new List<int>();
            for (int i = 0; i < TaskData.triggerTimes.Count; i++)
            {
                endFrames.Add(TaskData.triggerTimes[i] + TaskData.perDuration);
            }
        }

        private int creatIndex = 0;

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            if (TaskData.triggerTimes == null
                || TaskData.triggerTimes.Count <= 0)
            {
                InitTriggerTimes();
            }
            for (int i = 0; i < endFrames.Count; i++)
            {
                if (frameIdx >= endFrames[i])
                {
                    RecycleEffect(i);
                }
            }
            if (creatIndex < TaskData.effectNum)
            {
                for (int i = creatIndex; i < TaskData.effectNum; i++)
                {
                    if (frameIdx != TaskData.triggerTimes[i]
                        || i >= warningPoints.Count)
                    {
                        continue;
                    }
                    creatWarningEffect(warningPoints[i], SkillAbilityExecution.OwnerEntity);
                    creatIndex += 1;
                }
            }
            if (TaskData.followSpeed > 0
                && frameIdx <= TaskData.followEndFrame
                && SkillAbilityExecution.targetActorEntity != null
                && SkillAbilityExecution.targetActorEntity.CurrentHealth.Value > 0)
            {
                SetWarningPoints();
                float time = SkillAbilityExecution.SkillAbility.SkillFrameRate;
                for (int i = 0; i < creatIndex; i++)
                {
                    Vector3 startPos = SkillAbilityExecution.warningPoints[i];
                    SkillAbilityExecution.warningPoints[i] = startPos + (warningPoints[i] - startPos).normalized * time * TaskData.followSpeed;
                    Follow(i, SkillAbilityExecution.warningPoints[i]);
                }
                for (int i = creatIndex; i < warningPoints.Count; i++)
                {
                    if (i >= SkillAbilityExecution.warningPoints.Count)
                    {
                        continue;
                    }
                    SkillAbilityExecution.warningPoints[i] = warningPoints[i];
                }
            }
        }

        public override void BreakExecute(int frameIdx)
        {
            RecycleEffect();
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            RecycleEffect();
            endFrames = null;
            creatIndex = 0;
            base.EndExecute();
        }

        public List<GameObject> fxs = new List<GameObject>();

        public async void creatWarningEffect(Vector3 pos, CombatActorEntity attachEntity)
        {
            if (attachEntity == null)
            {
                return;
            }
            GameObject fx = await BattleResManager.Instance.CreatorFx(TaskData.waringEffect, false, 31);
            if (fx == null)
            {
                return;
            }
            fx.SetActive(false);
            fx.transform.localScale = TaskData.effectScale;
            fx.transform.localRotation = Quaternion.Euler(Vector3.zero);
            fx.transform.position = pos;
            if (TaskData.isAttachLookAt)
            {
                fx.transform.forward = Quaternion.Euler(Vector3.zero) * attachEntity.GetForward();
            }
            DecalsCtr ctr = fx.GetComponent<DecalsCtr>();
            ctr.HideOutDecals(TaskData.hideOutDecals);
            if (attachEntity.isAtker)
            {
                ctr.SetAsTeam();
            }
            else
            {
                ctr.SetAsEnemy();
            }
            switch (TaskData.waringEffectType)
            {
                case SKILL_PRE_WARING_EFFECT_TYPE.Circle:
                case SKILL_PRE_WARING_EFFECT_TYPE.Retangle:
                    ctr.SetCircleScale(TaskData.effectScale);
                    ctr.SetCircleSpeed(1 / TaskData.spreadSpeed);
                    break;
                case SKILL_PRE_WARING_EFFECT_TYPE.Ray:
                    ctr.SetRayScale(TaskData.effectScale, TaskData.rayLocalPos, TaskData.rayLocalScale);
                    ctr.SetRaySpeed(Mathf.Max(1f / ((TaskData.endFrame - TaskData.startFrame) * 1f / 30), TaskData.spreadSpeed));
                    break;
                // case SKILL_PRE_WARING_EFFECT_TYPE.Retangle:
                //     ctr.SetRetangleScale(TaskData.effectScale);
                //     ctr.SetRetangleSpeed(Mathf.Max(1f / ((TaskData.endFrame - TaskData.startFrame) * 1f / 30), TaskData.spreadSpeed));
                //     break;
            }
            fx.SetActive(true);
            BattleResManager.Instance.ChangeLayer(fx, 0);
            if (fxs == null)
            {
                fxs = new List<GameObject>();
            }
            fxs.Add(fx);
            if (attachEntity.CurrentHealth.Value <= 0)
            {
                RecycleEffect();
            }
        }

        public void RecycleEffect(int index)
        {
            if (index >= fxs.Count)
            {
                return;
            }
            BattleResManager.Instance.RecycleEffect(TaskData.waringEffect, fxs[index]);
            fxs[index] = null;
        }

        public void RecycleEffect()
        {
            if (fxs != null)
            {
                for (int i = 0; i < fxs.Count; i++)
                {
                    if (fxs[i] == null)
                    {
                        continue;
                    }
                    BattleResManager.Instance.RecycleEffect(TaskData.waringEffect, fxs[i]);
                }
            }
            fxs = null;
        }

        public void Follow(int index, Vector3 pos)
        {
            if (fxs.Count <= index
                || fxs[index] == null)
            {
                return;
            }
            fxs[index].transform.position = pos;
        }
    }
}
namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

#if !SERVER
    using CodeHero;
#endif

    public sealed class CreateTriggerTaskData : AbilityTaskData
    {
        public Action<CombatActorEntity, CombatUnitEntity, float, bool> OnTriggerEnterCallback;
        public SKILL_AFFECT_TARGET_TYPE affectTargetType = SKILL_AFFECT_TARGET_TYPE.Enemy;
        public SKILL_JUDGEX_TYPE judgexType = SKILL_JUDGEX_TYPE.Auto;
        public int TriggerInterval = 0;
        public int TriggerTimes = 1;
        public float HurtRatio = 1;
        public bool OneHitKill = false;

        public SKILL_JUDGEX_SHAPE JudgeShapeType;
        public Vector3 CenterStart;
        public Vector3 CenterEnd;
        public float RadiusStart;
        public float RadiusEnd;
        public float OuterRadiusStart;
        public float OuterRadiusEnd;
        public float RotYStart;
        public float RotYEnd;
        public float AngleStart;
        public float AngleEnd;
        public Vector3 SizeStart = new Vector3(0, 6, 0);
        public Vector3 SizeEnd = new Vector3(0, 6, 0);
        public AnimationCurve centerDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve rotYDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve sizeDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve angleDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve radiusDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        public string AttachPart = "Bone039";
        public int HitIndex = 0;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.AttackBox;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.LOGIC;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            CreateTriggerActionElement actionElement = element as CreateTriggerActionElement;
            judgexType = actionElement.JudgexType;
            TriggerInterval = actionElement.intervalTime;
            TriggerTimes = actionElement.triggerTimes;
            JudgeShapeType = actionElement.JudgeShapeType;
            CenterStart = actionElement.CenterStart;
            CenterEnd = actionElement.CenterEnd;
            RadiusStart = actionElement.RadiusStart;
            RadiusEnd = actionElement.RadiusEnd;
            OuterRadiusStart = actionElement.OuterRadiusStart;
            OuterRadiusEnd = actionElement.OuterRadiusEnd;
            RotYStart = actionElement.RotYStart;
            RotYEnd = actionElement.RotYEnd;
            AngleStart = actionElement.AngleStart;
            AngleEnd = actionElement.AngleEnd;
            SizeStart = actionElement.SizeStart;
            SizeEnd = actionElement.SizeEnd;
            OneHitKill = actionElement.oneHitKill;
            HurtRatio = actionElement.hurtRatio;
            AttachPart = actionElement.AttachPart;
            radiusDeformation = actionElement.radiusDeformation;
            if (radiusDeformation == null)
            {
                radiusDeformation = AnimationCurve.Linear(0, 0, 1, 1);
            }
            centerDeformation = AnimationCurve.Linear(0, 0, 1, 1);
            rotYDeformation = AnimationCurve.Linear(0, 0, 1, 1);
            sizeDeformation = AnimationCurve.Linear(0, 0, 1, 1);
            angleDeformation = AnimationCurve.Linear(0, 0, 1, 1);
        }
    }

    public class SkillCreateTriggerActionTask : AbilityTask
    {
        private CreateTriggerTaskData _taskData;
        public CreateTriggerTaskData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as CreateTriggerTaskData;
                }
                return _taskData;
            }
        }
        public Action<CombatActorEntity, CombatUnitEntity, float, bool> OnTriggerEnterCallback;
        private List<CombatUnitEntity> effectEntitys = new List<CombatUnitEntity>();

        List<CombatActorEntity> entitys = new List<CombatActorEntity>();
        int times = 0;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            OnTriggerEnterCallback = TaskData.OnTriggerEnterCallback;
            if (TaskData.TriggerInterval == 0)
            {
                TargetExecuteTrigger();
            }
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            if (TaskData.TriggerInterval != 0)
            {
                int interval = frameIdx - TaskData.startFrame;
                if (interval % TaskData.TriggerInterval != 0
                    || interval == 0)
                {
                    return;
                }
                if (times > TaskData.TriggerTimes)
                {
                    return;
                }
                TargetExecuteTrigger();
                effectEntitys.Clear();
                times++;
            }
            else
            {
                if (TaskData.judgexType == SKILL_JUDGEX_TYPE.Auto
                    || TaskData.judgexType == SKILL_JUDGEX_TYPE.ConditionSelect
                    || TaskData.judgexType == SKILL_JUDGEX_TYPE.PlayerSelect)
                {
                    return;
                }
            }
            float currentTime = (frameIdx - TaskData.startFrame) * (1000f * SkillAbilityExecution.SkillAbility.SkillFrameRate);
            switch (TaskData.JudgeShapeType)
            {
                case SKILL_JUDGEX_SHAPE.Cube:
                    this.CubeTriggerCheck(currentTime);
                    break;
                case SKILL_JUDGEX_SHAPE.Sector:
                    this.SectorTriggerCheck(currentTime);
                    break;
                case SKILL_JUDGEX_SHAPE.Cylinder:
                    this.CylinderTriggerCheck(currentTime);
                    break;
                case SKILL_JUDGEX_SHAPE.Annular:
                    this.AnnularTriggerCheck(currentTime);
                    break;
            }
            DrawGizmosShape(currentTime);
        }

        private void TargetExecuteTrigger()
        {
            if (TaskData.judgexType == SKILL_JUDGEX_TYPE.Auto
                || TaskData.judgexType == SKILL_JUDGEX_TYPE.ConditionSelect
                || TaskData.judgexType == SKILL_JUDGEX_TYPE.PlayerSelect)
            {
                List<CombatActorEntity> targetList = new List<CombatActorEntity>();
                if (SkillAbilityExecution.LastHitEntitysDic.Count > 0
                    && SkillAbilityExecution.LastHitEntitysDic.ContainsKey(0))
                {
                    targetList = SkillAbilityExecution.LastHitEntitysDic[0];
                }
                else
                {
                    targetList = SkillAbilityExecution.AllTargetActorEntity;
                }
                if (targetList.Count == 0)
                {
                    targetList.Add(SkillAbilityExecution.targetActorEntity);
                }
                for (int i = 0; i < targetList.Count; i++)
                {
                    TaskData.OnTriggerEnterCallback?.Invoke(targetList[i], null, TaskData.HurtRatio, TaskData.OneHitKill);
                    effectEntitys.Add(targetList[i]);
                }
            }
        }

        private void CubeTriggerCheck(float currentTime)
        {
            CombatActorEntity entity = SkillAbilityExecution.OwnerEntity;
            AnimationCurve centerDeformation = TaskData.centerDeformation;
            AnimationCurve sizeDeformation = TaskData.sizeDeformation;
            float sizeX = Mathf.Lerp(TaskData.SizeStart.x, TaskData.SizeEnd.x, Mathf.Clamp01(sizeDeformation.Evaluate(Mathf.Min(currentTime / TaskData.lifeTime, 1))));
            float sizeZ = Mathf.Lerp(TaskData.SizeStart.z, TaskData.SizeEnd.z, Mathf.Clamp01(sizeDeformation.Evaluate(Mathf.Min(currentTime / TaskData.lifeTime, 1))));
            float centerX = Mathf.Lerp(TaskData.CenterStart.x, TaskData.CenterStart.x, Mathf.Clamp01(centerDeformation.Evaluate(Mathf.Min(currentTime / TaskData.lifeTime, 1))));
            float centerZ = Mathf.Lerp(TaskData.CenterEnd.z, TaskData.CenterEnd.z, Mathf.Clamp01(centerDeformation.Evaluate(Mathf.Min(currentTime / TaskData.lifeTime, 1))));
            Vector3 cubeCenter = Vector3.zero;
            if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetAreaSelect)
            {
                cubeCenter = SkillAbilityExecution.targetActorEntity.GetPosition();
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.PreWarningAreaSelect)
            {
                cubeCenter = SkillAbilityExecution.GetWarningPoint(0);
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetDirAreaSelect)
            {
                cubeCenter = entity.GetPosition() + Quaternion.Euler(0, entity.GetEulerAngles().y, 0) * new Vector3(centerX, TaskData.CenterStart.y, centerZ + sizeZ / 2);
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.InputAreaSelect)
            {
                cubeCenter = SkillAbilityExecution.InputPoint;
            }
            else
            {
                cubeCenter = entity.GetPosition() + Quaternion.Euler(0, entity.GetEulerAngles().y, 0) * new Vector3(centerX, TaskData.CenterStart.y, centerZ + sizeZ / 2);
            }
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            entitys.Clear();
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (!enemy.Alive()
                    || effectEntitys.Contains(enemy)
                    || enemy.IsCantSelect)
                {
                    continue;
                }
                if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
                {
                    if (enemy.isAtker == SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    if (enemy.UID == SkillAbilityExecution.OwnerEntity.UID)
                    {
                        continue;
                    }
                    bool isHit = CubeHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), entity, TaskData.SizeStart, TaskData.SizeEnd, cubeCenter, centerX, centerZ, sizeX, sizeZ);
                    if (isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Team)
                {
                    if (enemy.isAtker != SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    bool isHit = CubeHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), entity, TaskData.SizeStart, TaskData.SizeEnd, cubeCenter, centerX, centerZ, sizeX, sizeZ);
                    if (isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Self)
                {
                    if (enemy.UID != SkillAbilityExecution.OwnerEntity.UID)
                    {
                        continue;
                    }
                    TaskData.OnTriggerEnterCallback?.Invoke(SkillAbilityExecution.OwnerEntity, null, TaskData.HurtRatio, TaskData.OneHitKill);
                }
            }
            for (int i = 0; i < entitys.Count; i++)
            {
                OnTriggerEnterCallback?.Invoke(entitys[i], null, TaskData.HurtRatio, TaskData.OneHitKill);
                effectEntitys.Add(entitys[i]);
            }
        }

        bool CubeHit(Vector3 targetPos, float targetHight, float targetRadiu, CombatActorEntity entity, Vector3 sizeStart, Vector3 sizeEnd, Vector3 cubeCenter, float centerX, float centerZ, float sizeX, float sizeZ)
        {
            bool isHit = false;
            Vector3 enemyPos = targetPos;
            float maxH = enemyPos.y + targetHight;
            float minH = enemyPos.y - targetHight;
            float cubeMaxH = cubeCenter.y + sizeStart.y * 0.5f;
            float cubeMinH = cubeCenter.y - sizeStart.y * 0.5f;
            if ((sizeStart.y == 0 && sizeEnd.y == 0)
                || (minH <= cubeMaxH && maxH >= cubeMinH))
            {
                Vector3 unitPos = entity.World2Local(new Vector3(enemyPos.x, 0, enemyPos.z) - entity.GetPositionXZ());
                Vector2 circleCenter = new Vector2(unitPos.x, unitPos.z);
                Vector2 rtCenter = new Vector2(centerX, centerZ + sizeZ / 2);
                Vector2 atkDir = new Vector2(Mathf.Abs(circleCenter.x - rtCenter.x), Mathf.Abs(circleCenter.y - rtCenter.y));
                Vector2 rectDia = new Vector2(sizeX / 2, sizeZ / 2);
                Vector2 v = atkDir - rectDia;
                v = new Vector2(Mathf.Max(v.x, 0), Mathf.Max(v.y, 0));
                isHit = v.magnitude < targetRadiu;
            }
            return isHit;
        }

        private void SectorTriggerCheck(float currentTime)
        {
            CombatActorEntity entity = SkillAbilityExecution.OwnerEntity;
            AnimationCurve centerDeformation = TaskData.centerDeformation;
            AnimationCurve sizeDeformation = TaskData.sizeDeformation;
            AnimationCurve rotYDeformation = TaskData.rotYDeformation;
            AnimationCurve radiusDeformation = TaskData.radiusDeformation;
            float height = TaskData.SizeStart.y;
            AnimationCurve angleDeformation = TaskData.angleDeformation;
            float duration = TaskData.lifeTime;
            float centerX = duration <= 0 ? TaskData.CenterStart.x : Mathf.Lerp(TaskData.CenterStart.x, TaskData.CenterEnd.x, Mathf.Clamp01(centerDeformation.Evaluate(currentTime / duration)));
            float centerZ = duration <= 0 ? TaskData.CenterStart.z : Mathf.Lerp(TaskData.CenterStart.z, TaskData.CenterEnd.z, Mathf.Clamp01(centerDeformation.Evaluate(currentTime / duration)));
            Vector3 sectorCenter = Vector3.zero;
            if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetAreaSelect)
            {
                sectorCenter = SkillAbilityExecution.targetActorEntity.GetPosition();
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.PreWarningAreaSelect)
            {
                sectorCenter = SkillAbilityExecution.GetWarningPoint(0);
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.InputAreaSelect)
            {
                sectorCenter = SkillAbilityExecution.InputPoint;
            }
            else
            {
                sectorCenter = entity.GetPosition() + Quaternion.Euler(entity.GetEulerAngles()) * new Vector3(centerX, TaskData.CenterStart.y, centerZ);
            }
            float curRot = duration <= 0 ? TaskData.RotYStart : Mathf.Lerp(TaskData.RotYStart, TaskData.RotYEnd, Mathf.Clamp01(rotYDeformation.Evaluate(currentTime / duration)));
            Vector3 sectorDir = (Quaternion.Euler(0, curRot, 0) * entity.GetForward()).normalized;
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            entitys.Clear();
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (!enemy.Alive()
                    || effectEntitys.Contains(enemy)
                    || enemy.IsCantSelect)
                {
                    continue;
                }
                if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
                {
                    if (enemy.isAtker == SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    if (enemy.UID == SkillAbilityExecution.OwnerEntity.UID)
                    {
                        continue;
                    }
                    bool isHit = SectorCubeHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), entity, TaskData.SizeStart, TaskData.SizeEnd, sectorCenter, height, sectorDir, duration, TaskData.RadiusStart, TaskData.RadiusEnd, TaskData.AngleStart, TaskData.AngleEnd, currentTime, radiusDeformation, angleDeformation);
                    if (isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Team)
                {
                    if (enemy.isAtker != SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    bool isHit = SectorCubeHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), entity, TaskData.SizeStart, TaskData.SizeEnd, sectorCenter, height, sectorDir, duration, TaskData.RadiusStart, TaskData.RadiusEnd, TaskData.AngleStart, TaskData.AngleEnd, currentTime, radiusDeformation, angleDeformation);
                    if (isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Self)
                {
                    if (enemy.UID != SkillAbilityExecution.OwnerEntity.UID)
                    {
                        continue;
                    }
                    TaskData.OnTriggerEnterCallback?.Invoke(SkillAbilityExecution.OwnerEntity, null, TaskData.HurtRatio, TaskData.OneHitKill);
                }
            }
            for (int i = 0; i < entitys.Count; i++)
            {
                OnTriggerEnterCallback?.Invoke(entitys[i], null, TaskData.HurtRatio, TaskData.OneHitKill);
                effectEntitys.Add(entitys[i]);
            }
        }

        bool SectorCubeHit(Vector3 targetPos, float targetHight, float targetRadiu, CombatActorEntity entity, Vector3 sizeStart, Vector3 sizeEnd, Vector3 sectorCenter, float height, Vector3 sectorDir, float duration, float radiusStart, float radiusEnd, float angleStart, float angleEnd, float currentTime, AnimationCurve radiusDeformation, AnimationCurve angleDeformation)
        {
            bool isHit = false;
            Vector3 enemyPos = targetPos;
            float maxH = enemyPos.y + targetHight;
            float minH = enemyPos.y - targetHight;
            float sectorMaxH = sectorCenter.y + height * 0.5f;
            float sectorMinH = sectorCenter.y - height * 0.5f;
            if ((sizeStart.y == 0 && sizeEnd.y == 0)
                || (maxH >= sectorMinH && minH <= sectorMaxH))
            {
                float curRadius = duration <= 0 ? radiusStart : Mathf.Lerp(radiusStart, radiusEnd, Mathf.Clamp01(radiusDeformation.Evaluate(currentTime / duration)));
                float curAngle = duration <= 0 ? angleStart : Mathf.Lerp(angleStart, angleEnd, Mathf.Clamp01(angleDeformation.Evaluate(currentTime / duration)));
                isHit = SectorCircle(new Vector2(enemyPos.x, enemyPos.z), targetRadiu, new Vector2(sectorCenter.x, sectorCenter.z), curAngle, curRadius, new Vector2(sectorDir.x, sectorDir.z));
            }
            return isHit;
        }

        private void CylinderTriggerCheck(float currentTime)
        {
            CombatActorEntity entity = SkillAbilityExecution.OwnerEntity;
            AnimationCurve centerDeformation = TaskData.centerDeformation;
            AnimationCurve sizeDeformation = TaskData.sizeDeformation;
            AnimationCurve radiusDeformation = TaskData.radiusDeformation;
            float duration = TaskData.lifeTime;
            float centerX = duration <= 0 ? TaskData.CenterStart.x : Mathf.Lerp(TaskData.CenterStart.x, TaskData.CenterEnd.x, Mathf.Clamp01(centerDeformation.Evaluate(currentTime / duration)));
            float centerZ = duration <= 0 ? TaskData.CenterStart.z : Mathf.Lerp(TaskData.CenterStart.z, TaskData.CenterEnd.z, Mathf.Clamp01(centerDeformation.Evaluate(currentTime / duration)));
            Vector3 cylinderCenter = Vector3.zero;
            if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetAreaSelect)
            {
                cylinderCenter = SkillAbilityExecution.targetActorEntity.GetPosition();
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.PreWarningAreaSelect)
            {
                cylinderCenter = SkillAbilityExecution.GetWarningPoint(0);
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.InputAreaSelect)
            {
                cylinderCenter = SkillAbilityExecution.InputPoint;
            }
            else
            {
                cylinderCenter = entity.GetPosition() + Quaternion.Euler(entity.GetEulerAngles()) * new Vector3(centerX, TaskData.CenterStart.y, centerZ);
            }
            float height = TaskData.SizeStart.y;
            float curRadius = duration <= 0 ? TaskData.RadiusStart : Mathf.Lerp(TaskData.RadiusStart, TaskData.RadiusEnd, Mathf.Clamp01(radiusDeformation.Evaluate(currentTime / duration)));
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            entitys.Clear();
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (!enemy.Alive()
                    || effectEntitys.Contains(enemy)
                    || enemy.IsCantSelect)
                {
                    continue;
                }
                if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
                {
                    if (enemy.isAtker == SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    bool isHit = CylinderHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), enemy.GetPositionXZ(), enemy, TaskData.SizeStart, TaskData.SizeEnd, cylinderCenter, height, curRadius);
                    if (isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Team)
                {
                    if (enemy.isAtker != SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    bool isHit = CylinderHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), enemy.GetPositionXZ(), enemy, TaskData.SizeStart, TaskData.SizeEnd, cylinderCenter, height, curRadius);
                    if (isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Self)
                {
                    TaskData.OnTriggerEnterCallback?.Invoke(SkillAbilityExecution.OwnerEntity, null, TaskData.HurtRatio, TaskData.OneHitKill);
                }
            }
            for (int i = 0; i < entitys.Count; i++)
            {
                OnTriggerEnterCallback?.Invoke(entitys[i], null, TaskData.HurtRatio, TaskData.OneHitKill);
                effectEntitys.Add(entitys[i]);
            }
        }

        bool CylinderHit(Vector3 targetPos, float targetHight, float targetRadiu, Vector3 targetPosXZ, CombatUnitEntity entity, Vector3 sizeStart, Vector3 sizeEnd, Vector3 cylinderCenter, float height, float curRadius)
        {
            bool isHit = false;
            Vector3 enemyPos = targetPos;
            float rangeValue = curRadius + entity.GetTouchRadiu();
            float maxH = enemyPos.y + targetHight;
            float minH = enemyPos.y - targetHight;
            float cylinderMaxH = cylinderCenter.y + height * 0.5f;
            float cylinderMinH = cylinderCenter.y - height * 0.5f;
            if ((sizeStart.y == 0 && sizeEnd.y == 0)
                || (minH <= cylinderMaxH && maxH >= cylinderMinH))
            {
                isHit = (rangeValue * rangeValue) >= MathHelper.DoubleDistanceVect3(targetPosXZ, new Vector3(cylinderCenter.x, 0, cylinderCenter.z));
            }
            return isHit;
        }

        /// <summary>
        /// 环形碰撞检查
        /// </summary>
        /// <param name="currentTime"></param>
        private void AnnularTriggerCheck(float currentTime)
        {
            CombatActorEntity entity = SkillAbilityExecution.OwnerEntity;
            AnimationCurve centerDeformation = TaskData.centerDeformation;
            AnimationCurve sizeDeformation = TaskData.sizeDeformation;
            AnimationCurve radiusDeformation = TaskData.radiusDeformation;
            float duration = TaskData.lifeTime;
            float centerX = duration <= 0 ? TaskData.CenterStart.x : Mathf.Lerp(TaskData.CenterStart.x, TaskData.CenterEnd.x, Mathf.Clamp01(centerDeformation.Evaluate(currentTime / duration)));
            float centerZ = duration <= 0 ? TaskData.CenterStart.z : Mathf.Lerp(TaskData.CenterStart.z, TaskData.CenterEnd.z, Mathf.Clamp01(centerDeformation.Evaluate(currentTime / duration)));
            Vector3 cylinderCenter = Vector3.zero;
            if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetAreaSelect)
            {
                cylinderCenter = SkillAbilityExecution.targetActorEntity.GetPosition();
            }
            else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.PreWarningAreaSelect)
            {
                cylinderCenter = SkillAbilityExecution.GetWarningPoint(0);
            }
            else
            {
                cylinderCenter = entity.GetPosition() + Quaternion.Euler(entity.GetEulerAngles()) * new Vector3(centerX, TaskData.CenterStart.y, centerZ);
            }
            float height = TaskData.SizeStart.y;
            float curRadius = duration <= 0 ? TaskData.RadiusStart : Mathf.Lerp(TaskData.RadiusStart, TaskData.RadiusEnd, Mathf.Clamp01(radiusDeformation.Evaluate(currentTime / duration)));
            float curOuterRadius = duration <= 0 ? TaskData.OuterRadiusStart : Mathf.Lerp(TaskData.OuterRadiusStart, TaskData.OuterRadiusEnd, Mathf.Clamp01(radiusDeformation.Evaluate(currentTime / duration)));
            var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
            entitys.Clear();
            while (data.MoveNext())
            {
                CombatActorEntity enemy = data.Current.Value;
                if (!enemy.Alive()
                    || effectEntitys.Contains(enemy)
                    || enemy.IsCantSelect)
                {
                    continue;
                }
                if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
                {
                    if (enemy.isAtker == SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    bool isHit = CylinderHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), enemy.GetPositionXZ(), enemy, TaskData.SizeStart, TaskData.SizeEnd, cylinderCenter, height, curRadius);
                    bool isOuterHit = CylinderHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), enemy.GetPositionXZ(), enemy, TaskData.SizeStart, TaskData.SizeEnd, cylinderCenter, height, curOuterRadius);
                    if (isOuterHit && !isHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Team)
                {
                    if (enemy.isAtker != SkillAbilityExecution.OwnerEntity.isAtker)
                    {
                        continue;
                    }
                    bool isHit = CylinderHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), enemy.GetPositionXZ(), enemy, TaskData.SizeStart, TaskData.SizeEnd, cylinderCenter, height, curRadius);
                    bool isOuterHit = CylinderHit(enemy.GetPosition() + enemy.GetCenter(), enemy.GetTouchHight() / 2, enemy.GetHitRadiu(), enemy.GetPositionXZ(), enemy, TaskData.SizeStart, TaskData.SizeEnd, cylinderCenter, height, curOuterRadius);
                    if (!isHit && isOuterHit)
                    {
                        entitys.Add(enemy);
                    }
                }
                else if (TaskData.affectTargetType == SKILL_AFFECT_TARGET_TYPE.Self)
                {
                    TaskData.OnTriggerEnterCallback?.Invoke(SkillAbilityExecution.OwnerEntity, null, TaskData.HurtRatio, TaskData.OneHitKill);
                }
            }
            for (int i = 0; i < entitys.Count; i++)
            {
                OnTriggerEnterCallback?.Invoke(entitys[i], null, TaskData.HurtRatio, TaskData.OneHitKill);
                effectEntitys.Add(entitys[i]);
            }
        }

        /// <summary>
        /// 扇形检查
        /// </summary>
        /// <param name="circleCenter"></param>
        /// <param name="circleRadius"></param>
        /// <param name="sectorCenter"></param>
        /// <param name="sectorAngle"></param>
        /// <param name="sectorRadius"></param>
        /// <param name="sectorDir"></param>
        /// <returns></returns>
        bool SectorCircle(Vector2 circleCenter, float circleRadius, Vector2 sectorCenter, float sectorAngle, float sectorRadius, Vector2 sectorDir)
        {
            float distance = Vector2.Distance(sectorCenter, circleCenter); //距离
            Vector2 temVec = circleCenter - sectorCenter;
            Vector2 fromDir = Quaternion.Euler(0, -sectorAngle / 2, 0) * sectorDir.normalized;
            Vector2 toDir = Quaternion.Euler(0, sectorAngle / 2, 0) * sectorDir.normalized;
            float angle = Mathf.Acos(Vector3.Dot(sectorDir.normalized, temVec.normalized)) * Mathf.Rad2Deg;
            if (distance < sectorRadius + circleRadius)
            {
                if (angle <= sectorAngle / 2)
                {
                    return true;
                }
            }
            if (LineCircle(sectorCenter, sectorCenter + fromDir * sectorRadius, circleCenter, circleRadius))
            {
                return true;
            }
            if (LineCircle(sectorCenter, sectorCenter + toDir * sectorRadius, circleCenter, circleRadius))
            {
                return true;
            }
            return false;
        }

        bool LineCircle(Vector2 p1, Vector2 p2, Vector2 circleCenter, float radius)
        {
            var value1 = PointInCircle(p1, circleCenter, radius);
            var value2 = PointInCircle(p2, circleCenter, radius);
            if (value1 && value2)
            {
                return false;
            }
            else if (value1 || value2)
            {
                return true;
            }
            else
            {
                float a, b, c, dist1, dist2, angle1, angle2;
                if (p1.x == p2.x)
                {
                    a = 1;
                    b = 0;
                    c = -p1.x;
                }
                else if (p1.y == p2.y)
                {
                    a = 0;
                    b = 1;
                    c = -p1.y;
                }
                else
                {
                    a = p1.y - p2.y;
                    b = p2.x - p1.x;
                    c = p1.x * p2.y - p1.y * p2.x;
                }
                dist1 = a * circleCenter.x + b * circleCenter.y + c;
                dist1 *= dist1;
                dist2 = (a * a + b * b) * radius * radius;
                if (dist1 > dist2)
                    return false;
                angle1 = (circleCenter.x - p1.x) * (p2.x - p1.x) + (circleCenter.y - p1.y) * (p2.y - p1.y);
                angle2 = (circleCenter.x - p2.x) * (p1.x - p2.x) + (circleCenter.y - p2.y) * (p1.y - p2.y);
                if (angle1 > 0
                    && angle2 > 0)
                    return true;
                return false;
            }
        }

        bool PointInCircle(Vector2 point, Vector2 circleCenter, float radius)
        {
            return Vector2.Distance(point, circleCenter) <= radius;
        }

        void DrawGizmosShape(float currentTime)
        {
            // #if !SERVER
            //             if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            //             {
            //                 return;
            //             }
            //             if (SkillAbilityExecution.SkillAbility.SkillConfigObject.ID != 25023)
            //             {
            //                 return;
            //             }
            //             if (SkillAbilityExecution.OwnerEntity == null)
            //             {
            //                 return;
            //             }
            //             GameObject go = GameObject.Find(SkillAbilityExecution.Id.ToString());
            //             GizmosShape judgeGizmos;
            //             if (go == null)
            //             {
            //                 judgeGizmos = new GameObject(SkillAbilityExecution.Id.ToString()).AddComponent<GizmosShape>();
            //             }
            //             else
            //             {
            //                 judgeGizmos = go.GetComponent<GizmosShape>();
            //             }
            //             if (go == null)
            //             {
            //                 return;
            //             }
            //             if (judgeGizmos)
            //             {
            //                 judgeGizmos.type = TaskData.JudgeShapeType;
            //                 judgeGizmos.Size = Vector3.Lerp(TaskData.SizeStart, TaskData.SizeEnd, Mathf.Clamp01(1));
            //                 if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetAreaSelect)
            //                 {
            //                     judgeGizmos.Center = SkillAbilityExecution.targetActorEntity.GetPosition();
            //                 }
            //                 else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.PreWarningAreaSelect)
            //                 {
            //                     judgeGizmos.Center = SkillAbilityExecution.GetWarningPoint(0);
            //                 }
            //                 else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.TargetDirAreaSelect)
            //                 {
            //                     Creature actor = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
            //                     Transform trans = string.IsNullOrEmpty(TaskData.AttachPart) ? actor.transform : GameObjectHelper.FindChild(actor.transform, TaskData.AttachPart);
            //                     go.transform.position = trans.position;
            //                     go.transform.eulerAngles = trans.GetEulerAngle();
            //                     judgeGizmos.Center = Quaternion.Euler(0, trans.GetEulerAngle().y, 0) * new Vector3(TaskData.CenterStart.x, TaskData.CenterStart.y, TaskData.CenterStart.z);
            //                 }
            //                 else if (TaskData.judgexType == SKILL_JUDGEX_TYPE.InputAreaSelect)
            //                 {
            //                     judgeGizmos.Center = SkillAbilityExecution.InputPoint;
            //                 }
            //                 else
            //                 {
            //                     judgeGizmos.Center = SkillAbilityExecution.OwnerEntity.GetPosition() + Quaternion.Euler(0, SkillAbilityExecution.OwnerEntity.GetEulerAngles().y, 0) * new Vector3(TaskData.CenterStart.x, TaskData.CenterStart.y, TaskData.CenterStart.z + TaskData.SizeStart.z / 2);
            //                 }
            //                 judgeGizmos.Angle = Mathf.Lerp(TaskData.AngleStart, TaskData.AngleEnd, 1);
            //                 judgeGizmos.Radius = Mathf.Lerp(TaskData.RadiusStart, TaskData.RadiusEnd, Mathf.Clamp01(TaskData.radiusDeformation.Evaluate(currentTime / TaskData.lifeTime)));
            //                 judgeGizmos.OuterRadius = Mathf.Lerp(TaskData.OuterRadiusStart, TaskData.OuterRadiusEnd, 1);
            //                 judgeGizmos.RotY = Mathf.Lerp(TaskData.RotYStart, TaskData.RotYEnd, 1);
            //             }
            // #endif
        }
    }
}
namespace BattleEngine.Logic
{
    using System.Collections.Generic;
    using UnityEngine;

#if !SERVER
    using DG.Tweening;
    using View;
#endif

    public class BulletEntity : CombatUnitEntity
    {
        public CastTrajectoryTaskData taskData;
        private List<CombatUnitEntity> effectiveEntities = new List<CombatUnitEntity>();
        public Vector3 curPos, curDir, startPos, endPos, firstHitCenter;
        public float curDis, curMoveTime, bulletHeight;
        public CombatActorEntity Owner;
        public CombatActorEntity target;
        public SkillAbility skillAbility;
        private bool isBack;
#if !SERVER
        private GameObject bullet;
#endif
        private bool hit = false;
        private bool needCheckTrigger = true;

        public override void Awake(object initData)
        {
            taskData = (CastTrajectoryTaskData)initData;
            bulletHeight = taskData.colliderRadius;
            effectiveEntities.Clear();
            Owner = GetParent<CombatActorEntity>();
            BattleLogicManager.Instance.BattleData.bulletList.Add(this);
        }

        public override void Born(Vector3 pos, Vector3 rot, float _size)
        {
            effectiveEntities.Clear();
            if (taskData.trajectoryType == TRAJECTORYITEM_TYPE.Parabola)
            {
                if (taskData.selectPosType == TRAJECTORYITEM_POS_TYPE.DistancePos)
                {
                    startPos = pos;
                    curPos = pos;
                    endPos = startPos + Owner.GetForward() * taskData.flyMaxDist;
                }
                else
                {
                    startPos = pos;
                    curPos = pos;
                    if (target != null)
                    {
                        endPos = target.GetPosition();
                    }
                }
            }
            else if (taskData.trajectoryType == TRAJECTORYITEM_TYPE.Free)
            {
                curPos = pos;
                curDir = rot;
            }
            else if (taskData.trajectoryType == TRAJECTORYITEM_TYPE.SelectPos)
            {
                curPos = pos;
                curDir = (endPos - curPos).normalized;
            }
            else if (taskData.trajectoryType == TRAJECTORYITEM_TYPE.SelectTarget)
            {
                curPos = pos;
                curDir = Owner.GetForward();
                if (target != null)
                {
                    Vector3 targetCenter = target.GetPosition() + target.GetCenter();
                    Vector3 dirOffset = targetCenter - curPos;
                    dirOffset.y = 0;
                    curDir = dirOffset.normalized;
                }
            }
            else
            {
                curPos = pos;
                curDir = Owner.GetForward();
            }
            base.Born(curPos, rot);
            SetForward(curDir);
            SetLifeState(ACTOR_LIFE_STATE.Alive);
            curDis = 0;
            isBack = false;
            if (taskData.flyFixedTime)
            {
                taskData.flySpeed = MathHelper.DistanceVec3(endPos, new Vector3(pos.x, 0, pos.z)) / (taskData.flyTime * 0.001f);
            }
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                this.GameObject.transform.position = pos;
                CreatBullet();
            }
#endif
        }

        public void Stop()
        {
            needCheckTrigger = false;
        }

        public async void CreatBullet()
        {
#if !SERVER
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            if (taskData == null
                || string.IsNullOrEmpty(taskData.effectPath))
                return;
            bullet = await BattleResManager.Instance.CreatorFx(taskData.effectPath, this.GameObject.transform, Vector3.zero);
            if (bullet == null)
                return;
            if (GameObject == null)
            {
                BattleResManager.Instance.RecycleEffect(taskData.effectPath, bullet);
                return;
            }
            bullet.transform.localScale = taskData.effectScale;
            bullet.transform.localRotation = Quaternion.Euler(Vector3.zero);
            bullet.SetActive(true);
#endif
        }

        public override void LogicUpdate(float LogicDeltaTime)
        {
            if (ShieldWallCtr.BulletCheckTrigger(this))
            {
                Destroy(this);
                return;
            }
            if (Alive() && needCheckTrigger)
            {
                TriggerCheck();
            }
            if (Alive()) { }
            MoveCheck(LogicDeltaTime);
            CheckBulletDestory();
        }

        private void TriggerCheck()
        {
            hit = false;
            if (target == null)
            {
                return;
            }
            if (taskData == null
                || taskData.trajectoryType == TRAJECTORYITEM_TYPE.SelectTarget && target.CurrentHealth.Value <= 0)
            {
                CreatDot();
                Destroy(this);
                return;
            }
            if (taskData.trajectoryType == TRAJECTORYITEM_TYPE.SelectTarget
                && Owner.UID == target.UID)
            {
                effectiveEntities.Add(Owner);
                taskData.OnTriggerEnterCallback?.Invoke(Owner, null, taskData.hurtRatio);
                hit = true;
                firstHitCenter = Owner.GetPosition();
                if (taskData.trajectoryType != TRAJECTORYITEM_TYPE.Back
                    && !taskData.isPenetrate) { }
            }
            else
            {
                var data = BattleLogicManager.Instance.BattleData.allActorDic.GetEnumerator();
                while (data.MoveNext())
                {
                    CombatActorEntity enemy = data.Current.Value;
                    if (taskData.onlyHurtTarget
                        && enemy.UID != target.UID)
                    {
                        continue;
                    }
                    if (skillAbility != null
                        && skillAbility.SkillConfigObject.AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Team)
                    {
                        if (enemy.isAtker != Owner.isAtker
                            || enemy.Id == Owner.Id)
                        {
                            continue;
                        }
                    }
                    else if (skillAbility != null
                             && skillAbility.SkillConfigObject.AffectTargetType == SKILL_AFFECT_TARGET_TYPE.Enemy)
                    {
                        if (enemy.isAtker == Owner.isAtker)
                        {
                            continue;
                        }
                    }
                    else if (Owner.UID == target.UID)
                    {
                        continue;
                    }
                    if (taskData.trajectoryType == TRAJECTORYITEM_TYPE.SelectTarget)
                    {
                        if (enemy.UID != target.UID)
                        {
                            continue;
                        }
                    }
                    if (enemy.Alive()
                        && !effectiveEntities.Contains(enemy)
                        && !enemy.IsCantSelect)
                    {
                        Vector3 unitCenter = enemy.GetPosition() + enemy.GetCenter();
                        Vector3 bulletCenter = curPos;
                        float curRadius = taskData.colliderRadius + enemy.GetHitRadiu();
                        if (Mathf.Max(unitCenter.y + enemy.GetTouchHight() / 2, bulletCenter.y + bulletHeight / 2) - Mathf.Min(unitCenter.y - enemy.GetTouchHight() / 2, bulletCenter.y - bulletHeight / 2) < bulletHeight + enemy.GetTouchHight())
                        {
                            if (MathHelper.DoubleDistanceVect3(GetPositionXZ(), enemy.GetPositionXZ()) <= curRadius * curRadius)
                            {
                                effectiveEntities.Add(enemy);
                                taskData.OnTriggerEnterCallback?.Invoke(enemy, null, taskData.hurtRatio);
                                hit = true;
                                firstHitCenter = enemy.GetPosition();
                                if (taskData.trajectoryType != TRAJECTORYITEM_TYPE.Back
                                    && !taskData.isPenetrate) { }
                            }
                        }
                    }
                }
            }
            if (hit)
            {
                CreatDestoryEffect(curPos + GetForward() * bulletHeight);
            }
            if (hit
                && taskData.trajectoryType != TRAJECTORYITEM_TYPE.Back
                && !taskData.isPenetrate)
            {
                CreatDot();
                Destroy(this);
            }
        }

        private void MoveCheck(float delta)
        {
            curMoveTime += delta;
            float movedTime = curMoveTime;
            Vector3 atkDir = curDir;
            switch (taskData.trajectoryType)
            {
                case TRAJECTORYITEM_TYPE.SelectPos:
                case TRAJECTORYITEM_TYPE.Free:
                    atkDir = curDir;
                    Move(atkDir, movedTime, delta);
                    break;
                case TRAJECTORYITEM_TYPE.SelectTarget:
                    if (target != null
                        && target.Alive()
                        && MathHelper.DoubleDistanceVect3(GetPositionXZ(), target.GetPositionXZ() + target.GetCenter()) > bulletHeight * bulletHeight)
                    {
                        Vector3 targetCenter = target.GetPosition() + target.GetCenter();
                        atkDir = (targetCenter - curPos).normalized;
                    }
                    Move(atkDir, movedTime, delta);
                    break;
                case TRAJECTORYITEM_TYPE.Back:
                    MoveBack(delta);
                    break;
                case TRAJECTORYITEM_TYPE.Parabola:
                    ParabolaMove();
                    break;
            }
            if (effectiveEntities.Count > 0
                && taskData.trajectoryType != TRAJECTORYITEM_TYPE.Back
                && !taskData.isPenetrate)
            {
                Destroy(this);
            }
        }

        private void Move(Vector3 atkDir, float movedTime, float delta)
        {
            curDir = atkDir;
            float currentTime = movedTime;
            float maxTime = taskData.cureTime * 0.001f;
            float CureValue = maxTime > 0 ? taskData.speedCurve.Evaluate(currentTime / maxTime) : 1;
            Vector3 moveDelta = atkDir * CureValue * taskData.flySpeed * delta;
            SetPosition(GetPosition() + moveDelta);
            curPos += moveDelta;
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                if (this != null
                    && this.GameObject != null
                    && this.GameObject.transform != null
                    && CurrentLifeState != ACTOR_LIFE_STATE.Dead)
                {
                    this.GameObject.transform.LookAt(this.GameObject.transform.position + moveDelta);
                    this.GameObject.transform.DOMove(curPos, MathHelper.DistanceVec3(curPos, this.GameObject.transform.position) / (CureValue * taskData.flySpeed)).SetEase(Ease.Linear);
                }
                else
                {
                    if (bullet != null)
                    {
                        BattleResManager.Instance.RecycleEffect(taskData.effectPath, bullet);
                    }
                }
            }
#endif
            if (taskData.flyMaxDist > 0)
            {
                Vector3 dir = new Vector3(GetPosition().x - curPos.x, 0, GetPosition().z - curPos.z);
                curDis += dir.magnitude;
                if (curDis > taskData.flyMaxDist)
                {
                    Destroy(this);
                }
            }
        }

        private void MoveBack(float deltaTime)
        {
            if (!isBack)
            {
                float maxTime = taskData.flyMaxDist / taskData.flySpeed;
                float precent1 = taskData.speedCurve.Evaluate(curMoveTime / maxTime);
                SetPosition(GetPosition() + (curDir * taskData.flySpeed * deltaTime));
                curPos += (curDir * taskData.flySpeed * deltaTime);
                Vector3 dir = new Vector3(GetPosition().x - curPos.x, 0, GetPosition().z - curPos.z);
#if !SERVER
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    this.GameObject.transform.position += (curDir * taskData.flySpeed * deltaTime); //*precent1;
                    this.GameObject.transform.LookAt(dir);
                }
#endif
                curDis += dir.magnitude;
                if (curDis > taskData.flyMaxDist)
                {
                    isBack = true;
                }
            }
            else
            {
                if ((Owner.GetPosition() - GetPosition()).magnitude < 0.5)
                {
                    Destroy(this);
                    return;
                }
                float maxTime = taskData.flyMaxDist / taskData.flySpeed;
                float precent1 = taskData.speedCurve.Evaluate(curMoveTime / maxTime);
                Vector3 dir = (Owner.GetPosition() - GetPosition()).normalized;
                SetPosition(GetPosition() + dir * taskData.flySpeed * deltaTime * precent1);
#if !SERVER
                if (BattleLogicManager.Instance.IsOpenBattleViewLayer)
                {
                    this.GameObject.transform.position += dir * taskData.flySpeed * deltaTime * precent1;
                    this.GameObject.transform.LookAt(dir);
                }
#endif
                curDis += (dir * taskData.flySpeed * deltaTime * precent1).magnitude;
                if (curDis > taskData.flyMaxDist)
                {
                    Destroy(this);
                }
            }
        }

        private void ParabolaMove()
        {
            if ((endPos - GetPosition()).magnitude < 0.1f)
            {
                CreatDot();
                Destroy(this);
                return;
            }
            float hDistance = (endPos - startPos).magnitude;
            float vDistance = ParabolHeight(hDistance, taskData.parabolaHeight);
            float maxTime = (endPos - startPos).magnitude / taskData.flySpeed;
            float height = taskData.parabolaCurve.Evaluate(curMoveTime / maxTime) * taskData.parabolaHeight;
            curPos = Vector3.Lerp(startPos, endPos, Mathf.Clamp01(curMoveTime / maxTime)) + Vector3.up * height;
            SetPosition(curPos);
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer
                && this.GameObject != null)
            {
                Vector3 dir = (curPos - this.GameObject.transform.position).normalized;
                this.GameObject.transform.LookAt(curPos);
                if (this != null
                    && this.GameObject != null
                    && this.GameObject.transform != null
                    && CurrentLifeState != ACTOR_LIFE_STATE.Dead)
                {
                    this.GameObject.transform.DOMove(curPos, BattleLogicDefine.LogicSecTime);
                }
                else
                {
                    if (bullet != null)
                    {
                        BattleResManager.Instance.RecycleEffect(taskData.effectPath, bullet);
                    }
                }
            }
#endif
            if (curMoveTime >= maxTime)
            {
                CreatDot();
                Destroy(this);
            }
        }

        float ParabolHeight(float hDistance, float ori_vDistance)
        {
            float vDistance = ori_vDistance;
            vDistance = hDistance * 0.2f;
            return vDistance;
        }

        public override void OnDestroy()
        {
#if !SERVER
            if (BattleLogicManager.Instance.IsOpenBattleViewLayer
                && bullet != null)
            {
                bullet.transform.parent = null;
                TimerMgr.Instance.BattleSchedulerTimerDelay(taskData.destoryDelay, () => { BattleResManager.Instance.RecycleEffect(taskData.effectPath, bullet); });
            }
#endif
            SetLifeState(ACTOR_LIFE_STATE.Dead);
            if (BattleLogicManager.Instance.BattleData != null)
                BattleLogicManager.Instance.BattleData.bulletList.Remove(this);
            base.OnDestroy();
        }

        private Vector3 bornPos = Vector3.zero;

        public void CreatDot()
        {
            if (taskData.dotElement != null)
            {
                string path = "";
                if (!string.IsNullOrEmpty(taskData.dotElement.effectPrefab))
                {
                    path = AddressablePathConst.SkillEditorPathParse(taskData.dotElement.effectPrefab);
                }
                bornPos.x = curPos.x;
                if (Owner != null)
                {
                    bornPos.y = Owner.GetPosition().y;
                }
                else
                {
                    bornPos.y = 0;
                }
                bornPos.z = curPos.z;
                DotCtr.CreatDot(bornPos, Vector3.zero, taskData.dotElement.scale, path, taskData.dotElement.durationTime, taskData.dotElement.intervalTime, taskData.dotElement.radius, 1, taskData.dotElement.Effects, Owner, skillAbility.SkillConfigObject.AffectTargetType, taskData.dotElement.effectiveOnce, taskData.dotElement.applyEffectDirect);
            }
        }

        public async void CreatDestoryEffect(Vector3 pos)
        {
#if !SERVER
            if (!BattleLogicManager.Instance.IsOpenBattleViewLayer)
            {
                return;
            }
            if (skillAbility.SkillBaseConfig.SkillID > 0 && hit)
            {
                Creature creature = BattleManager.Instance.ActorMgr.GetActor(skillAbility.OwnerEntity.UID);
                var aa = StrBuild.Instance.ToStringAppend("skill_", skillAbility.SkillBaseConfig.SkillID.ToString());
                WwiseEventManager.SendEvent(TransformTable.SkillHit, aa, creature.gameObject);
            }
            if (hit && effectiveEntities.Count > 0)
            {
                for (int i = 0; i < effectiveEntities.Count; i++)
                {
                    CombatActorEntity hitEnemy = effectiveEntities[i] as CombatActorEntity;
                    if (hitEnemy == null)
                    {
                        continue;
                    }
                    Creature hitCreature = BattleManager.Instance.ActorMgr.GetActor(hitEnemy.UID);
                    WwiseEventManager.SendEvent(TransformTable.HeroVoice_Hit, hitEnemy.ConfigID.ToString(), hitCreature.gameObject);
                }
            }
            if (!string.IsNullOrEmpty(taskData.destroyEffect))
            {
                GameObject destroyEffect = await BattleResManager.Instance.CreatorFx(taskData.destroyEffect);
                if (destroyEffect == null)
                {
                    return;
                }
                destroyEffect.transform.position = pos;
                destroyEffect.transform.localScale = taskData.destoryEffectScale;
            }
#endif
        }

        public void CheckBulletDestory()
        {
            if (taskData.destoryTime == 0)
            {
                return;
            }
            if (curMoveTime > taskData.destoryTime)
            {
                Destroy(this);
            }
        }
    }
}
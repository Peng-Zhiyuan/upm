/* Created:Loki Date:2023-02-06*/

#if !SERVER
using BattleEngine.View;
#endif

namespace BattleEngine.Logic
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public partial class SkillAbilityExecution
    {
        public AbilityTaskData CreateAbilityTaskData(SKILL_ACTION_ELEMENT_TYPE skillElementItemType)
        {
            var type = SkillAbilityHelp.Instance.GetAbilityTaskData(skillElementItemType);
            if (type == null)
            {
                return null;
            }
            object obj = Activator.CreateInstance(type);
            return obj as AbilityTaskData;
        }

        public AbilityTask CreateAbilityLogicTask(AbilityTaskData abilityTaskData)
        {
            if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Trajectory)
            {
                var taskData = abilityTaskData as CastTrajectoryTaskData;
                if (taskData == null)
                {
                    return null;
                }
                taskData.OnTriggerEnterCallback = (other, part, hurtRatio) => { AbilityEntity.ApplyAbilityEffectsTo(other, part, hurtRatio); };
                return CreateWithParent<SkillCastTrajectoryActionTask>(this, taskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.AttackBox)
            {
                var taskData = abilityTaskData as CreateTriggerTaskData;
                if (taskData == null)
                {
                    return null;
                }
                taskData.HitIndex = this.HitIndex;
                taskData.affectTargetType = SkillAbility.SkillConfigObject.AffectTargetType;
                taskData.OnTriggerEnterCallback = (other, part, hurtRatio, kill) =>
                {
                    if (LastHitEntitysDic.ContainsKey(taskData.HitIndex))
                    {
                        LastHitEntitysDic[taskData.HitIndex].Add(other);
                    }
                    else
                    {
                        LastHitEntitysDic[taskData.HitIndex] = new List<CombatActorEntity>();
                        LastHitEntitysDic[taskData.HitIndex].Add(other);
                    }
                    AbilityEntity.ApplyAbilityEffectsTo(other, part, hurtRatio, kill);
                    if (HitEffectTaskDataLst.Count > 0)
                    {
                        if (taskData.HitIndex >= HitEffectTaskDataLst.Count)
                        {
                            SkillCtr.SkillCreatHitEffect(other, HitEffectTaskDataLst[0]);
                        }
                        else
                        {
                            SkillCtr.SkillCreatHitEffect(other, HitEffectTaskDataLst[taskData.HitIndex]);
                        }
                    }
                };
                taskData.lifeTime = Mathf.FloorToInt((abilityTaskData.endFrame - abilityTaskData.startFrame) * SkillAbility.SkillFrameRate * 1000f);
                var task = CreateWithParent<SkillCreateTriggerActionTask>(this, taskData);
                HitIndex += 1;
                return task;
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Move)
            {
                var taskData = abilityTaskData as MoveActionTaskData;
                if (taskData == null)
                {
                    return null;
                }
                taskData.InputTargetPos = SkillAbility.InputTargetPos;
                return CreateWithParent<SkillMoveActionTask>(this, taskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Dot)
            {
                return CreateWithParent<SkillCreatDotActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.ShieldWall)
            {
                return CreateWithParent<SkillCreatShieldWallActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Charge)
            {
                return CreateWithParent<SkillChargeActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Summoning)
            {
                return CreateWithParent<SkillSummoningActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.BattleEvent)
            {
                return CreateWithParent<SkillBattleEventActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.BeatBack)
            {
                return CreateWithParent<SkillBeatBackActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Jump)
            {
                return CreateWithParent<SkillJumpActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.AirBorne)
            {
                return CreateWithParent<SkillAirBorneActionTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.TargetTogether)
            {
                return CreateWithParent<SkillTargetTogetherTask>(this, abilityTaskData);
            }
            return null;
        }

        public AbilityViewTask CreateAbilityViewTask(AbilityTaskData abilityTaskData)
        {
#if !SERVER
            if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Animation)
            {
                return CreateWithParent<SkillPlayAnimationActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Effect)
            {
                var taskData = abilityTaskData as SkillCreateEffectTaskData;
                if (string.IsNullOrEmpty(taskData.effectPrefabName))
                {
                    return null;
                }
                if (taskData.isSelfPlay)
                {
                    var creature = BattleManager.Instance.ActorMgr.GetActor(OwnerEntity.UID);
                    if (creature != null
                        && !creature.Selected)
                    {
                        return null;
                    }
                }
                return CreateWithParent<SkillCreateEffectActionViewTask>(this, taskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.Explosion)
            {
                var taskData = abilityTaskData as CreateExplosionTaskData;
                if (string.IsNullOrEmpty(taskData.ExplosionPrefabName))
                {
                    return null;
                }
                taskData.TargetPoint = targetActorEntity.GetPosition();
                taskData.lifeTime = Mathf.FloorToInt((taskData.endFrame - taskData.startFrame) * SkillAbility.SkillFrameRate * 1000f);
                return CreateWithParent<SkillCreateExplosionActionViewTask>(this, taskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.HitEffect)
            {
                var taskData = abilityTaskData as CreatHitEffectTaskData;
                if (taskData == null
                    || string.IsNullOrEmpty(taskData.hitFxPrefabName))
                {
                    return null;
                }
                taskData.SkillID = SkillAbility.SkillBaseConfig.SkillID;
                HitEffectTaskDataLst.Add(taskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.PreWaring)
            {
                return CreateWithParent<SkillPreWarningActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.CameraAni)
            {
                return CreateWithParent<SkillCameraAnimActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.BattleEvent)
            {
                return CreateWithParent<SkillBattleEventActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.ParabolaDirection)
            {
                return CreateWithParent<SkillParabolaDirectionActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.ScreenShake)
            {
                return CreateWithParent<SkillScreenShakeActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.TimeLine)
            {
                var task = CreateWithParent<SkillBattleTimeLineActionTask>(this, abilityTaskData);
                task.delegateTimelineEnd = () =>
                {
                    if (SkillAbility.SkillBaseConfig.skillType == (int)SKILL_TYPE.SPSKL
                        && OwnerEntity != null
                        && OwnerEntity.isAtker
                        && OwnerEntity.LinkerUIDLst.Count > 0)
                    {
                        BattleLogicManager.Instance.ExecuteLinkerToBattle(OwnerEntity.UID);
                    }
                };
                return task;
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.VirtulCamera)
            {
                return CreateWithParent<SkillVirtulCameraActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.CameraShake)
            {
                return CreateWithParent<SkillCameraShakeActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.CameraEffect)
            {
                return CreateWithParent<SkillCameraEffectActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.SkinMeshFresnelEffect)
            {
                return CreateWithParent<SkillFresnelActionViewTask>(this, abilityTaskData);
            }
            else if (abilityTaskData.GetSkillActionElementType() == SKILL_ACTION_ELEMENT_TYPE.FlowerBulletEffect)
            {
                return CreateWithParent<SkillFlowerBulletActionViewTask>(this, abilityTaskData);
            }
#endif
            return null;
        }
    }
}
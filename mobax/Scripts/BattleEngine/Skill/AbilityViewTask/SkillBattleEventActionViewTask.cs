namespace BattleEngine.View
{
    using UnityEngine;
    using Logic;

    public sealed class SkillBattleEventActionViewTask : AbilityViewTask
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
                case SKILL_EVENT_TYPE.UI_OPEN:
                {
                    GameEventCenter.Broadcast(GameEvent.ShowUI, true);
                    break;
                }
                case SKILL_EVENT_TYPE.UI_CLOSE:
                {
                    GameEventCenter.Broadcast(GameEvent.ShowUI, false);
                    break;
                }
                case SKILL_EVENT_TYPE.UI_CloseDark:
                {
                    break;
                }
                case SKILL_EVENT_TYPE.Camera_Skill1:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    if (role == null
                        || !role.Selected)
                        return;
                    //CameraManager.Instance.TryChangeState(CameraState.Skill, 1);
                    break;
                }
                case SKILL_EVENT_TYPE.Camera_Skill2:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    if (role == null
                        || !role.Selected)
                        return;
                    //CameraManager.Instance.TryChangeState(CameraState.Skill, 2);
                    break;
                }
                case SKILL_EVENT_TYPE.show_SkinMesh:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    role.RoleRender.ShowSkinnedMeshRender(TaskData.param);
                    break;
                }
                case SKILL_EVENT_TYPE.hide_SkinMesh:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    role.RoleRender.HideSkinnedMeshRender(TaskData.param);
                    break;
                }
                case SKILL_EVENT_TYPE.Clock_Move:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    EffectManager.Instance.ShowClockEffect(role, "fx_skill1_1501011_3", 1f, 0.3f, new Vector3(0, -0.8f, 0), 2.5f);
                    break;
                }
                case SKILL_EVENT_TYPE.SummonCat:
                {
                    BattleManager.Instance.SummonManager.SummonCatToSence();
                    break;
                }
                case SKILL_EVENT_TYPE.ShowItemWords:
                {
                    GameEventCenter.Broadcast(GameEvent.ShowItemWords, TaskData.param);
                    break;
                }
                case SKILL_EVENT_TYPE.FreshEffect:
                {
                    var role = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
                    if (role != null)
                    {
                        role.ShowFreshEffect(TaskData.param2, TaskData.param3, TaskData.param4, TaskData.color);
                    }
                    break;
                }
                case SKILL_EVENT_TYPE.ShowGameObject:
                {
                    if (string.IsNullOrEmpty(TaskData.param))
                    {
                        break;
                    }
                    var role = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
                    if (TaskData.param.Equals("root"))
                    {
                        role.GetModelObject.SetActive(true);
                    }
                    else
                    {
                        Transform bone = role.RoleRender.GetBoneTrans(TaskData.param);
                        if (bone != null)
                        {
                            bone.SetActive(true);
                        }
                    }
                    break;
                }
                case SKILL_EVENT_TYPE.HideGameObject:
                {
                    if (string.IsNullOrEmpty(TaskData.param))
                    {
                        break;
                    }
                    var role = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
                    if (TaskData.param.Equals("root"))
                    {
                        role.GetModelObject.SetActive(false);
                    }
                    else
                    {
                        Transform bone = role.RoleRender.GetBoneTrans(TaskData.param);
                        if (bone != null)
                        {
                            bone.SetActive(false);
                        }
                    }
                    break;
                }
            }
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
        }

        public override void PauseExecute(int frameIdx)
        {
            base.PauseExecute(frameIdx);
        }

        public override void BreakExecute(int frameIdx)
        {
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            if (!isPlayEnable)
            {
                return;
            }
            switch (TaskData.eventType)
            {
                case SKILL_EVENT_TYPE.Camera_Skill1:
                case SKILL_EVENT_TYPE.Camera_Skill2:
                {
                    CameraManager.Instance.TryChangeState(CameraState.Free2, 0);
                    break;
                }
                case SKILL_EVENT_TYPE.show_SkinMesh:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    role.RoleRender.HideSkinnedMeshRender(TaskData.param);
                    break;
                }
                case SKILL_EVENT_TYPE.hide_SkinMesh:
                {
                    var Creator = SkillAbilityExecution.OwnerEntity;
                    var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
                    role.RoleRender.ShowSkinnedMeshRender(TaskData.param);
                    break;
                }
                case SKILL_EVENT_TYPE.UI_OPEN:
                {
                    //GameEventCenter.Broadcast(GameEvent.ShowUI, false);
                    break;
                }
                case SKILL_EVENT_TYPE.UI_CLOSE:
                {
                    GameEventCenter.Broadcast(GameEvent.ShowUI, true);
                    break;
                }
                case SKILL_EVENT_TYPE.ShowGameObject:
                {
                    if (string.IsNullOrEmpty(TaskData.param))
                    {
                        break;
                    }
                    var role = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
                    if (TaskData.param.Equals("root"))
                    {
                        role.GetModelObject.SetActive(false);
                    }
                    else
                    {
                        Transform bone = role.RoleRender.GetBoneTrans(TaskData.param);
                        if (bone != null)
                        {
                            bone.SetActive(false);
                        }
                    }
                    break;
                }
                case SKILL_EVENT_TYPE.HideGameObject:
                {
                    if (string.IsNullOrEmpty(TaskData.param))
                    {
                        break;
                    }
                    var role = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
                    if (TaskData.param.Equals("root"))
                    {
                        role.GetModelObject.SetActive(true);
                    }
                    else
                    {
                        Transform bone = role.RoleRender.GetBoneTrans(TaskData.param);
                        if (bone != null)
                        {
                            bone.SetActive(true);
                        }
                    }
                    break;
                }
            }
            base.EndExecute();
        }
    }
}
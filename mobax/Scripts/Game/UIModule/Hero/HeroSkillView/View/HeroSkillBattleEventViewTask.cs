/* Created:Loki Date:2022-10-27*/

using BattleEngine.Logic;
using UnityEngine;

public sealed class HeroSkillBattleEventViewTask : HeroSkillViewTask
{
    private BattleEventActionTaskData TaskData;

    public override void InitTask(AbilityTaskData data)
    {
        base.InitTask(data);
        TaskData = AbilityTaskData as BattleEventActionTaskData;
    }

    public override void BeginExecute(int frameIdx)
    {
        if (TaskData.eventType == SKILL_EVENT_TYPE.ShowGameObject)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                Transform bone = ActorObject.GetComponent<RoleRender>().GetBoneTrans(TaskData.param);
                if (bone != null)
                {
                    bone.SetActive(true);
                }
            }
        }
        else if (TaskData.eventType == SKILL_EVENT_TYPE.HideGameObject)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                Transform bone = ActorObject.GetComponent<RoleRender>().GetBoneTrans(TaskData.param);
                if (bone != null)
                {
                    bone.SetActive(false);
                }
            }
        }
        else if (TaskData.eventType == SKILL_EVENT_TYPE.show_SkinMesh)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                ActorObject.GetComponent<RoleRender>().ShowSkinnedMeshRender(TaskData.param);
            }
        }
        else if (TaskData.eventType == SKILL_EVENT_TYPE.hide_SkinMesh)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                ActorObject.GetComponent<RoleRender>().HideSkinnedMeshRender(TaskData.param);
            }
        }
    }

    public override void DoExecute(int frameIdx) { }

    public override void EndExecute()
    {
        if (TaskData.eventType == SKILL_EVENT_TYPE.ShowGameObject)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                Transform bone = ActorObject.GetComponent<RoleRender>().GetBoneTrans(TaskData.param);
                if (bone != null)
                {
                    bone.SetActive(false);
                }
            }
        }
        else if (TaskData.eventType == SKILL_EVENT_TYPE.HideGameObject)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                Transform bone = ActorObject.GetComponent<RoleRender>().GetBoneTrans(TaskData.param);
                if (bone != null)
                {
                    bone.SetActive(true);
                }
            }
        }
        else if (TaskData.eventType == SKILL_EVENT_TYPE.show_SkinMesh)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                ActorObject.GetComponent<RoleRender>().HideSkinnedMeshRender(TaskData.param);
            }
        }
        else if (TaskData.eventType == SKILL_EVENT_TYPE.hide_SkinMesh)
        {
            if (!string.IsNullOrEmpty(TaskData.param))
            {
                ActorObject.GetComponent<RoleRender>().ShowSkinnedMeshRender(TaskData.param);
            }
        }
    }
}
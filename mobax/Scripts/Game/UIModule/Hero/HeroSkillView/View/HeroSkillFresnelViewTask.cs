/* Created:Loki Date:2022-10-17*/

using BattleEngine.Logic;
using UnityEngine;
using BattleEngine.View;

public class HeroSkillFresnelViewTask : HeroSkillViewTask
{
    private SkillFresnelActionTaskkData TaskData;
    private RoleRender ownRoleRender;
    private SkinnedMeshRenderer[] _renderers;
    private MaterialPropertyBlock _materialPropertyBlock;

    public override void InitTask(AbilityTaskData data)
    {
        base.InitTask(data);
        TaskData = AbilityTaskData as SkillFresnelActionTaskkData;
    }

    public override void BeginExecute(int frameIdx)
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
        ownRoleRender = ActorObject.GetComponent<RoleRender>();
        Transform root = GameObjectHelper.FindChild(ActorObject.transform, TaskData.skinMeshRoot);
        _renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (_renderers == null)
        {
            return;
        }
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null)
            {
                continue;
            }
            RenderUtil.SwitchKeyword(_renderers[i], "_FRESNEL_EFFECT", true);
            RenderUtil.SetMpFloat(_renderers[i], "_FresnelEffect", 1, _materialPropertyBlock);
            RenderUtil.SetMpColor(_renderers[i], "_FresnelColor", TaskData.color, _materialPropertyBlock);
        }
    }

    public override void DoExecute(int frameIdx) { }

    public override void EndExecute()
    {
        if (_renderers == null)
        {
            return;
        }
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] == null)
            {
                continue;
            }
            RenderUtil.SwitchKeyword(_renderers[i], "_FRESNEL_EFFECT", false);
            RenderUtil.SetMpFloat(_renderers[i], "_FresnelEffect", 0, _materialPropertyBlock);
        }
    }
}
/* Created:Loki Date:2023-02-02*/

namespace BattleEngine.View
{
    using UnityEngine;
    using Logic;

    public sealed class SkillFresnelActionTaskkData : AbilityTaskData
    {
        public string skinMeshRoot = "";
        public Color color = Color.yellow;
        public int fadeInFrame = 0;
        public int fadeOutFrame = 0;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.SkinMeshFresnelEffect;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            SkillFresnelActionElement actioneEement = element as SkillFresnelActionElement;
            skinMeshRoot = actioneEement.skinMeshRoot;
            fadeInFrame = actioneEement.FadeInFrame;
            fadeOutFrame = actioneEement.FadeOutFrame;
            color = ColorUtil.HexToColor(actioneEement.hexColor);
        }
    }

    public sealed class SkillFresnelActionViewTask : AbilityViewTask
    {
        private SkillFresnelActionTaskkData _taskData;
        public SkillFresnelActionTaskkData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as SkillFresnelActionTaskkData;
                }
                return _taskData;
            }
        }

        private Creature ownCreature;
        private RoleRender ownRoleRender;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _materialPropertyBlock;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            _materialPropertyBlock = new MaterialPropertyBlock();
            ownCreature = BattleManager.Instance.ActorMgr.GetActor(SkillAbilityExecution.OwnerEntity.UID);
            ownRoleRender = ownCreature.GetComponent<RoleRender>();
            Transform root = ownCreature.GetBone(TaskData.skinMeshRoot);
            _renderers = root.GetComponentsInChildren<Renderer>();
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

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
        }

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
                RenderUtil.SetMpColor(_renderers[i], "_FresnelColor", TaskData.color, _materialPropertyBlock);
            }
            base.EndExecute();
        }
    }
}
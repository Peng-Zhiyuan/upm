namespace BattleEngine.View
{
    using UnityEngine;
    using Logic;

    public sealed class VirtulCameraData : AbilityTaskData
    {
        public string animation;

        public override SKILL_ACTION_ELEMENT_TYPE GetSkillActionElementType()
        {
            return SKILL_ACTION_ELEMENT_TYPE.VirtulCamera;
        }

        public override SKILL_ACTION_ELEMENT_PLATFORM GetSkillActionElementPlatform()
        {
            return SKILL_ACTION_ELEMENT_PLATFORM.VIEW;
        }

        public override void Init(SkillActionElementItem element)
        {
            base.Init(element);
            VirtulCameraElement actionElement = element as VirtulCameraElement;
            animation = actionElement.anim;
        }
    }

    public sealed class SkillVirtulCameraActionViewTask : AbilityViewTask
    {
        private VirtulCameraData _taskData;
        public VirtulCameraData TaskData
        {
            get
            {
                if (_taskData == null)
                {
                    _taskData = taskInitData as VirtulCameraData;
                }
                return _taskData;
            }
        }
        private CombatActorEntity Creator;
        private GameObject VMCamera;

        private bool isPlayEnable = true;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            CombatActorEntity actorEntity = SkillAbilityExecution.OwnerEntity;
            isPlayEnable = actorEntity.isAtker && actorEntity.PosIndex != BattleConst.SSPAssistPosIndexStart;
            if (!isPlayEnable)
            {
                return;
            }
            LoadPrefab();
        }

        public async void LoadPrefab()
        {
            Creator = SkillAbilityExecution.OwnerEntity;
            var role = BattleManager.Instance.ActorMgr.GetActor(Creator.UID);
            if (role == null)
                return;
            var address = NameFixUtil.ChangeExtension("skillcam", ".prefab");
            var prefab = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(address);
            VMCamera = GameObject.Instantiate(prefab) as GameObject;
            VMCamera.transform.SetParent(role.transform);
            VMCamera.transform.localPosition = Vector3.zero;
            VMCamera.transform.localRotation = Quaternion.identity;
            var animtor = VMCamera.transform.GetComponent<Animator>();
            if (animtor != null && TaskData != null)
            {
                animtor.Play(TaskData.animation, 0, 0);
            }
            CameraManager.Instance.HideCamera();
            CameraManager.Instance.SetMainCamera(VMCamera.transform);
        }

        public override void BreakExecute(int frameIdx)
        {
            if (!isPlayEnable)
            {
                return;
            }
            if (VMCamera != null)
                GameObject.Destroy(VMCamera);
            CameraManager.Instance.SetMainCamera(null);
            CameraManager.Instance.ShowCamera();
            base.BreakExecute(frameIdx);
        }

        public override void EndExecute()
        {
            if (!isPlayEnable)
            {
                return;
            }
            if (VMCamera != null)
                GameObject.Destroy(VMCamera);
            CameraManager.Instance.SetMainCamera(null);
            CameraManager.Instance.ShowCamera();
            base.EndExecute();
        }
    }
}
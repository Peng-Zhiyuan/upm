namespace BattleEngine.View
{
    using System.Collections.Generic;
    using UnityEngine;
    using System.Threading.Tasks;
    using Logic;

    public class SkillCameraCtr : Singleton<SkillCameraCtr>
    {
        private Dictionary<string, GameObject> cameraAnimDic = new Dictionary<string, GameObject>();

        private async Task<GameObject> LoadCameraAnim(string cameraPath)
        {
            if (cameraAnimDic.ContainsKey(cameraPath)
                && cameraAnimDic[cameraPath] != null)
            {
                cameraAnimDic[cameraPath].SetActive(true);
                return cameraAnimDic[cameraPath];
            }
            var model = await BucketManager.Stuff.Battle.GetOrAquireAsync<GameObject>(cameraPath);
            if (model == null)
            {
                BattleLog.LogWarning("Cant find model  " + cameraPath);
                return null;
            }
            GameObject obj = GameObject.Instantiate(model);
            cameraAnimDic[cameraPath] = obj;
            return obj;
        }

        public async void PlayCameraAnim(string cameraPath, Transform parent, Vector3 offsetVec3)
        {
            GameObject cameraObject = await LoadCameraAnim(cameraPath);
            if (parent != null)
            {
                TransformUtil.InitTransformInfo(cameraObject, parent);
                cameraObject.transform.localPosition += offsetVec3;
            }
            else
            {
                cameraObject.transform.parent = null;
                cameraObject.transform.position = offsetVec3;
                cameraObject.transform.rotation = Quaternion.identity;
                cameraObject.transform.localScale = Vector3.one;
            }
            Animation[] anims = cameraObject.GetComponentsInChildren<Animation>();
            for (int i = 0; i < anims.Length; i++)
            {
                anims[i].Play(anims[i].clip.name);
            }
            CameraManager.Instance.HideCamera();
            CameraManager.Instance.SetMainCamera(cameraObject.transform);
        }

        public void HideCameraAnim(string cameraPath)
        {
            if (cameraAnimDic.ContainsKey(cameraPath)
                && cameraAnimDic[cameraPath] != null)
            {
                cameraAnimDic[cameraPath].SetActive(false);
            }
            CameraManager.Instance.SetMainCamera(null);
            CameraManager.Instance.ShowCamera();
        }

#region Old
        public List<SkillCameraAnimActionViewTask> animTasks = new List<SkillCameraAnimActionViewTask>();
        public uint skillId = 0;
        public bool Playing
        {
            get { return animTasks.Count > 0; }
        }
        public int PlayingLevel
        {
            get
            {
                if (animTasks.Count <= 0
                    || animTasks[0] == null
                    || animTasks[0].TaskData == null)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
        }

        public float cameraDis = 0;
        public float cameraPitch = 0;
        public float cameraYaw = 0;

        public void BreakAnimTasks()
        {
            for (int i = 0; i < animTasks.Count; i++)
            {
                if (animTasks[i] == null)
                {
                    continue;
                }
                animTasks[i].BreakExecute(0);
            }
            animTasks.Clear();
            skillId = 0;
        }

        public void CheckCameraAnimState()
        {
            for (int i = 0; i < animTasks.Count; i++)
            {
                if (animTasks[i] == null
                    || animTasks[i].TaskData == null)
                {
                    continue;
                }
                break;
            }
        }

        public void RemoveFinishTask()
        {
            for (int i = 0; i < animTasks.Count; i++)
            {
                if (animTasks[i] == null
                    || animTasks[i].TaskData == null)
                {
                    animTasks.RemoveAt(i);
                    i -= 1;
                }
            }
            if (animTasks.Count <= 0)
            {
                animTasks.Clear();
                skillId = 0;
            }
        }

        protected GameObject targetObject;
        public Transform camRootTrans { get; private set; }
        public CameraBone MainCamera { get; private set; }
        public CameraBone SkillCamera { get; private set; }

        public Vector3 GetDir()
        {
            if (CamAnimDuration > 0)
            {
                return new Vector3(0, SkillCamera.AixRot.localEulerAngles.y, 0);
            }
            else
            {
                return new Vector3(0, MainCamera.AixRot.localEulerAngles.y, 0);
            }
        }

        public Camera GetActiveCamera()
        {
            if (CamAnimDuration > 0)
            {
                return SkillCamera.MainCamera;
            }
            else
            {
                return MainCamera.MainCamera;
            }
        }

        public CameraBone GetActiveBone()
        {
            if (CamAnimDuration > 0)
            {
                return SkillCamera;
            }
            else
            {
                return MainCamera;
            }
        }

#region 动画
        public bool isStart { get; private set; }
        public int CamAnimDuration { get; protected set; }
        private Animator animator;
        private AnimatorOverrideController overrideController;
        private AnimationClipOverrides clipOverrides;
        private bool isUnscaledTime = false;

        public void Pause()
        {
            isStart = false;
        }

        public void Resume()
        {
            isStart = true;
        }

        public async void ShowCameraAni(CombatActorEntity actorEntity, Vector3 offset, int duration, float timeScale, string aniName, bool isUnScaledTime = false)
        {
            GameObject actor = null;
            await ShowCameraAni(actor, offset, duration, timeScale, aniName, isUnScaledTime);
        }

        public async Task ShowCameraAni(GameObject actor, Vector3 offset, int duration, float timeScale, string aniName, bool isUnScaledTime = false)
        {
            if (CamAnimDuration > 0)
                return;
            // camRootTrans = BattleManager.Instance.SceneView.Cine.transform;
            // if (MainCamera == null) {
            //     MainCamera = new CameraBone(BattleManager.Instance.SceneView.Cine.view.gameObject);
            // }
            // if (SkillCamera == null) {
            //     SkillCamera = new CameraBone(BattleManager.Instance.SceneView.Cine.view.gameObject);
            // }
            // MainCamera.gameObject.SetActive(false);
            // SkillCamera.gameObject.SetActive(true);
            // targetObject = actor;
            // if (actor != null) {
            //     Pause();
            //     Tween tween = SkillCamera.transform.DOLocalMove(targetObject.transform.localPosition + Quaternion.Euler(actor.transform.eulerAngles) * offset, 0.1f).SetEase(Ease.InQuad);
            //     tween.onComplete = () => { Resume(); };
            // }
            // if (animator == null) {
            //     animator = SkillCamera.AixOffset.GetComponent<Animator>();
            //     if (animator != null && animator.runtimeAnimatorController != null) {
            //         overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            //         animator.runtimeAnimatorController = overrideController;
            //         clipOverrides = new AnimationClipOverrides(overrideController.overridesCount);
            //         overrideController.GetOverrides(clipOverrides);
            //     }
            // }
            // isUnscaledTime = isUnScaledTime;
            // CamAnimDuration = duration;
            // string path = string.Format("Assets/AddressableRes/Camera/{0}.anim", aniName);
            // AnimationClip clip = await AddressableRes.loadAddressableResAsync<AnimationClip>(path);
            // clipOverrides["default"] = clip;
            // overrideController["default"] = clip;
            // overrideController.ApplyOverrides(clipOverrides);
            // animator.speed = timeScale;
            // if (isUnScaledTime) {
            //     animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            // }
            // else {
            //     animator.updateMode = AnimatorUpdateMode.Normal;
            // }
        }

        Vector3 posOffsetDelta;
        Vector3 rotOffsetDelta;
        float filedOfViewDelta;
        SKILL_USE_CAM_TYPE useCamType;

        public void CameraMove(SKILL_USE_CAM_TYPE camType, CombatActorEntity actorEntity, Vector3 posOffset, Vector3 rotOffset, float filedOfView, int duration)
        {
            GameObject actor = null;
            CameraMove(camType, actor, posOffset, rotOffset, filedOfView, duration);
        }

        public void CameraMove(SKILL_USE_CAM_TYPE camType, GameObject actorObject, Vector3 posOffset, Vector3 rotOffset, float filedOfView, int duration)
        {
            if (CamAnimDuration > 0)
                return;
            // camRootTrans = BattleManager.Instance.SceneView.Cine.transform;
            // if (MainCamera == null) {
            //     MainCamera = new CameraBone(GameObjectHelper.FindChild(camRootTrans, "MainCamera").gameObject);
            // }
            // if (SkillCamera == null) {
            //     SkillCamera = new CameraBone(GameObjectHelper.FindChild(camRootTrans, "ActorSkillCamera").gameObject);
            // }
            // if (camType == SKILL_USE_CAM_TYPE.Main) {
            //     MainCamera.gameObject.SetActive(true);
            //     SkillCamera.gameObject.SetActive(false);
            // }
            // else {
            //     MainCamera.gameObject.SetActive(false);
            //     SkillCamera.gameObject.SetActive(true);
            // }
            // targetObject = actorObject;
            // CamAnimDuration = duration;
            // posOffsetDelta = posOffset * 1f / duration;
            // rotOffsetDelta = rotOffset * 1f / duration;
            // filedOfViewDelta = filedOfView * 1f / duration;
            // useCamType = camType;
            // Resume();
        }

        public void Update(float delta)
        {
            if (!isStart)
                return;
            OnCameraAnim(delta);
            OnCameraMove(delta);
        }

        public void EndCameraAnim()
        {
            CamAnimDuration = 0;
            MainCamera.CameraTransform.localPosition = Vector3.zero;
            SkillCamera.CameraTransform.localPosition = Vector3.zero;
            MainCamera.gameObject.SetActive(true);
            SkillCamera.gameObject.SetActive(false);
        }

        public void OnCameraAnim(float delta)
        {
            if (targetObject != null
                && CamAnimDuration > 0
                && SkillCamera.CameraTransform.transform.localPosition == Vector3.zero)
            {
                SkillCamera.CameraTransform.LookAt(targetObject.transform, Vector3.up);
            }
            if (CamAnimDuration > 0)
            {
                CamAnimDuration -= 1;
                if (CamAnimDuration <= 0)
                {
                    EndCameraAnim();
                }
            }
        }

        public void OnCameraMove(float delta)
        {
            if (CamAnimDuration > 0)
            {
                CamAnimDuration -= 1;
                if (useCamType == SKILL_USE_CAM_TYPE.Main)
                {
                    MainCamera.AixDis.localPosition += posOffsetDelta;
                    MainCamera.AixRot.localRotation = Quaternion.Euler(MainCamera.AixRot.localRotation.eulerAngles + rotOffsetDelta);
                    MainCamera.MainCamera.fieldOfView += filedOfViewDelta;
                }
                else
                {
                    SkillCamera.AixDis.localPosition += posOffsetDelta;
                    SkillCamera.AixRot.localRotation = Quaternion.Euler(MainCamera.AixRot.localRotation.eulerAngles + rotOffsetDelta);
                    SkillCamera.MainCamera.fieldOfView += filedOfViewDelta;
                }
                if (CamAnimDuration <= 0)
                {
                    EndCameraAnim();
                }
            }
        }
#endregion
    }

    public class CameraBone
    {
        public GameObject gameObject { get; private set; }
        public Transform transform { get; private set; }
        public Transform AixOffset { get; private set; }
        public Transform AixRot { get; private set; }
        public Transform AixDis { get; private set; }
        public Transform CameraTransform { get; private set; }
        public Camera MainCamera { get; private set; }

        public CameraBone(GameObject go)
        {
            gameObject = go;
            transform = gameObject.transform;
            AixOffset = GameObjectHelper.FindChild(transform, "AixOffset");
            AixRot = GameObjectHelper.FindChild(transform, "AixRot");
            AixDis = GameObjectHelper.FindChild(transform, "AixDis");
            CameraTransform = GameObjectHelper.FindChild(transform, "Main Camera");
            MainCamera = CameraTransform.GetComponent<Camera>();
        }
#endregion
    }

    public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
    {
        public AnimationClipOverrides(int capacity) : base(capacity) { }
        public AnimationClip this[string name]
        {
            get { return this.Find(x => x.Key.name.Contains(name)).Value; }
            set
            {
                int index = this.FindIndex(x => x.Key.name.Contains(name));
                if (index != -1)
                {
                    this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
                }
            }
        }
    }
}
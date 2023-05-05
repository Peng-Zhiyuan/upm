using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotTrajectoryActionTaskData : PlotActionAbilityTaskData
    {
        public EPlotActionType ActionType = EPlotActionType.Trajectory;
        public int AttachTargetId = 0;
        public string EffectPath = "";
        public Vector3 EffectScale = Vector3.one;
        public string DestroyEffect = "";
        public string DestroyAudio;
        public Vector3 DestroyEffectScale = Vector3.one;
        public Vector3 FireOffset = Vector3.zero;
        public Vector3 CenterOffset = Vector3.zero;
        public float DestroyDelay = 0;
        public int FlyCount = 1;
        public int FlyTimeOffset;
        public int FlyAngleOffset = 0;
        public float ColliderRadius = 1;
        public float HurtRatio = 1.0f;
        public int FlySpeed = 0;
        public AnimationCurve SpeedCurve = AnimationCurve.Constant(0, 1, 1);
        public int CureTime = 1000;
        public int HurtTargetId = 0;
        public float ParabolaHeight;
        public AnimationCurve ParabolaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public override void Init(PlotComicsActionElementItem element)
        {
            base.Init(element);

            PlotComicsTrajectoryActionElement actionElement = (PlotComicsTrajectoryActionElement) element;
            this.AttachTargetId = actionElement.attachTargetId;
            this.EffectPath = AddressablePathConst.SkillEditorPathParse(actionElement.effectPath);
            this.EffectScale = actionElement.effectScale;
            this.DestroyEffect = !string.IsNullOrEmpty(actionElement.destroyEffect)
                ? AddressablePathConst.SkillEditorPathParse(actionElement.destroyEffect)
                : "";
            this.DestroyAudio = !string.IsNullOrEmpty(actionElement.destroyAudio)
                ? AddressablePathConst.SkillEditorPathParse(actionElement.destroyAudio)
                : "";
            this.DestroyEffectScale = actionElement.destroyEffectScale;
            this.FireOffset = actionElement.fireOffset;
            this.CenterOffset = actionElement.centerOffset;
            this.DestroyDelay = actionElement.destroyDelay;
            this.FlyCount = actionElement.flyCount;
            this.FlyTimeOffset = actionElement.flyTimeOffset;
            this.FlyAngleOffset = actionElement.flyAngleOffset;
            this.ColliderRadius = actionElement.colliderRadius;
            this.HurtRatio = actionElement.hurtRatio;
            this.FlySpeed = actionElement.flySpeed;
            this.SpeedCurve = actionElement.speedCurve;
            this.CureTime = actionElement.cureTime;
            this.HurtTargetId = actionElement.hurtTargetId;
            this.ParabolaHeight = actionElement.parabolaHeight;
            this.ParabolaCurve = actionElement.parabolaCurve;
        }
    }

    public class PlotTrajectoryActionTask : PlotActionAbilityTask
    {
        public PlotTrajectoryActionTaskData TaskData => (PlotTrajectoryActionTaskData) this.TaskInitData;

        public override void BeginExecute(int frameIdx)
        {
            base.BeginExecute(frameIdx);
            this.InitSomeRoot();
        }

        public override void DoExecute(int frameIdx)
        {
            base.DoExecute(frameIdx);
            this.ExecuteTrajectory(frameIdx);
        }

        public override async Task Preload()
        {
            await this.Bucket.GetOrAquireAsync<GameObject>(this.TaskData.EffectPath);
        }

        public override Task EndExecute()
        {
            return base.EndExecute();
        }

        #region ---初始化---

        private Bucket Bucket => BucketManager.Stuff.Plot;

        private GameObject effRoot;
        private bool _isEditorMode;
        List<List<Vector3>> _moveList;
        Vector3 _selectedPos;
        List<GameObject> _bulletList;
        private List<int> _reverseList;
        List<ParticleSystem[]> vfxParticleSystems;

        private PlotRuntimeModelCacheInfo hurtCacheInfo;
        private PlotRuntimeModelCacheInfo attachCacheInfo;

        private GameObject _fx;

        private void InitSomeRoot()
        {
            this.effRoot = this.ParentRoot.transform.Find(PlotDefineUtil.PLOT_RUNTIME_BULLET_ROOT_PATH).gameObject;
            this.hurtCacheInfo = PlotRuntimeModelCacheManager.GetModelObj(this.TaskData.HurtTargetId);
            this.attachCacheInfo = PlotRuntimeModelCacheManager.GetModelObj(this.TaskData.AttachTargetId);
        }

        #endregion

        #region ---播放---

        async void ExecuteTrajectory(int frameIndex)
        {
            if (this.TaskData != null)
            {
                float sampleRate = 1f / PlotDefineUtil.DEFAULT_FRAME_NUM;
                int startFrame = this.TaskData.StartFrame;
                int endFrame = this.TaskData.EndFrame;
                int totalFrame = endFrame - startFrame;
                int offsetFrame = frameIndex - startFrame;
                if (frameIndex < startFrame || frameIndex > endFrame)
                {
                    this.OnDestroy();
                    return;
                }

                this.FillMoveList();
                this.CreateBullet();
                for (int i = 0; i < this._bulletList.Count; i++)
                {
                    bool hitted = false;
                    int point = frameIndex - startFrame;
                    if (point >= this._moveList[i].Count)
                    {
                        point = this._moveList[i].Count - 1;
                        hitted = true;
                    }

                    GameObject bullet = this._bulletList[i];
                    int appearFrame = i * TaskData.FlyTimeOffset;
                    bool show = point >= appearFrame && !hitted;
                    bullet.SetActive(show);
                    if ((point - appearFrame) > 0)
                    {
                        bullet.transform.localPosition = this._moveList[i][point - appearFrame];
                        int preFrame = point - appearFrame - 1;
                        Vector3 prePos = preFrame >= 0 && preFrame < this._moveList[i].Count
                            ? this._moveList[i][point - appearFrame - 1]
                            : bullet.transform.localPosition;
                        if (bullet.transform.localPosition != prePos)
                            bullet.transform.forward = (bullet.transform.localPosition - prePos).normalized;
                        else
                            bullet.transform.forward = Vector3.forward;
                    }
                }

                this.SimulateParticleSystem(offsetFrame * sampleRate);
            }
            else
            {
                this.OnDestroy();
            }
        }

        async void CreateBullet()
        {
            if (this._bulletList == null)
            {
                PlayWWise();

                this._bulletList = new List<GameObject>();
                vfxParticleSystems = new List<ParticleSystem[]>();
                GameObject tempObj;
                for (int i = 0; i < this.TaskData.FlyCount; i++)
                {
                    if (!string.IsNullOrEmpty(this.TaskData.EffectPath))
                    {
                        GameObject asset = this.Bucket.Get<GameObject>(this.TaskData.EffectPath);
                        tempObj = Object.Instantiate(asset);
                        tempObj.transform.localScale = this.TaskData.EffectScale;
                        var list = tempObj.GetComponentsInChildren<ParticleSystem>();
                        vfxParticleSystems.Add(list);
                        for (int j = 0; j < list.Length; j++)
                        {
                            // // 固定随机数
                            // list[j].useAutoRandomSeed = false;
                            // list[j].randomSeed = 120; //预览时先固定随机值
                        }
                    }
                    else
                    {
                        tempObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    }

                    if (effRoot != null && tempObj != null)
                    {
                        tempObj.transform.parent = effRoot.transform;
                        tempObj.transform.localPosition = this._moveList[i][0];
                        this._bulletList.Add(tempObj);
                    }
                }
            }
        }

        bool _isPlaying = false;

        void PlayWWise()
        {
            if (_isPlaying) return;
            _isPlaying = true;
            var comicsRes = PlotComicsManager.Stuff.CurPlayComicsRes;
            WwiseEventManager.SendEvent(TransformTable.Comics, $"{comicsRes}_effect");
        }

        void SimulateParticleSystem(float normalizedTime)
        {
            if (vfxParticleSystems == null) return;
            for (int j = 0; j < vfxParticleSystems.ToArray().Length; j++)
            {
                var list = vfxParticleSystems[j];
                for (int i = 0; i < list.Length; i++)
                {
                    list[i].Simulate(normalizedTime);
                    //SceneView.RepaintAll();
                }
            }
        }

        void FillMoveList()
        {
            if (this._moveList != null)
            {
                return;
            }

            if (hurtCacheInfo == null || attachCacheInfo == null) return;

            _reverseList = new List<int>();
            this._moveList = new List<List<Vector3>>();
            Vector3 dir;
            Transform playerTrans = attachCacheInfo.ModelObj.transform;
            _selectedPos = hurtCacheInfo.ModelObj.transform.position;

            var distance = Vector3.Distance(_selectedPos, playerTrans.position);
            int maxFrame = (int) ((distance / this.TaskData.FlySpeed) * PlotDefineUtil.DEFAULT_FRAME_NUM);
            int totalAngle = (this.TaskData.FlyCount - 1) * this.TaskData.FlyAngleOffset;
            for (int i = 0; i < this.TaskData.FlyCount; i++)
            {
                this._moveList.Add(new List<Vector3>());
                this._moveList[i].Add(Quaternion.Euler(playerTrans.eulerAngles) * this.TaskData.FireOffset +
                                      playerTrans.position);
            }

            float frameDuration = 1f / PlotDefineUtil.DEFAULT_FRAME_NUM;
            for (int i = 0; i < maxFrame + (this.TaskData.FlyTimeOffset * this.TaskData.FlyCount); i++)
            {
                for (int j = 0; j < this.TaskData.FlyCount; j++)
                {
                    int appearFrame = j * this.TaskData.FlyTimeOffset;
                    float speed = this.TaskData.FlySpeed;
                    if (i > j * this.TaskData.FlyTimeOffset)
                    {
                        Vector3 endPos = hurtCacheInfo.ModelObj.transform.position;
                        Vector3 prePos = this._moveList[j][this._moveList[j].Count - 1];
                        // 只针对抛物线
                        Vector3 offset = (endPos - prePos);
                        dir = (new Vector3(offset.x, 0, offset.z)).normalized;

                        Vector3 totalOffset = endPos - this._moveList[j][0];
                        float totalFrame = (new Vector3(totalOffset.x, 0, totalOffset.z)).magnitude /
                            speed * PlotDefineUtil.DEFAULT_FRAME_NUM;
                        float height = this.TaskData.ParabolaHeight *
                                       this.TaskData.ParabolaCurve.Evaluate((i - appearFrame) /
                                                                            (float) totalFrame);
                        Vector3 posHor = new Vector3(prePos.x, 0, prePos.z) + dir * speed * frameDuration;
                        this._moveList[j].Add(posHor + Vector3.up * (height + this.TaskData.FireOffset.y));
                    }
                }
            }
        }

        #endregion

        void OnDestroy()
        {
            if (this._bulletList != null)
            {
                for (int i = 0; i < this._bulletList.Count; i++)
                {
                    Object.Destroy(this._bulletList[i]);
                }
            }

            this._bulletList = null;
            vfxParticleSystems = null;
            this._moveList = null;
            this._reverseList = null;
            _isPlaying = false;
        }
    }
}
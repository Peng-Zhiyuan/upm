using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.Utilities;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotActionAbilityExecution
    {
        private Transform _parent;

        public PlotActionAbilityExecution(Transform parent)
        {
            this._parent = parent;
        }

        // 添加下一步的监听器
        public void AddNextStepListener()
        {
        }

        #region ---初始化注册---

        private PlotComicsConfigObject _comicsData;
        private List<PlotActionAbilityTask> _actionTaskList;

        public async Task BeginExecute(PlotComicsConfigObject comicsData)
        {
            this._actionTaskList = new List<PlotActionAbilityTask>();
            this._comicsData = comicsData;

            var actionElements = comicsData.actionElements;
            if (actionElements.Count <= 0) return;

            this._intervalTime = 0f;
            this._curFrame = 0;
            this._lastFrame = -1;
            this._isPlay = false;

            for (int i = 0; i < actionElements.Count; i++)
            {
                var actionElement = actionElements[i];
                if (!actionElement.enabled) continue;
                if (actionElement.type == EPlotActionType.Animation)
                {
                    var taskData = new PlotAnimationActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotAnimationActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.Mask)
                {
                    var taskData = new PlotMaskActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotMaskActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.MaskDissolve)
                {
                    var taskData = new PlotMaskDissolveActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotMaskDissolveActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.Bubble)
                {
                    var taskData = new PlotBubbleActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotBubbleActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.Camera)
                {
                    var taskData = new PlotCameraActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotCameraActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.MoveAbsolute)
                {
                    var taskData = new PlotMoveActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotMoveActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.TimeLine)
                {
                    var taskData = new PlotTimelineActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotTimelineActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.Effect)
                {
                    var taskData = new PlotEffectActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotEffectActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
                else if (actionElement.type == EPlotActionType.Trajectory)
                {
                    var taskData = new PlotTrajectoryActionTaskData();
                    taskData.Init(actionElement);

                    var task = new PlotTrajectoryActionTask();
                    task.SetInitData(taskData, this._parent);
                    this._actionTaskList.Add(task);
                }
            }

            // 这里需要对数组进行帧大小排序  否则会影响最后结尾帧的效果
            this.SortActionTaskList();
        }

        private void SortActionTaskList()
        {
            this._actionTaskList.Sort((a, b) =>
            {
                var aData = a.TaskInitData as PlotActionAbilityTaskData;
                var bData = b.TaskInitData as PlotActionAbilityTaskData;
                if (aData == null || bData == null) return 0;
                return aData.StartFrame - bData.StartFrame;
            });
        }

        #endregion

        #region ---预加载---

        public async Task Preload()
        {
            for (int i = 0; i < this._actionTaskList.Count; i++)
            {
                var actionTask = this._actionTaskList[i];
                await actionTask.Preload();
            }
        }

        #endregion

        #region ---Update更新---

        private float _intervalTime = 0f;
        private int _curFrame = 0; // 当前帧
        private int _lastFrame = -1; // 上一帧
        private int _totalEndFrame;
        private bool _isPlay = false;
        private Action _onPlayCompleted; // 单分镜播放结束
        private float _speed = 1f; // 剧情播放速度
        private bool _pause = false; // 剧情暂停
        private bool _isLast;

        public float Speed
        {
            get => this._speed;
            set
            {
                if (this._speed.Equals(value)) return;
                this._speed = value;
            }
        }
        public bool Pause
        {
            get => this._pause;
            set
            {
                if (this._pause.Equals(value)) return;
                this._pause = value;
            }
        }
        public void Stop()
        {
            this._isPlay = false;
        }

        public void Start2Play(int totalEndFrame, bool isLast, Action onPlayCompleted)
        {
            this._isLast = isLast;
            this._isPlay = true;
            this._totalEndFrame = totalEndFrame;
            this._onPlayCompleted = onPlayCompleted;
        }

        private float lostTime;

        public void OnUpdate()
        {
            if (!this._isPlay || this.Pause) return;
            var delta = Time.deltaTime;
            var scaledLostTime = delta * this._speed;
            lostTime += scaledLostTime;

            var frameInterval = 1f / PlotDefineUtil.DEFAULT_FRAME_NUM;

            while (lostTime >= frameInterval)
            {
                if (this._curFrame > this._totalEndFrame)
                {
                    break;
                }

                lostTime -= frameInterval;
                this._curFrame++;
                this.OnUpdate(this._curFrame);
            }

            if (this._curFrame > this._totalEndFrame)
            {
                this.OnEnd();
            }
        }

        public async Task OnEnd()
        {
            Debug.Log("刷新onEnd方法");
            this._isPlay = false;
            await this.EndExecute();
            if (!this._isLast)
            {
                await this.SingleEnd();
            }

            await SingleEndSetPicture();
            this._onPlayCompleted?.Invoke();
        }

        private async void OnUpdate(int frame)
        {
            if (this._lastFrame != frame)
            {
                this.DoExecute(frame);
                this._lastFrame = frame;
            }
        }

        public async void DoExecute(int frame)
        {
            PlotActionAbilityTaskData taskData = null;
            for (int i = 0; i < this._actionTaskList.Count; i++)
            {
                var actionTask = this._actionTaskList[i];
                if (actionTask == null)
                {
                    // this._actionTaskList.RemoveAt(i);
                    // i--;
                    continue;
                }

                taskData = actionTask.TaskInitData as PlotActionAbilityTaskData;
                if (taskData == null)
                {
                    await actionTask.EndExecute();
                    // this._actionTaskList.RemoveAt(i);
                    // i--;
                    continue;
                }

                if (taskData.StartFrame == frame)
                {
                    actionTask.BeginExecute(frame);
                }

                if (taskData.StartFrame < frame
                    && taskData.EndFrame >= frame)
                {
                    actionTask.DoExecute(frame);
                }

                if (taskData.EndFrame < frame)
                {
                    await actionTask.EndExecute();
                    // this._actionTaskList.RemoveAt(i);
                    // i--;
                }
            }
        }

        #endregion

        #region ---结束---

        public async Task EndExecute()
        {
            for (int i = 0, len = this._actionTaskList.Count; i < len; i++)
            {
                var actionTask = this._actionTaskList[i];

                if (actionTask == null)
                {
                    continue;
                }

                var taskData = actionTask.TaskInitData as PlotActionAbilityTaskData;
                if (taskData == null)
                {
                    continue;
                }

                // 气泡的单独处理
                if (taskData.ActionType.Equals(EPlotActionType.Bubble))
                {
                    continue;
                }

                actionTask.DoExecute(taskData.EndFrame);
                await actionTask.EndExecute();
            }

            await this.DoLastBubbleEndExecute();

            this._intervalTime = 0;
            this._lastFrame = -1;
            this._curFrame = 0;
        }

        /// <summary>
        /// 筛选出气泡的，气泡只需要取最后一帧的显示就好
        /// </summary>
        private async Task DoLastBubbleEndExecute()
        {
            // 气泡要过滤出来
            PlotBubbleActionTask lastBubble = null;
            var endFrame = 0;
            for (int i = 0, len = this._actionTaskList.Count; i < len; i++)
            {
                var actionTask = this._actionTaskList[i];

                if (actionTask == null)
                {
                    continue;
                }

                var taskData = actionTask.TaskInitData as PlotActionAbilityTaskData;
                if (taskData == null)
                {
                    continue;
                }

                if (taskData.ActionType != EPlotActionType.Bubble) continue;

                // 气泡的单独处理 && cache中存在该气泡id
                var bubbleData = (PlotBubbleActionTaskData) taskData;
                var bubbleCacheInfo = PlotRuntimeBubbleCacheManager.GetBubbleObj(bubbleData.ChooseId);
                if (bubbleCacheInfo == null) continue;

                if (taskData.EndFrame >= endFrame)
                {
                    lastBubble = (PlotBubbleActionTask) actionTask;
                }
            }

            if (lastBubble == null) return;

            var lastBubbleTaskData = (PlotBubbleActionTaskData) lastBubble.TaskInitData;
            if (lastBubbleTaskData == null) return;

            // var nearestBubble = this.GetNearestBubbleSetting(lastBubbleTaskData.ChooseId);
            // // 这里需要判断最后一帧的设置是否绑定了alpha的设置
            // if (lastBubbleTaskData.TransAni is {openAlpha: true} || nearestBubble == null)
            // {
            //     lastBubble.BeginExecute(0);
            //     lastBubble.DoExecute(lastBubbleTaskData.EndFrame);
            //     await lastBubble.EndExecute();
            // }
            // else
            // {
            //     // 过滤屏蔽屌其他气泡
            //     // this.HideOtherBubbles(lastBubbleTaskData.ChooseId);
            //
            //     // 找到离他最近的一次帧动画设置 && 开启了alpha设置的
            //     lastBubble.BeginExecute(0);
            //     lastBubble.BeginSkip(nearestBubble.TransAni.endAlpha);
            //     lastBubble.DoExecute(lastBubbleTaskData.EndFrame);
            //     await lastBubble.EndExecute();
            // }
        }

        private void HideOtherBubbles(int bubbleId)
        {
        }

        private PlotBubbleActionTaskData GetNearestBubbleSetting(int bubbleId)
        {
            PlotBubbleActionTaskData nearestBubble = null;

            for (int i = 0, len = this._actionTaskList.Count; i < len; i++)
            {
                var actionTask = this._actionTaskList[i];

                if (actionTask == null)
                {
                    continue;
                }

                var taskData = actionTask.TaskInitData as PlotActionAbilityTaskData;
                if (taskData == null)
                {
                    continue;
                }

                if (taskData.ActionType != EPlotActionType.Bubble) continue;
                var bubbleData = (PlotBubbleActionTaskData) taskData;

                // 如果不是指定的气泡相关 那么直接就屏蔽
                if (bubbleData.ChooseId != bubbleId)
                {
                    var cacheInfo = PlotRuntimeBubbleCacheManager.GetBubbleObj(bubbleData.ChooseId);
                    if (cacheInfo == null) continue;

                    var bubbleObj = cacheInfo.BubbleObj;
                    if (bubbleObj == null) continue;

                    var canvas = bubbleObj.GetComponent<CanvasGroup>();
                    canvas.alpha = 0;
                    continue;
                }

                if (bubbleData.TransAni == null ||
                    !bubbleData.TransAni.openAlpha) continue;

                nearestBubble = (PlotBubbleActionTaskData) bubbleData;
            }

            return nearestBubble;
        }

        private async Task SingleEnd()
        {
            // PlotRuntimeBubbleCacheManager.ClearBubbleMap();
            // 这里生成picture 然后下一个分镜内容
            await this.SetPictureRoot();
        }

        private async Task SingleEndSetPicture()
        {
            for (int i = 0; i < this._actionTaskList.Count; i++)
            {
                var actionTaskData = this._actionTaskList[i].TaskInitData as PlotActionAbilityTaskData;
                if (actionTaskData == null) continue;
                var actionTask = this._actionTaskList[i] as PlotMaskActionTask;
                if (actionTask == null) continue;

                actionTask.ReExecute();

                if (actionTaskData.ActionType == EPlotActionType.Mask ||
                    actionTaskData.ActionType == EPlotActionType.TimeLine)
                {
                    await actionTask.SetPicture();
                }
            }
        } 

        // 找到遮罩的选项并设置成picture
        private async Task SetPictureRoot()
        {
            for (int i = 0; i < this._actionTaskList.Count; i++)
            {
                var actionTaskData = this._actionTaskList[i].TaskInitData as PlotActionAbilityTaskData;
                if (actionTaskData == null) continue;
                var actionTask = this._actionTaskList[i] as PlotMaskActionTask;
                if (actionTask == null) continue;

                actionTask.ReExecute();
            }
        }

        #endregion
    }
}
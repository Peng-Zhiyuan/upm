using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotConfigAbilityExecution
    {
        private Transform _parent;

        public PlotConfigAbilityExecution(Transform parent)
        {
            this._parent = parent;
        }

        #region ---初始化注册---

        private PlotComicsConfigObject _comicsData;
        private List<PlotConfigAbilityTask> _configTaskList;

        public async Task BeginExecute(PlotComicsConfigObject comicsData)
        {
            this._configTaskList = new List<PlotConfigAbilityTask>();
            this._comicsData = comicsData;

            var configElements = comicsData.comicsElements;
            if (configElements.Count <= 0) return;

            for (int i = 0; i < configElements.Count; i++)
            {
                var configElement = configElements[i];
                if (!configElement.enabled) continue;
                if (configElement.type == EPlotComicsElementType.SceneMap)
                {
                    var taskData = new PlotMapConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotMapConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
                else if (configElement.type == EPlotComicsElementType.Timeline)
                {
                    var taskData = new PlotTimelineConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotTimelineConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
                else if (configElement.type == EPlotComicsElementType.SceneCamera)
                {
                    var taskData = new PlotCameraConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotCameraConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
                else if (configElement.type == EPlotComicsElementType.SceneModel)
                {
                    var taskData = new PlotModelConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotModelConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
                else if (configElement.type == EPlotComicsElementType.SceneModelEnv)
                {
                    var taskData = new PlotEnvModelConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotEnvModelConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
                else if (configElement.type == EPlotComicsElementType.CameraMask)
                {
                    var taskData = new PlotMaskFrameConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotMaskFrameConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
                else if (configElement.type == EPlotComicsElementType.Bubble)
                {
                    var taskData = new PlotBubbleConfigTaskData();
                    taskData.Init(configElement);

                    var task = new PlotBubbleConfigTask();
                    task.SetInitData(taskData, this._parent);
                    this._configTaskList.Add(task);
                }
            }
        }

        #endregion

        #region ---预加载---

        public async Task Preload()
        {
            for (int i = 0; i < this._configTaskList.Count; i++)
            {
                var actionTask = this._configTaskList[i];
                await actionTask.Preload();
            }
        }

        #endregion

        #region ---生成---

        public async Task DoExecute()
        {
            PlotConfigAbilityTaskData taskData = null;
            this.SortConfigTaskList();
            for (int i = 0; i < this._configTaskList.Count; i++)
            {
                var configTask = this._configTaskList[i];
                if (configTask == null)
                {
                    // this._configTaskList.RemoveAt(i);
                    // i--;
                    continue;
                }

                taskData = configTask.TaskInitData as PlotConfigAbilityTaskData;
                if (taskData == null)
                {
                    await configTask.EndExecute();
                    // this._configTaskList.RemoveAt(i);
                    // i--;
                    continue;
                }

                var timelineSetting = this.GetTimelineSetting();
                // 如果是遮罩
                if (taskData.ElementType == EPlotComicsElementType.CameraMask)
                {
                    var maskConfigTask = (PlotMaskFrameConfigTask) configTask;
                    maskConfigTask.SetShowTimelineRawImg(timelineSetting);
                }

                await configTask.BeginExecute();
            }
        }

        private (bool showTimeline, int timelineId) GetTimelineSetting()
        {
            bool result = false;
            int timelineId = -1;
            for (int i = 0; i < this._configTaskList.Count; i++)
            {
                var configElement = this._configTaskList[i];
                var taskData = configElement.TaskInitData as PlotConfigAbilityTaskData;
                if (taskData == null) continue;
                // 如果是遮罩不存在的话 那么直接就隐藏整个canvas
                if (taskData.ElementType == EPlotComicsElementType.Timeline)
                {
                    result = true;
                    var timelineData = (PlotTimelineConfigTaskData) taskData;
                    timelineId = timelineData.TimelineId;
                }
            }

            return (result, timelineId);
        }

        private void SortConfigTaskList()
        {
            this._configTaskList.Sort((a, b) =>
            {
                var aData = a.TaskInitData as PlotConfigAbilityTaskData;
                var bData = b.TaskInitData as PlotConfigAbilityTaskData;
                if (aData == null || bData == null) return 0;
                return (int) aData.Priority - (int) bData.Priority;
            });
        }

        #endregion

        #region ---结束---

        public async Task EndExecute()
        {
            for (int i = 0, len = this._configTaskList.Count; i < len; i++)
            {
                var actionTask = this._configTaskList[i];
                if (actionTask == null || actionTask.TaskInitData == null)
                {
                    continue;
                }

                await actionTask.EndExecute();
            }
        }

        public async Task OnEnd()
        {
            await this.EndExecute();
        }

        #endregion

        #region ---清理---


        #endregion
    }
}
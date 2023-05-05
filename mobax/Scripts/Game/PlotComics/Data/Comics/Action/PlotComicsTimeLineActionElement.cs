using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;

namespace Plot.Runtime
{
    [PlotComicsActionElementItem("TimeLine", (int) EPlotActionType.TimeLine)]
    public class PlotComicsTimeLineActionElement : PlotComicsActionElementItem
    {
        public virtual string Label => "TimeLine";

        [ToggleGroup("enabled"), LabelText("TimeLine ID")] [LabelWidth(100)] [OnValueChanged("OnInitTimelineFrame")]
        public int timelineId;

        private void OnInitTimelineFrame()
        {
            var cacheInfo = PlotEditorTimelineCacheManager.GetTimelineCache(this.timelineId);
            if (cacheInfo == null) return;
            var director = cacheInfo.TimelineObj.GetComponentInChildren<PlayableDirector>();
            if (director == null) return;

            this.endFrame = Mathf.CeilToInt((float) director.duration / (1f / PlotDefineUtil.DEFAULT_FRAME_NUM));
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotRuntimeTimelineCacheInfo
    {
        public GameObject TimelineObj;
        public PlotTimelineConfigTaskData ConfigElement;
    }

    public class PlotRuntimeTimelineCacheManager
    {
        private static Dictionary<int, PlotRuntimeTimelineCacheInfo> _timelineDic = new Dictionary<int, PlotRuntimeTimelineCacheInfo>();

        public static void AddTimelineCache2Map(int timelineId, PlotTimelineConfigTaskData data, GameObject obj)
        {
            if (_timelineDic.ContainsKey(timelineId))
            {
                _timelineDic[timelineId] = new PlotRuntimeTimelineCacheInfo()
                {
                    TimelineObj = obj,
                    ConfigElement = data,
                };
            }
            else
            {
                _timelineDic.Add(timelineId, new PlotRuntimeTimelineCacheInfo()
                {
                    TimelineObj = obj,
                    ConfigElement = data,
                });
            }
        }

        public static void RemoveTimeline(int timelineId)
        {
            _timelineDic.Remove(timelineId);
        }

        public static PlotRuntimeTimelineCacheInfo GetTimelineCache(int timelineId)
        {
            return DictionaryUtil.TryGet(_timelineDic, timelineId, default);
        }

        public static void ClearTimelineObj()
        {
            if (_timelineDic.Count > 0)
            {
                var timelineObjs = _timelineDic.Values.ToList();
                foreach (var timelineObj in timelineObjs)
                {
                    Object.DestroyImmediate(timelineObj.TimelineObj);
                }
            }

            _timelineDic.Clear();
        }
    }
}
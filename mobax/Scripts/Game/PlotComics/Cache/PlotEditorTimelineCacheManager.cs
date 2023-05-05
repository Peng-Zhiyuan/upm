using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotTimelineCacheInfo
    {
        public GameObject TimelineObj;
        public PlotComicsTimeLineConfigElement ConfigElement;
    }

    public class PlotEditorTimelineCacheManager
    {
        private static Dictionary<int, PlotTimelineCacheInfo> _timelineDic = new Dictionary<int, PlotTimelineCacheInfo>();

        public static void AddTimelineCache2Map(int timelineId, PlotComicsTimeLineConfigElement data, GameObject obj)
        {
            if (_timelineDic.ContainsKey(timelineId))
            {
                _timelineDic[timelineId] = new PlotTimelineCacheInfo()
                {
                    TimelineObj = obj,
                    ConfigElement = data,
                };
            }
            else
            {
                _timelineDic.Add(timelineId, new PlotTimelineCacheInfo()
                {
                    TimelineObj = obj,
                    ConfigElement = data,
                });
            }
        }

        public static void RemoveTimeline(int modelId)
        {
            _timelineDic.Remove(modelId);
        }

        public static PlotTimelineCacheInfo GetTimelineCache(int timelineId)
        {
            return DictionaryUtil.TryGet(_timelineDic, timelineId, default);
        }

        public static void ClearModelMap()
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
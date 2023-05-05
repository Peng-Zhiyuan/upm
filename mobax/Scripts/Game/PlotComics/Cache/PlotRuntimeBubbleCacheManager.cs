using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotRuntimeBubbleCacheInfo
    {
        public GameObject BubbleObj;
        public PlotBubbleConfigTaskData ConfigElement;
    }

    public class PlotRuntimeBubbleCacheManager
    {
        private static Dictionary<int, PlotRuntimeBubbleCacheInfo> _bubbleDic =
            new Dictionary<int, PlotRuntimeBubbleCacheInfo>();

        public static void AddBubble2Map(int bubbleId, PlotBubbleConfigTaskData data, GameObject obj)
        {
            if (_bubbleDic.ContainsKey(bubbleId))
            {
                _bubbleDic[bubbleId] = new PlotRuntimeBubbleCacheInfo()
                {
                    BubbleObj = obj,
                    ConfigElement = data,
                };
            }
            else
            {
                _bubbleDic.Add(bubbleId, new PlotRuntimeBubbleCacheInfo()
                {
                    BubbleObj = obj,
                    ConfigElement = data,
                });
            }
        }

        public static PlotRuntimeBubbleCacheInfo GetBubbleObj(int bubbleId)
        {
            return DictionaryUtil.TryGet(_bubbleDic, bubbleId, default);
        }

        public static void RemoveBubble(int bubbleId)
        {
            _bubbleDic.Remove(bubbleId);
        }

        public static void ClearBubbleMap()
        {
            if (_bubbleDic.Count > 0)
            {
                var bubbleObjs = _bubbleDic.Values.ToList();
                foreach (var bubbleObj in bubbleObjs)
                {
                    Object.DestroyImmediate(bubbleObj.BubbleObj);
                }
            }

            _bubbleDic.Clear();
        }
    }
}
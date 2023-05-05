using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotBubbleCacheInfo
    {
        public GameObject BubbleObj;
        public PlotComicsBubbleElement ConfigElement;
    }

    public class PlotEditorBubbleCacheManager
    {
        private static Dictionary<int, PlotBubbleCacheInfo> _bubbleDic = new Dictionary<int, PlotBubbleCacheInfo>();

        public static void AddBubble2Map(int bubbleId, PlotComicsBubbleElement data, GameObject obj)
        {
            if (_bubbleDic.ContainsKey(bubbleId))
            {
                _bubbleDic[bubbleId] = new PlotBubbleCacheInfo()
                {
                    BubbleObj = obj,
                    ConfigElement = data,
                };
            }
            else
            {
                _bubbleDic.Add(bubbleId, new PlotBubbleCacheInfo()
                {
                    BubbleObj = obj,
                    ConfigElement = data,
                });
            }
        }

        public static PlotBubbleCacheInfo GetBubbleObj(int bubbleId)
        {
            return DictionaryUtil.TryGet(_bubbleDic, bubbleId, default);
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
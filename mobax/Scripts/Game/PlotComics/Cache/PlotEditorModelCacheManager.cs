using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotModelCacheInfo
    {
        public GameObject ModelObj;
        public PlotComicsSceneModelElement ConfigElement;
    }

    public class PlotEditorModelCacheManager
    {
        private static Dictionary<int, PlotModelCacheInfo> _modelDic = new Dictionary<int, PlotModelCacheInfo>();

        public static void AddModel2Map(int modelId, PlotComicsSceneModelElement data, GameObject obj)
        {
            if (_modelDic.ContainsKey(modelId))
            {
                _modelDic[modelId] = new PlotModelCacheInfo()
                {
                    ModelObj = obj,
                    ConfigElement = data,
                };
            }
            else
            {
                _modelDic.Add(modelId, new PlotModelCacheInfo()
                {
                    ModelObj = obj,
                    ConfigElement = data,
                });
            }
        }

        public static void RemoveModel(int modelId)
        {
            _modelDic.Remove(modelId);
        }

        public static PlotModelCacheInfo GetModelObj(int modelId)
        {
            return DictionaryUtil.TryGet(_modelDic, modelId, default);
        }

        public static void ClearModelMap()
        {
            if (_modelDic.Count > 0)
            {
                var bubbleObjs = _modelDic.Values.ToList();
                foreach (var bubbleObj in bubbleObjs)
                {
                    Object.DestroyImmediate(bubbleObj.ModelObj);
                }
            }

            _modelDic.Clear();
        }
    }
}
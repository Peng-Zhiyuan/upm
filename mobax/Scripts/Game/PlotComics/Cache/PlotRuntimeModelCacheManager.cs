using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Plot.Runtime
{
    public class PlotRuntimeModelCacheInfo
    {
        public GameObject ModelObj;
        public Vector3 Pos;
        public Vector3 Rotation;
        public Vector3 Scale = Vector3.one;
        public string ActionName;
        public int Id;
    }

    public class PlotRuntimeModelCacheManager
    {
        private static Dictionary<int, PlotRuntimeModelCacheInfo> _modelDic =
            new Dictionary<int, PlotRuntimeModelCacheInfo>();

        public static void AddModel2Map(int modelId, PlotModelConfigTaskData data, GameObject obj)
        {
            if (_modelDic.ContainsKey(modelId))
            {
                _modelDic[modelId] = new PlotRuntimeModelCacheInfo()
                {
                    ModelObj = obj,
                    Pos = data.Pos,
                    Rotation = data.Rotation,
                    Scale = data.Scale,
                    ActionName = data.ActionName,
                    Id = data.Id,
                };
            }
            else
            {
                _modelDic.Add(modelId, new PlotRuntimeModelCacheInfo()
                {
                    ModelObj = obj,
                    Pos = data.Pos,
                    Rotation = data.Rotation,
                    Scale = data.Scale,
                    ActionName = data.ActionName,
                    Id = data.Id,
                });
            }
        }

        public static void AddModel2Map(int modelId, PlotEnvModelConfigTaskData data, GameObject obj)
        {
            if (_modelDic.ContainsKey(modelId))
            {
                _modelDic[modelId] = new PlotRuntimeModelCacheInfo()
                {
                    ModelObj = obj,
                    Pos = data.Pos,
                    Rotation = data.Rotation,
                    Scale = data.Scale,
                    ActionName = data.ActionName,
                    Id = data.Id,
                };
            }
            else
            {
                _modelDic.Add(modelId, new PlotRuntimeModelCacheInfo()
                {
                    ModelObj = obj,
                    Pos = data.Pos,
                    Rotation = data.Rotation,
                    Scale = data.Scale,
                    ActionName = data.ActionName,
                    Id = data.Id,
                });
            }
        }

        public static void RemoveModel(int modelId)
        {
            _modelDic.Remove(modelId);
            _modelDic.Remove(modelId);
        }

        public static PlotRuntimeModelCacheInfo GetModelObj(int modelId)
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
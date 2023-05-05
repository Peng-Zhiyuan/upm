using System.Collections.Generic;
using UnityEngine;
using BattleSystem.Core;
using System.Threading.Tasks;
using System;

namespace BattleSystem.ProjectCore
{
    /// <summary>
    /// 项目级（非通用的）战斗核心
    /// 依赖通用通用战斗核心
    /// 被战斗表现依赖
    /// </summary>
    public class ProjectBattleCoreUtil
    {
        public static async Task<BattleCore> Create(BattleCoreParam param)
        {
            // 初始化通用战斗核心
            var core = new BattleCore();
            await core.InitAsync(param);
            var mapType = param.pveParam.MapType;
            if (mapType == EPveMapType.RogueLike
                || mapType == EPveMapType.PveRogueLike)
            {
                PathFindingManager.Instance.CreateRogueLikeMap(mapType, param.pveParam.MapParts, param.pveParam.EnvironmentParts, param.pveParam.MapEffects, param.pveParam.SceneId,param.pveParam.FormationIndex);
            }

            // 获得寻路网格资源地址
            var graphInfoList = PathFindingManager.Instance.GetPathfindingGraph(param.pveParam.MapType, param.pveParam.SceneId);

            // 获得寻路网格数据
            var graphAssetList = new List<byte[]>();
            var offsetList = new List<FixedVector3>();
            var rotationList = new List<FixedVector3>();
            foreach (var graphInfo in graphInfoList)
            {
                var address = graphInfo.graphAddress;
                var offset = graphInfo.offset;
                var rotation = graphInfo.rotation;
                var graphTextAsset = await BucketManager.Stuff.Battle.GetOrAquireAsync<TextAsset>(address, true);
                if (graphTextAsset == null)
                {
                    throw new Exception($"[ProjectBattleCore] graph address '{address}' not found");
                }
                var data = graphTextAsset.bytes;
                graphAssetList.Add(data);
                offsetList.Add(offset);
                rotationList.Add(rotation);
            }

            // 加载寻路网格
            //var astarManager = AstarPathCore.active;
            var astarManager = PathFindingManager.Instance.AstarPathCore;
            astarManager.LoadGraphAdditional(graphAssetList, offsetList, rotationList);
            return core;
        }
    }
}
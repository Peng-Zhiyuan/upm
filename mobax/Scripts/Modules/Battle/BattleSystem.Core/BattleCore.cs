using PathfindingCore;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Text;
using PathfindingCore.Drawing;

namespace BattleSystem.Core
{
    public class BattleCore
    {
        public CoreEngine coreEngine;

        public object param;

        public async Task InitAsync(object param)
        {
            Debug.Log("[BattleCore] Init");
            this.coreEngine = new CoreEngine(this);

            // pzy:
            // Astar 的 Gizmos 管理器
            var platform = CrossPlatform.platform;
            if (platform == CrossPlatformType.Unity)
            {
                DrawingManager.Init();
            }

            // 寻路
            var astar = PathFindingManager.Instance.AstarPathCore;
            astar.maxNearestNodeDistance = 0.3f;
            astar.threadCount = PathfindingCore.ThreadCount.AutomaticLowLoad;
            astar.logPathResults = PathfindingCore.PathLog.OnlyErrors;
            astar.debugMode = GraphDebugMode.Areas;
        }
    }
}
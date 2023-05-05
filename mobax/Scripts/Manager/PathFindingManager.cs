using BattleEngine.Logic;
using System;
using System.Collections.Generic;
using BattleSystem.ProjectCore;
using UnityEngine;

public class PathFindingManager : Singleton<PathFindingManager>
{
    Dictionary<int, CoreObject> intanceIdToCoreObjectDic = new Dictionary<int, CoreObject>();

    public AstarPathCore _AstarPathCore;

    public AstarPathCore AstarPathCore
    {
        get
        {
            if (_AstarPathCore == null)
            {
                Create();
            }

            return _AstarPathCore;
        }
    }

    void Create()
    {
        if (_AstarPathCore == null)
        {
            var co = CoreEngine.lastestInstance.Create("PathFinding");
            _AstarPathCore = co.AddComponent<AstarPathCore>();
        }
    }

    //public bool 

    public void Destroy()
    {
        _AstarPathCore = null;
    }

    public virtual List<GraphInfo> GetPathfindingGraph(EPveMapType mapType, int battleMapId = -1)
    {
        if (mapType == EPveMapType.RogueLike || mapType == EPveMapType.PveRogueLike)
        {
            var mapGenerator = MapGenerateCore.Instance;
            return mapGenerator.GenerateAStarGraphs();
        }

        // 从战斗地图中获得导航图资产地址
        var graphAddress = GetGraphFromBattleMap(battleMapId);

        // 构造返回结构
        var graphInfo = new GraphInfo();
        graphInfo.graphAddress = graphAddress;
        var ret = new List<GraphInfo>();
        ret.Add(graphInfo);

        return ret;
    }

    string GetGraphFromBattleMap(int battleMapId)
    {
        var battleMapRow = StaticData.BattleMapTable[battleMapId];
        var graph = battleMapRow.navGraph;
        return graph;
    }

    public void CreateRogueLikeMap(EPveMapType mapType, List<MapPartConfig> mapParts,
        List<EnvironmentPartConfig> environments, List<MapEffectBase> mapEffects, int sceneId = -1,
        EFormationIndex formationIndex = EFormationIndex.None)
    {
        var mapGenerator = MapGenerateCore.Instance;
        if (sceneId <= 0)
        {
            Debug.LogError("当前pve需要接入rogueLike场景，场景id配置小于0");
            return;
        }

        mapGenerator.Init(sceneId);

        switch (mapType)
        {
            case EPveMapType.RogueLike:
                mapGenerator.Generate(formationIndex);
                break;
            case EPveMapType.PveRogueLike:
                if (mapParts == null || mapParts.Count <= 0)
                {
                    mapGenerator.Generate();
                    Debug.LogError("设定的地图数据没有获取到，请策划查看配置");
                    return;
                }

                mapGenerator.Generate(mapParts, environments, mapEffects);
                break;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class AStarHelper:Single<AStarHelper>
{
    //int[,] map =
    //{
    //    {1,1,1,1,1,1,1,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,0,0,0,0,0,0,1 },
    //    {1,1,1,1,1,1,1,1 },
    //};
    int width = 6;
    int height = 8;
    int[,] map =
    {
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
        {0,0,0,0,0,0},
    };

    //private int GetMapVal(int x, int y)
    //{
    //    return map[y, x];
    //}

    //private Vector2Int ToMapPos(Vector2Int pos)
    //{
    //    return pos + Vector2Int.one;
    //}

    //private Vector2Int FromMapPos(Vector2Int pos)
    //{
    //    return pos - Vector2Int.one;
    //}

    private void SetMapVal(int x, int y, int val)
    {
        map[y, x] = val;
    }

    public int[,] GenMap(int mapId)
    {
        // for (var x = 0; x < width; x++)
        // {
        //     for (var y = 0; y < height; y++)
        //     {
        //         Vector2Int cord = new Vector2Int(x, y);
        //         int tileId = MapUtil.GetTileId(cord);
        //         //var tile_type = MapUtil.GetMapTileType(mapId, cord);
        //         //if (tile_type < 0)
        //         //{
        //         //    SetMapVal(x, y, -1);
        //         //}
        //         if (!BattleCtrl.Instance.Core.moveTileHash.Contains(tileId))
        //         {
        //             SetMapVal(x, y, -1);
        //         }
        //         else if (BattleCtrl.Instance.Core.pieceDic.ContainsKey(tileId))
        //         {
        //             if (!BattleCtrl.Instance.Core.IsTeamHero(BattleCtrl.Instance.Core.FocusHero, BattleCtrl.Instance.Core.pieceDic[tileId]))
        //             {
        //                 SetMapVal(x, y, -1);
        //             }
        //             else
        //             {
        //                 SetMapVal(x, y, 2);
        //             }

        //         }
        //         else
        //         {
        //             SetMapVal(x, y, 1);
        //         }

        //     }
        // }
        return map;
    }

    public List<Vector2Int> CalculatePath(int[,] map, Vector2Int startPos, Vector2Int targetPos)
    {
        //startPos = ToMapPos(startPos);
        //targetPos = ToMapPos(targetPos);
        var targetNode = AStar.Instance.Execute(map, width, height, startPos.x, startPos.y, targetPos.x, targetPos.y);
        //AStar.Instance.DisplayPath(node);
        var path = AStar.Instance.GetPathList(targetNode);
        //Debug.LogError("path:" + path.Count);
        List<Vector2Int> pathTileList = new List<Vector2Int>();
        foreach (var node in path)
        {
            if(node == null ) continue;
            var cord = new Vector2Int(node.point.x, node.point.y);
            pathTileList.Add(cord);
        }
    
        return pathTileList;
    }
}

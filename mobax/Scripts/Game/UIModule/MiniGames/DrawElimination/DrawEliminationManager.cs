using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public static class DrawEliminationManager
{
    public const int NumH = 5;
    public const int NumV = 5;

    private static int _eliminateMin;
    private static int _eliminateMax;
    private static int _stand;
    private static string[] _items;
    private static List<int> _linkStack;
    private static Dictionary<int, int> _eliminateMap;
    private static Dictionary<int, int> _dropMap;

    public static bool EnableRedo { get; set; } = true;
    public static string[] Items => _items;
    // spot list
    public static List<int> Links => _linkStack;
    // spot -> drop num
    public static Dictionary<int, int> Drops => _dropMap;
    // column -> eliminate num in this column
    public static Dictionary<int, int> Eliminates => _eliminateMap;

    static DrawEliminationManager()
    {
        _items = new string[NumH * NumV];
        _linkStack = new List<int>();
        _eliminateMap = new Dictionary<int, int>();
        _dropMap = new Dictionary<int, int>();
        _eliminateMin = DrawEliminationData.EliminateMin;
        _eliminateMax = DrawEliminationData.EliminateMax;
        _stand = -1;
    }

    /// <summary>
    /// 创建开始地图
    /// </summary>
    public static void MakeRandomMap()
    {
        for (var v = 0; v < NumV; ++v)
        {
            for (var h = 0; h < NumH; ++h)
            {
                var spot = GetSpot(h, v);
                _items[spot] = _FetchItem();
            }
        }
    }

    /// <summary>
    /// 用地图上的图形随机出需求
    /// </summary>
    /// <param name="arr"></param>
    public static void RandomDemands(string[] arr)
    {
        // 先重置数组
        var spots = new int[arr.Length];

        var spot = spots[0] = Random.Range(0, _items.Length);
        var avails = new int[4];
        arr[0] = _items[spot];
        for (var i = 1; i < spots.Length; ++i)
        {
            var (h, v) = GetHV(spot);
            var num = InternalAdd(GetSpot(h + 1, v), GetSpot(h - 1, v), GetSpot(h, v + 1), GetSpot(h, v - 1));
            spot = spots[i] = avails[Random.Range(0, num)];
            arr[i] = _items[spot];
        }
        Debug.Log($"当前随机到的位置是: {string.Join(",", spots)}");

        int InternalAdd(params int[] addedSpots)
        {
            var num = 0;
            foreach (var addedSpot in addedSpots)
            {
                if (addedSpot >= 0 && Array.IndexOf(spots, addedSpot) < 0)
                {
                    avails[num] = addedSpot;
                    ++num;
                }
            }
            
            return num;
        }
    }
    
    /// <summary>
    /// 判断两个点是否可以连接
    /// </summary>
    /// <param name="spot1"></param>
    /// <param name="spot2"></param>
    /// <returns></returns>
    public static bool CouldLink(int spot1, int spot2)
    {
        // 首先要保证相连
        return _IsNeighbor(spot1, spot2);
    }

    /// <summary>
    /// 连到某个点（必须符合连接的条件）
    /// </summary>
    /// <param name="spot">要连的点</param>
    /// <returns>是否成功连接</returns>
    public static bool LinkTo(int spot)
    {
        if (_stand != -1 && (_linkStack.Contains(spot) || !CouldLink(_stand, spot)) || _linkStack.Count >= _eliminateMax) return false;
        
        _stand = spot;
        _linkStack.Add(spot);
        return true;
    }

    /// <summary>
    /// 重做选中
    /// </summary>
    /// <param name="spot"></param>
    /// <returns>返回被redo的那个点</returns>
    public static int Redo(int spot)
    {
        if (!EnableRedo) return -1;
        var len = _linkStack.Count;
        if (len <= 1 || _linkStack[len - 2] != spot) return -1;
        
        var endLink = _linkStack[len - 1];
        _linkStack.RemoveAt(len - 1);
        _stand = spot;
        return endLink;
    }

    public static bool CanEliminate()
    {
        return _linkStack.Count >= _eliminateMin;
    }

    /// <summary>
    /// 消掉已经连的格子，并同时计算每个需要掉下来的格子
    /// </summary>
    /// <returns></returns>
    public static void Eliminate()
    {
        _eliminateMap.Clear();
        foreach (var spot in _linkStack)
        {
            var (h, v) = GetHV(spot);
            _items[spot] = null;
            if (_eliminateMap.TryGetValue(h, out var num))
            {
                _eliminateMap[h] = Math.Min(num, v);
            }
            else
            {
                _eliminateMap[h] = v;
            }
        }

        _dropMap.Clear();
        var keys = _eliminateMap.Keys.ToArray();
        foreach (var h in keys)
        {
            var bottom = _eliminateMap[h];
            var num = 1;
            // 从上往下掉， 跟我们设的坐标是相反的位置
            for (var i = bottom + 1; i < NumV; ++i)
            {
                var v = i;
                var spot = GetSpot(h, v);
                if (_items[spot] == null)
                {
                    ++num;
                }
                else
                {
                    _dropMap[spot] = num;
                    // 放置到新位置
                    _items[GetSpot(h, v - num)] = _items[spot];
                }
            }

            for (var i = 0; i < num; ++i)
            {
                var v = NumV - i - 1;
                _items[GetSpot(h, v)] = _FetchItem();
            }
            // 然后再次用这个字典记录
            // 一开始记录的是bottom的index，现在换成这个column的消除数
            _eliminateMap[h] = num;
        }
    }

    /// <summary>
    /// 清掉回合内容
    /// </summary>
    public static void ClearRound()
    {
        _linkStack.Clear();
        _stand = -1;
    }

    public static (int, int) GetHV(int spot)
    {
        return (spot % NumH, spot / NumH);
    }

    public static int GetSpot(int h, int v)
    {
        if (h >= NumH || h < 0 || v >= NumV || v < 0)
            return -1;
        
        return NumH * v + h;
    }

    public static int GetRotation(int spot1, int spot2)
    {
        var (h1, v1) = GetHV(spot1);
        var (h2, v2) = GetHV(spot2);
        if (v2 > v1) return 0;
        if (v2 < v1) return 180;
        if (h2 > h1) return -90;
        if (h2 < h1) return 90;

        return 0;
    }

    /// <summary>
    /// 是否临近点（横或者竖挨着）
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static bool _IsNeighbor(int a, int b)
    {
        var (ah, av) = GetHV(a);
        var (bh, bv) = GetHV(b);

        if ((ah - bh) * (av - bv) != 0) return false;
        return Mathf.Abs(ah - bh) == 1 || Mathf.Abs(av - bv) == 1;
    }

    private static string _FetchItem()
    {
        var list = DrawEliminationData.ItemSource;
        return list[Random.Range(0, list.Length)];
    }
}
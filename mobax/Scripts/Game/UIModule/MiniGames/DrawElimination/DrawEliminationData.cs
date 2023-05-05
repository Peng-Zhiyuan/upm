using System;
using System.Collections.Generic;
using UnityEngine;

public static class DrawEliminationData
{
    // 单次最小消除数
    public static int EliminateMin
    {
        get => StaticData.BaseTable.TryGet("SgameMinChoose");
    }
    
    // 单次最大消除数
    public static int EliminateMax
    {
        get => StaticData.BaseTable.TryGet("SgameMaxChoose");
    }

    // 最大遗弃数量
    public static int AbandonMax
    {
        get => StaticData.BaseTable.TryGet("SgameMaxStorage");
    }

    // 遗弃选择的扣分
    public static int AbandonDecrease
    {
        get => StaticData.BaseTable.TryGet("SgameMissPoint");
    }

    // 一个顾客的停留时间
    public static int CustomerTime
    {
        get => StaticData.BaseTable.TryGet("SgameTime");
    }

    // 一局游戏时间
    public static int GameTime
    {
        get => StaticData.BaseTable.TryGet("SgameMaxTime");
    }
    
    // 胜利所需分数
    public static int WinScore
    {
        get => StaticData.BaseTable.TryGet("SgameMaxPoint");
    }
    
    
    private static string[] _itemSource;
    // 道具的数据源
    public static string[] ItemSource
    {
        get
        {
            if (null == _itemSource)
            {
                _itemSource = StaticData.SgameItemTable.ElementList.ConvertAll(item => item.Icon).ToArray();
            }

            return _itemSource;
        }
    }

    
    // 顾客时间算积分
    private static Dictionary<int, int> _customerTimeMap;
    public static Dictionary<int, int> CustomerTimeMap
    {
        get
        {
            if (null == _customerTimeMap)
            {
                _customerTimeMap = new Dictionary<int, int>();
                StaticData.SgamePointTable.ElementList.ForEach(row =>
                {
                    _customerTimeMap[row.Id] = row.Point;
                });
            }

            return _customerTimeMap;
        }
    }
    
    
    // 游戏时间算积分
    private static Dictionary<int, int> _gameTimeMap;
    public static Dictionary<int, int> GameTimeMap
    {
        get
        {
            if (null == _gameTimeMap)
            {
                _gameTimeMap = new Dictionary<int, int>();
                StaticData.SgameTimePointTable.ElementList.ForEach(row =>
                {
                    _gameTimeMap[row.Id] = row.Point;
                });
            }

            return _gameTimeMap;
        }
    }
}
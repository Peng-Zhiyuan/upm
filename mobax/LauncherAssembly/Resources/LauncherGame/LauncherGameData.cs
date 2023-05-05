using System;
using System.Collections.Generic;
using UnityEngine;

public static class LauncherGameData
{
    // 单次最小消除数
    public static int EliminateMin
    {
        get => 3; //StaticData.BaseTable.TryGet("SgameMinChoose");
    }
    
    // 单次最大消除数
    public static int EliminateMax
    {
        get => 5; //StaticData.BaseTable.TryGet("SgameMaxChoose");
    }

    // 最大遗弃数量
    public static int AbandonMax
    {
        get => 10; //StaticData.BaseTable.TryGet("SgameMaxStorage");
    }

    // 遗弃选择的扣分
    public static int AbandonDecrease
    {
        get => 50; //StaticData.BaseTable.TryGet("SgameMissPoint");
    }

    // 一个顾客的停留时间
    public static int CustomerTime
    {
        get => 20; //StaticData.BaseTable.TryGet("SgameTime");
    }

    // 一局游戏时间
    public static int GameTime
    {
        get => 90; //StaticData.BaseTable.TryGet("SgameMaxTime");
    }
    
    // 胜利所需分数
    public static int WinScore
    {
        get => 1000; //StaticData.BaseTable.TryGet("SgameMaxPoint");
    }
    
    
    private static string[] _itemSource;
    // 道具的数据源
    public static string[] ItemSource
    {
        get
        {
            if (null == _itemSource)
            {
                // _itemSource = StaticData.SgameItemTable.ElementList.ConvertAll(item => item.Icon).ToArray();
                _itemSource = new []
                {
                    "21501",
                    "21503",
                    "21504",
                    "21506",
                    "21507",
                    "21508",
                };
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
                _customerTimeMap = new Dictionary<int, int>
                {
                    [0] = 40,
                    [5] = 60,
                    [10] = 80,
                    [15] = 100,
                    [-1] = -50,
                };
                // StaticData.SgamePointTable.ElementList.ForEach(row =>
                // {
                //     _customerTimeMap[row.Id] = row.Point;
                // });
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
                _gameTimeMap = new Dictionary<int, int>
                {
                    [130] = 200,
                    [120] = 100,
                    [100] = 0,
                    [80] = -100,
                    [0] = -200,
                };
                // StaticData.SgameTimePointTable.ElementList.ForEach(row =>
                // {
                //     _gameTimeMap[row.Id] = row.Point;
                // });
            }

            return _gameTimeMap;
        }
    }

    private static int[][] _judgeList;

    public static int[][] JudgeList
    {

        get
        {
            if (null == _judgeList)
            {
                _judgeList = new []
                {
                    new [] {1, 1000},
                    new [] {2, 900},
                    new [] {3, 800},
                    new [] {4, 700},
                    new [] {5, 600},
                    new [] {6, 0},
                };
            }

            return _judgeList;
        }
    }

    public static Dictionary<int, string[]> _talkMap;
    
    public static Dictionary<int, string[]> TalkMap
    {
        get
        {
            if (null == _talkMap)
            {
                _talkMap = new Dictionary<int, string[]>
                {
                    [1] = new [] {"gameTrash_1_1", "gameTrash_1_2", "gameTrash_1_3"},
                    [2] = new [] {"gameTrash_2_1", "gameTrash_2_2", "gameTrash_2_3"},
                    [3] = new [] {"gameTrash_3_1", "gameTrash_3_2"},
                    [4] = new [] {"gameTrash_4_1"},
                };
            }

            return _talkMap;
        }
    }
}

public enum LauncherGameJudgeEnum
{
    S = 1,
    A,
    B,
    C,
    D,
    E,
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class CounterUtil 
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">统计方式，数据表中叫做 condition </param>
    /// <param name="key">统计用参数，数据表中叫做 key</param>
    /// <param name="arg">另外一种统计参数，某些统计方式专用，数据表欧中叫做 arg </param>
    /// <param name="startData">如果统计方式是历史记录，指定统计开始的日期</param>
    /// <param name="endDate">如果统计方式是历史记录，指定统计结束的日期</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static int Count(GameTaskCounterType type, int key, int[] arg, DateTime? startData = null, DateTime? endDate = null)
    {
        if (type == GameTaskCounterType.None)
        {
            return 0;
        }
        else if (type == GameTaskCounterType.Normal)
        {
            // 从某个数据库中查询
            var count = NormalCount(key);
            return count;
        }
        else if (type == GameTaskCounterType.Weekly)
        {
            // 从 daily 中累加本周的统计
            // 暂未实现
            Debug.LogError("[GameTaskUtil] Weekly counter is not implemented yet.");
            return 0;
        }
        else if (type == GameTaskCounterType.DailyHistroy)
        {
            // 更复杂的历史统计，仅在活动中使用
            if (startData == null)
            {
                startData = Database.Stuff.dailyDatabase.MinDateTime;
            }

            if (endDate == null)
            {
                endDate = Database.Stuff.dailyDatabase.MaxDateTime;
            }

            var count = Database.Stuff.dailyDatabase.Count(startData.Value, endDate.Value, key);
            return count;
        }
        else if (type == GameTaskCounterType.Stage)
        {
            var stageId = arg[0];
            var completedCount = Database.Stuff.stageDatabase.GetStageCompleteCount(stageId);
            return completedCount;
        }

        throw new Exception($"[GameTaskUtil] not support Counter type '{type}'");
    }


    static int NormalCount(int key)
    {
        // 等级
        if(key == 93101 || key == 10002)
        {
            return Database.Stuff.roleDatabase.Me.lv;
        }


        var isDailyRow = IsDailyRowId(key);
        if (isDailyRow)
        {
            // 是日常数据
            var dailyDatabase = Database.Stuff.dailyDatabase;
            var today = Clock.Now;
            var count = dailyDatabase.Count(today, today, key);
            return count;
        }
        else
        {
            // 是记录数据
            var recordDatabase = Database.Stuff.recordDatabase;
            var count = recordDatabase.GetValue(key);
            return count;
        }
    }

    static bool IsDailyRowId(int rowId)
    {
        var ret = StaticData.DailyTable.ContainsKey(rowId);
        return ret;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class WeeklyTaskUtil 
{
    public static WeekTaskRow GetTaskRow(int taskId)
    {
        var row = StaticData.WeekTaskTable[taskId];
        return row;
    }

    public static bool IsWeeklyTask(int taskId)
    {
        var ret = StaticData.WeekTaskTable.ContainsKey(taskId);
        return ret;
    }

    public static bool IsSubmited(int taskId)
    {
        var b = IsWeeklyTask(taskId);
        if(!b)
        {
            throw new Exception("[WeeklyTaskUtil] taskId: " + taskId + " is not a weekly task");
        }
        var isSubmited = Database.Stuff.taskDatabase.IsNonChainTaskSubmited(taskId);
        return isSubmited;
    }

    public static bool IsScoreRewardSubmited(int rewardId)
    {
        var activeId = "zx11";
        var ret = Database.Stuff.activeDatabase.HasFlag(activeId, rewardId);
        return ret;
    }

    /// <summary>
    /// 获取当期的周活跃度
    /// </summary>
    /// <returns></returns>
    public static int MyScore
    {
        get
        {
            var sum = 0;
            var rowList = AllTaskRowList;
            foreach (var row in rowList)
            {
                var id = row.Id;
                var isSubmited = Database.Stuff.taskDatabase.IsNonChainTaskSubmited(id);
                if (isSubmited)
                {
                    var liveness = row.Score;
                    sum += liveness;
                }
            }

            return sum;
        }
    }


    public static List<WeekTaskRow> AllTaskRowList
    {
        get
        {
            var rowList = StaticData.WeekTaskTable.ElementList;
            return rowList;
        }
    }

    public static bool HasAnyScoreRewardCanSubmit
    {
        get
        {
            var rowList = StaticData.WeekRewardTable.ElementList;
            var myLiveness = MyScore;
            foreach (var row in rowList)
            {
                var submited = IsScoreRewardSubmited(row.Id);
                if (submited)
                {
                    continue;
                }
                var need = row.Score;
                if (myLiveness >= need)
                {
                    return true;
                }
            }
            return false;
        }
 
    }

    public static bool HasAnyTaskCanSubmit
    {
        get
        {
            var taskList = AllTaskRowList;
            foreach (var row in taskList)
            {
                var id = row.Id;
                var isSubmited = IsSubmited(id);
                if (isSubmited)
                {
                    continue;
                }
                var info = GameTaskUtil.GetMyProgress(id);
                if (info.isCompleted)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static (bool isCompleted, int count, int targetCount) GetProcess(int taskId)
    {
        var row = GetTaskRow(taskId);

        var targetCount = row.Value;
        var targetId = row.Target;
        var record = targetId;

        var startDate = Clock.ThisWeek;
        var endDate = startDate.AddDays(6);

        var count = CounterUtil.Count(GameTaskCounterType.DailyHistroy, record, null, startDate, endDate);
        var isCompleted = count >= targetCount;
        return (isCompleted, count, targetCount);
    }

    public static bool HasAnyTaskOrScoreRewardCanSubmit
    {
        get
        {
            if (HasAnyTaskCanSubmit)
            {
                return true;
            }
            else if (HasAnyScoreRewardCanSubmit)
            {
                return true;
            }
            return false;
        }

    }

    public static async Task SubmitTask(int taskId, DisplayType displayType)
    {
        var info = await GameTaskApi.SubmitWeeklyAsync(taskId, displayType);
        Database.Stuff.taskDatabase.Add(info);
    }

    public static async Task SubmitReward(int id, DisplayType displayType)
    {
        var activeInfo = await GameTaskApi.SubmitWeeklyRewardAsync(id, displayType);
        Database.Stuff.activeDatabase.Add(activeInfo);
    }

    public static async Task SubmitAllScoreRewardAsync(bool show = true)
    {
        try
        {
            var rowList = StaticData.WeekRewardTable.ElementList;
            var myLiveness = MyScore;
            foreach (var row in rowList)
            {
                var submited = IsScoreRewardSubmited(row.Id);
                if (submited)
                {
                    continue;
                }
                var need = row.Score;
                if (myLiveness >= need)
                {
                    await SubmitReward(row.Id, DisplayType.Cache);
                }
            }
        }
        finally
        {
            if (show)
            {
                UiUtil.CleanAndShowAllCachedReward();
            }
        }

    }
}

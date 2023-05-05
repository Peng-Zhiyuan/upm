using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using CustomLitJson;

public static class GameTaskUtil
{


    public static bool IsSubmited(int taskId)
    {
        var isWeekly = WeeklyTaskUtil.IsWeeklyTask(taskId);
        if(isWeekly)
        {
            return WeeklyTaskUtil.IsSubmited(taskId);
        }

        var isDaily = GameTaskUtil.IsDailyTask(taskId);
        if (isDaily)
        {
            return DailyTaskUtil.IsSubmited(taskId);
        }

        // 链任务判断复杂未实现， 都返回没提交
        return false;
    }

    public static async Task SubmitAsyncList(List<int> taskIdList, bool show = true)
    {
        try
        {
            foreach (var id in taskIdList)
            {
                await GameTaskUtil.SubmitAsync(id, DisplayType.Cache);
            }
        }
        finally
        {
            if(show)
            {
                UiUtil.CleanAndShowAllCachedReward();
            }
        }
    }

    public static async Task<bool> SubmitAsync(int taskId, DisplayType displayType = DisplayType.Show)
    {
        var (isCompelted, _, _) = GameTaskUtil.GetMyProgress(taskId);
        if (!isCompelted)
        {
            return false;
        }

        var isWeeklyTask = WeeklyTaskUtil.IsWeeklyTask(taskId);
        if (isWeeklyTask)
        {
            await WeeklyTaskUtil.SubmitTask(taskId, displayType);
            return true;
        }

        var isDailyTask = GameTaskUtil.IsDailyTask(taskId);
        if (isDailyTask)
        {
            var info = await GameTaskApi.SubmitDailyAsync(taskId, displayType);
            Database.Stuff.taskDatabase.Add(info);
            return true;
        }
        

        {
            var ret = await GameTaskApi.SubmitChainAsync(taskId, displayType);
            var chainId = GameTaskUtil.GetAchievementRow(taskId).Id;
            Database.Stuff.taskDatabase.Add(ret);
            //Database.Stuff.taskDatabase.ModifyValue(chainId, taskId);
            TrackManager.UnlockAchievement(taskId.ToString());
        }


        return true;
    }

    public static bool IsTaskChain(int rowId)
    {
        return StaticData.TaskSeriesTable.ContainsKey(rowId);
    }

    public static TaskRow GetTaskRow(int rowId)
    {
        return StaticData.TaskTable[rowId];
    }

    public static bool IsTaskExists(int rowId)
    {
        return StaticData.TaskTable.ContainsKey(rowId);
    }

    //public static bool IsChainTask(int taskId)
    //{
    //    var row = GameTaskUtil.GetTaskRow(taskId);
    //    var chainId = row.Series;
    //    if (chainId == 0)
    //    {
    //        return false;
    //    }
    //    return true;
    //}

    public static TaskSeriesRow GetAchievementRow(int taskId)
    {
        var row = GameTaskUtil.GetTaskRow(taskId);
        var chainId = row.Series;
        var chainRow = StaticData.TaskSeriesTable[chainId];
        return chainRow;
    }

    public static ITaskRow GetAnyTaskRow(int taskId)
    {
        var achievementTaskRow = StaticData.TaskTable.TryGet(taskId);
        if (achievementTaskRow != null)
        {
            return achievementTaskRow;
        }

        var dailyTaskRow = StaticData.DailyTaskTable.TryGet(taskId);
        if (dailyTaskRow != null)
        {
            return dailyTaskRow;
        }

        throw new Exception("[GameTaskUtil] not found task for id: " + taskId);
    }

    public static bool IsDailyTask(int taskId)
    {
        var ret = StaticData.DailyTaskTable.ContainsKey(taskId);
        return ret;
    }


    public static (bool isCompleted, int count, int targetCount) GetMyProgress(int taskId)
    {
        var isWeekly = WeeklyTaskUtil.IsWeeklyTask(taskId);
        if(isWeekly)
        {
            return WeeklyTaskUtil.GetProcess(taskId);
        }

        var row = GetAnyTaskRow(taskId);

        var targetCount = row.Value;
        var targetId = row.Target;
        var isDailyTask = IsDailyTask(taskId);
        if (isDailyTask)
        {
            var record = targetId;
            var count = CounterUtil.Count(GameTaskCounterType.Normal, record, null);
            var isCompleted = count >= targetCount;
            return (isCompleted, count, targetCount);
        }
        else
        {
            // 应该是普通任务
            var normalTaskRow = row as TaskRow;

            var counterRow = StaticData.TaskTargetTable.TryGet(targetId);

            // 没有配置，可能还在开发中，那就当做没有完成
            if (counterRow == null)
            {
                Debug.LogError($"[GameTaskUtil] not found task target {targetId}, skip");
                return (false, 0, targetCount);
            }

            var arg = normalTaskRow.Args;
            var count = Count(counterRow, arg);
            var isComplete = count >= targetCount;
            return (isCompleted: isComplete, count: count, targetCount: targetCount);
        }
    }

    static int Count(TaskTargetRow counterRow, int[] arg)
    {
        var counterType = (GameTaskCounterType) counterRow.Condition;
        var key = counterRow.Key;
        var count = CounterUtil.Count(counterType, key, arg);
        return count;
    }


    // 在指定任务链上，我下一步（现在要显示的）要做的任务是什么？
    // 返回 0 表示没有下一步了
    public static int GetMyNextTaskRowIdOfChain(int chainId)
    {
        var info = Database.Stuff.taskDatabase.GetByChainIdOrTaskRowId(chainId);
        if (info == null)
        {
            var chainRow = StaticData.TaskSeriesTable[chainId];
            var startTaskRowId = chainRow.Key;
            return startTaskRowId;
        }

        var lastCompleteTaskRowId = info.val;
        //let taskRow = TsProtoStaticData.TaskTable.Get(lastCompleteTaskRowId)
        // let nextTaskRowId = taskRow.limit[1]

        var nextTaskRowId = GameTaskManager.GetNextTaskInChain(lastCompleteTaskRowId);
        return nextTaskRowId;
    }


    public static DailyTaskRow GetDailyTaskRow(int taskId)
    {
        var row = StaticData.DailyTaskTable[taskId];
        return row;
    }

    static List<DailyTaskRow> _getAllDailyTaskRowCache;

    // 1 每日 2 bp
    public static List<DailyTaskRow> GetAllDailyTaskRow(int type = 1)
    {
        if (_getAllDailyTaskRowCache != null)
        {
            return _getAllDailyTaskRowCache;
        }

        var rowList = StaticData.DailyTaskTable.ElementList;
        var ret = new List<DailyTaskRow>();
        foreach (var one in rowList)
        {
            if (one.Id == 7271000)
            {
                // 特殊配置，不是正经任务，仅用来记录活跃度领取状态
                continue;
            }

            if(one.Type == type)
            {
                ret.Add(one);
            }
        }

        _getAllDailyTaskRowCache = ret;
        return ret;
    }

    /// <summary>
    /// 获取当天活跃度
    /// </summary>
    /// <returns></returns>
    public static int MyDailyScore
    {
        get
        {
            var sum = 0;
            var rowList = GetAllDailyTaskRow(1);
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

    public static async void ReportDailyEventWorldChat()
    {
        await GameTaskApi.ChatAsync();
    }

    public static async void ReportDailyEventFollow(int count)
    {
        await GameTaskApi.FollowAsync(count);
    }

    public static List<int> ToSubmitDailyRewardIdList()
    {
        var ret = new List<int>();
        var rowList = StaticData.DailyRewardTable.ElementList;
        var myLiveness = GameTaskUtil.MyDailyScore;
        foreach (var row in rowList)
        {
            var submited = DailyTaskUtil.IsScoreRewardSubmited(row.Id);
            if (submited)
            {
                continue;
            }
            var need = row.Score;
            if (myLiveness >= need)
            {
                ret.Add(row.Id);
            }
        }
        return ret;
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public static class DailyTaskUtil 
{
    public static DailyTaskRow GetDailyTaskRow(int taskId)
    {
        var row = StaticData.DailyTaskTable[taskId];
        return row;
    }

    public static bool IsSubmited(int taskId)
    {
        var isDaily = GameTaskUtil.IsDailyTask(taskId);
        if (isDaily)
        {
            var isSubmited = Database.Stuff.taskDatabase.IsNonChainTaskSubmited(taskId);
            return isSubmited;
        }
        else
        {
            throw new Exception("[DailyTaskUtil] taskId: " + taskId + " is not a daily task");
        }
    }

    public static bool IsScoreRewardSubmited(int rewardId)
    {
        var activeId = "zx10";
        var ret = Database.Stuff.activeDatabase.HasFlag(activeId, rewardId);
        return ret;
    }

    public static bool HasAnyLivenessRewardCanSubmit()
    {
        var rowList = StaticData.DailyRewardTable.ElementList;
        var myLiveness = GameTaskUtil.MyDailyScore;
        foreach(var row in rowList)
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

    public static bool HasAnyDailyTaskOrDailyRewardCanSubmit()
    {
        var dataList = GameTaskUtil.GetAllDailyTaskRow(1);
        foreach(var row in dataList)
        {
            var id = row.Id;
            var isSubmited = IsSubmited(id);
            if(isSubmited)
            {
                continue;
            }
            var info = GameTaskUtil.GetMyProgress(id);
            if (info.isCompleted)
            {
                return true;
            }
        }

        var anyLivenessRewardCanSubmit = HasAnyLivenessRewardCanSubmit();
        if(anyLivenessRewardCanSubmit)
        {
            return true;
        }
        return false;
    }

    public static async Task SubmitReward(int id, DisplayType displayType)
    {
        var active = await GameTaskApi.SubmitDailyRewardAsync(id, displayType);
        Database.Stuff.activeDatabase.Add(active);
    }

    public static async Task SubmitAllRewardAsync(bool show = true)
    {
        try
        {
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

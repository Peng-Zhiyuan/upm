using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AchievementTaskUtil
{
    public static bool HasAnyTaskOrRewardCanSubmit
    {
        get
        {
            var b = HasAnyStoryTaskCanSubmit();
            if (b)
            {
                return true;
            }
            var b2 = HasAnyNormalAchievementTaskCanSubmit();
            if (b2)
            {
                return true;
            }
            var b3 = DailyTaskUtil.HasAnyDailyTaskOrDailyRewardCanSubmit();
            if (b3)
            {
                return true;
            }
            var b4 = WeeklyTaskUtil.HasAnyTaskOrScoreRewardCanSubmit;
            if (b4)
            {
                return true;
            }
            return false;
        }
    }

    public static bool HasAnyStoryTaskCanSubmit()
    {
        // 故事任务是一种任务链任务
        var taskList = CreateMyExecutingAchievementTaskList(1);
        foreach (var task in taskList)
        {
            var id = task.Id;
            var info = GameTaskUtil.GetMyProgress(id);
            if (info.isCompleted)
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasAnyNormalAchievementTaskCanSubmit()
    {
        // 普通成就任务是一种任务链任务
        var taskList = CreateMyExecutingAchievementTaskList(0);
        foreach (var task in taskList)
        {
            var id = task.Id;
            var info = GameTaskUtil.GetMyProgress(id);
            if (info.isCompleted)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 任务链中正在执行的那些任务（未解锁的任务会被排除）
    /// </summary>
    /// <param name="subType">0 普通成就任务，1 故事成就任务,2 好友bp, 3 公会bp</param>
    /// <returns></returns>
    public static List<TaskRow> CreateMyExecutingAchievementTaskList(int? subType = null)
    {
        var ret = new List<TaskRow>();
        var rowList = StaticData.TaskSeriesTable.ElementList;
        foreach (var row in rowList)
        {
            var chainId = row.Id;
            var taskId = GameTaskUtil.GetMyNextTaskRowIdOfChain(chainId);
            if (taskId == 0)
            {
                continue;
            }
            var isExists = GameTaskUtil.IsTaskExists(taskId);
            if (!isExists)
            {
                continue;
            }
            var taskRow = StaticData.TaskTable[taskId];
            if(subType != null)
            {
                var type = taskRow.subType;
                if(type != subType)
                {
                    continue;
                }
            }
            ret.Add(taskRow);
        }

        // 排除没解锁的任务
        for(int i = ret.Count - 1; i >= 0; i--)
        {
            var row = ret[i];
            var magicList = row.Unlocks;
            if(magicList != null && magicList.Length > 0)
            {
                var type = ArrayUtil.TryGet(magicList, 0);
                var param = ArrayUtil.TryGet(magicList, 1);
                if(type == 1)
                {
                    var needItem = param;
                    var holdCount = Database.Stuff.itemDatabase.GetHoldCount(needItem);
                    if(holdCount == 0)
                    {
                        ret.RemoveAt(i);
                    }
                }
            }
        }

        return ret;
    }

}

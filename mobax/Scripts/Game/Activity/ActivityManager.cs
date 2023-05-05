using System;
using System.Collections.Generic;
using UnityEngine;
using BattleEngine.Logic;

public class ActivityManager : Singleton<ActivityManager>
{
    private Dictionary<int, ActivityItem> _itemMap;
    private Dictionary<int, Action<int>> _openHandlerMap;
    private Dictionary<int, Action<int>> _endHandlerMap;

    public ActivityStatus GetStatus(int activityId)
    {
        if (_itemMap.TryGetValue(activityId, out var item))
        {
            return item.Status;
        }
        return ActivityStatus.Ended;
    }

    public ActivityItem GetDetail(int activityId)
    {
        if (_itemMap.TryGetValue(activityId, out var item))
        {
            return item;
        }
        return null;
    }

    public void OnOpen(int activityId, Action<int> callback)
    {
        _openHandlerMap ??= new Dictionary<int, Action<int>>();
        _openHandlerMap[activityId] = callback;
    }

    public void OnEnd(int activityId, Action<int> callback)
    {
        _endHandlerMap ??= new Dictionary<int, Action<int>>();
        _endHandlerMap[activityId] = callback;
    }

    public void UpdateActivityTask()
    {
        _itemMap = new Dictionary<int, ActivityItem>();
        List<ActivityRow> lst = StaticData.ActivityTable.ElementList;
        for (int i = 0; i < lst.Count; i++)
        {
            if (IsActivityBegin(lst[i]))
            {
                _CheckTime(lst[i]);
            }
        }
    }

    private void _CheckTime(ActivityRow activity)
    {
        var item = ActivityItem.TryCreate(activity);
        if (null != item)
        {
            _itemMap[activity.Id] = item;
            item.OnStart = _OnActivityStart;
            item.OnEnd = _OnActivityEnd;
        }
    }

    private void _OnActivityStart(int activityId)
    {
        if (_openHandlerMap.TryGetValue(activityId, out var callback))
        {
            // 用完即删
            _openHandlerMap.Remove(activityId);
            callback?.Invoke(activityId);
        }
    }

    private void _OnActivityEnd(int activityId)
    {
        if (_endHandlerMap.TryGetValue(activityId, out var callback))
        {
            // 用完即删
            _endHandlerMap.Remove(activityId);
            callback?.Invoke(activityId);
        }
    }

    public bool IsActivityBegin(ActivityRow activityRow)
    {
        if (activityRow.Id == 7)
        {
            if (SignUtil.IsExpired(1))
                return false;
            return true;
        }
        return !IsActivityTimeOver(activityRow);
    }

    public static bool IsActivityTimeOver(ActivityRow row)
    {
        if (row.TType == 0
            || string.IsNullOrEmpty(row.STime))
        {
            return false;
        }
        DateTime beginTime = ClockHelper.GetBeginTime(row.TType, row.STime);
        DateTime endTime = beginTime.AddDays(row.Days);
        if (beginTime > Clock.Now
            || endTime <= Clock.Now)
        {
            return true;
        }
        return false;
    }

    public bool IsHaveGetActivity()
    {
        UpdateActivityTask();
        var data = _itemMap.GetEnumerator();
        while (data.MoveNext())
        {
            if (IsActivityRedPoint(data.Current.Value.Cfg.Id))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsActivityRedPoint(int activityID)
    {
        ActivityRow activityRow = StaticData.ActivityTable.TryGet(activityID);
        if (activityRow == null)
        {
            return false;
        }
        if (activityRow.Jump != null
            && activityRow.Jump.Id == 36)
        {
            return ActivityTaskHelp.ActivityTaskCheckRedPoint(activityRow.Jump.Parm);
        }
        return false;
    }
}

public enum ActivityStatus
{
    BeforeOpen = 1,
    Opening,
    Ended
}

public class ActivityItem
{
    public Action<int> OnStart;
    public Action<int> OnEnd;

    public ActivityRow Cfg;
    public DateTime StartTime;
    public DateTime EndTime;
    public ActivityStatus Status { get; private set; }

    public static ActivityItem TryCreate(ActivityRow cfg)
    {
        if (cfg.TType != 0)
        {
            var beginTime = ClockHelper.GetBeginTime(cfg.TType, cfg.STime);
            var endTime = beginTime.AddDays(cfg.Days);
            if (Clock.Now >= endTime)
            {
                // 活动已经结束了， 就不创建了
                return null;
            }
        }
        return new ActivityItem(cfg);
    }

    public ActivityItem(ActivityRow cfg)
    {
        Cfg = cfg;
        StartTime = cfg.TType == 0 ? default : ClockHelper.GetBeginTime(cfg.TType, cfg.STime);
        EndTime = cfg.TType == 0 ? default : StartTime.AddDays(Cfg.Days);
        _CheckTime();
    }

    public void _CheckTime()
    {
        var now = Clock.Now;
        // 未开始
        if (default != StartTime
            && now < StartTime)
        {
            Status = ActivityStatus.BeforeOpen;
            Clock.On(StartTime, _CheckTime);
            return;
        }
        // 已经结束
        if (default != EndTime
            && now >= EndTime)
        {
            Status = ActivityStatus.Ended;
            OnEnd?.Invoke(Cfg.Id);
            Debug.Log($"【活动】「{Cfg.Des.Localize()}」结束");
            return;
        }
        // 剩下的都是活动开放的情况
        Status = ActivityStatus.Opening;
        OnStart?.Invoke(Cfg.Id);
        Debug.Log($"【活动】「{Cfg.Des.Localize()}」开启");
        if (default != EndTime)
        {
            Clock.On(EndTime, _CheckTime);
        }
    }
}

enum ActivityEnum
{
    T1_Time = 1,
    T2_OneTime // 一次性的
}
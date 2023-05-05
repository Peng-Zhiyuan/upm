using System;
using System.Globalization;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

public class Clock
{
    // utc起始时间
    private static DateTime _utcFrom = new DateTime(1970, 1, 1, 0, 0, 0);
    // 时间戳
    public static long TimestampMs => ClockBehavior.Stuff.TimestampMs;
    public static long TimestampSec => ClockBehavior.Stuff.Timestamp;
    // 当前时间
    public static DateTime Now => ToDateTime(TimestampMs);
    // 今日之零点
    public static DateTime Today => Now.Date;
    // 明日之零点
    public static DateTime Tomorrow => Today.AddDays(1);
    // 昨日之零点
    public static DateTime Yesterday => Today.AddDays(-1);

    // 时区
    public static int Timezone { get; set; } = 8;

    // 下周一零点
    public static DateTime NextWeek => ThisWeek.AddDays(7);

    // 这周一零点
    public static DateTime ThisWeek
    {
        get
        {
            var dayOfWeek = (int)Today.DayOfWeek;
            if (dayOfWeek == 0)
            {
                dayOfWeek = 7;
            }
            return Today.AddDays(1 - dayOfWeek);
        }
    }

    // 下月起始之刻
    public static DateTime NextMonth => ThisMonth.AddMonths(1);

    // 本月起始之刻
    public static DateTime ThisMonth => Today.AddDays(1 - Today.Day);

    public static DateTime ToDateTime(long timestampMs)
    {
        // 如果时间是整型范围内的数字，那么应该是秒。那么折算成毫秒
        if (timestampMs < 10000000000)
        {
            timestampMs = timestampMs * 1000;
        }
        return _utcFrom.AddMilliseconds(timestampMs).AddHours(Timezone);
    }

    /// <summary>
    /// 时间解析
    /// </summary>
    /// <param name="timePase">yyyyMMddHH</param>
    /// <returns></returns>
    public static DateTime PaseIntToDateTime(long yyyyMMddHH)
    {
        if (yyyyMMddHH >= int.MaxValue
            || yyyyMMddHH < 1000000000)
        {
            Debug.LogError("The Time pase is error");
            return Clock.Now;
        }
        return DateTime.ParseExact(yyyyMMddHH.ToString(), "yyyyMMddHH", CultureInfo.InvariantCulture);
    }

    public static DateTime? ToDateTimeAllowNull(long timestampMs)
    {
        if(timestampMs == 0)
        {
            return null;
        }
        return ToDateTime(timestampMs);
    }

    public static long ToTimestampMs(DateTime dt)
    {
        return (long)(dt.AddHours(-Timezone) - _utcFrom).TotalMilliseconds;
    }

    public static long ToTimestampS(DateTime dt)
    {
        return (long)(dt.AddHours(-Timezone) - _utcFrom).TotalSeconds;
    }

    public static void UpdateTimestamp(long ts)
    {
        ClockBehavior.Stuff.UpdateTimestamp(ts);
    }

    public static int On(DateTime dt, Action callback)
    {
        long timestampMs = ToTimestampMs(dt);
        return On(timestampMs, callback);
    }

    /// <summary>
    /// 监听单位为秒的时间戳，到点触发回调
    /// </summary>
    /// <param name="timestamp">秒为单位</param>
    /// <param name="callback">到点回调</param>
    /// <returns></returns>
    public static int On(int timestamp, Action callback)
    {
        return On(timestamp * 1000L, callback);
    }

    /// <summary>
    /// 监听单位为毫秒的时间戳，到点触发回调
    /// </summary>
    /// <param name="timestampMs">毫秒为单位</param>
    /// <param name="callback">到点回调</param>
    /// <returns></returns>
    public static int On(long timestampMs, Action callback)
    {
        var alarm = new Alarm { TimestampMs = timestampMs, Ring = callback, };
        ClockBehavior.Stuff.AddAlarm(alarm);
        return alarm.Id;
    }

    // 监听某一个小时的时间
    public static int OnHour(int hour, Action callback)
    {
        var dt = Today.AddHours(hour);
        long timestampMs = ToTimestampMs(dt);
        var alarm = new Alarm { TimestampMs = timestampMs, Ring = callback, RepeatType = RepeatType.Day, };
        ClockBehavior.Stuff.AddAlarm(alarm);
        return alarm.Id;
    }

    public static bool Off(int alarmId)
    {
        return ClockBehavior.Stuff.RemoveAlarm(alarmId);
    }
}

internal class Alarm
{
    private static int _aid;

    public int Id { get; }
    public RepeatType RepeatType { get; set; }
    public long TimestampMs { get; set; }
    public Action Ring { get; set; }

    public Alarm()
    {
        Id = ++_aid;
    }
}

internal enum RepeatType
{
    Day = 1,
    Week,
}

internal class Node
{
    public Alarm alarm;
    public Node prev;
    public Node next;
}

internal class ClockBehavior : StuffObject<ClockBehavior>
{
    // 更新的毫秒差
    private const int UPDATE_INTERVAL = 500;

    private int _recordTime;
    private int _duration;
    private long _recordTimestamp;
    private long _timestampMs;
    private Node _head;
    private Node _tail;

    [ShowInInspector]
    public long TimestampMs => _timestampMs;

    public long Timestamp => TimestampMs / 1000;

    /// <summary>
    /// 把alarm加到双向链表中
    /// </summary>
    /// <param name="alarm"></param>
    public void AddAlarm(Alarm alarm)
    {
        var newNode = new Node { alarm = alarm, };
        _UpdateAlarm(alarm);
        _IntoQueue(newNode);
    }

    /// <summary>
    /// 同步时间戳（可能时间久了会有时间误差，会一直同步）
    /// </summary>
    /// <param name="ts"></param>
    public void UpdateTimestamp(long ts)
    {
        _recordTime = (int)(Time.realtimeSinceStartup * 1000);
        _recordTimestamp = ts;
        _timestampMs = ts;
        _duration = 0;
    }

    /// <summary>
    /// 移除闹钟
    /// </summary>
    /// <param name="alarmId"></param>
    /// <returns>是否成功移除，否标识未找到对应的alarm</returns>
    public bool RemoveAlarm(int alarmId)
    {
        var node = _head;
        while (null != node)
        {
            if (node.alarm.Id == alarmId)
            {
                if (node.prev != null)
                {
                    node.prev.next = node.next;
                }
                if (node.next != null)
                {
                    node.next.prev = node.prev;
                }
                return true;
            }
            node = node.next;
        }
        return false;
    }

    private void Update()
    {
        int duration = _DurationSinceRecord();
        // 时间戳需要实时的
        _timestampMs = _recordTimestamp + duration;
        // 给定的间隔才会刷新判断闹钟是否到点
        if (duration - _duration > UPDATE_INTERVAL)
        {
            // LogUtil.Log($"【timestamp】record: {_recordTimestamp}, duration: {duration}, 时间戳：{_timestamp}");
            _CheckAlarm();
            _duration = duration;
        }
    }

    /// <summary>
    /// 检查闹钟是否已经到点
    /// </summary>
    private void _CheckAlarm()
    {
        var node = _head;
        while (node != null)
        {
            var alarm = node.alarm;
            // 判断第一个闹钟是否到点（闹钟都是按时间排序进来的）
            // 时间到了就开始Ring，应该不会有时间大跳跃的情况吧
            if (_timestampMs > alarm.TimestampMs)
            {
                // LogUtil.Log($"【闹钟】闹钟到点：{Clock.DateTime(alarm.timestamp)}, timestamp: {alarm.timestamp}");
                var ringedNode = node;
                alarm.Ring?.Invoke();
                if (node == _head)
                {
                    _head = node.next;
                }
                else
                {
                    node.prev.next = node.next;
                }
                node = node.next;
                // 如果是需要重复开启的闹钟，那么还需要重新调整顺序，加到链表里去
                if (default != alarm.RepeatType)
                {
                    ringedNode.prev = ringedNode.next = null;
                    _UpdateAlarm(alarm);
                    _IntoQueue(ringedNode);
                }
            }
            else
            {
                break;
            }
        }
    }

    private void _IntoQueue(Node node)
    {
        Node tmpNode = _head;
        Node prevNode = null;
        while (true)
        {
            if (tmpNode == null)
            {
                if (prevNode == null)
                {
                    _head = node;
                }
                else
                {
                    prevNode.next = node;
                    node.prev = prevNode;
                }
                _tail = node;
                break;
            }
            if (node.alarm.TimestampMs < tmpNode.alarm.TimestampMs)
            {
                var prev = tmpNode.prev;
                tmpNode.prev = node;
                node.next = tmpNode;
                if (null != prev)
                {
                    node.prev = prev;
                    prev.next = node;
                }

                // 如果node指向头，就要把头给更新了
                if (tmpNode == _head)
                {
                    _head = node;
                }
                break;
            }
            prevNode = tmpNode;
            tmpNode = tmpNode.next;
        }
    }

    // 调整到合适的时间
    private void _UpdateAlarm(Alarm alarm)
    {
        if (default != alarm.RepeatType)
        {
            var past = TimestampMs - alarm.TimestampMs;
            // 如果还没超过这个时间， 那么就不需要调整了
            if (past < 0) return;
            var dayMs = TimeSpan.TicksPerDay / TimeSpan.TicksPerMillisecond;
            // 更新到下一个计时周期
            switch (alarm.RepeatType)
            {
                case RepeatType.Day:
                    alarm.TimestampMs += dayMs * (past / dayMs + 1);
                    break;
                case RepeatType.Week:
                    alarm.TimestampMs += 7 * dayMs * (past / dayMs + 1);
                    break;
            }
        }
    }

    private int _DurationSinceRecord()
    {
        return (int)(Time.realtimeSinceStartup * 1000) - _recordTime;
    }
}
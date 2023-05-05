using System;
using System.Collections.Generic;

public static class DateUtil
{
    public static DateTime ZeroTime = new DateTime(1970, 1, 1);

    public static DateTime TimestampToDateTime(long timestampMs, int timeZone)
    {
        var zeroTimestampDate = new DateTime(1970, 1, 1, 0, 0, 0);
        zeroTimestampDate = zeroTimestampDate.AddHours(timeZone);
        var date = zeroTimestampDate.AddMilliseconds(timestampMs);
        return date;
    }

    public static long DateTimeToTimestamp(DateTime dateTime, int timeZone)
    {
        var zeroTimezoneDate = dateTime.Subtract(TimeSpan.FromHours(timeZone));
        var timestanpMs = zeroTimezoneDate.Millisecond;
        return timestanpMs;
    }

    public static bool IsBetween(DateTime value, DateTime? start, DateTime? end)
    {
        if(start != null)
        {
            if(value < start)
            {
                return false;
            }
        }

        if(end != null)
        {
            if(value >= end)
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsSameDay(DateTime a, DateTime b)
    {
        if (a.Year != b.Year)
        {
            return false;
        }
        if (a.Month != b.Month)
        {
            return false;
        }
        if (a.Day != b.Day)
        {
            return false;
        }
        return true;
    }

    public static bool IsSameMonth(DateTime a, DateTime b)
    {
        if (a.Year != b.Year)
        {
            return false;
        }
        else if (a.Month != b.Month)
        {
            return false;
        }
        return true;
    }

    public static int LocalTimezone
    {
        get
        {
            var utcDate = DateTime.UtcNow;
            var localDate = DateTime.Now;
            var shiftTimespan = localDate - utcDate;
            var shiftHours = shiftTimespan.Hours;
            return shiftHours;
        }
    }

    /// <summary>
    /// 日期索引是以 yyyymmdd 格式组成的整数
    /// </summary>
    /// <param name="dateIndex"></param>
    /// <returns></returns>
    public static DateTime DateIndexToDateTime(int dateIndex)
    {
        var year = dateIndex / 10000;
        var month = (dateIndex % 10000) / 100;
        var day = dateIndex % 100;
        var ret = new DateTime(year, month, day);
        return ret;
    }

    public static int DateToDateIndex(DateTime date)
    {
        return date.Year * 10000 + date.Month * 100 + date.Day;
    }

    public static int DateToDateIndexMonth(DateTime date)
    {
        return date.Year * 100 + date.Month;
    }

    /// <summary>
    /// 从指定日期时间开始，倒退到上一个星期几。
    /// 如果开始时间已经是指定的星期几，则不进行倒退
    /// </summary>
    /// <param name="dateTime">开始倒退的时间</param>
    /// <param name="dayOfWeek">星期几</param>
    /// <param name="keepTimePart">是否要保留时间部分</param>
    /// <returns></returns>
    public static DateTime BackToDayOfWeek(DateTime dateTime, DayOfWeek dayOfWeek, bool keepTimePart = true)
    {
        var dateTimeDayOfWeekValue = (int)dateTime.DayOfWeek;
        if (dateTimeDayOfWeekValue == 0)
        {
            dateTimeDayOfWeekValue = 7;
        }
        var targetValue = (int)dayOfWeek;
        if (targetValue == 0)
        {
            targetValue = 7;
        }
        if (targetValue < dateTimeDayOfWeekValue)
        {
            targetValue += 7;
        }
        var needBackDays = targetValue - dateTimeDayOfWeekValue;
        var needBackTimespan = TimeSpan.FromDays(needBackDays);
        var ret = dateTime - needBackTimespan;
        if (!keepTimePart)
        {
            ret = CleanTimePart(ret);
        }
        return ret;
    }

    public static DateTime CleanTimePart(DateTime dateTime)
    {
        var year = dateTime.Year;
        var month = dateTime.Month;
        var day = dateTime.Day;
        var ret = new DateTime(year, month, day);
        return ret;
    }

    /// <summary>
    /// 倒计时使用格式，只显示最大的两个单位，最大单位是"天"
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static string TimeSpanToStringWith2Unit(TimeSpan timeSpan)
    {
        var day = GetText("date_day");
        var hour = GetText("date_hour");
        var minute = GetText("date_minute");
        var sec = GetText("date_sec");
        if (timeSpan.Days > 0)
        {
            return $"{timeSpan.Days} {day} {timeSpan.Hours} {hour}";
        }
        else if (timeSpan.Hours > 0)
        {
            return $"{timeSpan.Hours} {hour} {timeSpan.Minutes} {minute}";
        }
        else if (timeSpan.Minutes > 0)
        {
            return $"{timeSpan.Minutes} {minute} {timeSpan.Seconds} {sec}";
        }
        else if (timeSpan.Seconds > 0)
        {
            return $"{timeSpan.Seconds} {sec}";
        }
        else
        {
            return $"0 {sec}";
        }
    }

    public static string TimeSpanToStringWith2UnitNoSpacing(TimeSpan timeSpan)
    {
        var day = GetText("date_day");
        var hour = GetText("date_hour");
        var minute = GetText("date_minute");
        var sec = GetText("date_sec");
        if (timeSpan.Days > 0)
        {
            return $"{timeSpan.Days:00}{day}{timeSpan.Hours:00}{hour}";
        }
        else if (timeSpan.Hours > 0)
        {
            return $"{timeSpan.Hours:00}{hour}{timeSpan.Minutes:00}{minute}";
        }
        else if (timeSpan.Minutes > 0)
        {
            return $"{timeSpan.Minutes:00}{minute}{timeSpan.Seconds:00}{sec}";
        }
        else if (timeSpan.Seconds > 0)
        {
            return $"{timeSpan.Seconds:00}{sec}";
        }
        else
        {
            return $"0{sec}";
        }
    }

    static string GetText(string key)
    {
        if (onGetText == null)
        {
            return key;
        }
        return onGetText.Invoke(key);
    }

    public static Func<string, string> onGetText;

    /// <summary>
    /// 倒计时使用格式，只显示最大的两个单位，最大单位是"天"
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static string TimeSpanToStringWith1Unit(TimeSpan timeSpan, bool spaceAroundNumber = true)
    {
        var day = GetText("date_day");
        var hour = GetText("date_hour");
        var minute = GetText("date_minute");
        var sec = GetText("date_sec");

        // 当英文时，数字和单词之间必须有空格
        var lan = LocalizationManager.Stuff.Language;
        if (lan == "en")
        {
            spaceAroundNumber = true;
        }
        if (timeSpan.Days > 0)
        {
            if (spaceAroundNumber)
            {
                return timeSpan.Days + " " + day;
            }
            else
            {
                return timeSpan.Days + day;
            }
        }
        else if (timeSpan.Hours > 0)
        {
            if (spaceAroundNumber)
            {
                return timeSpan.Hours + " " + hour;
            }
            else
            {
                return timeSpan.Hours + hour;
            }
        }
        else if (timeSpan.Minutes > 0)
        {
            if (spaceAroundNumber)
            {
                return timeSpan.Minutes + " " + minute;
            }
            else
            {
                return timeSpan.Minutes + minute;
            }
        }
        else if (timeSpan.Seconds > 0)
        {
            if (spaceAroundNumber)
            {
                return timeSpan.Seconds + " " + sec;
            }
            else
            {
                return timeSpan.Seconds + sec;
            }
        }
        else
        {
            if (spaceAroundNumber)
            {
                return "0 " + sec;
            }
            else
            {
                return "0" + sec;
            }
        }
    }

    public static string DateTimeToDay(DateTime dateTime)
    {
        return string.Format(LocalizationManager.Stuff.GetText("common_dateTime_longStr"), dateTime.Year, dateTime.Month, dateTime.Day);
    }
}

public enum TimeFormatType
{
    Digital,
    Localized,
}

public static class TimeExtension
{
    public static string ToStringHour(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "00:00:00";
        }
        else
        {
            var format = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(format, (int)t.TotalHours, t.Minutes, (int)t.Seconds);
        }
    }

    public static string ToStringMinute(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "00:00";
        }
        else
        {
            var format = "{0:D2}:{1:D2}";
            return string.Format(format, (int)t.TotalMinutes, t.Seconds);
        }
    }



    public static string ToMillionSecond(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "0.00";
        }
        else
        {
            var format = "{0:D}.{1:D2}";
            return string.Format(format, (int)t.TotalSeconds, (int)(t.Milliseconds / 10));
        }
    }

    public static string ToStringDay(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "0.00:00:00";
        }
        else
        {
            var format = "{0:D} {1:D2}:{2:D2}:{3:D2}";
            return string.Format(format, (int)t.TotalDays + "D", t.Hours, t.Minutes, t.Seconds);
        }
    }

    public static string ToString(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        double seconds = span.TotalSeconds;
        if (seconds > 86400)
            return span.ToStringDay(formatType);
        else if (seconds > 3600)
            return span.ToStringHour(formatType);
        else
            return span.ToStringMinute(formatType);
    }

    public static string ToStringHourWithoutS(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "00:00";
        }
        else
        {
            var format = "{0:D2}:{1:D2}";
            return string.Format(format, (int)t.TotalHours, t.Minutes);
        }
    }

    public static string ToStringMinuteWithoutS(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "00";
        }
        else
        {
            var format = "{0:D2}";
            return string.Format(format, (int)t.TotalMinutes);
        }
    }

    public static string ToStringDayWithoutS(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        TimeSpan t = span;
        if (t.TotalSeconds < 0)
        {
            return "0.00:00";
        }
        else
        {
            var format = "{0:D}.{1:D2}:{2:D2}";
            return string.Format(format, (int)t.TotalDays, t.Hours, t.Minutes);
        }
    }

    public static string ToStringWithoutS(this TimeSpan span, TimeFormatType formatType = TimeFormatType.Digital)
    {
        double seconds = span.TotalSeconds;
        if (seconds > 86400)
            return span.ToStringDay(formatType);
        else if (seconds > 3600)
            return span.ToStringHour(formatType);
        else
            return span.ToStringHour(formatType);
    }

    public static string ToStringArray<T>(this IList<T> arr)
    {
        if (arr == null)
            return "null";
        int size = arr.Count;
        if (size == 0)
            return "[]";
        string str = "[" + arr[0];
        for (int i = 1; i < size; i++)
            str += ", " + arr[i].ToString();
        str += "]";
        return str;
    }

    public static string GetRemainDay(TimeSpan span)
    {
        StrBuild.Instance.ClearSB();
        StrBuild.Instance.Append((int)span.TotalDays);
        StrBuild.Instance.Append(LocalizationManager.Stuff.GetText("common_day"));
        StrBuild.Instance.Append(span.Hours);
        StrBuild.Instance.Append(LocalizationManager.Stuff.GetText("common_hour"));
        return StrBuild.Instance.GetString();
    }

    public static string GetRemainSecond(TimeSpan span)
    {
        StrBuild.Instance.ClearSB();
        StrBuild.Instance.Append(span.Hours);
        StrBuild.Instance.Append(LocalizationManager.Stuff.GetText("common_hour"));
        StrBuild.Instance.Append(span.Minutes);
        StrBuild.Instance.Append(LocalizationManager.Stuff.GetText("common_minute"));
        StrBuild.Instance.Append(span.Seconds);
        StrBuild.Instance.Append(LocalizationManager.Stuff.GetText("common_second"));
        return StrBuild.Instance.GetString();
    }
}
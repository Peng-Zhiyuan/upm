using System;

public static class DateUtil
{
    public static DateTime TimestampToDateTime(long timestampMs, int timeZone)
    {
        var zeroTimestampDate = new DateTime(1970, 1, 1, 0, 0, 0);
        zeroTimestampDate.AddHours(timeZone);
        var date = zeroTimestampDate.AddMilliseconds(timestampMs);
        return date;
    }

    public static long DateTimeToTimestamp(DateTime dateTime, int timeZone)
    {
        var zeroTimezoneDate = dateTime.Subtract(TimeSpan.FromHours(timeZone));
        var timestanpMs = zeroTimezoneDate.Millisecond;
        return timestanpMs;
    }
    
    public static bool IsSameDay(DateTime a, DateTime b)
    {
        if(a.Year != b.Year)
        {
            return false;
        }
        if(a.Month != b.Month)
        {
            return false;
        }
        if(a.Day != b.Day)
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
}
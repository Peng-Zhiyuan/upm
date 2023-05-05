using System;
using UnityEngine;

public static class ClockHelper
{
    public static DateTime GetBeginTime(int TType, string sTime)
    {
        if (TType == 1)
        {
            return Convert.ToDateTime(sTime).ToUniversalTime().AddHours(Clock.Timezone);
        }
        else if (TType == 2)
        {
            DateTime beginTime = Clock.ToDateTime(Database.Stuff.roleDatabase.Me.logon);
            int SDay = Mathf.Max(0, (int.Parse(sTime) - 1));
            return new DateTime(beginTime.Year, beginTime.Month, beginTime.Day).AddDays(SDay);
        }
        return Clock.Now;
    }
    
    public static bool IsBetween(DateTime ? startDate, DateTime ? endDate)
    {
        var now = Clock.Now;
        if (startDate == null)
        {
            startDate = DateTime.MinValue;
        }
        if (endDate == null)
        {
            endDate = DateTime.MaxValue;
        }
        if (now >= startDate && now <= endDate)
        {
            return true;
        }
        return false;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TicketUtil
{
    public static int GetHoldCount(int ticketId, int staticCount, long lastCaculateTimeMs, long nowMs, int timezone)
    {
        var isTicket = IsTicket(ticketId);
        if(!isTicket)
        {
            throw new Exception($"[TicketUtil] row id `{ticketId}` is not a ticket");
        }
        var max = GetMax(ticketId);
        if (staticCount > max)
        {
            return staticCount;
        }
        
        var timeSpanRecover = GetRecoverCountOfTimeSpanRule(ticketId, lastCaculateTimeMs, nowMs);
        var resetRecover = GetRecoverCountOfResetDateRule(ticketId, lastCaculateTimeMs, nowMs, timezone, max);
        var count = staticCount + timeSpanRecover + resetRecover;
        if(count > max)
        {
            count = max;
        }
        return count;
    }

    public static bool IsTicket(int rowId)
    {
        var ret = StaticData.TicketTable.ContainsKey(rowId);
        return ret;
    }

    public static TicketRow GetTicketRow(int rowId)
    {
        var row = StaticData.TicketTable[rowId];
        return row;
    }

    /// <summary>
    /// 最大数量限制
    /// </summary>
    public static int GetMax(int rowId)
    {
        var row = GetTicketRow(rowId);
        var limit = row.Limits;
        var k = limit[0];
        var x = limit[1];
        var y = limit[2];
        var a = ResolveK(k);
        var ret = a + (x / 10000f) + y;
        return (int)ret;
    }

    static int ResolveK(int k)
    {
        if(k == 0)
        {
            return 0;
        }
        else
        {
            var count = GetAnyRowCount(k);
            return count;
        }
    }

    public static int GetAnyRowCount(int anyRowId)
    {
        var dataType = StaticDataUtil.GetServerDataModel(anyRowId);
        if(dataType == ServerDataModel.Item)
        {
            var ret = Database.Stuff.itemDatabase.GetHoldCount(anyRowId);
            return ret;
        }
        else if(dataType == ServerDataModel.Role)
        {
            throw new Exception($"[TicketUtil] get count of row id '{anyRowId}' is not implement yet (dataType: {dataType})");
        }
        throw new Exception($"[TicketUtil] get count of row id '{anyRowId}' is not implement yet");

    }

    /// <summary>
    /// 计算在按时间间隔恢复规则下的恢复量
    /// </summary>
    /// <param name="ticketId"></param>
    /// <param name="lastCaculateTime"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public static int GetRecoverCountOfTimeSpanRule(int ticketId, long lastCaculateTimeMs, long nowMs)
    {
        var row = GetTicketRow(ticketId);
        var dot = row.Dots;
        var seconds = dot[0];
        var count = dot[1];
        if(seconds == 0 || count == 0)
        {
            return 0;
        }
        var delta = nowMs - lastCaculateTimeMs;
        if (delta < 0)
        {
            delta = 0;
        }
        var ret = (int)(delta /(seconds * 1000)) * count;
        return ret;
    }

    public static int GetRecoverCountOfResetDateRule(int ticketId, long lastCaculateTimeMs, long nowMs, int timezone, int max)
    {
        var row = GetTicketRow(ticketId);
        var cycle = row.Cycles;
        var resetType = cycle[0];
        var count = cycle[1];
        if(resetType == 0)
        {
            // 不重置
            return 0;
        }
        else if(resetType == 1)
        {
            // 每天重置
            var lastCaculateDate = DateUtil.TimestampToDateTime(lastCaculateTimeMs, timezone);
            var nowDate = DateUtil.TimestampToDateTime(nowMs, timezone);
            if(nowDate < lastCaculateDate)
            {
                return 0;
            }
            var lastCaculateDateTotalDays = (int)(lastCaculateDate - DateUtil.ZeroTime).TotalDays;
            var nowDateTotalDays = (int)(nowDate - DateUtil.ZeroTime).TotalDays;
            var passDays = nowDateTotalDays - lastCaculateDateTotalDays;
            if(count == 0)
            {
                // 0 表示总是回满
                if(passDays > 0)
                {
                    return max;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var totalCount = passDays * count;
                return totalCount;
            }

        }
        else if(resetType == 2)
        {
            // 每月重置
            var lastCaculateDate = DateUtil.TimestampToDateTime(lastCaculateTimeMs, timezone);
            var nowDate = DateUtil.TimestampToDateTime(nowMs, timezone);
            if (nowDate < lastCaculateDate)
            {
                return 0;
            }
            var isSameMonth = DateUtil.IsSameMonth(lastCaculateDate, nowDate);
            if (isSameMonth)
            {
                return 0;
            }
            if(count == 0)
            {
                // 0 表示回满
                return max;
            }
            else
            {
                return count;
            }
        }
        else
        {
            throw new Exception($"[TicketUtil] reset type {resetType} is not implement yet");
        }
    }
}

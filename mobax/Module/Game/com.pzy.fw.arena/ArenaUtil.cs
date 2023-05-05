using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

public static class ArenaUtil
{
    public static int Formation1Power
    {
        get
        {
            var myPower = FormationUtil.GetFormationPower(EFormationIndex.Normal1);
            return myPower;
        }
    }

    public static DateTime Seassion1StartDate()
    {
        var serverOpenTime = LoginManager.Stuff.session.userGameServerInfo.ServerTime;
        var serverOpenDate = Clock.ToDateTime(serverOpenTime);
        var mondayDate = DateUtil.BackToDayOfWeek(serverOpenDate, DayOfWeek.Monday, false);
        var startDate = mondayDate.AddHours(10);
        return startDate;
    }

    /// <summary>
    /// 赛季编号，从 1 开始
    /// </summary>
    /// <returns></returns>
    public static int GetSessionNumber(int sessionIndex)
    {
        var date = DateUtil.DateIndexToDateTime(sessionIndex);
        var seassion1Date = Seassion1StartDate();
        seassion1Date = DateUtil.CleanTimePart(seassion1Date);
        var delta = date - seassion1Date;
        var days = (int)delta.TotalDays;
        var seassionCount = days / 28;
        return seassionCount + 1;
    }



    /// <summary>
    /// sessionIndex 包含了一个年月日日期
    /// 28天一个赛季
    /// </summary>
    /// <param name="sessionIndex"></param>
    /// <returns></returns>
    public static DateTime GetSessionStartDate(int sessionIndex)
    {
        var date = DateUtil.DateIndexToDateTime(sessionIndex);
        // 朱志文：
        // 赛季总是周一上午 10 点开始
        date = date.AddHours(10);
        return date;
    }

    /// <summary>
    /// 赛季的结束日期
    /// </summary>
    /// <param name="sessionIndex"></param>
    /// <returns></returns>
    public static DateTime GetSessionEndDate(int sessionIndex)
    {
        var startDate = GetSessionStartDate(sessionIndex);
        startDate = DateUtil.CleanTimePart(startDate);
        var endDate = startDate.AddDays(28);
        endDate = endDate.AddHours(-2);
        return endDate;
    }

    public static ArenaRow GetArenaRowByScore(int score)
    {
        var rowList = StaticData.ArenaTable.ElementList;
        var count = rowList.Count;
        for (int i = count - 1; i >= 0; i--)
        {
            var row = rowList[i];
            if (score >= row.Score)
            {
                return row;
            }
        }
        throw new Exception("[ArenaUtil] not foun arena row for score: " + score);
    }

    public static bool HasArenaRow(int rowId)
    {
        var ret = StaticData.ArenaTable.ContainsKey(rowId);
        return ret;
    }

    public static ArenaRow GetArenaRow(int rowId)
    {
        var ret = StaticData.ArenaTable[rowId];
        return ret;
    }

    public static (ArenaRow nowRow, ArenaRow nextRow) GetRankInfo(int score)
    {
        var nowRow = GetArenaRowByScore(score);
        var nowRowId = nowRow.Id;
        var nextRowId = nowRowId + 1;
        var hasNext = HasArenaRow(nextRowId);
        if (!hasNext)
        {
            return (nowRow, null);
        }
        else
        {
            var nextRow = GetArenaRow(nextRowId);
            return (nowRow, nextRow);
        }
    }

    /// <summary>
    /// 计算胜利奖励积分
    /// </summary>
    /// <param name="enemyScore">敌人当前积分</param>
    /// <param name="myScore">我的当前积分</param>
    /// <returns></returns>
    public static int GetRewardScore(int enemyScore, int myScore)
    {
        var a = (enemyScore - myScore) / 10f;
        var b = Math.Max(a, 0);
        var c = Math.Min(b, 10);
        var myRankId = GetArenaRowByScore(myScore).Id;
        var d = 8 - myRankId;
        var e = 10 + c + d * 4;
        return (int)e;
    }

    public static int GetRewardCountOfItemId(int arenaId, int itemId)
    {
        var row = GetArenaRow(arenaId);
        var rewardList = row.battleRewards;
        foreach (var one in rewardList)
        {
            var thisId = one.Id;
            if (thisId == itemId)
            {
                return one.Num;
            }
        }
        return 0;
    }

    static int? _refreshEnemyCountMax;
    public static int RefreshEnemyCountMax
    {
        get
        {
            if (_refreshEnemyCountMax == null)
            {
                var rowList = StaticData.ArenaRefreshTable.ElementList;
                var maxId = 0;
                foreach (var row in rowList)
                {
                    var thisId = row.Id;
                    if (thisId > maxId)
                    {
                        maxId = thisId;
                    }
                }
                _refreshEnemyCountMax = maxId;
            }
            return _refreshEnemyCountMax.Value;
        }
    }

    static int? _buyTicektCountMax;
    public static int BuyTicketCountMax
    {
        get
        {
            if (_buyTicektCountMax == null)
            {
                var rowList = StaticData.ArenaPurchaseTable.ElementList;
                var maxId = 0;
                foreach (var row in rowList)
                {
                    var thisId = row.Id;
                    if (thisId > maxId)
                    {
                        maxId = thisId;
                    }
                }
                _buyTicektCountMax = maxId;
            }
            return _buyTicektCountMax.Value;
        }
    }

    public static int MyRefreshedEnemyCount
    {
        get
        {
            var ret = Database.Stuff.dailyDatabase.Get(DailyId.ArenaRefreshCount);
            return ret;
        }
    }

    public static int MyBuyTicketCount
    {
        get
        {
            var ret = Database.Stuff.dailyDatabase.Get(DailyId.ArenaBuyTicketCount);
            return ret;
        }
    }

    public static ArenaRefreshRow MyNextRefreshRow
    {
        get
        {
            var max = RefreshEnemyCountMax;
            var refreshedCount = MyRefreshedEnemyCount;
            if (refreshedCount >= max)
            {
                return null;
            }
            var nextRefreshRowId = refreshedCount + 1;
            var row = StaticData.ArenaRefreshTable[nextRefreshRowId];
            return row;
        }
    }

    public static ArenaPurchaseRow MyNextBuyTicketRow
    {
        get
        {
            var max = BuyTicketCountMax;
            var myCount = MyBuyTicketCount;
            if (myCount >= max)
            {
                return null;
            }
            var nextRowId = myCount + 1;
            var row = StaticData.ArenaPurchaseTable[nextRowId];
            return row;
        }
    }

    /// <summary>
    /// 在不同的季节（jijie）中会使用不同的 icon
    /// 会根据赛季（session）的开始月份判断季节
    /// </summary>
    /// <param name="rankId"></param>
    /// <param name="sessionIndex"></param>
    /// <returns></returns>
    public static string GetRankIconAddress(int rankId, int sessionIndex)
    {
        var rankRow = StaticData.ArenaTable[rankId];
        var iconBase = rankRow.Icon;
        var iconAddress = $"{iconBase}_{sessionIndex}.png";
        return iconAddress;
    }

    /// <summary>
    /// 返回赛季（session）所在的季节（jijie）
    /// </summary>
    /// <param name="seasionIndex"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static int GetJijieOfSeassionIndex(int seasionIndex)
    {
        var date = DateUtil.DateIndexToDateTime(seasionIndex);
        var month = date.Month;

        var jijieRowList = StaticData.ArenaIconTable.ElementList;
        foreach (var one in jijieRowList)
        {
            var monthList = one.Months;
            if (ArrayUtil.Contains(monthList, month))
            {
                var jijieId = one.Id;
                return jijieId;
            }
        }
        throw new Exception("[ArenaUtil] not found jijie for seasion index: " + seasionIndex);
    }

    public static string GetIconWithJijie(int rowId, int sessionIndex)
    {
        var row = StaticData.ArenaTable[rowId];
        var iconBase = row.Icon;
        var jijieId = GetJijieOfSeassionIndex(sessionIndex);
        var ret = $"{iconBase}_{jijieId}.png";
        return ret;
    }

    public static int GetStar(int rowId)
    {
        if(rowId == 21)
        {
            return 0;
        }
        else
        {
            var yu = rowId % 4;
            if(yu == 0)
            {
                yu = 4;
            }
            var ret = yu - 1;
            return ret;
        }
    }

    /// <summary>
    /// 可用挑战次数
    /// </summary>
    public static int ChanceCount
    {
        get
        {
            var ticketId = ItemId.ArenaTicket;
            var holdCount = Database.Stuff.itemDatabase.GetHoldCount(ticketId);
            return holdCount;
        }
    }

    public static async Task<bool> BuyChanceAsync()
    {
        var ticketHoldCount = Database.Stuff.itemDatabase.GetHoldCount(ItemId.ArenaTicket);
        var ticketMaxCount = TicketUtil.GetMax(ItemId.ArenaTicket);
        if (ticketHoldCount >= ticketMaxCount)
        {
            ToastManager.ShowLocalize("arena_buy_ticket_full");
            return false;
        }

        var row = ArenaUtil.MyNextBuyTicketRow;
        if (row == null)
        {
            ToastManager.ShowLocalize("arena_buy_ticket_use_out");
            return false;
        }

        var firstCostItem = row.Subs[0];
        var costItemId = firstCostItem.Id;
        var costCount = firstCostItem.Num;
        var itemName = ItemUtil.GetLocalizedName(costItemId);
        var buyCountMax = ArenaUtil.BuyTicketCountMax;
        var myBuyCount = ArenaUtil.MyBuyTicketCount;


        var arg0 = costCount;
        var arg1 = itemName;
        var arg2 = myBuyCount;
        var arg3 = buyCountMax;
        //var text = $"是否花费 <color=#ffff00>{costCount} {itemName}</color> 购买一次竞技场挑战次数？\n(今日已购买 {myBuyCount}/{buyCountMax} 次)";
        var text = LocalizationManager.Stuff.GetText("arena_buy_ticket", arg0, arg1, arg2, arg3);
        var choice = await Dialog.AskAsync("", text);
        if(!choice)
        {
            return false;
        }

        UiUtil.CheckEnough(costItemId, costCount);

        var index = myBuyCount + 1;
        await ArenaApi.PurchaseTicketAsync(index);

        TrackManager.CustomReport("Buy_Arena", "num", index.ToString());
        return true;
    }


    public static async Task<bool> ResetEnemyAsync()
    {
        var nextRefreshRow = ArenaUtil.MyNextRefreshRow;
        if (nextRefreshRow == null)
        {
            // 次数已用尽
            return false;
        }
        var costList = nextRefreshRow.Subs;
        // 默认只有一个消耗品
        var cost = costList[0];
        var itemId = cost.Id;
        var count = cost.Num;


        var max = ArenaUtil.RefreshEnemyCountMax;
        var nowCount = ArenaUtil.MyRefreshedEnemyCount;
        string text = null;
        if (count != 0)
        {
            var itemName = ItemUtil.GetLocalizedName(itemId);

            var arg0 = count;
            var arg1 = itemName;
            var arg2 = nowCount;
            var arg3 = max;
            //text = $"是否花费 <color=#ffff00>{count} {itemName}</color> 刷新竞技场对手？\n(今日已刷新 {nowCount}/{max} 次)";
            text = LocalizationManager.Stuff.GetText("arena_refresh_enemy", arg0, arg1, arg2, arg3);
        }
        else
        {
            text = LocalizationManager.Stuff.GetText("arena_refresh_enemy_free");
            var arg0 = nowCount;
            var arg1 = max;
            //text = $"是否刷新竞技场对手？\n(今日已刷新 {nowCount}/{max} 次)";
            text = LocalizationManager.Stuff.GetText("arena_refresh_enemy_free", arg0, arg1);
        }

        var choice = await Dialog.AskAsync("", text);
        if(!choice)
        {
            return false;
        }
        await ArenaManager.Stuff.RefreshEnemyAsync();
        return true;
    }
    

}

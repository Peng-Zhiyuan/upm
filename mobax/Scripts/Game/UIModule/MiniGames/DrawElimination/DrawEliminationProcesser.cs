using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * 处理结束事宜
 */
public static class DrawEliminationProcesser
{
    private const string CookiePassTimes = "Eliminate_passTimes";
    private const string CookieFastPassTime = "Eliminate_fastPassTime";
    private const string CookieBestScore = "Eliminate_bestScore";
    private const string CookieBestJudge = "Eliminate_bestJudge";

    public static int PassTimes => RoleLocalCache.GetInt(CookiePassTimes);
    public static int FastPassTime => RoleLocalCache.GetInt(CookieFastPassTime);
    public static int BestScore => RoleLocalCache.GetInt(CookieBestScore);
    public static int BestJudge => RoleLocalCache.GetInt(CookieBestJudge);
    
    public static async void Finish(int score, int costTime, int missNum)
    {
        // 只有分数超过了设定分数， 这个记录才会被记录下来
        if (score >= DrawEliminationData.WinScore)
        {
            // 通关次数 +1
            RoleLocalCache.SetInt(CookiePassTimes, PassTimes + 1);
        }
        var finalScore = score + _GetTimeScore(costTime) - missNum * DrawEliminationData.AbandonDecrease;
        if (BestScore <= 0 || finalScore > BestScore)
        {
            // 这个分数目前只记录， 不再哪里做显示了
            RoleLocalCache.SetInt(CookieBestScore, finalScore);
            RoleLocalCache.SetInt(CookieFastPassTime, costTime);
        }
        Debug.Log($"最终得分：{finalScore}， 消耗时间：{costTime}");
        
        var judgeRow = GetJudgement(finalScore);
        if (BestJudge <= 0 || judgeRow.Point > BestJudge || (judgeRow.Point == BestJudge && costTime < FastPassTime))
        {
            RoleLocalCache.SetInt(CookieBestJudge, judgeRow.Point);
            RoleLocalCache.SetInt(CookieFastPassTime, costTime);
        }
        List<ItemInfo> rewards = null;
        if (!TodayPrized())
        {
            var jsonDataTransactionList =  await ActiveApi.EliminatePrize(judgeRow.Id);
            var transactionList = UiUtil.JsonDataListToDatabaseTransactionList(jsonDataTransactionList);
            rewards = UiUtil.DatabaseTransactionListToDisplayableItemInfoList(transactionList);
            // 写入时间
            Database.Stuff.roleDatabase.Me.evaluate = Clock.TimestampMs;
        }
        
        // 打点上报
        TrackManager.CustomReport("Sgame", new Dictionary<string, string>
        {
            ["time"] = "" + costTime / 1000f,
            ["evaluate"] = judgeRow.Name,
            ["point"] = "" + finalScore,
            ["loss"] = "" + missNum,
        });
        
        await UIEngine.Stuff.ShowFloatingAsync<DrawEliminationResultFloating>(param: new DrawEliminationResultParam
        {
            judge = judgeRow.Name,
            costTime = costTime,
            rewards = rewards,
        });
    }

    /// <summary>
    /// 获得评价
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public static SgameEvaluateRow GetJudgement(int score)
    {
        var list = StaticData.SgameEvaluateTable.ElementList;
        foreach (var row in list)
        {
            if (score >= row.Point)
            {
                return row;
            }
        }

        return list[list.Count - 1];
    }

    /// <summary>
    /// 今天是否领过奖了
    /// </summary>
    /// <returns></returns>
    public static bool TodayPrized()
    {
        var prizedTs = Database.Stuff.roleDatabase.Me.evaluate;
        if (prizedTs <= 0) return false;
        
        var prizedDay = Clock.ToDateTime(prizedTs);
        return prizedDay.Year == Clock.Now.Year
               && prizedDay.DayOfYear == Clock.Now.DayOfYear;
    }
    
    private static int _GetTimeScore(int costTime)
    {
        var costMap = DrawEliminationData.GameTimeMap;
        var leftTime = DrawEliminationData.GameTime * 1000 - costTime;
        if (costTime >= 0 && leftTime >= 0)
        {
            var leftTimeArr = costMap.Keys.ToArray();
            for (var i = leftTimeArr.Length - 1; i >= 0; i--)
            {
                var key = leftTimeArr[i];
                if (leftTime >= key * 1000)
                {
                    return costMap[key];
                }
            }
        }
        
        return costMap[0];
    }
}
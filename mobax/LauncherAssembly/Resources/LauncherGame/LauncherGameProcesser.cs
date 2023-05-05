
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/**
 * 处理结束事宜
 */
public static class LauncherGameProcesser
{
    private const string CookiePassTimes = "LauncherGame_passTimes";
    private const string CookieFastPassTime = "LauncherGame_fastPassTime";
    private const string CookieBestScore = "LauncherGame_bestScore";
    private const string CookieBestJudge = "LauncherGame_bestJudge";
    
    public static bool Finished { get; private set; }
    
    public static void ClearPops()
    {
        LauncherUiManager.Stuff.Remove<LauncherGameResultPop>();
    }

    public static void SetFinished()
    {
        LauncherUiManager.Stuff.Remove<LauncherGamePage>();
        Finished = true;
    }

    public static void Replay()
    {
        ClearPops();
        
        var page = Object.FindObjectOfType<LauncherGamePage>();
        if (null != page)
        {
            page.ReStart();
        }
        else
        {
            LauncherUiManager.Stuff.Show<LauncherGamePage>("LauncherGame");
        }
    }
    
    public static void Finish(int score, int costTime, int missNum)
    {
        var finalScore = score + _GetTimeScore(costTime) - missNum * LauncherGameData.AbandonDecrease;
        var judgeLevel = GetJudgement(finalScore);
        
        var pop = LauncherUiManager.Stuff.Show<LauncherGameResultPop>("LauncherGame");
        pop.SetInfo((LauncherGameJudgeEnum) judgeLevel, costTime);
    }
    
    /// <summary>
    /// 获得评价
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public static int GetJudgement(int score)
    {
        var list = LauncherGameData.JudgeList;
        foreach (var row in list)
        {
            if (score >= row[1])
            {
                return row[0];
            }
        }

        return list[list.Length - 1][0];
    }
    
    private static int _GetTimeScore(int costTime)
    {
        var costMap = LauncherGameData.GameTimeMap;
        var leftTime = LauncherGameData.GameTime * 1000 - costTime;
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
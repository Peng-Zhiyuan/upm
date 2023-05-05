using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public static class LauncherGameHelper
{
    /// <summary>
    /// links寻找匹配的demands元素，如果全满足就返回所有的位置，如果有不满足的，就返回0
    /// </summary>
    /// <param name="demands"></param>
    /// <param name="linkItems"></param>
    /// <returns></returns>
    public static int Match(string[] demands, List<string> linkItems)
    {
        var matchFlag = 0;
        foreach (var demand in demands)
        {
            var from = 0;
            while (true)
            {
                var index = linkItems.IndexOf(demand, from);
                if (index < 0)
                {
                    // 一种道具没找到就直接结束了
                    matchFlag = 0;
                    break;
                }

                // 因为可能会有相同道具， 那么如果是用过了就得继续往下找
                var flag = 1 << index;
                if ((matchFlag & flag) == flag)
                {
                    from = index + 1;
                }
                else
                {
                    matchFlag |= flag;
                    break;
                }
            }

            if (matchFlag == 0) break;
        }

        return matchFlag;
    }
    
    /// <summary>
    /// 计算顾客满意度
    /// </summary>
    /// <param name="costTime"></param>
    /// <returns></returns>
    public static int GetCustomerScore(int costTime)
    {
        var costMap = LauncherGameData.CustomerTimeMap;
        var leftTime = LauncherGameData.CustomerTime * 1000 - costTime;
        if (costTime >= 0 && leftTime >= 0)
        {
            var leftTimeArr = costMap.Keys.ToArray();
            for (var i = leftTimeArr.Length - 1; i >= 0; i--)
            {
                var key = leftTimeArr[i];
                if (key >= 0 && leftTime >= key * 1000)
                {
                    return costMap[key];
                }
            }
        }
        
        return costMap[-1];
    }

    /// <summary>
    /// 取得等待的阶段（用来显示颜色）
    /// </summary>
    /// <param name="leftTime"></param>
    /// <returns></returns>
    public static int GetWaitStage(int leftTime)
    {
        var costMap = LauncherGameData.CustomerTimeMap;
        var leftTimeArr = costMap.Keys.ToArray();
        var count = leftTimeArr.Length;
        for (var i = count - 1; i >= 0; i--)
        {
            var key = leftTimeArr[i];
            if (key >= 0 && leftTime >= key * 1000)
            {
                return i;
            }
        }

        return 0;
    }

    public static void TweenTo(int from, int to, float duration, Action<int> onUpdate)
    {
        var currentValue = from;
        DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            onUpdate?.Invoke(x);
        }, to, duration);
    }
}
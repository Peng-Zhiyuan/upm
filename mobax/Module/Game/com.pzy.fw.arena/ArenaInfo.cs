using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaInfo 
{
    public string _id;
    public string uid;
    /// <summary>
    /// 本赛季最佳积分
    /// </summary>
    public int best;

    /// <summary>
    /// 失败次数are
    /// </summary>
    public int fails;

    /// <summary>
    /// 胜利次数
    /// </summary>
    public int wins;

    /// <summary>
    /// 段位
    /// </summary>
    public int level;

    /// <summary>
    /// 积分
    /// </summary>
    public int score;

    /// <summary>
    /// 赛季标志，日期index，eg：20200708
    /// </summary>
    public int circle;

    /// <summary>
    /// 结算相关标志，客户端应该不需要关心
    /// </summary>
    public int settle;

    /// <summary>
    /// 奖励领取的位桶
    /// </summary> 
    public ulong reward;

    public bool IsRewardReceived(int id)
    {
        var ret = BitUtil.IsBitSet(this.reward, id);
        return ret;
    }

    
}

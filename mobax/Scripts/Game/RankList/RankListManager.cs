/* Created:Loki Date:2023-03-07*/

using System.Collections.Generic;
using System.Threading.Tasks;
using CustomLitJson;
using UnityEngine;

public class RankListManager : Single<RankListManager>
{
    private Dictionary<string, List<RankListInfo>> rankLisDic = new Dictionary<string, List<RankListInfo>>();
    private Dictionary<string, NetPage<RankListInfo>> netPageDic = new Dictionary<string, NetPage<RankListInfo>>();

    public NetPage<RankListInfo> GetNetPageInfo(string rankKey)
    {
        if (netPageDic.ContainsKey(rankKey))
        {
            return netPageDic[rankKey];
        }
        return null;
    }

    /// <summary>
    /// 获取排行榜数据
    /// </summary>
    /// <param name="rankKey">排行榜名称</param>
    /// <param name="index">开始位</param>
    /// <param name="offset">数量</param>
    /// <returns></returns>
    public async Task<List<RankListInfo>> GetRankListData(string rankKey, int indexPage, int offset)
    {
        if (!rankLisDic.ContainsKey(rankKey))
        {
            await ASyncRankListData(rankKey, indexPage, offset);
            return rankLisDic[rankKey];
        }
        List<RankListInfo> lst = rankLisDic[rankKey];
        int index = Mathf.Max(0, indexPage - 1);
        if (index * offset >= lst.Count)
        {
            await ASyncRankListData(rankKey, indexPage, offset);
            return rankLisDic[rankKey];
        }
        return rankLisDic[rankKey];
    }

    private async Task ASyncRankListData(string rankKey, int indexPage, int offset)
    {
        int record = 0;
        if (netPageDic.ContainsKey(rankKey))
        {
            record = netPageDic[rankKey].record;
        }
        var arg = new JsonData();
        arg["name"] = rankKey;
        arg["page"] = indexPage;
        arg["size"] = offset;
        arg["record"] = record;
        var ret = await NetworkManager.Stuff.CallAsync<NetPage<RankListInfo>>(ServerType.Game, "socialize/rank/paging", arg, isBlock: false);
        if (rankLisDic.ContainsKey(rankKey))
        {
            rankLisDic[rankKey].AddRange(ret.rows);
        }
        else
        {
            var resultInfo = new List<RankListInfo>();
            resultInfo.AddRange(ret.rows);
            rankLisDic[rankKey] = resultInfo;
        }
        rankLisDic[rankKey].Sort(SortRankList);
        netPageDic[rankKey] = ret;
    }

    /// <summary>
    /// 获取个人赛季排行
    /// </summary>
    /// <param name="radarType"></param>
    /// <returns></returns>
    public async Task<RankListInfo> RequestSeflRankAsync(string rankKey, string circle, int league = 0)
    {
        var jd = new JsonData();
        jd["name"] = rankKey;
        if (league > 0)
        {
            jd["id"] = league;
        }
        jd["circle"] = circle;
        var ret = await NetworkManager.Stuff.CallAsync<RankListInfo>(ServerType.Game, "socialize/rank/rank", jd);
        return ret;
    }

    private int SortRankList(RankListInfo data1, RankListInfo data2)
    {
        if (data1.rank < data2.rank)
        {
            return -1;
        }
        else if (data1.rank == data2.rank)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}

public class RankListInfo
{
    /// <summary>
    /// 此时为公会id
    /// </summary>
    public string uid;

    /// <summary>
    /// 当前排名
    /// </summary>
    public int rank;

    /// <summary>
    /// 分数
    /// </summary>
    public long score;
}
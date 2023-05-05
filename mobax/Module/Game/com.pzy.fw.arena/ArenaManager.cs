using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using System.Linq;

public class ArenaManager : StuffObject<ArenaManager>
{
    public ArenaInfo info;
    public List<ArenaEnemyInfo> enemyInfoList = new List<ArenaEnemyInfo>();

    ///// <summary>
    ///// 我的最佳历史赛季的信息
    ///// </summary>
    //public ArenaInfo bestSessionInfo;

    public List<ArenaInfo> histroyList = new List<ArenaInfo>();
    
    public List<string> CurrentEnemyUidList
    {
        get
        {
            if(this.enemyInfoList != null)
            {
                var idList = this.enemyInfoList.Select(info => info._id).ToList() ;
                return idList;
            }
            else
            {
                return null;
            }
        }
    }

    public async Task SyncAllAsync()
    {
        await this.SyncMyInfo();
        await this.SyncEnemyInfoAsync();
        await this.SyncHistoryInfoAsync();
    }

    async Task SyncHistoryInfoAsync()
    {
        histroyList.Clear();
        var infoList = await ApiUtil.FetchByPage(apiDelegate: ArenaApi.RequestHistroyAsync, pageSize: 0, recordCount: 20);
        histroyList.AddRange(infoList);
    }

    async Task SyncEnemyInfoAsync()
    {
        var ignoreUserIdList = this.CurrentEnemyUidList;
        this.enemyInfoList.Clear();
        var list = await ArenaApi.RequestPlayersAsync(ignoreUserIdList);
        if(list != null)
        {
            enemyInfoList.AddRange(list);
        }
    }


    public async Task SyncMyInfo()
    {
        var info = await ArenaApi.RequestInfoAsync();
        this.info = info;
    }


    public List<ArenaEnemyInfo> ranklist = new List<ArenaEnemyInfo>();

    // -1 未上榜
    public int myRanking;
    public async Task SyncRanklist()
    {
        this.ranklist.Clear();
        var infoList = await ApiUtil.FetchByPage(apiDelegate: ArenaApi.RequestRanklistAsync, pageSize: 0, recordCount: 20);
        this.ranklist.AddRange(infoList);

        ranklist.Sort((a, b) =>
        {
            var aSort = a.arena.rank;
            var bSort = b.arena.rank;
            if (aSort < bSort)
            {
                return -1;
            }
            else if (aSort > bSort)
            {
                return 1;
            }
            return -1;
        });

        await this.SyncMyRankingAsync();
    }

    public async Task SyncMyRankingAsync()
    {
        this.myRanking = await ArenaApi.RequestMyRankAsync();
    }

    public int PowerOfMyAttackTeam
    {
        get
        {
            var formationIndex = FormationUtil.GetDefaultFormationIndex();
            return FormationUtil.GetFormationPower(formationIndex);
        }
       
    }



    public DateTime SessionStartDate
    {
        get
        {
            var sessionIndex = this.info.circle;
            var ret = ArenaUtil.GetSessionStartDate(sessionIndex);
            return ret;
        }
    }

    public DateTime SessionEndDate
    {
        get
        {
            var sessionIndex = this.info.circle;
            var ret = ArenaUtil.GetSessionEndDate(sessionIndex);
            return ret;
        }
    }

    public DateTime NextSessionStartDate
    {
        get
        {
            var sessionIndex = this.info.circle;
            var sessionStartDate = DateUtil.DateIndexToDateTime(sessionIndex);
            var nextSessionStartDate = sessionStartDate.AddDays(7);
            return nextSessionStartDate;
        }
    }

    public TimeSpan TimeSpanToNextSessionStart
    {
        get
        {
            var startDate = NextSessionStartDate;
            var now = Clock.Now;
            var timeSpan = startDate - now;
            return timeSpan;
        }
    }

    /// <summary>
    /// 当前赛季是否已休赛
    /// </summary>
    public bool IsSessionOffed
    {
        get
        {
            var endDate = this.SessionEndDate;
            var now = Clock.Now;
            var ret = now >= endDate;
            return ret;
        }
    }

    public async Task RefreshEnemyAsync()
    {
        var nextRefreshRow = ArenaUtil.MyNextRefreshRow;
        if (nextRefreshRow == null)
        {
            throw new Exception("[ArenaUtil] refresh chance useout");
        }
        var id = nextRefreshRow.Id;
        var ignore = this.CurrentEnemyUidList;
        enemyInfoList.Clear();
        var list = await ArenaApi.RefreshAsync(id, ignore);
        if(list != null)
        {
            this.enemyInfoList.AddRange(list);
        }
    }

    public async Task SyncBecouseNewSessionStartAsync()
    {
        var beforeSessionIndex = this.info.circle;
        await this.SyncAllAsync();
        var postSessionIndex = this.info.circle;
        if(beforeSessionIndex == postSessionIndex)
        {
            throw new Exception("[ArenaManager] sync success, but got same session index");
        }
    }

    public string LocalizedRankName
    {
        get
        {
            var score = this.info.score;
            var arenaRow = ArenaUtil.GetArenaRowByScore(score);
            var text = arenaRow.Rank;
            var localizedName = LocalizationManager.Stuff.GetText(text);
            return localizedName;
        }
    }

    public async Task ReceiveScoreRewardAsync(int id)
    {
        var rewardBitBucket = await ArenaApi.RequestReceiveSocreReward(id);
        this.info.reward = rewardBitBucket;
    }

    /// <summary>
    /// 一键领取积分奖励
    /// </summary>
    /// <returns></returns>
    public async Task ReceiveAllScoreRewardAsync()
    {
        var rewardBitBucket = await ArenaApi.RequestReceiveSocreReward(0);
        this.info.reward = rewardBitBucket;
    }

    public bool HasAnyScoreRewardReceivable
    {
        get
        {
            if(this.info == null)
            {
                return false;
            }
            var rowList = StaticData.ArenaRewardTable.ElementList;
            foreach (var row in rowList)
            {
                var id = row.Id;
                var isReceived = this.info.IsRewardReceived(id);
                if (isReceived)
                {
                    continue;
                }
                var needScore = row.Score;
                var myHighScore = this.info.best;
                if (myHighScore >= needScore)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public ArenaRow MyArenaRow
    {
        get
        {
            var myScore = this.info.score;
            var row = ArenaUtil.GetArenaRowByScore(myScore);
            return row;
        }
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomLitJson;

public class ArenaRecordManager : StuffObject<ArenaRecordManager>
{
    public Dictionary<int, ArenaRecordSession> sessionIdToSessionInfoDic = new Dictionary<int, ArenaRecordSession>();

    public ArenaRecordBattle GetTestData()
    {
        var info = new ArenaRecordBattle();
        info.heroInfoList = new List<ArenaRecordHero>();
        for (int i = 0; i < 12; i++)
        {
            var heroInfo = new ArenaRecordHero();
            heroInfo.attack = Random.Range(1, 10000);
            heroInfo.defence = Random.Range(1, 10000);
            heroInfo.heal = Random.Range(1, 10000);
            heroInfo.camp = ArenaBattleHeroCamp.Self;
            if(i >= 6)
            {
                heroInfo.camp = ArenaBattleHeroCamp.Opponent;
            }
            info.heroInfoList.Add(heroInfo);
        }
        return info;
    }

    public List<ArenaRecordBattle> GetRecordBattleOfSession(int sessionIndex)
    {
        var ret = new List<ArenaRecordBattle>();
        this.LoadSessionIfNeed(sessionIndex);
        var sessionInfo = this.sessionIdToSessionInfoDic[sessionIndex];
        var recordBattleList = sessionInfo.battleList;
        ret.AddRange(recordBattleList);
        return ret;
    }

    public ArenaRecordBattle GetBattleInfo(int sessionIndex, string battleId)
    {
        this.LoadSessionIfNeed(sessionIndex);
        var sessionInfo = this.sessionIdToSessionInfoDic[sessionIndex];
        var battleInfoList = sessionInfo.battleList;
        foreach (var battleInfo in battleInfoList)
        {
            if (battleInfo.battleId == battleId)
            {
                return battleInfo;
            }
        }
        return null;
    }

    public void AddBattleInfo(ArenaRecordBattle info)
    {
        var sessionIndex = info.session;
        this.LoadSessionIfNeed(sessionIndex);
        var sessionInfo = this.sessionIdToSessionInfoDic[sessionIndex];

        var id = info.battleId;
        sessionInfo.RemoveInfoBattleBattleId(id);

        sessionInfo.battleList.Add(info);
        if (sessionInfo.battleList.Count > 50)
        {
            sessionInfo.battleList.RemoveAt(0);
        }
        this.Save();
    }

    string GetKeyOfId(int sessionIndex)
    {
        var uid = Database.Stuff.roleDatabase.Me._id;
        return $"{uid}.ArenaSession." + sessionIndex;
    }

    public void Save()
    {
        foreach(var kv in sessionIdToSessionInfoDic)
        {
            var info = kv.Value;
            var sessionId = info.sessionId;

            var json = JsonMapper.Instance.ToJson(info);
            var key = GetKeyOfId(sessionId);
            PlayerPrefs.SetString(key, json);
        }
        PlayerPrefs.Save();
    }

    public void LoadSessionIfNeed(int sessionIndex)
    {
        if (sessionIdToSessionInfoDic.ContainsKey(sessionIndex))
        {
            return;
        }
        else
        {
            LoadSession(sessionIndex);
        }
    }

    public void LoadSession(int sessionIndex)
    {
        var key = GetKeyOfId(sessionIndex);
        var json = PlayerPrefs.GetString(key);
        if (!string.IsNullOrEmpty(json))
        {
            var info = JsonMapper.Instance.ToObject<ArenaRecordSession>(json);
            sessionIdToSessionInfoDic[sessionIndex] = info;
        }
        else
        {
            var info = new ArenaRecordSession();
            info.sessionId = sessionIndex;
            sessionIdToSessionInfoDic[sessionIndex] = info;
        }
    }

}

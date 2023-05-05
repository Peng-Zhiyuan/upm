using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaRecordSession 
{
    public int sessionId;
    public List<ArenaRecordBattle> battleList = new List<ArenaRecordBattle>();
    //public Dictionary<string, ArenaRecordBattle> battleIdToInfoDic = new Dictionary<string, ArenaRecordBattle>();

    public void RemoveInfoBattleBattleId(string id)
    {
        var index = -1;
        for(int i = 0; i < battleList.Count; i++)
        {
            var info = battleList[i];
            if(info.battleId == id)
            {
                index = i;
            }
        }
        if(index > -1)
        {
            battleList.RemoveAt(index);
        }
    }
}

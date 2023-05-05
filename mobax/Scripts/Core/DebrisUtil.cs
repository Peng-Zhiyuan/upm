using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class DebrisUtil
{
    public static int GetHeroId(int debrisRowId)
    {
        var dic = DebrisIdToHeroIdDic;
        var has = dic.ContainsKey(debrisRowId);
        if (!has)
        {
            throw new Exception("[DebrisUtil] no heroId for debris: " + debrisRowId);
        }
        var heroId = dic[debrisRowId];
        return heroId;
    }

    public static int GetHeroRarity(int debrisRowId)
    {
        var heroId = DebrisUtil.GetHeroId(debrisRowId);
        var heroRow = StaticData.HeroTable[heroId];
        var rarity = heroRow.Qlv;
        return rarity;
    }

    public static int GetComposeNeedCount(int debrisRowId)
    {
        var rarity = GetHeroRarity(debrisRowId);
        if (rarity == 1)
        {
            return StaticData.BaseTable["heroSynthesisR"];
        }
        else if (rarity == 2)
        {
            return StaticData.BaseTable["heroSynthesisSr"];
        }
        else if (rarity == 3)
        {
            return StaticData.BaseTable["heroSynthesisSsr"];
        }
        else
        {
            return 99999;
        }
    }

    static Dictionary<int, int> _debrisIdToHeroIdDic;
    public static Dictionary<int, int> DebrisIdToHeroIdDic
    {
        get
        {
            if (_debrisIdToHeroIdDic == null)
            {
                _debrisIdToHeroIdDic = new Dictionary<int, int>();
                var rowList = StaticData.HeroTable.ElementList;
                foreach(var row in rowList)
                {
                    var debrisId = row.Soul;
                    _debrisIdToHeroIdDic[debrisId] = row.Id;
                }
            }
            return _debrisIdToHeroIdDic;
        }


    }
}

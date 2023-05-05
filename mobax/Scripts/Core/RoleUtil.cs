using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoleUtil
{

    public static int GetShowHero(RoleInfo info)
    {
        var show = info.show;
        if (show == 0)
        {
            return (int)HeroId.BaSheGong;
        }

        return show;
    }

    public static int GetKanBan(int roleShow)
    {
        if (roleShow == 0)
        {
            return (int) HeroId.BaSheGong;
        }

        return roleShow;
    }

    public static int GetAssistHeroId(RoleInfo info)
    {
        var heroId = info.assist > 0 ? info.assist : StaticData.BaseTable.TryGet("defaultAssistHeroId");
        return heroId;

    }


    public static int GetAvatar(string icon)
    {
        if (string.IsNullOrEmpty(icon))
        {
            return (int) HeroId.BaSheGong;
        }

        return int.Parse(icon);
    }
    // 看板娘

    public static LevelRow GetLv(int exp)
    {
        var lst = StaticData.LevelTable.ElementList;
        int currentLevel = 1;
        for (int i = 0; i < lst.Count; i++)
        {
            if (exp < lst[i].Exp)
            {
                break;
            }

            currentLevel = lst[i].Id;
        }

        return StaticData.LevelTable[currentLevel];
    }

    public static bool isLevelUp(int level, int currentExp, int addExp)
    {
        return false;
    }
}
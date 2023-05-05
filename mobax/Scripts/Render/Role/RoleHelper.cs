using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleHelper
{
    public static string GetAvatarName(HeroInfo heroInfo)
    {
        if (heroInfo.ItemInfo == null)
        {
            return StaticData.HeroTable[heroInfo.HeroId].Model;
        }
        var avatarId = heroInfo.AvatarInfo.Item1;
        if (avatarId == 0)
        {
            if (!string.IsNullOrEmpty(StaticData.HeroTable[heroInfo.ItemInfo.id].Model))
            {
                return StaticData.HeroTable[heroInfo.ItemInfo.id].Model;
            }
        }
        return StaticData.AvatarTable[avatarId].Modl;
    }

    public static bool ShowWeapon(int heroId, int avatarId = 0)
    {
        if (avatarId == 0)
        {
            var hero = StaticData.HeroTable[heroId];
            return hero.showWeapon > 0;
        }
        return false;
    }

    public static string GetAvatarName(int heroId, int avatarId = 0)
    {
        if (avatarId == 0)
        {
            var hero = StaticData.HeroTable[heroId];
            if (!string.IsNullOrEmpty(hero.Model))
            {
                return hero.Model;
            }
           /* if (HeroDressData.Instance.AvatarDic.ContainsKey(heroId))
            {
                var avatarList = HeroDressData.Instance.AvatarDic[heroId];
                avatarId = avatarList[0].Id;
            }*/
        }
        return StaticData.AvatarTable[avatarId].Modl;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroDressData:Single<HeroDressData> 
{
    public  (int, int, int, long) CurrentAvatarInfo;
    public (int, int, int, long) GetAvatarInfo(HeroInfo hero)
    {
       var avatar =  hero.ItemInfo.attach.TryGet<string>("avatar", null);
        if (!string.IsNullOrEmpty(avatar))
        {
            var info = avatar.Split('_');
            if (info.Length == 4)
            {
                long tick = long.Parse(info[3]);
                if (info[3].Length == 13)
                {
                    tick = tick/100;
                }
                return (int.Parse(info[0]), int.Parse(info[1]), int.Parse(info[2]), tick);
            }
        }
        return (0, 0, 0, 0);
    }

   public string AvatarInfoToString((int, int, int, long) avatarInfo)
    {
        Debug.Log(avatarInfo.ToString());
        return  $"{avatarInfo.Item1}_{avatarInfo.Item2}_{avatarInfo.Item3}_{avatarInfo.Item4}";
      
    }


    public bool HasSkin(int heroId)
    {
        return StaticData.HeroTable[heroId].Qlv > 1;
    }

    public  Dictionary<int, List<AvatarRow>> avatarDic;

    public bool IsHairCanUse(int hairId)
    {
        if(hairId == 0) return true;
        ItemInfo info = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(hairId);
        if (info == null) return false;
        int expire = info.attach.TryGet<int>("expire", 0);
        if (info.val > 0 && expire <= Clock.ToTimestampS(Clock.Now))
        {
            return true;
        }
        if (expire > Clock.ToTimestampS(Clock.Now))
        {
            return true;
        }
        return false;
    }

    public int RemainHairTimeS(int hairId)
    {
        ItemInfo info = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(hairId);
        if (info == null) return 0;
        int expire = info.attach.TryGet<int>("expire", 0);

        if (expire < Clock.ToTimestampS(Clock.Now))
        {
            return 0;
        }
        return expire - (int)Clock.ToTimestampS(Clock.Now);
    }


    

    public Dictionary<int, List<AvatarRow>> AvatarDic
    {
        get 
        {
            if (avatarDic == null)
            {
                avatarDic = new Dictionary<int, List<AvatarRow>>();
                foreach (HeroRow hr in StaticData.HeroTable.ElementList)
                {
                    AvatarRow ar = new AvatarRow();
                    ar.Id = 0;
                    ar.heroId = hr.Id;
                    ar.Modl = hr.Model;
                    ar.Icon = $"HalfCard_{hr.Id}";
                    avatarDic[ar.heroId] = new List<AvatarRow>();
                    avatarDic[ar.heroId].Add(ar);
                }
                foreach (AvatarRow ar in StaticData.AvatarTable.ElementList)
                {
                    if (!avatarDic.ContainsKey(ar.heroId))
                    {
                        avatarDic[ar.heroId] = new List<AvatarRow>();
                    }
                    avatarDic[ar.heroId].Add(ar);
                }
            }
            return avatarDic;

        }
    }

    public string GetClothKey(HeroInfo hero)
    {
        return hero.HeroId + "_" + hero.AvatarInfo.Item1;
    }

    public Dictionary<string, List<ClothColorRow>> clothDic;
    public Dictionary<string, List<ClothColorRow>> ClothDic
    {
        get
        {
            if (clothDic == null)
            {
                clothDic = new Dictionary<string, List<ClothColorRow>>();
                foreach (HeroRow hr in StaticData.HeroTable.ElementList)
                {
                    var avatarList = AvatarDic[hr.Id];
                    foreach (var ar in avatarList)
                    {
                        string key = $"{ar.heroId}_{ar.Id}";
                        ClothColorRow ccr = new ClothColorRow();
                        ccr.Id = 0;
                        ccr.heroId = ar.heroId;
                        ccr.avatarId = 0;
                        ccr.Modl = null;
                        ccr.Icon = $"ClothColor_{ar.heroId}_01";
                        clothDic[key] = new List<ClothColorRow>();
                        clothDic[key].Add(ccr);
                    }
                  
                }

                foreach (ClothColorRow ccr in StaticData.ClothColorTable.ElementList)
                {
                    string key = $"{ccr.heroId}_{ccr.avatarId}";
                  /*  if (!clothDic.ContainsKey(key))
                    {
                        clothDic[key] = new List<ClothColorRow>();
                    }*/
                    clothDic[key].Add(ccr);
                }
            }
            return clothDic;

        }
    }

    public Dictionary<int, List<HairColorRow>> hairDic;
    public Dictionary<int, List<HairColorRow>> HairDic
    {
        get
        {
            if (hairDic == null)
            {
                hairDic = new Dictionary<int, List<HairColorRow>>();
                foreach (HeroRow hr in StaticData.HeroTable.ElementList)
                {
                    HairColorRow hcr  = new HairColorRow();
                    hcr.Id = 0;
                    hcr.heroId = hr.Id;
                    hcr.Modl = null;
                    hcr.Icon = $"Haircolor_{hcr.heroId}_01";
                    hairDic[hcr.heroId] = new List<HairColorRow>();
                    hairDic[hcr.heroId].Add(hcr);
                }
                foreach (HairColorRow hcr in StaticData.HairColorTable.ElementList)
                {
                    int key = hcr.heroId;
                   /* if (!hairDic.ContainsKey(key))
                    {
                        hairDic[key] = new List<HairColorRow>();
                    }*/
                    hairDic[key].Add(hcr);
                }
            }
            return hairDic;

        }

    }
}

using System.Collections.Generic;
using UnityEngine;

public class SpineUtil : Single<SpineUtil>
{
    private Dictionary<int, Dictionary<string, SdAnimRow>> heroAniDic = new Dictionary<int, Dictionary<string, SdAnimRow>>();
    private Dictionary<int, Dictionary<string, SdAnimRow>> modeAniDic = new Dictionary<int, Dictionary<string, SdAnimRow>>();

    public Dictionary<string, SdAnimRow> GetAniDic(int heroId)
    {
        if (!heroAniDic.ContainsKey(heroId))
        {
            var modeId = StaticData.HeroTable[heroId].Id;
            heroAniDic.Add(heroId, GetAniDicByModeId(modeId));
        }
        return heroAniDic[heroId];
    }

    public Dictionary<string, SdAnimRow> GetAniDicByModeId(int modeId)
    {
        if (!modeAniDic.ContainsKey(modeId))
        {
            var modeInfo = StaticData.SdModelTable[modeId];
            var c = StaticData.SdAnimTable.Count;
            Dictionary<string, SdAnimRow> dic = new Dictionary<string, SdAnimRow>();
            for (var i = 0; i < c; ++i)
            {
                var aniData = StaticData.SdAnimTable[i + 1];
                if (aniData.aID == modeInfo.Ani)
                {
                    dic.Add(aniData.Name, aniData);
                }
            }
            modeAniDic.Add(modeId, dic);
        }
        return modeAniDic[modeId];
    }

    public string GetSpinePath(int heroId)
    {
        var modeId = StaticData.HeroTable[heroId].Model;
        return "Assets/AddressableRes/" + StaticData.SdModelTable[int.Parse(modeId)].Spine + ".prefab";
    }

    public float GetSDScale(int heroId)
    {
        var modeId = StaticData.HeroTable[heroId].Model;
        return +StaticData.SdModelTable[int.Parse(modeId)].Scale;
    }

    public float GetSDHeight(int heroId)
    {
        //var modeId = ItemUtil.GetRoleInfo(heroId).Model;
        //var modeId = StaticData.HeroTable[heroId].model;
        return 1.7f;
    }

    public Vector3Int GetSDOffset(int heroId)
    {
        return Vector3Int.one;
    }
}
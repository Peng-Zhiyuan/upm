using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum HeroResType
{
    Icon = 1,
    Card, // 背景透明全身像
    Image, // 带背景立绘
    Skeleton,
    Attr,
    AttrSmall,
    SkillFrame, // 也是跟着元素类型走
    Job, // 职业小图标
    JobBig, //职业大图标
    HalfIcon, // 半身像
}

public static class HeroHelper
{
    private static Dictionary<Rarity, Color> _rarityColorMap;

    static HeroHelper()
    {
        _rarityColorMap = new Dictionary<Rarity, Color>
        {
            [Rarity.N] = new Color(0x58 / 255f, 0x8f / 255f, 0x57 / 255f),
            [Rarity.R] = new Color(0x62 / 255f, 0xc2 / 255f, 0xff / 255f),
            [Rarity.SR] = new Color(0xd2 / 255f, 0x60 / 255f, 0xdc / 255f),
            [Rarity.SSR] = new Color(0xff / 255f, 0xcf / 255f, 0x67 / 255f),
        };
    }
    
    /** 根据instance id获取配表id */
    public static int InstanceIdToRowId(string heroInstanceId)
    {
        var item = Database.Stuff.itemDatabase.GetItemInfoByInstanceId(heroInstanceId);
        return item.id;
    }
    
    /** 稀有度图片 */
    public static string GetRarityAddress(int rarity)
    {
        return GetRarityAddress((Rarity) rarity);
    }
    
    /** 稀有度图片 */
    public static string GetRarityAddress(Rarity rarity)
    {
        return $"Icon_{rarity.ToString().ToLower()}.png";
    }

    /** 获取职业 */
    public static string GetJobAddress(int job, bool big = false)
    {
        if (big)
        {
            return $"Icon_big_occ{job}.png";
        }
        
        return $"Icon_occ{job}.png";
    }

    public static string GetJobAddress(HeroJob job, bool big = false)
    {
        return GetJobAddress((int) job, big);
    }
    
    /** 稀有度背景图片 */
    public static string GetRarityBgAddress(int rarity)
    {
        return GetRarityBgAddress((Rarity) rarity);
    }
    
    /** 稀有度背景图片 */
    public static string GetRarityBgAddress(Rarity rarity)
    {
        return $"Rarity_bg_{rarity.ToString().ToLower()}.png";
    }
    
    public static string GetRarityNormalAddress(Rarity rarity)
    {
        return $"Rarity_{rarity.ToString().ToLower()}.png";
    }
    
    /** 稀有度头像框背景图片 */
    public static string GetRarityFrameAddress(Rarity rarity)
    {
        return $"Rarity_frame_{rarity.ToString().ToLower()}.png";
    }
    
    /** 稀有度头像框大背景图片 */
    public static string GetRarityFrameBigAddress(Rarity rarity)
    {
        return $"Rarity_frame_big_{rarity.ToString().ToLower()}.png";
    }
    
    /** 头像稀有度背景图片 */
    public static string GetIconRarityBgAddress(int rarity)
    {
        return GetIconRarityBgAddress((Rarity) rarity);
    }
    
    /** 头像稀有度背景图片 */
    public static string GetIconRarityBgAddress(Rarity rarity)
    {
        return $"Bg_rolehead_{rarity.ToString().ToLower()}.png";
    }

    /** 稀有度颜色 */
    public static Color GetRarityColor(int rarity)
    {
        return GetRarityColor((Rarity) rarity);
    }

    /** 稀有度颜色 */
    public static Color GetRarityColor(Rarity rarity)
    {
        return _rarityColorMap[rarity];
    }

    public static HeroRow GetRow(int id)
    {
        var row = StaticData.HeroTable[id];
        return row;
    }

    /** 获取英雄的各种类型的贴图 */
    public static string GetResAddress(int rowId, HeroResType textureType)
    {
        var row = StaticData.HeroTable[rowId];
        switch (textureType)
        {
            case HeroResType.Icon:
            {
                var ret = row.Head == "" ? $"Icon_{row.Id}.png" : $"{row.Head}.png";
                return ret;
            }
            case HeroResType.HalfIcon:
            {
                var ret =  $"HalfCard_{row.Model}.png";
                return ret;
            }
            case HeroResType.Card:
            {
                var ret = row.heroCard == "" ? $"Card_{row.Id}.png" : $"{row.heroCard}.png";
                return ret;
            }
            case HeroResType.Image:
            {
                var ret = row.heroImage == "" ? $"Image_{row.Id}.png" : $"{row.heroImage}.png";
                return ret;
            }
            case HeroResType.Attr:
            {
                var ret = $"hero_attr_{row.Element}.png";
                return ret;
            }
            case HeroResType.AttrSmall:
            {
                var ret = $"hero_attr_{row.Element}_s.png";
                return ret;
            }
            case HeroResType.SkillFrame:
            {
                var ret = $"skill_frame_{row.Element}.png";
                return ret;
            }
            case HeroResType.Job:
            {
                return GetJobAddress(row.Job);
            }
            case HeroResType.JobBig:
            {
                return GetJobAddress(row.Job, true);
            }
            case HeroResType.Skeleton:
            {
                return "wotelina_SkeletonData.asset";
            }
            default:
            {
                throw new Exception("[HeroHelper] unsupport hero res type: " + textureType);
            }
        }
    }
}
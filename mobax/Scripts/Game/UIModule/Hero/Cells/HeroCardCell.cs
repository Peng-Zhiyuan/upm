using System;
using UnityEngine;

public partial class HeroCardCell : MonoBehaviour
{
    private HeroInfo _heroInfo;

    public Action<HeroInfo> onClick;

    public HeroInfo HeroInfo => _heroInfo;
    
    public void SetInfo(HeroInfo heroInfo, int level = 0)
    {
        if (level == 0) level = heroInfo.Level;
        // 设置半身像
        UiUtil.SetSpriteInBackground(this.Image_icon,
            () => HeroHelper.GetResAddress(heroInfo.HeroId, HeroResType.HalfIcon),
            isDisplayDefault => { this.Image_icon.gameObject.SetActive(!isDisplayDefault); });
        // 设置品质
        UiUtil.SetSpriteInBackground(Image_frame, () => HeroHelper.GetRarityFrameBigAddress(heroInfo.Rarity));
        UiUtil.SetSpriteInBackground(Image_rarity, () => HeroHelper.GetRarityNormalAddress(heroInfo.Rarity));
        // 设置职业图标
        UiUtil.SetSpriteInBackground(Image_job, () => HeroHelper.GetJobAddress(heroInfo.Job));
        // 设置元素图标
        UiUtil.SetSpriteInBackground(Image_element, () => HeroHelper.GetResAddress(heroInfo.HeroId, HeroResType.AttrSmall));
        // 设置名字
        Text_name.SetLocalizer(heroInfo.Name);
        // 设置等级
        Text_level.text = level.ToString("00");
        // 设置星级
        for (var i = 0; i < HeroConst.StarMax; ++i)
        {
            var star = Node_stars.transform.Find($"star{i}");
            star.gameObject.SetActive(i < heroInfo.Star);
        }
        
        _heroInfo = heroInfo;
    }

    public void OnItemClick()
    {
        onClick?.Invoke(_heroInfo);
    }
}
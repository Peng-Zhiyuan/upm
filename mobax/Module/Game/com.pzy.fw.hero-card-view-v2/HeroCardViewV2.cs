using System;
using UnityEngine;
using Sirenix.OdinInspector;

public partial class HeroCardViewV2 : MonoBehaviour
{
    [ShowInInspector]
    ItemViewData _data;
    public ItemViewData Data
    {
        get
        {
            return _data;
        }
        set
        {
            if(_data == value)
            {
                return;
            }
            _data = value;
            this.Refresh();
        }
    }

    public void Refresh()
    {
        this.RefreshIcon();
        this.RefreshQuality();
        this.RefreshJob();
        this.RefreshElement();
        this.RefreshName();
        this.RefreshLevel();
        this.RefreshStart();
    }

    void RefreshIcon()
    {
        UiUtil.SetSpriteInBackground(this.Image_icon,
            () => HeroHelper.GetResAddress(this.Data.RowId, HeroResType.HalfIcon),
            isDisplayDefault => { this.Image_icon.gameObject.SetActive(!isDisplayDefault); });
    }

    void RefreshQuality()
    {
        UiUtil.SetSpriteInBackground(this.Image_frame, () => HeroHelper.GetRarityFrameBigAddress((Rarity)this.Data.Quality));
        UiUtil.SetSpriteInBackground(this.Image_rarity, () => HeroHelper.GetRarityNormalAddress((Rarity)this.Data.Quality));
    }

    void RefreshJob()
    {
        UiUtil.SetSpriteInBackground(this.Image_job, () => HeroHelper.GetJobAddress(this.Data.Job));
    }

    void RefreshElement()
    {
        UiUtil.SetSpriteInBackground(Image_element, () => HeroHelper.GetResAddress(this.Data.RowId, HeroResType.AttrSmall));
    }

    void RefreshName()
    {
        this.Text_name.text = this.Data.Name.Localize() ;
    }

    void RefreshLevel()
    {
        var level = this.Data.Level ?? 0;
        this.Text_level.text = level.ToString("00");
    }

    void RefreshStart()
    {
        var count = this.Data.StarRow?.Star ?? 0;
        for (var i = 0; i < HeroConst.StarMax; ++i)
        {
            var star = Node_stars.transform.Find($"star{i}");
            star.gameObject.SetActive(i < count);
        }
    }


}
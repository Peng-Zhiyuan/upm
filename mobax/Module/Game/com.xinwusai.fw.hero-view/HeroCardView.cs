using System;
using UnityEngine;

public partial class HeroCardView : MonoBehaviour
{
    private int _heroId;

    public int HeroId
    {
        get => this._heroId;
        set
        {
            this._heroId = value;
            this.Refresh();
        }
    }

    void Refresh()
    {
        if (this._heroId <= 0)
        {
            this.Add.SetActive(true);
            this.Normal.SetActive(false);
            return;
        }
    
        this.Add.SetActive(false);
        this.Normal.SetActive(true);
        this.RefreshIcon();
        this.RefreshQuality();
        this.RefreshJob();
        this.RefreshProp();
        this.RefreshLvAndStar();
    }
    
    void RefreshIcon()
    {
        UiUtil.SetSpriteInBackground(this.Image_icon,
            () => HeroHelper.GetResAddress(this._heroId, HeroResType.HalfIcon),
            isDisplayDefault => { this.Image_icon.gameObject.SetActive(!isDisplayDefault); });
    }
    
    void RefreshQuality()
    {
        var heroRow = HeroHelper.GetRow(this._heroId);
        this.Text_name.text = heroRow.Name.Localize();
    
        var address = HeroHelper.GetRarityFrameBigAddress((Rarity) heroRow.Qlv);
        var address1 = HeroHelper.GetRarityNormalAddress((Rarity) heroRow.Qlv);
    
        UiUtil.SetSpriteInBackground(this.QualityFrame, () => address);
        UiUtil.SetSpriteInBackground(this.Quality, () => address1);
    }
    
    void RefreshJob()
    {
        var row = HeroHelper.GetRow(this._heroId);
        var jobId = row.Job;
        var address = JobUtil.GetIconAddress(jobId);
        UiUtil.SetSpriteInBackground(this.Image_zhiye, () => address);
    }
    
    void RefreshProp()
    {
        var address = HeroHelper.GetResAddress(this._heroId, HeroResType.AttrSmall);
        UiUtil.SetSpriteInBackground(this.Image_shuxing, () => address);
    }
    
    void RefreshLvAndStar()
    {
        var heroInfo = HeroManager.Instance.GetHeroInfo(this._heroId);
        this.Text_lv.text = heroInfo.Level.ToString("00");
    
        // var sizeDelta = this.Star_num.rectTransform.sizeDelta;
        // this.Star_num.rectTransform.sizeDelta = new Vector2(25f * heroInfo.Star, sizeDelta.y);
    
        for (int i = 0; i < StarList.Count; i++)
        {
            StarList[i].SetActive(heroInfo.Star - 1 >= i);
        }
    }

    public Action OnClick;

    public void OnOpenFormationSetClick()
    {
        OnClick?.Invoke();
    }
}
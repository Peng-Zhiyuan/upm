using UnityEngine;

public partial class EnemyIconView : MonoBehaviour
{
    ItemViewData data;

    string IconAddress
    {
        get
        {
            var id = data.RowId;
            return HeroHelper.GetResAddress(id, HeroResType.Icon);
        }
    }

    public void Bind(ItemViewData data, int level = 0)
    {
        this.data = data;
        this.Refresh(level);
    }

    /// <summary>
    /// 脱离了ItemInfo的heroId
    /// </summary>
    /// <param name="heroId"></param>
    /// <param name="level"></param>
    public void Bind(int heroId, int level = 0)
    {
        var itemData = new ItemViewData();
        itemData.SetAsRowId(heroId);
        Bind(itemData, level);
    }

    public void Bind(HeroInfo heroInfo)
    {
        var itemData = new ItemViewData();
        itemData.SetAsItemInfo(heroInfo.ItemInfo);
        Bind(itemData);
    }

    void Refresh(int level)
    {
        this.RefreshIcon();
        this.RefreshQuality();
        this.RefreshJob();
        this.RefreshProp();
        this.RefreshLevel(level);
    }

    void RefreshLevel(int level)
    {
        if (level <= 0)
        {
            var row = StaticData.HeroTable.TryGet(this.data.RowId);
            var type = (EHeroType) row.Special;
            if (type == EHeroType.Monster)
            {
                this.RefreshMonsterLevel();
            }
        }
        else
        {
            this.Text_lv.gameObject.SetActive(true);
            this.Text_lv.text = level.ToString("00");
        }
    }

    // 怪物等级
    void RefreshMonsterLevel()
    {
        var monsterRow = StaticData.MonsterTable.ElementList.Find(val => val.heroID == this.data.RowId);
        if (monsterRow == null)
        {
            this.Text_lv.gameObject.SetActive(false);
            return;
        }

        this.Text_lv.gameObject.SetActive(true);
        this.Text_lv.text = monsterRow.monsterLv.ToString("00");
    }

    void RefreshJob()
    {
        var id = this.data.RowId;
        var row = HeroHelper.GetRow(id);
        var jobId = row.Job;
        var address = JobUtil.GetIconAddress(jobId);
        UiUtil.SetSpriteInBackground(this.Image_zhiye, () => address);
    }

    void RefreshProp()
    {
        // var id = this.data.RowId;
        // var row = HeroHelper.GetRow(id);
        // var address = HeroHelper.GetResAddress(id, HeroResType.AttrSmall);
        // UiUtil.SetSpriteInBackground(this.Image_shuxing, () => address);
    }

    void RefreshQuality()
    {
        // // 笪秋辰: speical字段, 只有0的头像显示稀有度
        // var rowId = this.data.RowId;
        // var heroRow = HeroHelper.GetRow(rowId);
        // var special = heroRow.Special;
        // var isShow = special == 0;
        // var address = HeroHelper.GetRarityFrameAddress((Rarity) heroRow.Qlv);
        // UiUtil.SetSpriteInBackground(this.Quality, () => address);
    }

    void RefreshIcon()
    {
        UiUtil.SetSpriteInBackground(this.Image_icon, () => IconAddress, isDisplayDefault =>
            {
                this.Image_default.gameObject.SetActive(isDisplayDefault);
                this.Image_icon.gameObject.SetActive(!isDisplayDefault);
            }
        );
    }
}
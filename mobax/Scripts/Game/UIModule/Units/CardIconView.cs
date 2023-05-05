using UnityEngine;

/// <summary>
/// xinwusai:此组件仅仅作为抽卡那边显示用 不可做其他用处
/// </summary>
public partial class CardIconView : MonoBehaviour
{
    private Bucket Bucket => BucketManager.Stuff.Main;

    private HeroRow _heroRow;

    public void SetData(int heroId, bool showLevel = false)
    {
        this._heroRow = StaticData.HeroTable.TryGet(heroId);

        this.HeroLevel.gameObject.SetActive(showLevel);

        this.SetBg();
        this.SetJob();
        this.SetRarity();
        this.SetHeroIcon();
    }

    private async void SetBg()
    {
        if (!this.BgQuality) return;

        this.BgQuality.enabled = false;
        var address = HeroHelper.GetIconRarityBgAddress(this._heroRow.Qlv);
        var sprite = await this.Bucket.GetOrAquireSpriteAsync(address);
        this.BgQuality.sprite = sprite;
        this.BgQuality.enabled = true;
    }

    private async void SetJob()
    {
        if (!this.Job) return;

        if (this._heroRow.Job == 0)
        {
            this.Job.enabled = false;
            return;
        }

        this.Job.enabled = false;
        var address = HeroHelper.GetResAddress(this._heroRow.Id, HeroResType.Job);
        var sprite = await this.Bucket.GetOrAquireSpriteAsync(address);
        this.Job.sprite = sprite;
        this.Job.enabled = true;
    }

    private async void SetRarity()
    {
        if (!this.Rarity) return;

        this.Rarity.enabled = false;
        var address = HeroHelper.GetRarityAddress(this._heroRow.Qlv);
        var sprite = await this.Bucket.GetOrAquireSpriteAsync(address);
        this.Rarity.sprite = sprite;
        this.Rarity.SetNativeSize();
        this.Rarity.enabled = true;
    }

    private async void SetHeroIcon()
    {
        if (!this.CardIcon) return;

        this.CardIcon.enabled = false;
        var address = HeroHelper.GetResAddress(this._heroRow.Id, HeroResType.Icon);
        var sprite = await this.Bucket.GetOrAquireSpriteAsync($"{address}.png");
        this.CardIcon.sprite = sprite;
        this.CardIcon.enabled = true;
    }
}
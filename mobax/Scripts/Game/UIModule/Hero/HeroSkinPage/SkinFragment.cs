using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

public partial class SkinFragment : LagacyFragment
{
    //---------------------------------------------------avatar-------------------------------------------------//
    private int _selIndex = 0;
    /// <summary>
    /// �������Ƭ�л���
    /// </summary>
    public override void OnSwitchedFrom()
    {

    }

    public override void OnPageNavigatedTo(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Back)
        {
            RefreshAsync();
        }
    }
    public async Task RefreshAsync()
    {
        if (!HeroDressData.Instance.AvatarDic.ContainsKey(DisplayHero.HeroId))
        {
            Debug.LogError("Not find dress data:" + DisplayHero.HeroId);
            return;
        }
        var data = HeroDressData.Instance.AvatarDic[DisplayHero.HeroId];
        var items = Enumerable.Range(0, data.Count).Select(i => new SkinListCellViewData(data[i], () => { this.UpdateSkinInfo(data[i]); })).ToArray();
        this.SkinScrollView.UpdateData(items);

        this.SkinScrollView.OnSelectionChanged(OnSelectionSkinChanged);


        if (data.Count <= 0)
        {
            Debug.LogError("Dress data is empty:" + DisplayHero.HeroId);
            return;
        }
        var avatarId = HeroDressData.Instance.CurrentAvatarInfo.Item1;
        int avatarIndex = data.FindIndex((row) => { if (row.Id == avatarId) return true; else return false; });

        this._selIndex = Mathf.Max(0, avatarIndex);
        this.SkinScrollView.SelectCell(this._selIndex);
        await this.UpdateSkinInfo(data[this._selIndex]);
    }

    public override void OnSwitchedTo()
    {
        RefreshAsync();
    }
    public HeroInfo DisplayHero
    {
        get
        {
            return HeroDisplayer.Hero;
        }
    }

    private async void OnSelectionSkinChanged(int index)
    {
        Debug.LogError("OnSelectionChanged:" + index);
        this._selIndex = index;
        var data = HeroDressData.Instance.AvatarDic[DisplayHero.HeroId];
        await this.UpdateSkinInfo(data[index]);
        //this.playingAnim = false;
    }

    public bool HasItem
    {
        get
        {
            return HeroDressData.Instance.CurrentAvatarInfo.Item1 == 0 || Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(HeroDressData.Instance.CurrentAvatarInfo.Item1) != null;
        }
    }

    public bool UsedItem
    {
        get
        {
            return this.DisplayHero.AvatarInfo.Item1 == HeroDressData.Instance.CurrentAvatarInfo.Item1;
        }
    }
    private ShopGoodsRow GetGoodsRow(int shelfId)
    {
        ShopShelfRow shopShelfRow = StaticData.ShopShelfTable[shelfId];
        ShopGoodsRow shopGoodsRow = StaticData.ShopGoodsTable.TryGet(shopShelfRow.Goods[0]);
        return shopGoodsRow;
    }

    private async Task UpdateSkinInfo(AvatarRow avatarRow)
    {
        HeroDressData.Instance.CurrentAvatarInfo = DisplayHero.AvatarInfo;
        HeroDressData.Instance.CurrentAvatarInfo.Item1 = avatarRow.Id;
        this.HeroName.text = LocalizationManager.Stuff.GetText(this.DisplayHero.Name);
      
        LinesWordBg.SetActive(!string.IsNullOrEmpty(avatarRow.Desc));
        if (!string.IsNullOrEmpty(avatarRow.Desc)) this.LinesWord.PlayAni(LocalizationManager.Stuff.GetText(avatarRow.Desc));
        bool canBuy = avatarRow.productId > 0;
        this.BuyButton.SetActive(!HasItem && canBuy);
        this.UseButton.SetActive(HasItem && !UsedItem);
        this.State.SetActive(HasItem && UsedItem);
        if (!HasItem && canBuy)
        {
          
            CostLabel.text = GetGoodsRow(avatarRow.productId).Subs[0].Num.ToString();
        }
        if (HeroDressData.Instance.CurrentAvatarInfo.Item1 > 0)
        {
            SkinCostPanel.gameObject.SetActive(true);
            this.SkinName.text = LocalizationManager.Stuff.GetText(avatarRow.Name);
        }
        else
        {
           // this.SkinGetButton.SetActive(false);
            SkinCostPanel.gameObject.SetActive(false);
            this.SkinName.text = LocalizationManager.Stuff.GetText("heroskin_default_skin");
        }
       
        HeroSkinPage skinPage = this.fragmentManager.view as HeroSkinPage;
        await skinPage.CurrentModelViewUnit.rolePreviewUnit.SetData(this.DisplayHero.HeroId, HeroDressData.Instance.CurrentAvatarInfo);
    }

    public async void OnGet()
    {
        var id = HeroDressData.Instance.CurrentAvatarInfo.Item1;
        if (id == 0) return;
        var f = await UIEngine.Stuff.ShowFloatingAsync<ItemInfoFloating>();
        f.Set(id);
    }
    public async void OnUse()
    {
        if (HasItem)
        {
            string info = HeroDressData.Instance.AvatarInfoToString(HeroDressData.Instance.CurrentAvatarInfo);
            TrackManager.CustomReport("Skin", info);
            await DressApi.RequestSwitchAvatarAsync(DisplayHero.HeroId, info);
            var data = HeroDressData.Instance.AvatarDic[DisplayHero.HeroId];
            await this.UpdateSkinInfo(data[this._selIndex]);
            ToastManager.ShowLocalize("heroskin_dress_suc");
        }
    }
    public async void OnBuy()
    {
        var id = HeroDressData.Instance.CurrentAvatarInfo.Item1;
        if (id == 0) return;
        await UIEngine.Stuff.ForwardOrBackToAsync<SkinBuyPage>(id);
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Linq;
public partial class ClothFragment : LagacyFragment
{
    private int _selIndex;
    public HeroInfo DisplayHero
    {
        get
        {
            return HeroDisplayer.Hero;
        }
    }
    public override void OnSwitchedFrom()
    {

    }

    private async void Refresh()
    {
        string clothKey = HeroDressData.Instance.GetClothKey(DisplayHero);
        if (!HeroDressData.Instance.ClothDic.ContainsKey(clothKey))
        {
            Debug.LogError("Not find cloth data:" + clothKey);
            return;
        }
        var data = HeroDressData.Instance.ClothDic[clothKey];
        var items = Enumerable.Range(0, data.Count).Select(i => new ClothListCellViewData(data[i], () => { this.UpdateClothInfo(data[i]); })).ToArray();
        this.ClothScrollView.UpdateData(items);

        this.ClothScrollView.OnSelectionChanged(OnSelectionClothChanged);


        //if (data.Count <= 1)
        //{
        //    this.UpdateDressInfo(data.First());
        //    return;
        // }
        if (data.Count <= 0)
        {
            Debug.LogError("Dress data is empty:" + DisplayHero.HeroId);
            return;
        }

        var clothId = HeroDressData.Instance.CurrentAvatarInfo.Item2;
        int clothIndex = data.FindIndex((row) => { if (row.Id == clothId) return true; else return false; });
        this._selIndex = Mathf.Max(0, clothIndex);
        this.ClothScrollView.SelectCell(this._selIndex);
        await this.UpdateClothInfo(data[this._selIndex]);
    }

    public override void OnPageNavigatedTo(PageNavigateInfo info)
    {
        if (info.terminalOperation == NavigateOperation.Back)
        {
            Refresh();
        }
    }
    public override void OnSwitchedTo()
    {
        Refresh();
    }

    private async void OnSelectionClothChanged(int index)
    {
        //Debug.LogError("OnSelectionChanged:" + index);
        this._selIndex = index;
        string key = HeroDressData.Instance.GetClothKey(DisplayHero);
        var data = HeroDressData.Instance.ClothDic[key];
        await this.UpdateClothInfo(data[index]);
        //this.playingAnim = false;
    }
    public bool HasItem
    {
        get
        {
            return HeroDressData.Instance.CurrentAvatarInfo.Item2 == 0 || Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(HeroDressData.Instance.CurrentAvatarInfo.Item2) != null;
        }
    }

    public bool UsedItem
    {
        get
        {
            return this.DisplayHero.AvatarInfo.Item2 == HeroDressData.Instance.CurrentAvatarInfo.Item2;
        }
    }

    private async Task UpdateClothInfo(ClothColorRow clothRow)
    {
        HeroDressData.Instance.CurrentAvatarInfo = DisplayHero.AvatarInfo;
        HeroDressData.Instance.CurrentAvatarInfo.Item2 = clothRow.Id;
        this.HeroName.text = LocalizationManager.Stuff.GetText(this.DisplayHero.Name);
        LinesWordBg.SetActive(!string.IsNullOrEmpty(clothRow.Desc));
        if (!string.IsNullOrEmpty(clothRow.Desc))
        {
            this.ClothLinesWord.StopAni();
            this.ClothLinesWord.PlayAni(LocalizationManager.Stuff.GetText(clothRow.Desc));
        }
        bool canBuy = clothRow.productId > 0;
        this.BuyButton.SetActive(!HasItem && canBuy);
        this.UseButton.SetActive(HasItem && !UsedItem);
        this.State.SetActive(HasItem && UsedItem);
        //CostLabel.text = PurchaseUtil.GetPriceLabel(clothRow.productId);
        if (HeroDressData.Instance.CurrentAvatarInfo.Item2 > 0)
        {
            SkinCostPanel.gameObject.SetActive(true);
            this.SkinName.text = LocalizationManager.Stuff.GetText(clothRow.Name);
        }
        else
        {
            //this.SkinGetButton.SetActive(false);
            SkinCostPanel.gameObject.SetActive(false);
            this.SkinName.text = LocalizationManager.Stuff.GetText("heroskin_default_cloth");
        }

        HeroSkinPage skinPage = this.fragmentManager.view as HeroSkinPage;
        await skinPage.CurrentModelViewUnit.rolePreviewUnit.SetData(this.DisplayHero.HeroId, HeroDressData.Instance.CurrentAvatarInfo);
    }


    /*
    private async Task UpdateClothInfo(ClothColorRow clothRow)
    {
        //Debug.LogError("currentAvatarInfo:" + HeroDressData.Instance.CurrentAvatarInfo.ToString());
        HeroDressData.Instance.CurrentAvatarInfo.Item1 = this.DisplayHero.AvatarInfo.Item1;
        HeroDressData.Instance.CurrentAvatarInfo.Item2 = clothRow.Id;

        var currentAvatarInfo = HeroDressData.Instance.CurrentAvatarInfo;
        //Debug.LogError("currentAvatarInfo:"+ currentAvatarInfo.ToString());
        this.HeroName.text = LocalizationManager.Stuff.GetText(this.DisplayHero.Name);
        bool hasItem = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(currentAvatarInfo.Item2) != null;
        if (hasItem)
        {
            bool usedItem = this.DisplayHero.AvatarInfo.Item2 == currentAvatarInfo.Item2;

            if (usedItem)
            {
                this.BuyButton.SetActive(false);
            }
            else
            {
                BtnLabel.text = LocalizationManager.Stuff.GetText("M4_chara_use");
                this.BuyButton.SetActive(true);
            }
        }
        else
        {
            BtnLabel.text = LocalizationManager.Stuff.GetText("M4_chara_buy");
            this.BuyButton.SetActive(true);
        }
        HeroSkinPage skinPage = this.fragmentManager.view as HeroSkinPage;
        await skinPage.CurrentModelViewUnit.rolePreviewUnit.SetData(this.DisplayHero.HeroId, HeroDressData.Instance.CurrentAvatarInfo);

    }
    */
    public async void OnGet()
    {
        var id = HeroDressData.Instance.CurrentAvatarInfo.Item2;
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
            string key = HeroDressData.Instance.GetClothKey(DisplayHero);
            var data = HeroDressData.Instance.ClothDic[key];
            await this.UpdateClothInfo(data[this._selIndex]);
            ToastManager.ShowLocalize("heroskin_dress_suc");
        }
    }
    public async void OnBuy()
    {
       var id =  HeroDressData.Instance.CurrentAvatarInfo.Item2;
        if (id == 0) return;
        await UIEngine.Stuff.ForwardOrBackToAsync<SkinBuyPage>(id);
    }

}

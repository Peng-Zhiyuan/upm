using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public partial class SkinBuyPage : Page
{
    int productId = 0;
    public override async Task OnNavigatedToPreperAsync(PageNavigateInfo navigateInfo) {
        int id = (int)navigateInfo.param;
        Debug.LogError("id:" + id);
        if (StaticData.AvatarTable.ContainsKey(id))
        {
            AvatarRow ar = StaticData.AvatarTable[id] as AvatarRow;
            SkinName.text = LocalizationManager.Stuff.GetText(ar.Name);
            SkinTip.text = LocalizationManager.Stuff.GetText(ar.Desc);
            ShopGoodsRow shopGoodsRow = this.GetGoodsRow(ar.productId);
            Cost.text = shopGoodsRow.Subs[0].Num.ToString();
            // Cost.text = PurchaseUtil.GetPriceLabel(ar.productId);

            productId = ar.productId;
            this.SkinIcon.sprite = await this.PageBucket.GetOrAquireSpriteAsync($"{ar.Icon}.png");
        }
        else if (StaticData.ClothColorTable.ContainsKey(id))
        {
            ClothColorRow ccr = StaticData.ClothColorTable[id] as ClothColorRow ;
            SkinName.text = LocalizationManager.Stuff.GetText(ccr.Name);
            SkinTip.text = LocalizationManager.Stuff.GetText(ccr.Desc);
            ShopGoodsRow shopGoodsRow = this.GetGoodsRow(ccr.productId);
            Cost.text = shopGoodsRow.Subs[0].Num.ToString();
            productId = ccr.productId;
            this.SkinIcon.sprite = await this.PageBucket.GetOrAquireSpriteAsync($"{ccr.Icon}.png");

        }
    }

    private ShopGoodsRow GetGoodsRow(int shelfId)
    {
        ShopShelfRow shopShelfRow = StaticData.ShopShelfTable[shelfId];
        ShopGoodsRow shopGoodsRow = StaticData.ShopGoodsTable.TryGet(shopShelfRow.Goods[0]);
        return shopGoodsRow;
    }

    async Task BuySkin(int shelfId)
    {
        ShopShelfRow shopShelfRow = StaticData.ShopShelfTable[shelfId];
        ShopGoodsRow shopGoodsRow = StaticData.ShopGoodsTable.TryGet(shopShelfRow.Goods[0]);
        ItemInfo costItemInfo = null;
        int costNum = 0;
        for (int i = 0; i < shopGoodsRow.Subs.Count; i++)
        {
            if (shopGoodsRow.Subs[i].Id == 0)
            {
                continue;
            }
            costItemInfo = Database.Stuff.itemDatabase.GetFirstItemInfoOfRowId(shopGoodsRow.Subs[i].Id);
            if (costItemInfo == null
                || costItemInfo.val < shopGoodsRow.Subs[i].Num)
            {
                ToastManager.Show(LocalizationManager.Stuff.GetText("M4_not_enough"));
                return;
            }
            costNum = shopGoodsRow.Subs[i].Num;
        }
        ItemRow itemRow = StaticData.ItemTable.TryGet(costItemInfo.id);
        if (itemRow == null)
        {
            return;
        }
        AvatarRow avatarRow = StaticData.AvatarTable.TryGet(shopGoodsRow.Items[0].Id);
        TrackManager.SendVirtualCurrency(LocalizationManager.Stuff.GetText(itemRow.Name), costItemInfo.id.ToString(), "", costNum, LocalizationManager.Stuff.GetText(avatarRow.Name));
        RewardInfo costItem = shopGoodsRow.Subs[0];
        ItemRow row = StaticData.ItemTable.TryGet(costItem.Id);
        var confirmMsg = LocalizationManager.Stuff.GetText("common_costTips", costItem.Num, LocalizationManager.Stuff.GetText(row.Name));
        var result = await Dialog.AskAsync("", confirmMsg);
        if (!result) return;
     
        await ShopApi.SubmitAsync(shopShelfRow.Id, shopGoodsRow.Id, 1);
    }

    public async void OnBuy()
    {
        if (productId == 0) { throw new System.Exception("not valid product id:" + productId); }

        await BuySkin(productId);//Operation.PurchaseAsync(productId);
        UIEngine.Stuff.RemoveFromStack<SkinBuyPage>();
    }

    public void OnBack()
    {
        UIEngine.Stuff.BackAsync();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PurchaseUtil 
{
    public static string GetPriceText(int paymentRowId)
    {
        var availiable = IggSdkManager.Stuff.IsSdkItemPriceInfoAvaliable(paymentRowId.ToString(), true);
        Debug.Log("[PurchaseUtil] id: " + paymentRowId + ", price info: " + availiable);
        var row = StaticData.PaymentTable[paymentRowId];
        if (!availiable)
        {
            return "[" + "price_get_fail".Localize() + "]";
        }
        var sdkItem = IggSdkManager.Stuff.GetSdkItem(paymentRowId.ToString());
        var a = sdkItem.GetPricing();
        var b = a.GetPriceTag();
        var priceText = b.GetText();
        return priceText;
    }

    /// <summary>
    /// 获取一个充值档位的文字标签，这个标签是显示在按钮上的
    /// </summary>
    /// <param name="paymentRowId"></param>
    /// <returns></returns>
    public static string GetPriceLabel(int paymentRowId, bool displayOnButton = false, bool showAction = true)
    {
        var availiable = IggSdkManager.Stuff.IsSdkItemPriceInfoAvaliable(paymentRowId.ToString(), false);
        Debug.Log("[PurchaseUtil] id: " + paymentRowId + ", price info: " + availiable);
        //var row = StaticData.PaymentTable[paymentRowId];
        var row = StaticData.PaymentTable.TryGet(paymentRowId);
        if(row == null)
        {
            throw new System.Exception("paymentRowId: " + paymentRowId + " not found");
        }

        string priceText = "";

        try
        {
            var sdkItem = IggSdkManager.Stuff.GetSdkItem(paymentRowId.ToString());
            var a = sdkItem?.GetPricing();
            var b = a?.GetInStorePriceTag();
            priceText = b?.GetText();
        }
        catch (System.Exception e)
        {


        }

         if (string.IsNullOrEmpty(priceText))
        {
            availiable = false;
        }

        if (!availiable)
        {
            if(showAction && displayOnButton)
            {
                var isSub = row.isSubscription == 1;
                if(!isSub)
                {
                    return "buy".Localize();
                }
                else
                {
                    return "subscrip".Localize();
                }
            }
            else
            {
                return "price_get_fail".Localize();
            }
        }


        var ret = "";
        if (row.isSubscription == 1)
        {
            if (showAction)
            {
                ret = "subscrip".Localize() + " " + priceText;
            }
            else
            {
                ret = priceText;
            }
        }
        else
        {
            ret = priceText;
        }
        


        Debug.Log("[PurchaseUtil] price get text: " + ret);
        return ret;
    }

    public static string GetPromitionPriceLabel(string productId)
    {
        var has = IggSdkManager.Stuff.IsSdkItemHasPromitionPrice(productId);
        if(!has)
        {
            return "";
        }

        var sdkItem = IggSdkManager.Stuff.GetSdkItem(productId);
        var b = sdkItem?.promotionPricing?.GetInStorePromotionPriceTag();
        var t = b.GetText();
        if(t == "")
        {
            var row = StaticData.PaymentTable.TryGet(int.Parse(productId));
            var isSub = row.isSubscription == 1;
            if (!isSub)
            {
                return "buy".Localize();
            }
            else
            {
                return "subscrip".Localize();
            }
        }
        return t;
    }

    public static bool IsFirstToBuy(int productId)
    {
        var count = Database.Stuff.orderDatabase.Count(productId);
        if(count == 0)
        {
            return true;
        }
        return false;
    }
}

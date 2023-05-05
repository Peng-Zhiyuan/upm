using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public partial class ShopShelfRow 
{
    public int ExpireRule
    {
        get
        {
            var rules = this.Rules;
            return rules[0];
        }
    }

    /// <summary>
    /// 当前上架的商品Id
    /// 通常情况下，如果需要从账号内数据获取，服务器总是会返回货架的信息
    /// 但是，在某些特殊情况下，比如说更新了配置表，服务器没重启等，可能找不到货架信息
    /// 因此，在这种情况下商品 id 会返回 null
    /// </summary>
    public int? GoodsId
    {
        get
        {
            var rule = this.ExpireRule;
            if (rule == 0)
            {
                // 服务器不记录购买信息, 不进行随机
                // 商品总是配置中的第一个
                var ret = this.Goods[0];
                return ret;
            }
            else
            {
                var dynamicInfo = DynamicShelfInfo;
                if(dynamicInfo == null)
                {
                    return null;
                }
                var ret = dynamicInfo.goods;
                return ret;
            }
        }
    }

    /// <summary>
    /// 对应的动态数据，规则为 0 的数据没有动态数据
    /// 这里包含了静态数据之外的概念，考虑是否移到别处
    /// </summary>
    public ShelfInfo DynamicShelfInfo
    {
        get
        {
            var id = this.Id;
            var info = ShopManager.Stuff.GetShelf(id);
            return info;
        }
    }

    /// <summary>
    /// 某些特殊的情况下，比如更新了数据表，但是服务器没重启
    /// 服务器没有为新货架创建数据，因此可能会没有商品数据
    /// </summary>
    public bool IsValid
    {
        get
        {
            if (this.GoodsId == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// 当前上架的商品的配置
    /// </summary>
    public ShopGoodsRow GoodsRow
    {
        get
        {
            var goodsId = this.GoodsId;
            if(goodsId == null)
            {
                return null;
            }
            var goodsRow = StaticData.ShopGoodsTable[goodsId.Value];
            return goodsRow;
        }
    }

    /// <summary>
    /// 当前上架的商品的获取物品的第一个
    /// 因为 UI 只支持显示一个物品，因此只配第一个
    /// </summary>
    public RewardInfo GainItemInfo
    {
        get
        {
            var goodsRow = this.GoodsRow;
            var itemList = goodsRow.Items;
            var firstItem = itemList[0];
            return firstItem;
        }
    }

    /// <summary>
    /// 当前上架商品的消耗物品的第一个
    /// 因为 UI 只支持显示一个物品，因此只配第一个
    /// </summary>
    public RewardInfo CostItemInfo
    {
        get
        {
            var goodsRow = this.GoodsRow;
            var itemList = goodsRow.Subs;
            var firstItem = itemList[0];
            return firstItem;
        }
    }

    /// <summary>
    /// 当前上架商品的购买次数上限
    /// </summary>
    public int MaxLimit
    {
        get
        {
            var goods = this.GoodsRow;
            var ret = goods.Max;
            return ret;
        }
    }

    /// <summary>
    /// 是否有动态数据，规则为 0 的货架没有动态数据
    /// </summary>
    public bool HasDynamicInfo
    {
        get
        {
            var rule = this.ExpireRule;
            if (rule == 0)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 已购买次数，规则为 0 的货架服务器不记录购买次数
    /// </summary>
    public int? BoughtCount
    {
        get
        {
            var has = HasDynamicInfo;
            if (has)
            {
                return this.DynamicShelfInfo.val;
            }
            return null;
        }
    }

    /// <summary>
    /// 是否符合购买限制
    /// </summary>
    public bool InBoughtLimit
    {
        get
        {
            var hasLimit = HasBoughtLimit;
            if (!hasLimit)
            {
                return true;
            }
            var boughtCount = this.BoughtCount;
            var limitCount = this.MaxLimit;
            if (boughtCount < limitCount)
            {
                return true;
            }
            return false;
        }
    }


    /// <summary>
    /// 是否有购买次数限制
    /// </summary>
    public bool HasBoughtLimit
    {
        get
        {
            var rule = this.ExpireRule;
            if (rule == 0)
            {
                return false;
            }
            var count = MaxLimit;
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 距离超时的时间间隔，某些货架没有超时时间
    /// </summary>
    public TimeSpan? TimeSpanToExpire
    {
        get
        {
            var rule = this.ExpireRule;
            if (rule == 0
                || rule == 5)
            {
                return null;
            }
            var dynamicInfo = this.DynamicShelfInfo;
            var expire = dynamicInfo.expire;
            if (expire == 0 || expire == -1)
            {
                return null;
            }
            var expireDate = Clock.ToDateTime(expire);
            var now = Clock.Now;
            var expireTimeSpan = expireDate - now;
            return expireTimeSpan;
        }
    }

    /// <summary>
    /// 是否已到达购买限制
    /// </summary>
    public bool IsSoldOut
    {
        get
        {
            var hasLimit = this.HasBoughtLimit;
            if (!hasLimit)
            {
                return false;
            }
            var limit = this.MaxLimit;
            var boughtCount = this.BoughtCount;
            if (boughtCount >= limit)
            {
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// 原价，仅显示用
    /// </summary>
    public int OriginPrice
    {
        get
        {
            var row = this.GoodsRow;
            var price = row.originPrice;
            return price;
        }
    }

    /// <summary>
    /// 在限购次数下，还可以购买的次数。如果没有限购，则返回 null
    /// </summary>
    public int? PurchaseLimitRemain
    {
        get
        {
            var hasLimit = this.HasBoughtLimit;
            if(hasLimit)
            {
                var limit = this.MaxLimit;
                var boughtCount = this.BoughtCount;
                var remain = limit - boughtCount;
                return remain;
            }
            else
            {
                return null;
            }
        }
    }

}

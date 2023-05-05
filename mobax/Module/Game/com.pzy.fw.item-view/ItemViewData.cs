using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class ItemViewData
{
    [ShowInInspector, ReadOnly]
    ItemViewDataType dataType;

    [ShowInInspector, ReadOnly]
    VirtualItem virtualItem;

    [ShowInInspector, ReadOnly]
    ItemInfo itemInfo;

    // pzy:
    // 需要移除
    [ShowInInspector, ReadOnly]
    CommonItem commonItemInfo;

    [ShowInInspector, ReadOnly]
    int rowId;

    public void SetAsCommonItem(CommonItem commonItem)
    {
        this.commonItemInfo = commonItem;
        this.dataType = ItemViewDataType.CommonItem;
    }


    public void SetAsItemInfo(ItemInfo itemInfo)
    {
        this.itemInfo = itemInfo;
        this.dataType = ItemViewDataType.ItemInfo;
    }

    public void SetAsVirtualItem(VirtualItem virtualItemInfo)
    {
        this.virtualItem = virtualItemInfo;
        this.dataType = ItemViewDataType.VirtualItemInfo;
    }

    public void SetAsRowId(int rowId)
    {
        this.rowId = rowId;
        this.dataType = ItemViewDataType.RowId;
    }


    public int? Count
    {
        get
        {
            var type = this.dataType;
            if(type == ItemViewDataType.ItemInfo)
            {
                return this.itemInfo.val;
            }
            else if(type == ItemViewDataType.VirtualItemInfo)
            {
                return this.virtualItem.val;
            }
            else if(type == ItemViewDataType.RowId)
            {
                return null;
            }
            else if (type == ItemViewDataType.CommonItem)
            {
                return this.commonItemInfo.Num;
            }
            else
            {
                throw new Exception("[ItemViewData] unsupported data type: " + type);
            }
        }
    }

    public string InstanceId
    {
        get
        {
            if (dataType == ItemViewDataType.ItemInfo)
            {
                return this.itemInfo._id;
            }
            else if (dataType == ItemViewDataType.ItemInfo)
            {
                
            }

            return null;
        }
    }

    public int RowId
    {
        get
        {
            var type = this.dataType;
            if(type == ItemViewDataType.ItemInfo)
            {
                return this.itemInfo.id;
            }
            else if(type == ItemViewDataType.VirtualItemInfo)
            {
                return this.virtualItem.id;
            }    
            else if(type == ItemViewDataType.RowId)
            {
                return this.rowId;
            }
            else if (type == ItemViewDataType.CommonItem)
            {
                return this.commonItemInfo.Id;
            }
            else
            {
                throw new Exception("[ItemViewData] unsupported data type: " + type);
            }
        }
    }

    public int Quality
    {
        get
        {
            if (this.dataType == ItemViewDataType.CommonItem)
            {
                return this.commonItemInfo.Qlv;
            }
            var id = this.RowId;
            var row = StaticDataRuntime.GetRow(id);
            var (qlv, has) = ReflectionUtil.TryGetPropertyValue<int>(row, "Qlv");
            if(!has)
            {
                return (int) Rarity.SSR;
            }
            return qlv;
        }
    }

    public int Job
    {
        get
        {
            var id = this.RowId;
            var row = StaticDataRuntime.GetRow(id);
            var (value, has) = ReflectionUtil.TryGetPropertyValue<int>(row, "Job");
            if (!has)
            {
                return -1;
            }
            return value;
        }
    }

    public string Name
    {
        get
        {
            var id = this.RowId;
            var row = StaticDataRuntime.GetRow(id, "Name");
            var (value, has) = ReflectionUtil.TryGetPropertyValue<string>(row, "Name");
            if (!has)
            {
                return "";
            }
            else
            {
                return value;
            }
        }
    }

    static string GetIconNameOfUniversalItem(int id)
    {
        var row = StaticDataRuntime.GetRow(id);
        var (icon, has) = ReflectionUtil.TryGetPropertyValue<string>(row, "Icon");
        if(!has)
        {
            return "";
        }
        else
        {
            return icon;
        }
    }

    public bool IsItemGroup
    {
        get
        {
            var id = this.RowId;
            var b = StaticData.ItemGroupTable.ContainsKey(id);
            return b;
        }
    }

    public bool IsItemPack
    {
        get
        {
            var id = this.RowId;
            var b = StaticData.ItemPacksTextTable.ContainsKey(id);
            return b;
        }
    }

    public bool IsHero
    {
        get
        {
            var id = this.RowId;
            var b = StaticData.HeroTable.ContainsKey(id);
            return b;
        }
    }

    public string IconName
    {
        get
        {
            var rowId = this.RowId;
            var isItemGroup = this.IsItemGroup;
            if (IsHero)
            {
                return $"Icon_{rowId}";
            }
            /*   else if (IsItemPack)
                 {
                     var row = StaticData.ItemPacksTextTable[rowId];
                     return row.Icon;
                 }
                 else*/
            else if (isItemGroup)
            {
                var row = StaticData.ItemGroupTextTable[rowId];
                return row.Icon;
            }
            else if (this.dataType == ItemViewDataType.CommonItem)
            {
                return this.commonItemInfo.Icon;
            }
            else
            {
                var ret = GetIconNameOfUniversalItem(rowId);
                return ret;
            }
        }
    }

    public int IType
    {
        get
        {
            var rowId = this.RowId;
            var itype = StaticDataUtil.GetIType(rowId);
            return itype;
        }
    }

    public HeroStarRow StarRow
    {
        get
        {
            var starId = this.StarId;
            if(starId == null)
            {
                return null;
            }
            var row = StaticData.HeroStarTable.TryGet(starId.Value);
            return row;
        }
    }

    public int? StarId
    {
        get
        {
            if (this.dataType == ItemViewDataType.ItemInfo)
            {
                var itemInfo = this.itemInfo;
                var value = itemInfo.TryGetAttachField<int>("star", 0);
                return value;
            }
            else if (this.dataType == ItemViewDataType.VirtualItemInfo)
            {
                var virtualItem = this.virtualItem;
                var value = virtualItem.star;
                return value;
            }
            else
            {
                return null;
            }
        }
    }

    public int? Level
    {
        get
        {
            if (this.dataType == ItemViewDataType.ItemInfo)
            {
                var itemInfo = this.itemInfo;
                var level = itemInfo.HeroLv;
                return level;
            }
            else if (this.dataType == ItemViewDataType.VirtualItemInfo)
            {
                var virtualItem = this.virtualItem;
                var level = virtualItem.lv;
                return level;
            }
            else
            {
                return null;
            }
        }
    }
}

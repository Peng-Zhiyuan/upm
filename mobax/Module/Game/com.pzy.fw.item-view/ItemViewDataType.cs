using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemViewDataType
{
    /**
     * 服务器所定义的物品实例，由 ItemDatabase 提供的数据结构
     */
    ItemInfo,

    /**
     * 自行构造的物品实例
     */
    VirtualItemInfo,

    /**
     * 没有物品实例，仅指定物品种类
     */
    RowId,


    /**
     * 通用物品实例
     */
    CommonItem,

}

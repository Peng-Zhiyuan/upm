using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TransactionType : int
{
    /**
     * 数量增加
     */
    Add = 1,

    /**
     * 数量减少
     */
    Sub = 2,

    /**
     * 内容变更
     */
    Set = 3,

    /**
     * 数据对象被删除
     */
    Delete = 4,

    /**
     * 新数据对象
     */
    New = 5,

    /**
     * 已被自动分解
     */
    AutoDecomposition = 6,

    /**
     * 溢出，转而使用其他渠道发放（比如：邮件）
     */
    Overflow = 7,

    /// <summary>
    /// 不需要处理的数据
    /// </summary>
    NoUse = 99,
}
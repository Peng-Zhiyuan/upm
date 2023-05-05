using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderInfo 
{
    public string id;

    /// <summary>
    /// 用户 id
    /// </summary>
    public string uid;
    
    /// <summary>
    /// 统计用的金额(例如活动统计使用)
    /// </summary>
    public int rmb;

    /// <summary>
    /// 商品id
    /// </summary>
    public int item;

    /// <summary>
    /// 状态
    /// </summary>
    public OrderStatus status;

    /// <summary>
    /// 订单创建时间
    /// </summary>
    public long create;

    /// <summary>
    /// 发货时间
    /// </summary>
    public long submit;

    /// <summary>
    /// 实际付款金额
    /// </summary>
    public float money;

    /// <summary>
    /// 实际付款币种符号
    /// </summary>
    public string currency;
}

public enum OrderStatus
{
    /// <summary>
    /// 还没有收到支付回调
    /// </summary>
    Create = 0,
    
    /// <summary>
    /// 已经付款，未发货
    /// </summary>
    Paid = 8,

    /// <summary>
    /// 已经发货
    /// </summary>
    Complete = 9,
}

using UnityEngine;

public class GiftInfo
{
    public string id;
    public string uid;
    public int iid;
    public OrderStatus status;
    public GiftSource source;
    public long create;
    public long update;
    public long expire; // 0 表示不过期
}

public enum GiftSource
{
    // 平台礼包
    Igg = 1,

    // 游戏自己的礼包
    Trigger = 2,
}
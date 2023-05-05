using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleInfo
{
    // eg: {"_id":"1111a","sid":1,"guid":"j012","name":"Basil Thor","lv":1,"exp":0,"vip":0,"logon":1658938370,"login":1659169518,"show":0,"arena":0,"plant":null,"stage":0,"memory":0,"signin":{"1":{"time":1659110400,"idx":3}}

    // 用户 id，在某些接口里叫做 uid
    public string _id;

    public int sid;

    // 或许是服务器内部的用户对象 id
    public string guid;

    // 显示用的名称
    public string name;

    // 等级
    public int lv;

    public int exp;

    public int vip;

    // 看板娘
    public int show;

    // 头像
    public string icon;

    // 记忆回廊
    public int memory;

    // 上次登录时间
    public long login;

    // 注册时间
    public long logon;

    // 编队信息  按照目前人数  6个长度单位为一个编队组
    [Obsolete("已删除，请直接使用power调用战力")]
    public int[] corps;

    // 携带战斗道具信息 3个长度单位为一个编队组
    public int[] items;

    // 统计战力
    public int power;

    //挂机种菜
    public PlantData[] plant; //挂机种菜[plant1,plant2,plant3,plant4]

    /// <summary>
    /// 签到信息，rowId 到信息
    /// </summary>
    public Dictionary<int, RoleSignin> signin;

    /// <summary>
    /// 旧新手引导数据
    /// </summary>
    public int gs;

    /// <summary>
    /// 新手引导v2数据
    /// </summary>
    public Dictionary<string, int> guide;

    // /// <summary>
    // /// 公会训练
    // /// </summary>
    public Dictionary<int, GuildTrainInfo> train;

    // 助战
    public int assist;

    // 设置工坊材料
    public int support;

    // 粉丝数量
    public int fans;

    // 关注数量
    public int follow;

    // 签名
    public string describe;

    // 小游戏上次领奖时间
    public long evaluate;

    // 公会Id
    public int league;

    //抽卡相关
    public RoleGacha gacha;

    // 头像框 id_expire
    public string headframe;

    // 工会打点上报次数
    public int reported;

    public Dictionary<int, int> diamond;

    public RoleDoubleTask doubleTask;

    public RoleFund fund;

    public RoleCat cat;

    /// <summary>
    /// 首充
    /// </summary>
    public Dictionary<int, FirstChargeInfo> first;

    public void SetGuide(string key, int value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        if (this.guide == null)
        {
            this.guide = new Dictionary<string, int>();
        }
        this.guide[key] = value;
    }

    public int GetGuide(string key, int defaultValue)
    {
        if (this.guide == null)
        {
            return defaultValue;
        }
        if (!this.guide.ContainsKey(key))
        {
            return defaultValue;
        }
        var ret = this.guide[key];
        return ret;
    }

    //爬塔记录
    public int tower = 0;
}

public class RoleSignin
{
    /// <summary>
    /// 领取的时间戳
    /// </summary>
    public long time;

    /// <summary>
    /// 上次领取的是第几个，从 1 开始
    /// </summary>
    public int idx;
}

public class RoleFund
{
    public int[] buy;
    public Dictionary<int, int> submit;
    public Dictionary<int, int> high;
}

public class RoleCat
{
    public int id;
    public int num;
}
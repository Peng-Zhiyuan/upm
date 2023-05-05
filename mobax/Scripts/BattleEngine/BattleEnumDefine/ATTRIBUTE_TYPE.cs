using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("属性类型")]
    public enum AttrType
    {
        [LabelText("空")]
        None = 0,
        [LabelText("耐力")]
        VIM,
        [LabelText("生命")]
        HP,
        [LabelText("攻击")]
        ATK,
        [LabelText("防御")]
        DEF,
        [LabelText("破甲")]
        BREAK,
        [LabelText("暴击")]
        CRIT,
        [LabelText("韧性")]
        TENA,
        [LabelText("暴伤")]
        RATE,
        [LabelText("精准")]
        HIT,
        [LabelText("格挡")]
        BLOCK,
        [LabelText("穿透")]
        PHYS,
        [LabelText("疗伤")]
        CURE,
        [LabelText("急速")]
        HASTE,
        [LabelText("生命%")]
        HPUP,
        [LabelText("攻击%")]
        ATKUP,
        [LabelText("防御%")]
        DEFUP,
        [LabelText("增伤")]
        HURT,
        [LabelText("免伤")]
        PARRY,
        [LabelText("怒气")]
        RAGE,
        [LabelText("火伤")]
        FIREATK,
        [LabelText("雷伤")]
        ELECATK,
        [LabelText("水伤")]
        WATERATK,
        [LabelText("风伤")]
        WINDATK,
        [LabelText("光伤")]
        LIGHEATK,
        [LabelText("暗伤")]
        DARKATK,
        [LabelText("火抗")]
        FIREDEF,
        [LabelText("雷抗")]
        ELECDEF,
        [LabelText("水抗")]
        WATERDEF,
        [LabelText("风抗")]
        WINDDEF,
        [LabelText("光抗")]
        LIGHEDEF,
        [LabelText("暗抗")]
        DARKDEF,
        [LabelText("经验收益")]
        EXPUP,
        [LabelText("金币收益")]
        GOLDUP,
        [LabelText("掉宝收益")]
        DROPUP,
        [LabelText("移速")]
        MOVESPEED,
        [LabelText("攻速")]
        ATKSPEED,
        [LabelText("固定减伤")]
        WARD,
        [LabelText("固定伤害")]
        IMPAIR
        /*, [LabelText("击破")]
        BREAK*/,
        MAXNUM,
    }

    /// <summary>
    /// 技能赋予战斗计算类型
    /// </summary>
    public enum SKILL_ATTR_TYPE
    {
        NULL,
        DAM1, //技能系数
        DAM2, //技能附加
        DAM4, //自身受损生命系数附加
        DAM5, //自身最大生命系数附加
        EXTCRIT, //额外暴击
        EXTRATE, //额外暴伤
    }

    public enum MANA_TYPE
    {
        NONE = 0,
        //火
        FIRE = 1
        //雷
        ,
        ELEC = 2
        //水
        ,
        WATER = 3
        //风
        ,
        WIND = 4
        //光
        ,
        LIGHE = 5
        //暗
        ,
        DARK = 6
    }
}
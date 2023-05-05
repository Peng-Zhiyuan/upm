namespace BattleEngine.Logic
{
    [System.Serializable]
    public enum BUFF_EFFECT_TYPE
    {
        None = 0, //无效果
        ATTR = 1, //属性变化
        PHYSIC = 2, //物理伤害
        MANA = 3, //元素伤害
        REAL_DAMAGE = 4, //真实伤害
        CURE = 5, //治疗效果
        PROTECTION = 6, //吸收伤害
        SUCK = 7, //攻击吸血
        REATTACK = 8, //反弹伤害
        CURE_HALO = 9, //治疗回血
        BE_REBORN = 10, //重生
        CLEAR_DEBUFF = 11, //净化减益
        FORBID_DEBUFF = 12, //抵抗减益
        ADD_SKILL_DAMAGE_12 = 13, //强化技能伤害
        ADD_SKILL_CRIT_DAMAGE = 14, //强化技能暴击
        DAMAGE_TOGETHER = 15, //分担伤害
        ADD_SKILL_DAMAGE_45 = 16, //强化技能伤害
        KILL_OWN = 17, //自杀
        CURE_DAMAGE_HALO = 18, //受损生命比例回血
        MAX_HP_DAMAGE = 19, //最大生命百分比伤害
        CURRENT_HP_DAMAGE = 20, //当前生命百分比伤害
        CLEAR_CONTROL_TYPE_BUFF = 21, //清楚指定控制类型buff（buff表controlType字段配置）
        FORCE_DAMGE_VALUE = 22,//强制伤害扣血量
        REPLACE_SKILL = 23,//替换技能效果,根据技能类型相对应替换
        NOT_CURE = 24,//不受治疗效果影响
        CHANGE_AVATAR = 25,//不受治疗效果影响
    }

    [System.Serializable]
    public enum BUFF_EFFECT_TARGET
    {
        SELF = 0, //自身
        CURRENT_TARGET = 1, //当前技能目标
        ALL_ENEMY = 2, //所有敌人,
        ALL_ENEMY_MIN_HP = 3, //全场血量最小敌人,
        ALL_ENEMY_MAX_HP = 4, //全场血量最多敌人,
        MIN_HP_SELF_TEAM = 5, //血量最少的队友,
        ALL_SELF_TEAM = 6, //所有队友,
        ELEMENT_FIRE_SELF_TEAM = 7, //火属性队友
        ELEMENT_ELEC_SELF_TEAM = 8, //雷属性队友
        ELEMENT_WATER_SELF_TEAM = 9, //水属性队友
        ELEMENT_WIND_SELF_TEAM = 10, //风属性队友
        ELEMENT_LIGHT_SELF_TEAM = 11, //光属性队友
        ELEMENT_DARK_SELF_TEAM = 12, //暗属性队友
        HIT_TARGET = 13,//当前技能命中的所有敌人,被动技能1,3,14
    }

    [System.Serializable]
    public enum BUFF_COMMON_CONFIG_ID
    {
        CAT_ADD_ATTACK = 2700, //猫咪加攻
        BREAK_VIM = 2701, //耐力击破
        ATTACK_FOCUS = 2702, //集火指令
        DEFEND_FOCUS = 2703, //守护指令
        SUPER_ARMOR = 2704, //霸体
        DIZZINESS = 2705, //防守关昏迷
        BEAT_BACK = 2706, //击退
        AIR_BORNE = 2707, //击飞
        ARENA_STRONG = 2708, //竞技场强化
        FinalBlow = 2709, //关卡加速BUFF
        SSPSkill_Cant_Control = 2710, //大招时候的霸体效果
        Client_Effect_Control = 2711, //客户端表现效果,主要用于停止AI操作
        Linker_Battle_God = 2713, //玩家链接者助战时保持无敌效果
        Force_HP = 2719, //强制扣除血量
        Force_Debuff = 2720, //不受debuff效果
        FORBID_CURE = 2721, //不接受治疗
    }
}
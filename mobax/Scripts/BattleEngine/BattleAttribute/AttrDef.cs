using System.Collections.Generic;
using UnityEngine;

namespace BattleEngine.Logic
{
    public enum GlobalK
    {
        BLOCK_1 = 1, //格挡免伤率
        MANA_2, //元素克制保底
        DAMAGEADDMIN_3, //伤害加成保底
        DEFENCE_4, //防御计算系数
        JobDamageAdd_5, //职业伤害加成
        PhyK_6, //穿透系数
        CDMIN_7, //技能冷却保底
        FAST_8, //急速系数
        VTMMINUS_9, //耐力扣除保底系数
        DAMAGEMIN_10, //最小伤害保底
        Percent_1000 = 11, //计算系数千分比
        JobAddCrit_12 = 12, //职业克制加成
        DefenderParam_13 = 13, //职业克制加成
        VITCD_20 = 20, //耐力恢复时免疫扣除时间：秒
        APMAX_21 = 21, //玩家初始AP上限
        APSPEED_22 = 22, //AP恢复速度：点/秒
        UseItemCD_24 = 24, //道具使用公共CD
        SubstituteJoinCD_25 = 25, //替补公共CD
        ENERGYMAX_30 = 30, //默认怒气值上限
        ENERGYINIT_31, //角色进入战斗初始怒气值
        ENERGYPER_32, //每次攻击增加怒气值（废弃）
        ENERGYATTACKED_33, //受到攻击增加怒气值
        ENERGYKILL_34, //击杀敌人增加怒气值
        NORMAL_BATTLETIME_35, //PVE整场战斗事件(秒)
        SUMMON_CAT_36, //猫出厂时间
        SUMMON_CAT_PER_37, //猫出厂概率
        Friend_To_Battle_38, //好友助战出场时间(秒)
        AtkFocusOnFireTime_39, //集火时间(秒)
        DefFocusOnFiretime_40, //保护时间(秒)
        BreakDefDamage_41, //破防后伤害倍率
        QTEBreakDefDamage_42, //破防QTE后伤害倍率
        SELF_OT_CHANGE_TARGET_MAX_VALUE_43, //仇恨切换目标阈值(己方)
        
        ExternalBuffer_50 = 50,
        ExternalBuffer_51,
        ExternalBuffer_52,
        ExternalBuffer_53,
    }

    public enum WeakType
    {
        None = 0,
        Fire = 1,
        Thunder = 2,
        Water = 4,
        Windy = 8,
        Light = 16,
        Dark = 32,
    }

    public enum Job
    {
        None = 0,
        JieWei = 1,
        TuXi = 2,
        YouJi = 3,
        ZhenDi = 4,
        ZhiYuan = 5,
    }

    public enum HitType
    {
        None = 0, //
        Hit = 1 << 1, //命中
        Crit = 1 << 2, //暴击
        Block = 1 << 4, //格挡
        Fire = 1 << 5, //火
        Electric = 1 << 6, //雷
        Water = 1 << 7, //水
        Wind = 1 << 8, //风
        Light = 1 << 9, //光
        Dark = 1 << 10, //暗
        Real = 1 << 11, //真实伤害
        Skill = 1 << 12, //技能伤害
        Normal = 1 << 13, //普攻伤害
        Cure = 1 << 14, //治疗伤害
        Protect = 1 << 15, //护盾
        XiShou = 1 << 16, //吸收
        Suck = 1 << 17, //吸血
        AntDamage = 1 << 18, //反弹
        Angry = 1 << 19, //怒气
        Dun = 1 << 20, //定身
        Stun = 1 << 21, //眩晕
        MinusDamage = 1 << 22, //固定减伤
        Weak = 1 << 23, //克制
        Break = 1 << 24, //耐力击破
        BreakDef = 1 << 25, //破防伤害
        ShareDamage = 1 << 26, //分担伤害
        BuffDamage = 1 << 27, //Buff伤害
        SPSKLDamage = 1 << 28, //大招伤害
    }

    public enum DamageType
    {
        None = 0,
        Physic, //物理伤害
        Mana, //元素伤害
        Real, //真实伤害
        Cure, //治疗技能
    }

    public enum emFinalAttr_Cal
    {
        CalcPer, //param1 * (1 + param2/ 1000.0)
        CalcAssign, // = param
    }

    public sealed class FinAttr
    {
        public AttrType type;
        public emFinalAttr_Cal cal;
        public int min;
        public int max;
        public int value;
        public List<AttrType> RealateAblis = new List<AttrType>();
    }

    public static class RoleAttrHelper
    {
        public static Dictionary<AttrType, FinAttr> finAblities = new Dictionary<AttrType, FinAttr>();

        static RoleAttrHelper()
        {
            RegistAttr();
        }

        public static void RegistAttr()
        {
            //耐力
            RegistFinalAttr(AttrType.VIM, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.VIM);
            //血量
            RegistFinalAttr(AttrType.HP, 0, 200000000, emFinalAttr_Cal.CalcPer, AttrType.HP, AttrType.HPUP);
            //攻击
            RegistFinalAttr(AttrType.ATK, 0, 200000000, emFinalAttr_Cal.CalcPer, AttrType.ATK, AttrType.ATKUP);
            //防御
            RegistFinalAttr(AttrType.DEF, 0, 200000000, emFinalAttr_Cal.CalcPer, AttrType.DEF, AttrType.DEFUP);
            //破甲
            RegistFinalAttr(AttrType.BREAK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.BREAK);
            //暴击
            RegistFinalAttr(AttrType.CRIT, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.CRIT);
            //任性
            RegistFinalAttr(AttrType.TENA, -200000, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.TENA);
            //暴伤
            RegistFinalAttr(AttrType.RATE, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.RATE);
            //精准
            RegistFinalAttr(AttrType.HIT, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.HIT);
            //格挡
            RegistFinalAttr(AttrType.BLOCK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.BLOCK);
            //职业精通
            RegistFinalAttr(AttrType.PHYS, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.PHYS);
            RegistFinalAttr(AttrType.CURE, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.CURE);
            RegistFinalAttr(AttrType.HASTE, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.HASTE);
            RegistFinalAttr(AttrType.HPUP, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.HPUP);
            RegistFinalAttr(AttrType.ATKUP, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.ATKUP);
            RegistFinalAttr(AttrType.DEFUP, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.DEFUP);
            RegistFinalAttr(AttrType.HURT, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.HURT);
            RegistFinalAttr(AttrType.PARRY, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.PARRY);
            RegistFinalAttr(AttrType.RAGE, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.RAGE);
            RegistFinalAttr(AttrType.FIREATK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.FIREATK);
            RegistFinalAttr(AttrType.ELECATK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.ELECATK);
            RegistFinalAttr(AttrType.WATERATK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.WATERATK);
            RegistFinalAttr(AttrType.WINDATK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.WINDATK);
            RegistFinalAttr(AttrType.LIGHEATK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.LIGHEATK);
            RegistFinalAttr(AttrType.DARKATK, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.DARKATK);
            RegistFinalAttr(AttrType.FIREDEF, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.FIREDEF);
            RegistFinalAttr(AttrType.ELECDEF, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.ELECDEF);
            RegistFinalAttr(AttrType.WATERDEF, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.WATERDEF);
            RegistFinalAttr(AttrType.WINDDEF, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.WINDDEF);
            RegistFinalAttr(AttrType.LIGHEDEF, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.LIGHEDEF);
            RegistFinalAttr(AttrType.DARKDEF, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.DARKDEF);
            RegistFinalAttr(AttrType.MOVESPEED, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.MOVESPEED);
            RegistFinalAttr(AttrType.ATKSPEED, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.ATKSPEED);
            RegistFinalAttr(AttrType.WARD, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.WARD);
            RegistFinalAttr(AttrType.IMPAIR, 0, 200000000, emFinalAttr_Cal.CalcAssign, AttrType.IMPAIR);
        }

        public static void RegistFinalAttr(AttrType type, int min, int max, emFinalAttr_Cal cal, params object[] args)
        {
            FinAttr finAblity = new FinAttr();
            finAblity.type = type;
            finAblity.min = min;
            finAblity.max = max;
            finAblity.cal = cal;
            foreach (var VARIABLE in args)
            {
                finAblity.RealateAblis.Add((AttrType)VARIABLE);
            }
            finAblities[type] = finAblity;
        }

        public static void Recalculate(BaseAttr abilities, ref Dictionary<AttrType, int> val)
        {
            foreach (var VARIABLE in finAblities)
            {
                var relates = VARIABLE.Value.RealateAblis;
                switch (VARIABLE.Value.cal)
                {
                    case emFinalAttr_Cal.CalcPer:
                    {
                        val[VARIABLE.Key] = (int)(abilities.GetAttrValue(relates[0]) * (1 + abilities.GetAttrValue(relates[1]) / 1000f));
                        break;
                    }
                    case emFinalAttr_Cal.CalcAssign:
                    {
                        val[VARIABLE.Key] = abilities.GetAttrValue(relates[0]);
                        break;
                    }
                    default:
                    {
                        BattleLog.LogWarning("无效的");
                        break;
                    }
                }
            }
        }

        public static void FixFinalAttr(float percent, ref Dictionary<AttrType, int> val)
        {
            val[AttrType.VIM] = Mathf.FloorToInt(val[AttrType.VIM] * percent);
            val[AttrType.HP] = Mathf.FloorToInt(val[AttrType.HP] * percent);
            val[AttrType.DEF] = Mathf.FloorToInt(val[AttrType.DEF] * percent);
            val[AttrType.ATK] = Mathf.FloorToInt(val[AttrType.ATK] * percent);
        }

        public static void RecalculateOneAttr(BaseAttr abilities, AttrType type, ref Dictionary<AttrType, int> val)
        {
            FinAttr VARIABLE = finAblities[type];
            var relates = VARIABLE.RealateAblis;
            switch (VARIABLE.cal)
            {
                case emFinalAttr_Cal.CalcPer:
                {
                    val[type] = (int)(abilities.GetAttrValue(relates[0]) * (1 + abilities.GetAttrValue(relates[1]) / 1000f));
                    break;
                }
                case emFinalAttr_Cal.CalcAssign:
                {
                    val[type] = abilities.GetAttrValue(relates[0]);
                    break;
                }
                default:
                {
                    BattleLog.LogWarning("无效的");
                    break;
                }
            }
        }
    }
}
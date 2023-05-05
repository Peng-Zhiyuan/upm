using Sirenix.OdinInspector;

namespace BattleEngine.Logic
{
    [System.Serializable]
    [LabelText("BUFF触发类型")]
    public enum BUFF_TRIGGER_TYPE
    {
#region 主动技能触发类型
        //无条件触发
        NONE = 0,
        //使用该技能且自身生命介于X~Y%时触发
        Activity_SELF_HP_RANGE = 1,
        //技能命中生命介于X~Y%的敌人时
        Activity_TARGET_HP_RANGE = 2,
        //使用该技能造成暴击时
        Activity_CAUSE_CRIT = 3,
        //使用该技能命中有指定ID buff的敌人时触发
        Activity_HAVE_BUFF_ID = 4,
        //使用该技能命中有指定类型buff的敌人时触发
        Activity_HAVE_BUFF_CONTROL_TYPE = 5,
#endregion

#region 被动技能触发类型
        //自身受到攻击且生命介于X~Y%时触发
        PASSIVE_BEHIT_HP_RANGE = 10,
        //自身受到暴击时触发
        PASSIVE_BEHIT_CRIT = 11,
        //自身触发格挡时触发
        PASSIVE_BEHIT_BLOCK = 12,
        //自身进入关卡X秒后触发
        PASSIVE_ENTER_FIGHT_TIME = 13,
        //自身释放指定类型技能命中触发
        PASSIVE_SPELL_SKILL_HIT = 14,
        //场上有角色死亡时触发
        PASSIVE_HAVE_DEAD_EVENT = 15,
        //自身攻击有指定buff的敌人时触发
        PASSIVE_ATTACK_TARGETBUFF = 16,
        //受到攻击时，概率触发
        PASSIVE_BEHIT = 17,
        //命中目标概率触发
        PASSIVE_HIT = 18,
        //场上有特定性别的角色
        PASSIVE_HAVE_SEX_HERO = 19,
        //角色从替补位上场
        PASSIVE_HERO_TURNON = 20,
        //场上，存在特定元素的角色
        PASSIVE_HAVE_MANA = 21,
        //释放大招结束后
        PASSIVE_SSPSKILL_FINISH = 22,
        //自身攻击有指定类型buff的敌人时触发
        PASSIVE_TARGET_BUFF_CONTROL = 23,
        //自身攻击造成暴击时触发
        PASSIVE_HIT_CRIT = 24,
        //自身受到治疗时触发
        PASSIVE_BE_CURE = 25,
        //战斗中每波敌人出现时
        PASSIVE_WAVE_EXECUTE = 26,
        //自身受到任意负面buff效果时触发
        PASSIVE_DEBUFF_TRIGGER = 27,
        //主角使用集火命令时
        PASSIVE_PLAYER_USE_FOCUS = 28,
        //场上出现特定角色时
        PASSIVE_TARGET_HERO = 29,
        //自身攻击且自身怒气介于X~Y时触发（固定值）
        PASSIVE_ATTACK_MP_RANGE = 30,
        //自身受到攻击且耐力介于X~Y时触发（固定值）
        PASSIVE_ATTACK_VIM_RANGE = 31,
        //自身被破防时触发
        PASSIVE_BE_BREAK = 32,
        //自身击杀敌人时触发
        PASSIVE_KILL_ENEMY = 33,
        //自身受到指定buff时触发
        PASSIVE_SELF_TARGET_BUFF = 34,
        //自身死亡时触发
        PASSIVE_DEAD_SELF = 35,
        //自身攻击比自身耐力低的敌人时触发
        PASSIVE_ATTACK_ENEMY_VIM = 36,
        //场上存在特定品质以上的角色触发(开场每存在1个生效1次）
        PASSIVE_TARGET_QLV = 37,
        //自身攻击特定元素的敌人时(1.火 2.雷 3.水4.风 5.光 6.暗)
        PASSIVE_TARGET_ELEMENT = 38,
        //自身攻击特定性别的敌人时(1.男 2.女)
        PASSIVE_TARGET_SEX = 39,
#endregion
    }
}
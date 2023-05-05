using System.Collections.Generic;
using UnityEngine;

namespace BattleSystem.Core
{
    public enum HurtType
    {
        P_HURT = 0, //物理伤害
        CURE = 1, //常规治疗
        M_HURT = 2, //魔法伤害
        FIRE = 3, //火系伤害
        WATER = 4, //水系伤害
        THUNDER = 5, //雷系伤害
        IMPRISON = 6, //禁锢
        VERTIGO = 7, //眩晕
        TAUNT = 8, //嘲讽
        DROP = 9, //坠落
        REPULSE = 10, //击退
    }
}
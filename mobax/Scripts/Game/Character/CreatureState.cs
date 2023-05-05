using UnityEngine;
using System.Collections;

public enum State
{
    Invalid = 0,        //未定义状态(异常状态)

    STATE_IDLE = 64,
    STATE_MOVE,         //移动状态
    STATE_RUN,          //跑动状态
    STATE_ATTACK,       //攻击
    STATE_PURSUE,       //追击
    STATE_INTERACTIVE,  //交互状态
    STATE_DEAD,
    STATE_HURT,
    STATE_UNAWAKED,
    STATE_CHANT,
    STATE_TRANSPORT, //传送出去
    STATE_TRANSPORTIn, //传送进入
    STATE_FightIdle, //战斗待机
    STATE_VICTORY,
    STATE_STUN,
    STATE_SUBSTITUTE,
    STATE_JOIN,
    STATE_LEAVE,
    STATE_AIRBOREN,//击飞效果
    STATE_SHOWWEAPON,
    STATE_LIMITED,
    STATE_STOP,
    STATE_SPRING,
}
